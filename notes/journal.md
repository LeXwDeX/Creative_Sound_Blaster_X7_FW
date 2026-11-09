# 工作日志：声卡固件分析

> 项目：`projects/soundcard-firmware/` · 开始：2026-11-09
> 本文件只追加，不改写历史。每轮工作结束前写一条记录。

## 条目格式

```text
2026-11-09 HH:MM | <动作> | <工具或命令> | <结果> | <证据路径或 ID> | 下一步：<动作>
```

字段说明：

- **动作**：这一轮做了什么，例如“首页侦察”“解析搜索接口”。
- **工具或命令**：实际调用的工具、CLI 命令或 MCP 参数；没有调用工具时写“人工阅读”。
- **结果**：观察到什么、成功或失败；失败写错误文本与已尝试的修复。
- **证据**：`evidence/E001-*.json`、`evidence/raw/...` 或“尚无”。
- **下一步**：下一项计划动作；没有时写“等待用户输入”。

## 记录

<!-- 从这里开始追加。每条记录一行或一个列表项，不要修改已有条目。 -->

- 2026-11-09 14:54 | 受理与身份 | `file`、`shasum -a 256` | 驱动包 119 MB InstallShield；PCFW 4.2 MB PE32；哈希已记录 | `PROJECT.md` | 下一步：解包 PCFW
- 2026-11-09 14:57 | PCFW 资源 | `7zz l -slt`、`7zz x`、`binwalk` | 发现 BC08（CSR DFU）、PIC32（Intel HEX）、MALCOLM_SBX（U1 2020/03/12）、GILROY | `artifacts/pcfw/` | 下一步：映射 SBX7 资源
- 2026-11-09 14:59 | 驱动包 | `tail -c`、`7zz x`、MSI 解包 | 465 个文件；`Sound Blaster X7.msi`、控制面板、驱动；`SndCrUSB.DLL` 为 `KsMalcCtl.DLL` COM | `artifacts/driver/` | 下一步：反编译控制面板
- 2026-11-09 15:02 | 控制面板反编译 | `ilspycmd` + `grep` | Direct Mode / SBX Master 处理器直接 SetParam，无 mute/延时；发现 `eParamSpeakerCalibrator_Mute=26` | `artifacts/decompiled/` | 下一步：写静音包装工具
- 2026-11-09 15:05 | 验证工具 | `dotnet build` | `tools/Sbx7MuteSwitch` 用厂商 `Interop.CtSndCr.dll` 编译通过（0 warnings / 0 errors） | `tools/Sbx7MuteSwitch/` | 下一步：Windows 真机低音量测试
- 2026-11-09 15:06 | 交付 | 写 `report.md`、更新 plan/unknowns、提交并推送 | 报告与工具进入 `Creative_Sound_Blaster_X7_FW` 仓库 | `report.md` | 等待 Windows 侧测试结果
- 2026-11-09 15:44 | 深度静态分析 | 4 路并行 workflow + 核查 | 确认 MALCOLM/PIC32/BC08/GILROY 结构与 2020/2015 差异；升级器 U1 资源选择与 65B HID 报文；控制面板 COM/HID 链；发现 feature 值错误与工具 CLSID/BindHardware 阻塞 | `report.md` 第 13 节；`artifacts/workflow/` | 下一步：修正工具并 Windows probe
- 2026-11-09 15:50 | 工具修正 | `dotnet build` | 改为 U1 CLSID 495E4C24（回退 7BFF1F7F）、`BindHardware(endpointId)`、NAudio 端点发现、`--probe`；编译 0/0 | `tools/Sbx7MuteSwitch/` | 下一步：Windows `--probe` 与低音量 A/B
- 2026-11-09 16:20 | 场景修正 | 用户确认 | 爆音在电视 HDMI 输入切换时发生（NS PCM → Apple TV Dolby），X7 光纤解码器随之切换；Windows 工具不适用 | `report.md` 第 14 节 | 下一步：Apple TV/TV 固定 PCM 复测
- 2026-11-09 16:28 | 原始文件归档 | `ls`/`shasum -a 256` | 用户把两个原始文件放入 `source/`；哈希与之前一致；`source/` 已加入 `.gitignore` | `source/` | 下一步：用 `source/` 作为项目输入路径

- 2026-11-09 16:20 | 原始文件归档 | `ls`/`shasum -a 256` | 用户把两个原始文件放入 `source/`；哈希与之前一致；`source/` 已加入 `.gitignore` | `source/` | 下一步：用 `source/` 作为项目输入路径
- 2026-11-09 17:05 | 8051 反汇编 | Ghidra 12.2 headless（8051:BE:16:default，手写入口+种子函数脚本） | 提取 8051 镜像（file[0x4B000:]，7257B）；Ghidra 识别 179 函数/1790 指令；破译主命令循环（XRAM 0x1BB 命令串，XRAM 0x1BA 输出模式字节，命令集 %/数字/c/s/b/l）；定位 **PCM↔Dolby 切换状态机 0x17AD-0x189E**（0x1E2 当前态/0x1E7 缓存态/0x1EDA bit0 硬件态/0x1E5 使能；PCM 分支参数 (0x1F,0x82),(0xB,0xD0),(2,0x14)，Dolby 分支 (0x0E,0x82)）；**切换路径无任何 mute/fade**；2015↔2020 状态机同构未修复（U012 解决）；0x963A/param 26 不在镜像；bank 函数（0x8EC3/0xFCE4/0x8FF5/0x7C94）不在本文件/PIC32，推断在 GILROY 加密段 | `evidence/E8051-01-8051-extract-and-analysis.md`、`artifacts/8051_image.bin`、`artifacts/ghidra_8051_export/` | 下一步：mute 机制取证（USBPcap 抓 mute SetParam 或 GILROY 解密），然后设计 patch

## 规则

- 每轮结束前写一条，不要事后补写模糊记录。
- 时间使用本地时区，记录到分钟。
- 失败、阻塞、暂停都要记录原因与恢复入口。
- 证据路径使用项目内相对路径。
- 不粘贴凭据、token、个人信息或无关的目标数据。
- 需要跨项目复用时，写明来源项目 slug 与批准依据。- 2026-11-09 17:45 | 硬件方案确认 | 官方文档（用户提供的 3 份 PDF 已下载至 reference/creative-docs/）+ 用户提供拆解信息 | X7 主芯片为 SB-Axx1 多核 DSP（SPDIF 接收器为 DSP 内部外设，无独立接收芯片）；DAC=Burr-Brown PCM1794A；耳放=TI TPA6120A2；数字功放=TI TPA3116D2；运放可换。**修正 U015 的架构假设**：0x1EDA bit0 是 SB-Axx1 内部 SPDIF 外设锁定状态位；8051 经 XRAM 桥写 DSP 寄存器空间（0xA1 疑似寄存器地址）。修复方案 D002 的 auto-mute 位=SB-Axx1 SPDIF 外设配置位，配置代码在 8051 侧 | `reference/creative-docs/`、notes/decisions.md D002 | 下一步：8051 初始化序列位级排查（写 0x1B0-0x1BE/0x1E0-0x1EF 的所有位语义）+ 2015/2020 初始化 diff
- 2026-11-09 17:58 | 8051 协议重建与版本 diff | 位级 XRAM 写入扫描 + 2015/2020 对齐 diff | 重建 8051→SB-Axx1 配置协议三层路径（FCE4 写参数族 / 8EC3 参数族 / 0x1A5-0x1A6 寄存器直写接口）；**2015→2020 唯一功能新增 = 初始化序列开头写一次 param2=0x1F**（其余 5 个 param2 值与全部参数序列一致，仅 bank 地址漂移 0x8E79→0x8EC3 等）；param 2 全部取值：0x0B/0x0D/0x0F/0x10/0x12/0x14（切换）/0x1F（2020 init）/0x2（13/2015 切换用 0xD）；镜像尾部 AJMP 0x1C63 越界确证 bank 截断 | `evidence/E8051-02-dsp-protocol.md` | 下一步：GILROY 解密评估（auto-mute 位在缺失 bank 中）或抓包取证
