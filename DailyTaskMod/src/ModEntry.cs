using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using DailyTaskMod.Helpers;
using DailyTaskMod.Tasks;

namespace DailyTaskMod
{
    /// <summary>
    /// 模组主入口 - 日常任务自动化模组
    /// 支持配置化自动化执行每天的各项农场工作。
    /// </summary>
    public class ModEntry : Mod
    {
        public static ModEntry Instance { get; private set; }
        public ModConfig Config { get; private set; }
        public TaskManager TaskManager { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            // 加载配置
            Config = helper.ReadConfig<ModConfig>();

            // 初始化助手
            GameHelper.Init(Monitor);

            // 初始化任务管理器
            TaskManager = new TaskManager(Monitor, helper.Translation);

            // 注册事件
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

            Monitor.Log($"Daily Task Automation v{ModManifest.Version} 已加载!", LogLevel.Info);
            Monitor.Log("按 K 键执行日常任务 (可在 config.json 中修改)", LogLevel.Info);
        }

        /// <summary>游戏启动后 - 注册 GMCM 配置菜单</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // 注册 Generic Mod Config Menu 支持
            RegisterConfigMenu();
        }

        /// <summary>存档加载完成</summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Monitor.Log($"存档已加载: {Game1.player?.Name}", LogLevel.Info);
        }

        /// <summary>新的一天开始</summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Monitor.Log("新的一天开始了!", LogLevel.Debug);
            // 重置任务管理器的状态
            TaskManager?.ResetNewDayFlag();
        }

        /// <summary>每 tick 更新</summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            TaskManager?.Update(Game1.currentGameTime);
        }

        /// <summary>按键监听</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Config.TaskToggleKey != SButton.None &&
                e.Pressed.Contains(Config.TaskToggleKey))
            {
                // 手动触发执行所有任务
                TaskManager?.ExecuteAllTasks();
            }
        }

        /// <summary>注册 Generic Mod Config Menu</summary>
        private void RegisterConfigMenu()
        {
            // 检查 GMCM 是否可用
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api == null)
            {
                Monitor.Log("未检测到 Generic Mod Config Menu，使用 config.json 直接配置", LogLevel.Info);
                return;
            }

            // 注册模组配置菜单
            api.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            // ====== 通用设置 ======
            api.AddSectionTitle(ModManifest, () => "通用设置");

            api.AddBoolOption(ModManifest,
                () => Config.EnableAutoRun,
                (val) => Config.EnableAutoRun = val,
                () => "启用自动运行",
                () => "新的一天自动执行日常任务");

            api.AddBoolOption(ModManifest,
                () => Config.AutoRunOnNewDay,
                (val) => Config.AutoRunOnNewDay = val,
                () => "每日自动执行",
                () => "每天开始时自动运行任务");

            api.AddBoolOption(ModManifest,
                () => Config.OnlyIfPlayerIdle,
                (val) => Config.OnlyIfPlayerIdle = val,
                () => "仅当玩家空闲时",
                () => "玩家未打开菜单时才执行");

            api.AddBoolOption(ModManifest,
                () => Config.ShowHudMessages,
                (val) => Config.ShowHudMessages = val,
                () => "显示 HUD 消息",
                () => "显示任务进度和结果消息");

            api.AddNumberOption(ModManifest,
                () => Config.MaxTasksPerRun,
                (val) => Config.MaxTasksPerRun = val,
                () => "每轮最大任务数",
                () => "每轮自动执行的最大任务数量",
                1, 50);

            api.AddKeybind(ModManifest,
                () => Config.TaskToggleKey,
                (val) => Config.TaskToggleKey = val,
                () => "任务快捷键",
                () => "手动执行所有任务的按键");

            // ====== 浇灌设置 ======
            api.AddSectionTitle(ModManifest, () => "浇灌设置");
            api.AddBoolOption(ModManifest,
                () => Config.Watering.Enabled, (v) => Config.Watering.Enabled = v,
                () => "启用浇灌", () => "自动浇灌未浇的作物");
            api.AddBoolOption(ModManifest,
                () => Config.Watering.RespectSprinklers, (v) => Config.Watering.RespectSprinklers = v,
                () => "跳过洒水器区域", () => "不浇已经在洒水器范围内的土地");

            // ====== 收获设置 ======
            api.AddSectionTitle(ModManifest, () => "收获设置");
            api.AddBoolOption(ModManifest,
                () => Config.CropHarvest.Enabled, (v) => Config.CropHarvest.Enabled = v,
                () => "启用收获", () => "自动收获已成熟的作物");
            api.AddBoolOption(ModManifest,
                () => Config.CropHarvest.HarvestFlowers, (v) => Config.CropHarvest.HarvestFlowers = v,
                () => "收获花朵", () => "是否收获花卉作物");

            // ====== 动物设置 ======
            api.AddSectionTitle(ModManifest, () => "动物设置");
            api.AddBoolOption(ModManifest,
                () => Config.AnimalPetting.Enabled, (v) => Config.AnimalPetting.Enabled = v,
                () => "抚摸动物", () => "自动抚摸所有动物");
            api.AddBoolOption(ModManifest,
                () => Config.AnimalProducts.Enabled, (v) => Config.AnimalProducts.Enabled = v,
                () => "收集动物产品", () => "自动收集蛋、奶、毛等产品");
            api.AddBoolOption(ModManifest,
                () => Config.AnimalFeeding.Enabled, (v) => Config.AnimalFeeding.Enabled = v,
                () => "喂养动物", () => "自动放置干草和开关门");

            // ====== 机器设置 ======
            api.AddSectionTitle(ModManifest, () => "机器设置");
            api.AddBoolOption(ModManifest,
                () => Config.MachineHarvest.Enabled, (v) => Config.MachineHarvest.Enabled = v,
                () => "收集机器产出", () => "自动从小桶、罐头瓶、熔炉等机器收集产出");

            // ====== 社交设置 ======
            api.AddSectionTitle(ModManifest, () => "社交设置");
            api.AddBoolOption(ModManifest,
                () => Config.Social.Enabled, (v) => Config.Social.Enabled = v,
                () => "启用社交", () => "送生日礼物、与配偶对话、摸宠物");
            api.AddBoolOption(ModManifest,
                () => Config.Social.GiveBirthdayGifts, (v) => Config.Social.GiveBirthdayGifts = v,
                () => "赠送生日礼物", () => "自动给生日 NPC 送礼");

            // ====== 采集设置 ======
            api.AddSectionTitle(ModManifest, () => "采集与清理");
            api.AddBoolOption(ModManifest,
                () => Config.Forage.Enabled, (v) => Config.Forage.Enabled = v,
                () => "采集与清理", () => "自动拾取采集品、清理杂草和碎石");

            // ====== 杂项设置 ======
            api.AddSectionTitle(ModManifest, () => "杂项设置");
            api.AddBoolOption(ModManifest,
                () => Config.Misc.Enabled, (v) => Config.Misc.Enabled = v,
                () => "启用杂项", () => "其他日常任务: 电视、鱼塘、山洞等");
            api.AddBoolOption(ModManifest,
                () => Config.Misc.CheckTV, (v) => Config.Misc.CheckTV = v,
                () => "看电视", () => "自动检查电视（天气/运气/食谱）");
            api.AddBoolOption(ModManifest,
                () => Config.Misc.CollectFishPonds, (v) => Config.Misc.CollectFishPonds = v,
                () => "收集鱼塘", () => "自动收集鱼塘产出");

            Monitor.Log("Generic Mod Config Menu 已注册!", LogLevel.Info);
        }
    }

    /// <summary>
    /// Generic Mod Config Menu API 接口定义
    /// </summary>
    public interface GenericModConfigMenuAPI
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);
        void AddFloatOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string> formatValue = null, string fieldId = null);
        void AddKeybind(IManifest mod, Func<SButton> getValue, Action<SButton> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
    }
}
