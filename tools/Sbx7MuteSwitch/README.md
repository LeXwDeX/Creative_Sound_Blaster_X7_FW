# Sbx7MuteSwitch

Windows-side helper for the SBX7 mode-switch pop. It binds to the installed
Creative control panel COM server, mutes the device through the
`SpeakerCalibrator_Mute` parameter, applies a Direct Mode / SBX Master change,
then restores the previous mute state.

It does **not** modify firmware, install a driver or flash the device.

## Requirements

- Windows with the Sound Blaster X7 driver and control panel installed.
- The installed `Interop.CtSndCr.dll` (normally under the control panel folder).
- .NET Framework 4.7.2 or newer; .NET SDK to build.
- NAudio (restored from NuGet) for render-endpoint discovery.

## Build

Point the build at the installed interop assembly:

```powershell
$env:INTEROP_CT_SNDCR_PATH = "C:\Program Files (x86)\Creative\Sound Blaster X7\Sound Blaster X7 Control Panel\Interop.CtSndCr.dll"
dotnet build -c Release
```

Or pass it directly:

```powershell
dotnet build -c Release -p:InteropCtSndCrPath="C:\...\Interop.CtSndCr.dll"
```

The output is `bin\Release\net472\Sbx7MuteSwitch.exe` (32-bit).

## Windows quick test (no build)

A packaged folder is produced as `artifacts/Sbx7MuteSwitch-win-x86.zip` on the
analysis machine (build artifact, not committed). Unzip it on Windows and
double-click, in order:

1. `probe.bat` — read-only; confirms the device and mute parameter are reachable.
2. `test-direct-off.bat` — low volume; mute 1 s, Direct Mode OFF, wait 1 s, restore.
3. `test-sbxmaster-off.bat` — the same sequence for SBX Master.

Each script pauses at the end, so the result stays visible.

## First: probe only

Do not start with a mode switch. Bind and read the current mute value first:

```powershell
.\Sbx7MuteSwitch.exe --probe
```

If auto-detection cannot find the X7, pass the endpoint ID explicitly:

```powershell
.\Sbx7MuteSwitch.exe --probe --endpoint-id "{0.0.0.00000000}.{...}"
```

The tool tries the U1 CLSID `495E4C24-85ED-4f19-885E-C2D01D7EA26C` first, then
the legacy `7BFF1F7F-1D65-49A0-AE26-7903C41BC5B3`. Override with `--clsid`.

## Mode switch

```powershell
.\Sbx7MuteSwitch.exe --mode direct --value off --dry-run
.\Sbx7MuteSwitch.exe --mode direct --value off --hold-ms 1000
```

Other modes:

```powershell
.\Sbx7MuteSwitch.exe --mode stereo-direct --value on
.\Sbx7MuteSwitch.exe --mode spdif-passthrough --value off
.\Sbx7MuteSwitch.exe --mode sbxmaster --value off
```

Options:

- `--probe`: bind, read context and mute value, change nothing.
- `--mode direct|stereo-direct|spdif-passthrough|sbxmaster`
- `--value on|off`
- `--endpoint-id ID`: omit to auto-detect an active render endpoint named
  "Sound Blaster X7".
- `--clsid GUID`: SoundCore CLSID; default is the U1 CLSID above.
- `--hold-ms N`: wait N ms after muting and after the switch (default 1000).
- `--dry-run`: print the parameter plan without calling COM.
- `--no-restore`: leave the device muted after the switch.

The tool reads the current mute value before changing anything and restores it in
a `finally` block, so a failed mode change does not leave the device muted.

## What it changes

| Action | Feature | Parameter |
|---|---|---|
| Mute | `eFeature_Effects_SpeakerCalibrator` (`0x10000100`) | `eParamSpeakerCalibrator_Mute` (26) |
| Direct Mode | `eFeature_System_ProcessingControl` (`0x01000002`) | `eParamProcessingControl_DirectMode` (7) |
| Stereo Direct Mode | `eFeature_System_ProcessingControl` (`0x01000002`) | `eParamProcessingControl_StereoDirectMode` (8) |
| S/PDIF In Passthrough | `eFeature_System_ProcessingControl` (`0x01000002`) | `eParamProcessingControl_SPDIFInPassThrough` (9) |
| SBX Master | `eFeature_EfxMasterControl` (`0x60000001`) | `eParamEfxMasterControl_SBXMasterOnOff` (7) |

The IDs come from the control panel's `Interop.CtSndCr.dll` and `SBX7.exe` metadata.
`0x10000080` is `SpeakerEnhancement`, not `SpeakerCalibrator`.

## Limits

- **Not verified on a real SBX7.** The analysis host is macOS; the COM calls have
  not been exercised. Start with `--probe`, then a low-volume A/B test.
- The path from `SpeakerCalibrator_Mute` (param 26) to the Malcolm SCP
  `ParamType_MasterMute` (145 / address `0x963A`) is inferred from the
  `CTHIDRpA.dll` table and the control panel's miscellaneous-command path; it has
  not been observed on the wire. The tool's mute may not silence the same output
  or the same DSP stage as the pop.
- If the pop is produced after the device's mute point (DSP/DAC/amplifier/relay),
  the host-side mute may not remove it. That result would point to firmware or DSP
  behavior and is recorded as an unknown in the project report.
- The tool does not bypass any protection, modify firmware or change the driver.