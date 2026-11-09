# 报告：Sound Blaster X7 (SBX7) 固件/驱动分析

> 项目：`projects/soundcard-firmware/` · 创建：2026-11-09 · 状态：verifying
> 目标：`SBX7_PCDRV_L9_1_02_03.exe`、`SBX7_PCFW_EN_200321.exe` · 模式：`deep`（静态）
> 授权：用户自有设备与 Creative 官方下载文件；未执行安装器、未连接/刷写设备。

## 1. 目标与授权

| 文件 | 大小 | SHA-256 | 格式 |
|---|---:|---|---|
| `SBX7_PCDRV_L9_1_02_03.exe` | 119 MB | `5f911460fb028b806be49b0d3576fad67fedada74f393ec5145d1f63191e0de3` | PE32 InstallShield 自解压 |
| `SBX7_PCFW_EN_200321.exe` | 4.2 MB | `32795a5b06b99504b40429a65a20c415fc305269225b7b2b3426aa23bb993e6d` | PE32 GUI，SBHIDFlash.EXE |

范围外：运行安装器/升级器、安装驱动、连接或刷写设备、漏洞利用、修改厂商签名。

## 2. 结论摘要

- `SBX7_PCFW_EN_200321.exe` 是 **Creative Device HID Firmware Upgrade Application**（`SBHIDFlash.EXE`，产品版本 `1.2.200321.170904`），通过 HID 与设备通信，内含 U1、Malcolm、Gilroy、PIC32、CSR Bluetooth 等升级类。
- 固件资源在 PE 的 `.rsrc/RCDATA` 中。`MALCOLM_SBX_RCDATA`（314,457 字节）含 `Copyright (c) 2011 Creative Labs, Inc. ... U1 2020/03/12-22:23:55`，是与 SBX7/U1 最匹配的主固件候选。
- `BC08_RCDATA` 以 `CSR-dfu2` / `ADK3.0 DFU` 开头，是 CSR 蓝牙 DFU 固件；`PIC32_RCDATA` 是 PIC32 的 Intel HEX；`GILROY_RCDATA` 为高熵二进制，未见明文。
- 驱动包是 InstallShield，内含 `Sound Blaster X7.msi`、控制面板与驱动；控制面板 `SBX7.exe` 是 .NET，`SndCrUSB.DLL` 实际为 `KsMalcCtl.DLL` COM 服务，`Interop.CtSndCr.dll` 是 P/Invoke 包装。
- 控制面板的 `Direct Mode` 与 `SBX Master` 点击处理是**直接 SetParam**，没有 mute、淡出或延时：
  - `ckboxDirectMode_Click` → `MalCtrl.SetParamInt(MalcolmControlID.SpkDirectMode, 1/0)`
  - `btnSBXMaster_Click` → `SetParamBool(MalcolmControlID.SBXMasterOnOff, ...)`
  - `btnVolMute_Click` 调的是 **Windows 播放静音**（`CTAudLib.SetWindowsPlaybackMute`），不是设备输出静音。
- 设备侧存在独立静音参数：`eFeature_Effects_SpeakerCalibrator`（`0x10000080`）+ `eParamSpeakerCalibrator_Mute = 26`。可以尝试在模式切换前后调用它，制造约 1 秒静默。

## 3. 固件包结构

`SBX7_PCFW_EN_200321.exe` 的 `.rsrc`：

| 资源 | 大小 | SHA-256 前缀 | 观察 |
|---|---:|---|---|
| `BC08_RCDATA` | 1,555,028 | `6902232a…` | `CSR-dfu2`、`ADK3.0 DFU`；CSR 蓝牙 DFU |
| `PIC32_RCDATA` | 649,347 | `34acb88a…` | ASCII Intel HEX；PIC32（MIPS）固件 |
| `MALCOLM_SBX_RCDATA` | 314,457 | `f0f4fb57…` | `U1 2020/03/12-22:23:55`；SBX7 主固件候选 |
| `GILROY_RCDATA` | 214,744 | `7a52fc4e…` | 高熵，无明文；另一设备/固件候选 |

升级器字符串中可见 `CFirmwareUpgradeU1::EndFirmwareUpgradeMalcolm/Gilroy/PIC32/CSRBluetooth`、`FirmwareUpgradeWriteMalcolm`、`GetMCUType`、`SetActiveMCU`、`InitMCUData`、`WaitForUnitReady`，以及 `HID.DLL` 的 `HidD_GetFeature` / `HidD_SetFeature` / `HidD_GetInputReport` 等导入。它通过 HID 选择 MCU 并写入固件，不是简单的文件复制。

## 4. 控制面板与静音路径

控制面板（`SBX7.exe` + `Interop.CtSndCr.dll` + `SndCrUSB.DLL`）通过 COM 调用 `CTSoundCore.CtHdaMgrClass`（CLSID `7BFF1F7F-1D65-49A0-AE26-7903C41BC5B3`）的 `ISoundCore.SetParamValue`。

关键参数：

| 动作 | Feature | Param |
|---|---|---|
| 设备静音 | `eFeature_Effects_SpeakerCalibrator` = `0x10000080` | `eParamSpeakerCalibrator_Mute` = 26 |
| Direct Mode | `eFeature_System_ProcessingControl` = `0x01000002` | `eParamProcessingControl_DirectMode` = 7 |
| Stereo Direct Mode | `eFeature_System_ProcessingControl` = `0x01000002` | `eParamProcessingControl_StereoDirectMode` = 8 |
| SBX Master | `eFeature_EfxMasterControl` = `0x60000001` | `eParamEfxMasterControl_SBXMasterOnOff` = 7 |

`_stParam` 的字段顺序是 `paramId`、`featureId`、`contextId`；`_stParamValue` 是 `paramType` + union。控制面板用 `eParamType_Bool` 设置静音/SBX Master，用 `eParamType_Dword` 设置 Direct Mode。

## 5. 爆音根因假设

`observation`：控制面板切换 Direct Mode / SBX Master 时没有设备静音或延时（E003）。
`inference`：切换会重配置 DSP/解码路径，瞬时直流或时钟变化直接送到输出，形成爆音；普通模式解码 Dolby 时该路径仍在输出，因此没有静默期。
`inference`：如果 `eParamSpeakerCalibrator_Mute` 的静音点在爆音源之前，主机侧“静音 → 1s → 切换 → 1s → 恢复”可以遮住爆音；如果爆音发生在静音点之后（DAC/功放/固件内部），主机侧静音无效。

这不是已证实的根因；需要 Windows + 真机实验或固件/HID 追踪确认。

## 6. 重建说明

不适用：本次是静态取证与驱动/固件分析，未做净室重建。

## 7. 比对结果

不适用：没有重建物或跨版本比对。

## 8. 验证工具

`tools/Sbx7MuteSwitch/` 是一个 Windows 侧小工具，使用控制面板的 COM 接口：

1. 读取当前 `SpeakerCalibrator_Mute`；
2. 设为静音；
3. 等待 `--hold-ms`（默认 1000 ms）；
4. 设置 `direct` / `stereo-direct` / `sbxmaster`；
5. 再等待 `--hold-ms`；
6. 在 `finally` 中恢复原静音状态。

已用厂商 `Interop.CtSndCr.dll` 编译通过（0 warnings / 0 errors），但**未在真实 Windows + SBX7 上运行**。构建与用法见该目录的 `README.md`。

## 9. 未知项与限制

- `U001`：`MALCOLM_SBX_RCDATA` 是否就是 SBX7 当前主固件；需要升级器代码的资源选择逻辑或设备版本字符串确认。
- `U002`：`eParamSpeakerCalibrator_Mute` 的静音点是否在爆音源之前；需要 Windows 真机实验。
- `U003`：用户所说的“SB → 普通”具体对应 Direct Mode、Stereo Direct Mode、SBX Master 还是 Dolby 解码开关；需要在控制面板上确认。
- `U004`：固件是否已有模式切换静音/渐变；需要分析 `MALCOLM_SBX_RCDATA` 或抓取 HID 命令。
- `U005`：`GILROY_RCDATA` 与 `MALCOLM_SBX_RCDATA` 的设备映射；升级器支持多个 MCU/产品。
- 未执行任何安装器/固件升级器；未连接设备；未验证签名/信任链。
- 本机是 macOS，无法运行 Windows COM 工具或真实设备测试。

## 10. 输入哈希与复现

```text
5f911460fb028b806be49b0d3576fad67fedada74f393ec5145d1f63191e0de3  SBX7_PCDRV_L9_1_02_03.exe
32795a5b06b99504b40429a65a20c415fc305269225b7b2b3426aa23bb993e6d  SBX7_PCFW_EN_200321.exe
```

复现（只读）：

```bash
# 固件包资源
7zz l SBX7_PCFW_EN_200321.exe
7zz x -oartifacts/pcfw SBX7_PCFW_EN_200321.exe

# 驱动包
tail -c +98324 SBX7_PCDRV_L9_1_02_03.exe > artifacts/driver/driver.cab
7zz x -oartifacts/driver/files artifacts/driver/driver.cab

# 控制面板反编译（ILSpy）
ilspycmd -p -o artifacts/decompiled/SBX7 ".../SBX7.exe"
ilspycmd -p -o artifacts/decompiled/Interop ".../Interop.CtSndCr.dll"
```

## 11. 清理情况与残留

- 未执行安装器或固件升级器；未连接/刷写设备。
- `artifacts/` 保存解包与反编译结果，已在 `.gitignore` 中排除，不提交到仓库。
- `tools/Sbx7MuteSwitch` 的 `bin/`、`obj/` 也在 `.gitignore` 中。
- 仓库只提交报告、计划、notes、脚本与工具源码。

## 12. Evidence 索引

| ID | 主题 | 类别 | 位置 |
|---|---|---|---|
| E001 | PCFW 身份、资源清单、字符串 | observation | `artifacts/pcfw/`（本地） |
| E002 | 驱动包与 MSI/控制面板文件 | observation | `artifacts/driver/`（本地） |
| E003 | 控制面板 Direct/SBX/静音处理器反编译 | observation + derivation | `artifacts/decompiled/`（本地） |
| E004 | COM 参数枚举与结构体 | derivation | `artifacts/decompiled/Interop/`（本地） |
| E005 | `Sbx7MuteSwitch` 编译通过 | observation | `tools/Sbx7MuteSwitch/` |