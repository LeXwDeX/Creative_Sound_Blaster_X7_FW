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

## 规则

- 每轮结束前写一条，不要事后补写模糊记录。
- 时间使用本地时区，记录到分钟。
- 失败、阻塞、暂停都要记录原因与恢复入口。
- 证据路径使用项目内相对路径。
- 不粘贴凭据、token、个人信息或无关的目标数据。
- 需要跨项目复用时，写明来源项目 slug 与批准依据。