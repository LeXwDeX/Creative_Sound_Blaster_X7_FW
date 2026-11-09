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
| E8051-01 | 2026-11-09 | `SBX7_PCFW_EN_200321.exe` 资源 `MALCOLM_SBX_RCDATA` | 提取（file[0x4B000:]）+ Ghidra 12.2 headless（8051:BE:16:default）+ 手工解码 | `f0f4fb575b6047adfd063e6fa6947549334b436e1935c43898bb16c2d7cf7821`（源）/ `c9c96e9fca7fbe472fe34917b8d7582478ee429012ed345ffdfe5fc5e7414025`（镜像） | `E8051-01-8051-extract-and-analysis.md`、`artifacts/8051_image.bin`、`artifacts/ghidra_8051_export/` | 8051 镜像布局、主命令循环、PCM↔Dolby 切换状态机（0x17AD-0x189E）、2015/2020 同构对比 | bank 函数缺失（AJMP 0x1C63 越界）；0x1EDA 硬件语义未验证 | observation/derivation |
| E8051-02 | 2026-11-09 | 同上（8051 镜像）+ 2015 版 MALCOLM | XRAM 位级写入扫描 + 版本对齐 diff | 同上 | `E8051-02-dsp-protocol.md` | 8051→SB-Axx1 配置协议三层路径（FCE4/8EC3 参数族、0x1A5/0x1A6 寄存器直写）；param 2 全值域；2015→2020 唯一差异 = (2,0x1F) | 参数寄存器语义未知（GILROY/缺失 bank）；无运行时验证 | observation/derivation |

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
| E8051-03 | 2026-11-09 | PCFW.exe x86 + PIC32 flat + GILROY/BC08 数据 | Ghidra 12.2（x86：Gilroy 升级函数定位）+ 手工分析 | 见 E8051-03 内引用 | `E8051-03-gilroy-static-exhausted.md` | GILROY 静态解密途径全排除（PC=memcpy、PIC32 无引用、无压缩头）；升级协议 0x3E 08/09/0A | 反编译器不可用（ARM mac），仅反汇编 | observation/derivation |
