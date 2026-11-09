# 计划：声卡固件分析

> 项目：`projects/soundcard-firmware/` · 创建：2026-11-09
> 阶段勾选行由 `scripts/project-status.sh` 统计进度；任务表用于分解与追踪。
> 下一项未完成任务就是下一步动作。计划变更先落盘再执行。

## P0 受理与授权

- [ ] 复述目标、交付物、范围内外与成功标准
- [ ] 记录授权来源、允许的运行时操作与有效期
- [ ] 记录目标身份（路径或 URL、包名与版本、设备或进程）
- [ ] 对文件目标计算 SHA-256
- [ ] 选择 `mode` 与 `target_type`，更新 `PROJECT.md`
- [ ] 在 `notes/journal.md` 写第一条记录

## P1 侦察与分类

- [ ] 查 `tools/list`，确认可用工具与实际 schema
- [ ] 判定目标类型、入口与依赖
- [ ] 盘点保护措施与可观察面
- [ ] 选择第一批取证工具并记录理由
- [ ] 把首批未知写入 `notes/unknowns.md`
- [ ] 勾选 P1 完成项并更新 `status` 为 `evidence`

## P2 证据收集

- [ ] 静态检查：格式、结构、入口、字符串、符号、配置、签名
- [ ] 保存首批 Evidence 与原始输出到 `evidence/`
- [ ] 记录输入 SHA-256、工具、命令、覆盖范围与限制
- [ ] 仅在批准后做运行时观察，并记录副作用与清理方式
- [ ] 为关键结论建立 Evidence ID
- [ ] 复核覆盖缺口并登记未知

## P3 解释与未知项

- [ ] 回答用户问题，逐条标注结论类别
- [ ] 为推断写明依据与验证方式
- [ ] 更新 `notes/decisions.md` 的关键判断
- [ ] 解决可解决的未知项并附证据
- [ ] 保留无法解决的未知项与原因
- [ ] 勾选 P3 完成项并更新 `status`

## P4 净室重建（可选）

- [ ] 确认用户要求重建，并确认 Clean room 或 Deep
- [ ] 运行 `craft-anything-cli commands` 与 `craft-anything-cli doctor`
- [ ] 在 `projects/soundcard-firmware/rebuild/` 创建或选择项目文件夹
- [ ] 只用可见事实与授权材料写 brief
- [ ] 记录实际使用的语言、框架或引擎
- [ ] 构建可复现，保存构建命令与结果
- [ ] 检查没有复制目标代码、资产或受保护表达

## P5 比对与收敛

- [ ] 从 `craft-anything-cli commands` 选择实际的 `rebuild_*` 检查工具
- [ ] 运行检查并记录差异、分数或失败项
- [ ] 把失败项登记到 `notes/unknowns.md`
- [ ] 迭代到成功标准，或记录无法收敛的残余差异
- [ ] 每轮更新 `notes/journal.md`
- [ ] 勾选 P5 完成项并更新 `status`

## P6 交付与清理

- [ ] 按骨架写 `report.md`
- [ ] 整理 Evidence 索引与输入哈希
- [ ] 列出未知项、覆盖缺口与限制
- [ ] 清理临时容器、进程、文件、缓存与日志
- [ ] 运行 `scripts/project-status.sh soundcard-firmware` 复核
- [ ] 更新 `PROJECT.md` 的 `status` 并写收尾记录

## 任务表

| ID | 任务 | 状态 | 产物 | 证据 |
|---|---|---|---|---|
| T001 | 受理与授权 | todo | `PROJECT.md` | - |
| T002 | 目标身份与输入哈希 | todo | `PROJECT.md`、`evidence/README.md` | - |
| T003 | 工具与路由选择 | todo | `notes/journal.md` | - |
| T004 | 首批静态证据 | todo | `evidence/E001-*.json` | E001 |
| T005 | 关键机制解释 | todo | `report.md` 草稿 | E001–E00N |
| T006 | 未知项收敛 | todo | `notes/unknowns.md` | E00N |
| T007 | 重建（可选） | todo | `rebuild/` | - |
| T008 | 比对与收敛（可选） | todo | `notes/journal.md`、`report.md` | - |
| T009 | 报告与交付 | todo | `report.md` | 全部 |
| T010 | 清理与状态更新 | todo | `PROJECT.md`、`notes/journal.md` | - |

任务状态：`todo`、`doing`、`done`、`blocked`。同一时间只保留一个 `doing`；阻塞时写明原因与解除条件。

## 里程碑

| 里程碑 | 判据 | 日期 |
|---|---|---|
| M1 受理完成 | P0 全部勾选，授权与成功标准可检查 | |
| M2 证据充分 | 关键结论有 Evidence ID，覆盖缺口已登记 | |
| M3 重建完成（可选） | 构建可复现，brief 不含 Deep 发现 | |
| M4 交付完成 | 报告、未知清单、清理与状态更新完成 | |