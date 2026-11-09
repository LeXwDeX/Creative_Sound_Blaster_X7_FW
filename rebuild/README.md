# 净室重建：声卡固件分析

> 项目：`projects/soundcard-firmware/` · 更新：2026-11-09
> 本目录保存 Craft Anything 的重建项目、捕获、检查结果与审批记录。

## 命令

在项目根目录执行。首次运行（目录已有本 README，按 Craft Anything 规则加 `--existing start-from`）：

```bash
craft-anything-cli rebuild <target> "<request>" \
  --project-folder projects/soundcard-firmware/rebuild --clean-room --existing start-from
```

后续轮次：

```bash
craft-anything-cli rebuild <target> "<request>" \
  --continue-in projects/soundcard-firmware/rebuild --clean-room
```

也可以先在空目录运行并省略 `--existing start-from`。显式传 `--clean-room`，不要依赖 Settings；只有用户明确批准 Deep 时才用 `--deep`。

运行前用 `craft-anything-cli commands` 查看实际工具，并用 `craft-anything-cli doctor` 检查构建、coding agent 与 AI backend。若 `craft-anything-cli ai list` 为空，用已安装的 agent 作为 backend（例如 `--backend "Claude Code"`），或让用户运行 `craft-anything-cli setup --client <client>` / 配置 AI backend；不要自动修改全局配置。

## Clean room 与 Deep
- 始终显式传 `--clean-room`，不要依赖 Settings：Clean room 只运行 `craft-anything/crates/engine/src/clean_room.rs` 中列出的工具，不读取目标内部。
- 只有用户明确批准时才用 `--deep` 或 Deep 模式；在 `notes/journal.md`、`report.md` 与 Evidence 中标注。
- Clean room 的 brief、builder 会话与检查中，不得出现 REA 的 Deep 发现、反编译代码、文件清单或符号表。
- 需要同时解释与重建时，Deep 证据与 Clean-room 可见事实分开保存，在文件命名与报告中标明来源。
- Clean room 不是操作系统沙箱；被启动的 coding agent 仍按自己的权限行事。

## 目录
以下内容都留在 `rebuild/` 内：

```text
rebuild/
  craft.json           项目文件
  capture/             study 捕获
  app/                 重建代码
  checks/              检查结果
  investigation.cany   调查状态
  inbox/               问题与审批
```

不要把这些文件写到项目外，也不要写进根目录。

## 资产与代码
- 不复制目标资产：图片、音频、文本、字体、模型等从证据重建，或使用用户提供的授权材料。
- 不把目标代码、反编译输出或受保护表达复制进 `app/`。
- 用户指定的语言、框架或引擎优先；没有指定时记录 Craft Anything 实际使用的栈。

## 检查

- 从 `craft-anything-cli commands` 选择实际的 `rebuild_*` 检查工具，不要凭记忆写工具名。
- 每轮把差异、分数或失败项写进 `notes/journal.md`，失败项登记到 `notes/unknowns.md`。
- 只记录实际测得的结果；不要声称未执行的运行或未测得的数字。

## 构建与清理

- 并行 Rust 构建时使用独立的 `CARGO_TARGET_DIR`，例如各自指向自己 `rebuild/app/target`。
- 构建输出、`target/`、`node_modules/`、`dist/`、`.venv/`、`.craft-anything/` 已在 `.gitignore` 中排除。
- 收尾时删除可再生成的构建缓存与临时文件，保留 `craft.json`、`checks/` 与报告引用的结果。
- 在 `report.md` 的“清理情况与残留”中写清删除与保留的内容。