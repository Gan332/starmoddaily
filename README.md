# Daily Task Automation — 星露谷物语日常任务自动化模组

## 概述

一个基于 **SMAPI** 的 Stardew Valley 模组，自动完成每日农场日常工作。
支持高度可配置的开关选项，可通过 `config.json` 或 **Generic Mod Config Menu** 界面灵活控制。

## 功能特性

- **浇水**: 自动浇灌未浇的作物，跳过洒水器覆盖区域
- **收获**: 自动收获成熟的作物（包括花卉和再生作物）
- **动物抚摸**: 抚摸所有农场动物（鸡舍和畜棚内）
- **动物产品**: 收集蛋、奶、毛、松露等动物产品
- **喂养**: 放置干草到喂料斗，好天气开门/坏天气关门
- **机器收集**: 小桶、罐头瓶、熔炉、压酪机、织布机等所有机器产出
- **树木**: 收集果树果实和树液采集器
- **采集**: 拾取地上采集品，清理杂草和碎石
- **社交**: 摸宠物、与配偶对话、给生日的 NPC 送礼物
- **杂项**: 看电视、收集鱼塘产出、山洞蘑菇、史莱姆球等

## 安装

1. 安装 [SMAPI](https://smapi.io/) 3.15.0+
2. 安装 [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
3. 编译: `cd DailyTaskMod && dotnet build -c Release`
4. 将 `bin/Release/net6.0/DailyTaskMod` 复制到 `Stardew Valley/Mods/`

## 使用

- 按 **K** 键手动执行所有日常任务 (可在 config.json 中修改)
- 每天进入游戏后自动运行 (可关闭)
- 安装 Generic Mod Config Menu 后可在游戏内设置菜单调整

## 配置

所有功能均可独立开关，在 `config.json` 中设置。
