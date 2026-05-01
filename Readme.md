# Flow.Launcher.Plugin.Zoxide

[简体中文](#简体中文) · [English](#English)

## 简体中文

适用于 [Flow Launcher](https://www.flowlauncher.com/) 的 [Zoxide](https://github.com/ajeetdsouza/zoxide) 集成插件，使用 **C#** 编写。

> **说明：** 本仓库为 C# 实现，若你曾使用基于 Python 的 [旧版插件](https://github.com/WantChane/Flow.Launcher.Plugin.ZoxidePy)，功能模型相近，但配置与部分细节以本仓库为准。

## 简介

本插件在 Flow Launcher 中调用 `zoxide query`，按使用习惯列出目录，选择后即可使用配置的命令打开；成功打开后可选择自动执行 `zoxide add`，路径不存在时会触发 `zoxide remove`。

## 功能特性

- **目录跳转**：根据 zoxide 评分与关键词快速匹配历史目录
- **可自定义打开方式**：在设置中配置多条命令（终端、编辑器等），使用 `{path}` 占位符
- **右键菜单**：对选中结果使用已启用的命令打开路径（可在设置中增删）
- **与 zoxide 同步**：成功打开后可后台 `zoxide add`（见各命令上的「成功打开后执行 zoxide add」选项）；若目录已不存在则 `zoxide remove`

> **说明：** 本插件不再提供 [ZoxidePy](https://github.com/WantChane/Flow.Launcher.Plugin.ZoxidePy) 的 `z cd` 功能；请考虑其他方法来向zoxide数据库中添加目录，例如修改注册表集成 `zoxide add` 到资源管理器的右键菜单。

## 环境要求

- 已安装 **Flow Launcher**
- 已安装 **zoxide**

## 安装

### 通过`Flow.Launcher.Plugin.PluginsManager`安装

执行 `pm install Zoxide by WantChane`

### 手动安装

1. 在 [Releases](https://github.com/WantChane/Flow.Launcher.Plugin.Zoxide/releases) 下载最新构建
2. 将解压后的插件文件夹放入 Flow Launcher 的插件目录
3. 重启 Flow Launcher
4. 在 **设置 → 插件 → Zoxide** 中配置 `zoxide.exe` 路径

## 配置

1. **zoxide 可执行文件**
   - 若已在 `PATH` 中，可填写 `zoxide.exe`
   - 否则填写完整路径，例如：`C:\Tools\zoxide.exe`
2. **命令超时**：等待 zoxide 子进程结束的最长时间（毫秒）
3. **默认命令与自定义命令**：选择用哪个命令打开目录；可为每条命令设置是否在成功打开后执行 `zoxide add`

默认触发关键字为 **`z`**。

## 使用

### 基本搜索

- 输入触发词 `z`、空格，再输入查询词（与 zoxide 行为基本一致）
- 示例：`z doc` 查找与 `doc` 相关的常用目录
- 注意，本插件**不支持** Flow Launcher 的使用拼音搜索功能

### 操作

- **Enter**：使用**默认命令**打开当前选中目录（默认为资源管理器）
- **右键**：在上下文菜单中选择其它已启用的命令打开该目录

## 许可证

本项目以 [MIT License](LICENSE) 授权。

## 致谢

- [Zoxide](https://github.com/ajeetdsouza/zoxide) — 目录跳转与 frecency 数据库
- [Flow Launcher](https://www.flowlauncher.com/) — 应用启动器与插件体系

---

## English

Zoxide integration for [Flow Launcher](https://www.flowlauncher.com/), written in **C#**.

> **Note:** This repository is the C# implementation. If you previously used the Python-based [legacy plugin](https://github.com/WantChane/Flow.Launcher.Plugin.ZoxidePy), the feature model is similar, but configuration and some details are defined by **this** repository.

## Description

This plugin invokes `zoxide query` from Flow Launcher, lists directories ranked by your usage habits, and opens the selection with your configured command. After a successful open you can optionally run `zoxide add` in the background; if the path no longer exists, `zoxide remove` is triggered.

## Features

- **Directory jumping**: Quickly match history directories using zoxide’s scores and your keywords
- **Custom open actions**: Configure multiple commands in settings (terminal, editor, …) with a `{path}` placeholder
- **Context menu**: Right-click a result to open the path with another enabled command (add or remove commands in settings)
- **Zoxide sync**: Optionally run `zoxide add` in the background after a successful open (see each command’s “Run zoxide add after opening path successfully” option); if the directory no longer exists, run `zoxide remove`

> **Note:** This plugin no longer provides the `z cd` functionality of [ZoxidePy](https://github.com/WantChane/Flow.Launcher.Plugin.ZoxidePy); please consider other methods to add directories to the zoxide database, such as modifying the registry to integrate `zoxide add` into the Explorer's right-click menu.

## Prerequisites

- **Flow Launcher** installed
- **Zoxide** installed

## Installation

### Install via `Flow.Launcher.Plugin.PluginsManager`

Execute `pm install Zoxide by WantChane`

### Manual

1. Download the latest release from [Releases](https://github.com/WantChane/Flow.Launcher.Plugin.Zoxide/releases)
2. Extract the plugin folder into your Flow Launcher plugins directory
3. Restart Flow Launcher
4. Set the path to `zoxide.exe` under **Settings → Plugins → Zoxide** if it is not on `PATH`

## Configuration

1. **Zoxide executable**
   - If on `PATH`, you can use `zoxide.exe`
   - Otherwise use a full path, e.g. `C:\Tools\zoxide.exe`
2. **Command timeout**: Maximum time to wait for the zoxide child process (milliseconds)
3. **Default & custom commands**: Choose how directories are opened; optionally enable `zoxide add` after a successful open per command

The default action keyword is **`z`**.

## Usage

### Basic search

- Type the action keyword `z`, a space, then your query (same idea as zoxide)
- Example: `z doc` to find directories you often use related to `doc`
- **Note:** This plugin does **not** support Flow Launcher’s Pinyin-based search

### Actions

- **Enter**: Open the selected directory with the **default** command (usually File Explorer)
- **Right-click**: Pick another enabled command from the context menu to open the same path

## License

[MIT](LICENSE)

## Acknowledgments

- [Zoxide](https://github.com/ajeetdsouza/zoxide) — Directory jumping and the frecency database
- [Flow Launcher](https://www.flowlauncher.com/) — Application launcher and plugin system
