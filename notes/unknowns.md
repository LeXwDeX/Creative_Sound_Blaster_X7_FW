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
  - 为什么未知：已完成 MALCOLM 头部/浮点表/8051 尾部结构识别，但尚未反汇编到 SCP 0x963A/0x26 或 fade/mute 状态机
  - 下一步探针：用 8051 反汇编 MALCOLM 0x4B96E 起代码；或 Windows USBPcap 抓 HID 报告
  - 证据：`report.md` 第 13 节；`artifacts/pcfw/.rsrc/RCDATA/MALCOLM_SBX_RCDATA`
- [ ] U005 `GILROY_RCDATA` 与 `MALCOLM_SBX_RCDATA` 的设备映射
  - 为什么未知：升级器支持多个 MCU/产品，资源名与设备型号的对应关系未确认
  - 下一步探针：分析 `CFirmwareUpgradeU1::GetMCUType` 与资源选择分支
  - 证据：PCFW 字符串与 `.rsrc` 清单
- [ ] U006 爆音根因
  - 为什么未知：没有 Windows 真机 A/B、HID 抓包或音频测量
  - 下一步探针：按 `report.md` 第 13 节的三步验证
  - 证据：`report.md` 第 5、13 节
- [ ] U007 SCP `MasterMute 0x963A` 的实际作用面
  - 为什么未知：param 26 → 145/0x963A 是 inferred；未观察到运行时 HID 命令
  - 下一步探针：USBPcap/HID ETW 抓 `26 01 96 3A 92` 并确认静音哪些输出
  - 证据：`CTHIDRpA.dll` 表项；`SndCrUSB.dll` 参数表
- [ ] U008 MALCOLM 主体 ISA、段表与 8051 加载基址
  - 为什么未知：32 位对齐数据更像 DSP 字流，但无符号/标签；8051 LCALL/LJMP 跨约 64KB，基址不唯一
  - 下一步探针：8051 反汇编 + 段表/加载器分析
  - 证据：`report.md` 第 13 节
- [ ] U009 `[CFirmwareUpgradeU1+4]` 选择标志与 MALCOLM_SBX 运行时选中
  - 为什么未知：未找到静态写入点；2015 版 A_SBX→MALCOLM_SBX 日志分支只是支持性证据
  - 下一步探针：反编译 `LoadEmbeddedImage`/`SetActiveMCU` 的调用者与命令行路径
  - 证据：PCFW 反汇编；`artifacts/workflow/`
- [ ] U010 2020 与 2015 两套固件资源差异的影响范围
  - 为什么未知：两套资源大小与哈希不同；结论必须按构建标注
  - 下一步探针：分别分析两套 MALCOLM/GILROY/PIC32/BC08，比较版本串与代码差异
  - 证据：`artifacts/workflow/updatefw/.rsrc/RCDATA`
- [ ] U011 电视数字音频输出能否固定为 PCM 或 Dolby Digital
  - 为什么未知：用户电视菜单选项未确认
  - 下一步探针：Apple TV 音频格式改 Stereo/PCM + 电视数字音频输出改 PCM，然后 NS → Apple TV 复测
  - 证据：用户描述；`report.md` 第 14 节
- [ ] U014 两个源都 PCM 时是否仍爆
  - 为什么未知：电视切 HDMI 可能短暂断开/重锁光纤，即使都是 PCM
  - 下一步探针：NS 与 Apple TV 都设 PCM，重复 HDMI 切换
  - 证据：`report.md` 第 14 节
- [ ] U012 官方 2020 固件是否修复 SPDIF PCM↔Dolby 切换爆音
  - 为什么未知：未对比 2020/2015 MALCOLM 的 SPDIF 切换处理，也未在真机上升级验证
  - 下一步探针：对比两套 MALCOLM 的 8051/DSP 差异；确认设备当前版本后再考虑官方升级
  - 证据：`artifacts/pcfw/.rsrc/RCDATA/MALCOLM_SBX_RCDATA`、`artifacts/workflow/updatefw/.rsrc/RCDATA/`
- [ ] U013 自定义固件补静音的可行性与恢复路径
  - 为什么未知：8051/DSP 代码、校验/签名与刷写恢复路径未验证
  - 下一步探针：先完成 8051/SCP 反汇编；没有恢复方案前不刷写
  - 证据：`report.md` 第 13、14 节

## 已解决

| ID | 结论 | 证据 | 日期 |
|---|---|---|---|
|  |  |  |  |

## 规则

- 未知不等于“没有”，也不等于“安全”。
- 覆盖不足、工具失败、provider 缺失都要登记，不得用“无发现”替代“未检查”。
- 推断未验证时写成未知，不要写成观察。
- 解决未知后保留原问题与证据，便于复查。