# 计划：Sound Blaster X7 (SBX7) 固件/驱动分析

> 项目：`projects/soundcard-firmware/` · 创建：2026-11-09
> 阶段勾选行由 `scripts/project-status.sh` 统计进度；任务表用于分解与追踪。
> 本次只做静态分析；Windows 真机测试是下一步，不在本机执行。

## P0 受理与授权

- [x] 复述目标、交付物、范围内外与成功标准
- [x] 记录授权来源、允许的运行时操作与有效期
- [x] 记录目标身份（两个安装包、大小、哈希、格式）
- [x] 对文件目标计算 SHA-256
- [x] 选择 `mode=deep` 与 `target_type=firmware`，更新 `PROJECT.md`
- [x] 在 `notes/journal.md` 写第一条记录
- [x] 建立 git 仓库并设置 origin

## P1 侦察与分类

- [x] 查 `tools/list` / CLI，确认可用工具与 schema
- [x] 判定目标类型：PE32 InstallShield 驱动包 + PE32 HID 固件升级器
- [x] 盘点 PCFW 资源与驱动/控制面板组件
- [x] 选择 binwalk/7zz/ILSpy/Ghidra 作为第一批工具
- [x] 把固件映射、静音点等未知写入 `notes/unknowns.md`
- [x] 更新 `status` 为 `evidence`

## P2 证据收集

- [x] 静态检查：PE 资源、字符串、驱动 CAB/MSI、控制面板 .NET/COM
- [x] 解包 PCFW 资源与驱动包到 `artifacts/`
- [x] 记录输入 SHA-256、工具、命令、覆盖范围与限制
- [x] 未执行运行时观察（本次不涉及）
- [x] 为关键结论建立 Evidence ID（E001–E005）
- [x] 复核覆盖缺口并登记未知（U001–U005）

## P3 解释与未知项

- [x] 回答“哪个是固件、固件在哪里、控制面板如何切换模式”
- [x] 为推断写明依据与验证方式
- [x] 更新 `notes/decisions.md` 的关键判断
- [x] 解决可解决的未知项并附证据
- [x] 保留无法解决的未知项与原因
- [x] 更新 `status` 为 `verifying`

## P4 净室重建（可选，本次不适用）

- [x] 确认用户要求重建，并确认 Clean room 或 Deep（本次不适用）
- [x] 运行 `craft-anything-cli commands` 与 `craft-anything-cli doctor`（本次不适用）
- [x] 在 `projects/soundcard-firmware/rebuild/` 创建或选择项目文件夹（本次不适用）
- [x] 只用可见事实与授权材料写 brief（本次不适用）
- [x] 记录实际使用的语言、框架或引擎（本次不适用）
- [x] 构建可复现，保存构建命令与结果（本次不适用）
- [x] 检查没有复制目标代码、资产或受保护表达（本次不适用）

## P5 比对与收敛（可选，本次不适用）

- [x] 从 `craft-anything-cli commands` 选择实际的 `rebuild_*` 检查工具（本次不适用）
- [x] 运行检查并记录差异、分数或失败项（本次不适用）
- [x] 把失败项登记到 `notes/unknowns.md`（本次不适用）
- [x] 迭代到成功标准，或记录无法收敛的残余差异（本次不适用）
- [x] 每轮更新 `notes/journal.md`（本次不适用）
- [x] 勾选 P5 完成项并更新 `status`（本次不适用）

## P6 交付与清理

- [x] 按骨架写 `report.md`
- [x] 整理 Evidence 索引与输入哈希
- [x] 列出未知项、覆盖缺口与限制
- [x] 清理临时文件；`artifacts/` 不提交
- [x] 运行 `scripts/project-status.sh soundcard-firmware` 复核
- [x] 更新 `PROJECT.md` 的 `status` 为 `verifying`；Windows 真机测试见 T007

## 任务表

| ID | 任务 | 状态 | 产物 | 证据 |
|---|---|---|---|---|
| T001 | 受理与授权 | done | `PROJECT.md` | - |
| T002 | 目标身份与输入哈希 | done | `PROJECT.md`、`report.md` | E001/E002 |
| T003 | PCFW 资源与固件映射 | done | `report.md` | E001 |
| T004 | 驱动包/控制面板解包 | done | `artifacts/driver/` | E002 |
| T005 | 控制面板反编译与参数定位 | done | `report.md`、`artifacts/decompiled/` | E003/E004 |
| T006 | 静音包装工具 | done | `tools/Sbx7MuteSwitch/` | E005 |
| T007 | Windows 真机验证 | todo | 测试记录 | U002 |
| T008 | 固件/HID 进一步分析（如需要） | todo | `report.md` 更新 | U004 |
| T009 | 最终报告与状态收尾 | todo | `report.md`、`PROJECT.md` | 全部 |

任务状态：`todo`、`doing`、`done`、`blocked`。同一时间只保留一个 `doing`；阻塞时写明原因与解除条件。

## 里程碑

| 里程碑 | 判据 | 日期 |
|---|---|---|
| M1 受理完成 | P0 全部勾选，授权与成功标准可检查 | 2026-11-09 |
| M2 证据充分 | 固件资源、控制面板参数与静音路径有证据 | 2026-11-09 |
| M3 工具可验证 | `Sbx7MuteSwitch` 编译通过，等待 Windows 真机 | 2026-11-09 |
| M4 交付完成 | Windows 测试结果、报告与状态更新完成 | 待定 |