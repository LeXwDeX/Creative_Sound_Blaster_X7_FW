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
- 设备侧存在独立静音参数：`eFeature_Effects_SpeakerCalibrator`（`0x10000100`）+ `eParamSpeakerCalibrator_Mute = 26`。`0x10000080` 是 `SpeakerEnhancement`，不是 SpeakerCalibrator。可以尝试在模式切换前后调用它，制造约 1 秒静默。

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
| 设备静音 | `eFeature_Effects_SpeakerCalibrator` = `0x10000100` | `eParamSpeakerCalibrator_Mute` = 26 |
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
- `U004`：固件是否已有模式切换静音/渐变；已完成 MALCOLM 的头部/浮点表/8051 尾部结构识别，但尚未反汇编到 SCP 0x963A 或 fade/mute 状态机。
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
| E006 | 升级器 CFirmwareUpgradeU1 / HID 报文 / 资源选择 | derivation | `artifacts/workflow/`、PCFW 反汇编 |
| E007 | MALCOLM / PIC32 / BC08 / GILROY 容器与架构识别 | derivation | `artifacts/pcfw/.rsrc/RCDATA` |
| E008 | SndCrUSB → CTHIDRpA → CTHID → HID 链路与参数表 | observation + inferred | `artifacts/decompiled/`、`CTHIDRpA.dll` |
| E009 | 2015 驱动包与 2020 PCFW 固件资源差异 | observation | `artifacts/workflow/updatefw/.rsrc/RCDATA` |

## 13. 深度分析补充（2026-11-09）

**完成度**：静态层面完成度较高；动态与真机验证为 0。准确说法是“升级器与主机参数路径基本查清、固件完成容器与架构识别，端到端解释和根因定位未完成”，不能回答“逆向完了”。

**升级器**：

- 已定位 `CFirmwareUpgradeU1` 的 `InitMCUData`、`SetActiveMCU`、`GetMCUType`、`LoadEmbeddedImage`（0x429e00 附近）、`FirmwareUpgradeWriteMalcolm`、`EndFirmwareUpgradeMalcolm` 等路径；Malcolm 的擦除/写入/校验/重试与 65 字节 HID 报文已从反汇编推断。
- 2020 PCFW 与 2015 驱动包的固件资源是不同构建：MALCOLM_SBX 314,457 vs 314,366 B；GILROY 214,744 vs 211,408 B；PIC32 649,347 vs 605,635 B；BC08 1,555,028 vs 1,561,044 B。任何固件结论都要标注来源构建。
- `MALCOLM_SBX` 在运行时是否被 `CFirmwareUpgradeU1` 选中，尚未找到 `[U1+4]` 选择标志的静态写入点；2015 版 `A_SBX→MALCOLM_SBX` 日志分支只是支持性证据。

**固件镜像**：

- `MALCOLM_SBX`：14 字节私有头 + 83 字节 U1 版本串 + 32 位对齐数据（inferred：DSP 字流/微码，不是已证实的指令集）+ 两段 `A1A2A3A4` 哨兵 + `BAADC0DE` + 0x4B528 起 256 项 IEEE-754 LE 浮点表 + 0x4B432 起 20 条 6 字节 thunk + 0x4B96E 起 8051 字节。8051 的加载基址/分段与主体 ISA 仍未确定。
- `PIC32`：Intel HEX 解码为 MIPS32 little-endian，主段 0x1D0060FC–0x1D03C223，DEVCFG @0x1FC02FF0；含 `Sound Blaster X7`、`iAP2`、`iPod`、`Attached - Host Mode` 等字符串，以及 VID 041E/PID 323A 的 USB 设备描述符。不是字面量“USB host”。
- `BC08`：CSR DFU2 容器，0x400 头、0x4C8 起 `CSRbcfw1`（ADK3.0 DFU / BlueCore7 为推断），未压缩未加密。
- `GILROY`：熵 7.9956，8 字节对齐重复块；推断为 8 字节块密码 ECB，算法与密钥未知。

**主机侧链路**：

- `SndCrUSB`（实际为 `KsMalcCtl`）→ `CTHIDRpA` → `CTHID` → `HID.DLL`；SBX7/U1 的 VID/PID 为 `041E/323A`。
- `SndCrUSB.dll` 0x465ed2 附近参数表可观察到 feature `0x10000100`、param `0x1a`、type Bool、size 4。
- `CTHIDRpA.dll` 表项可观察到 `26 01 96 3A 92`（index 145）与 `26 01 96 39 91`（index 144）；名称指针指向 `ParamType_MasterMute` / `ParamType_MasterVolume`。因此 **param 26 → SCP 145/0x963A 是 inferred**，不是直接观察；尚未抓包确认运行时实际发出该 HID 命令。
- 控制面板的 `SoundCoreObject.InitializeSoundCore` 先 `BindHardware(endpointId)` 再 `GetContext`；driverless U1 的 `ProductInfo` 返回 CLSID `495E4C24-85ED-4f19-885E-C2D01D7EA26C`，而 interop 类 `CtHdaMgrClass` 标注的是 `7BFF1F7F-1D65-49A0-AE26-7903C41BC5B3`。工具必须按实际注册选择 CLSID 并绑定端点。

**工具状态**：

- `tools/Sbx7MuteSwitch` 已修正为：默认 U1 CLSID `495E4C24…`、失败回退 `7BFF1F7F…`、`BindHardware(endpointId)`、NAudio 自动查找 “Sound Blaster X7” 渲染端点、`--probe` 只读检查、`--hold-ms` 前后各 1 秒、`finally` 恢复静音。
- 已用厂商 `Interop.CtSndCr.dll` 重新编译通过（0 warnings / 0 errors），但**仍未在 Windows + 真机运行**；`--probe` 是第一步。

**新增未知**：

- 爆音根因：DSP/解码重配置、主机/设备路径不同步、还是 DAC/功放/继电器下游瞬态。
- SCP `MasterMute 0x963A` 实际静音哪些输出、在 DSP 链哪一级；模式切换是否复位 mute。
- MALCOLM 8051 是否处理 SCP 0x26/0x963A、是否含 fade/ramp 状态机；主体 ISA/段表/加载基址。
- `CFirmwareUpgradeU1` 的 `[U1+4]` 选择标志写入点；MALCOLM_SBX 运行时是否真被选中。
- 2020 与 2015 两套固件资源差异对静态结论的影响范围。

**下一步最小验证**：

1. Windows 上先跑 `Sbx7MuteSwitch.exe --probe`，确认 COM 注册、端点绑定和 mute 参数可读。
2. 低音量 A/B：不静音 / 设备静音 / Windows 端点静音 三种条件，分别切 `direct`、`stereo-direct`、`spdif-passthrough`、`sbxmaster`；记录爆音在切换沿还是恢复沿。
3. 用 USBPcap/HID ETW 抓模式切换与静音报告，确认 `26 01 96 3A 92` 是否出现、切换后 mute 是否被复位。
4. 静态并行：用 8051 反汇编 MALCOLM 0x4B96E 起代码，搜索 SCP 0x26/0x963A 与 fade/mute 状态机。

## 14. 电视光纤场景（实际使用方式，2026-11-09 修正）

- 实际链路：**电视光纤输出 → X7 光纤输入 → X7 输出到音箱/耳机**；X7 独立供电，不接 PC。
- 爆音触发（用户实测）：**切换电视 HDMI 输入**，例如 Nintendo Switch（PCM）→ Apple TV（Dolby）；电视光纤输出格式随之从 PCM 变成 Dolby，X7 内部 SPDIF 接收/解码器重配置，爆一下。该场景没有 Windows 驱动/控制面板参与。
- 最先做的非固件测试：让两个 HDMI 源都输出 PCM——Apple TV 设置 → 视频和音频 → 音频格式 → 立体声/PCM；电视数字音频输出设为 PCM。然后 NS → Apple TV 再切一次。若爆音消失，问题即解决（代价是 Apple TV 杜比环绕变双声道）。
- 若两个源都 PCM 仍爆：说明电视切 HDMI 时光纤信号本身会断开/重锁，或 X7 输入切换有瞬态；此时才需要固件静音或外部静音。
- 若必须保留 Apple TV 杜比：先看电视能否固定输出 Dolby（把 NS 的 PCM 也转成 Dolby）；不能固定时，X7 固件补 1 秒静音是唯一路径，但风险高。
- 因此 `Sbx7MuteSwitch` **不适用于电视光纤场景**；保留它只用于 Windows USB 连接时的诊断实验，不作为本问题的修复方案。
- 安全的非固件方案（优先验证）：
  1. 电视数字音频输出固定 **PCM**：牺牲 Dolby 多声道，但不再发生 PCM↔Dolby 切换。
  2. 电视支持时固定输出 **Dolby Digital**（把 PCM 也转成 Dolby）：不再切换，但取决于电视是否支持转码。
  3. 改用模拟/蓝牙输入，避开 Dolby 码流切换。
- 固件方案：
  - 先确认电视菜单能否固定输出；能固定就不需要改固件。
  - 对比 2015 与 2020 两套 MALCOLM 固件，检查 2020 是否在 SPDIF 切换处增加 mute/fade；若官方已修，优先官方升级（需 Windows + USB，先确认设备当前版本与恢复路径）。
  - 自定义固件补 1 秒静音是最后手段：8051/DSP 代码、校验/签名与恢复路径未知，存在变砖风险，不推荐直接刷写。
- 待确认：
  - 电视数字音频输出菜单是否有 `PCM` / `Dolby Digital` / `自动` 三个选项。
  - 爆音是否**只在** PCM↔Dolby 切换时出现；固定其中一种后是否完全消失。