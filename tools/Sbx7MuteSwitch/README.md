# Sbx7MuteSwitch

Windows-side helper for the SBX7 mode-switch pop. It mutes the device through the
installed Creative control panel COM server, waits, applies a Direct Mode or SBX
Master change, waits again, then restores the previous mute state.

It does **not** modify firmware, install a driver or flash the device.

## Requirements

- Windows with the Sound Blaster X7 driver and control panel installed.
- The installed `Interop.CtSndCr.dll` (normally under the control panel folder).
- .NET Framework 4.7.2 or newer; .NET SDK to build.

The control panel's COM server must be registered by the normal driver install.
This tool uses the same `CTSoundCore.CtHdaMgrClass` interface as the control panel.

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

## Use

Check the parameter plan first:

```powershell
.\Sbx7MuteSwitch.exe --mode direct --value off --dry-run
```

Then run the switch with a one-second mute on each side:

```powershell
.\Sbx7MuteSwitch.exe --mode direct --value off --hold-ms 1000
```

Other modes:

```powershell
.\Sbx7MuteSwitch.exe --mode stereo-direct --value on
.\Sbx7MuteSwitch.exe --mode sbxmaster --value off
```

Options:

- `--hold-ms N`: wait N ms after muting and after the switch (default 1000).
- `--dry-run`: print the feature/parameter IDs without calling COM.
- `--no-restore`: leave the device muted after the switch.

The tool reads the current mute value before changing anything and restores it in a
`finally` block, so a failed mode change does not leave the device muted.

## What it changes

| Action | Feature | Parameter |
|---|---|---|
| Mute | `eFeature_Effects_SpeakerCalibrator` (`0x10000080`) | `eParamSpeakerCalibrator_Mute` (26) |
| Direct Mode | `eFeature_System_ProcessingControl` (`0x01000002`) | `eParamProcessingControl_DirectMode` (7) |
| Stereo Direct Mode | `eFeature_System_ProcessingControl` (`0x01000002`) | `eParamProcessingControl_StereoDirectMode` (8) |
| SBX Master | `eFeature_EfxMasterControl` (`0x60000001`) | `eParamEfxMasterControl_SBXMasterOnOff` (7) |

The IDs come from the control panel's `Interop.CtSndCr.dll` and `SBX7.exe` metadata.

## Limits

- Not verified on a real SBX7 here; the analysis host is macOS. Test on the Windows
  machine with the device, at low amplifier volume first.
- If the pop is produced after the device's mute point (DSP/DAC/firmware), the
  host-side mute may not remove it. That result would point to firmware or DSP
  behavior and is recorded as an unknown in the project report.
- The tool does not bypass any protection, modify firmware or change the driver.