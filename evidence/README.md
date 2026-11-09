# Evidence：声卡固件分析

> 项目：`projects/soundcard-firmware/` · 更新：2026-11-09
> 本目录保存可复查的证据与原始输出。目标数据不要提交到 git。

## 命名

```text
evidence/E001-<subject>.json       工具返回的 Evidence JSON
evidence/E002-<subject>.json       第二个 Evidence
evidence/raw/E001-<subject>.<ext>  原始输出、页面、响应或日志
```

- ID 使用 `E001`、`E002` 递增，稳定不变；已发布或已引用的 ID 不要复用。
- `<subject>` 用简短英文或拼音，例如 `homepage`、`search-api`、`login-flow`。
- 一个 Evidence 一个主题；不要把多个不相关的结果塞进同一个文件。

## 索引

| ID | 时间 | 目标 | 工具或命令 | 输入 SHA-256 | 输出 | 覆盖 | 限制 | 类别 |
|---|---|---|---|---|---|---|---|---|
|  |  |  |  |  |  |  |  |  |

索引可以放在本文件，也可以放在 `notes/journal.md`；但每条 Evidence 必须能从这里或 journal 找到。

## 必填信息

- **ID**：`E001` 形式。
- **时间**：采集时间与时区。
- **目标身份**：路径或 URL、包名与版本、设备或进程。
- **工具或命令**：实际调用的工具名、CLI 命令或 MCP 参数。
- **输入 SHA-256**：文件输入的哈希；无文件输入时写 `n/a` 并说明。
- **输出位置**：Evidence 与原始输出的相对路径。
- **覆盖范围**：检查了什么、没检查什么。
- **限制**：工具失败、provider 缺失、权限限制、环境差异。
- **结论类别**：`observation`、`derivation`、`inference` 或 `unknown`。

## 原始输出

- 工具返回的 Evidence JSON 原样保存，不要只保留摘要。
- 页面、响应、日志、截图等原始输出放 `evidence/raw/`。
- 如果工具只返回文本，原始文本放 `raw/`，另建同 ID 的索引 JSON 记录必填信息。

## 脱敏与保留

- 保存前删除凭据、token、Cookie、个人信息与无关的内网标识。
- 需要保留认证信息时，只保留字段名与结构，不保留值。
- 目标数据按授权范围保留；收尾时删除可再生成的原始输出，保留报告引用的最小证据。
- 不在报告、issue 或公开渠道粘贴未授权的目标数据。

## git

`.gitignore` 默认排除 `evidence/*`，只保留本文件。不要用 `git add -f` 提交 Evidence 或原始输出。