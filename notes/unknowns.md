# 未知项：声卡固件分析

> 项目：`projects/soundcard-firmware/` · 更新：2026-11-09
> 每条未知写成一个 `- [ ]` 列表项；解决后改为 `- [x]` 并附证据。
> `scripts/project-status.sh` 只统计以 `- [ ]` 开头的行。

## 字段

| 字段 | 写什么 |
|---|---|
| 问题 | 尚未建立的事实，一句话说清 |
| 为什么未知 | 缺少授权、provider、样本、时间、工具或覆盖 |
| 下一步探针 | 最小可执行动作，能推进或证伪该未知 |
| 证据 | Evidence ID；尚未取证时写“尚无” |

## 登记

<!-- 从这里开始登记。每个未知写成一行 `- [ ] U001 <问题>`，下面缩进写三个字段。 -->

- [ ] U001 `MALCOLM_SBX_RCDATA` 是否就是 SBX7 当前主固件
  - 为什么未知：资源名与 `U1 2020/03/12` 字符串高度匹配，但尚未从升级器代码确认资源选择条件
  - 下一步探针：反编译 `CFirmwareUpgradeU1::InitMCUData/SetActiveMCU`，或读取设备当前固件版本字符串
  - 证据：`MALCOLM_SBX_RCDATA`、PCFW 字符串
- [ ] U002 `eParamSpeakerCalibrator_Mute` 的静音点是否在爆音源之前
  - 为什么未知：只能在 Windows + 真机上验证；本机 macOS 无法运行 COM/设备
  - 下一步探针：低音量下运行 `Sbx7MuteSwitch --mode direct --value off --hold-ms 1000`
  - 证据：`tools/Sbx7MuteSwitch/`
- [ ] U003 “SB → 普通”具体对应哪个控制
  - 为什么未知：Direct Mode、Stereo Direct Mode、SBX Master、Dolby 解码开关都可能是用户描述的动作
  - 下一步探针：在控制面板确认 UI 状态，并分别测试三种模式
  - 证据：`report.md` 第 4 节
- [ ] U004 固件是否已有模式切换静音/渐变
  - 为什么未知：尚未反编译 `MALCOLM_SBX_RCDATA`，也未抓取模式切换时的 HID 命令
  - 下一步探针：Ghidra/rizin 分析固件；或 Windows USBPcap 抓 HID 报告
  - 证据：尚无
- [ ] U005 `GILROY_RCDATA` 与 `MALCOLM_SBX_RCDATA` 的设备映射
  - 为什么未知：升级器支持多个 MCU/产品，资源名与设备型号的对应关系未确认
  - 下一步探针：分析 `CFirmwareUpgradeU1::GetMCUType` 与资源选择分支
  - 证据：PCFW 字符串与 `.rsrc` 清单

## 已解决

| ID | 结论 | 证据 | 日期 |
|---|---|---|---|
|  |  |  |  |

## 规则

- 未知不等于“没有”，也不等于“安全”。
- 覆盖不足、工具失败、provider 缺失都要登记，不得用“无发现”替代“未检查”。
- 推断未验证时写成未知，不要写成观察。
- 解决未知后保留原问题与证据，便于复查。