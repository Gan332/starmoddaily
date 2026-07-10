using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StardewModdingAPI;
using StardewValley;
using DailyTaskMod.Tasks;
using DailyTaskMod.Helpers;

namespace DailyTaskMod
{
    /// <summary>
    /// 任务管理器 - 负责管理、调度和执行所有自动化任务
    /// </summary>
    public class TaskManager
    {
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translation;
        private readonly List<IAutomationTask> _tasks = new();

        private bool _isRunning = false;
        private int _ticksUntilAutoRun = 0;
        private bool _newDayHandled = false;

        public bool IsRunning => _isRunning;
        public IReadOnlyList<IAutomationTask> Tasks => _tasks.AsReadOnly();

        public TaskManager(IMonitor monitor, ITranslationHelper translation)
        {
            _monitor = monitor;
            _translation = translation;
            RegisterDefaultTasks();
        }

        /// <summary>注册所有默认任务</summary>
        private void RegisterDefaultTasks()
        {
            _tasks.Add(new WateringTask());
            _tasks.Add(new CropHarvestTask());
            _tasks.Add(new AnimalPettingTask());
            _tasks.Add(new AnimalProductTask());
            _tasks.Add(new AnimalFeedingTask());
            _tasks.Add(new MachineHarvestTask());
            _tasks.Add(new TreeTask());
            _tasks.Add(new ForageTask());
            _tasks.Add(new SocialTask());
            _tasks.Add(new MiscTask());
        }

        /// <summary>注册自定义任务</summary>
        public void RegisterTask(IAutomationTask task)
        {
            if (!_tasks.Any(t => t.TaskName == task.TaskName))
            {
                _tasks.Add(task);
                _monitor.Log($"已注册任务: {task.TaskName}", LogLevel.Debug);
            }
        }

        /// <summary>每 tick 更新 - 用于自动执行排队</summary>
        public void Update(GameTime gameTime)
        {
            if (!Game1.hasLoadedGame || Game1.player == null)
                return;

            var config = ModEntry.Instance?.Config;
            if (config == null || !config.EnableAutoRun)
                return;

            // 新的一天，自动运行
            if (config.AutoRunOnNewDay && !_newDayHandled &&
                Game1.timeOfDay >= 600 && Game1.timeOfDay <= 620 &&
                !Game1.newDay)
            {
                _newDayHandled = true;
                _ticksUntilAutoRun = config.AutoRunDelayTicks;
                _monitor.Log("新的一天! 准备执行日常任务...", LogLevel.Info);
            }

            // 检查是否是新的一天重置标记
            if (Game1.newDay)
            {
                _newDayHandled = false;
                _isRunning = false;
                return;
            }

            // 自动执行倒计时
            if (_ticksUntilAutoRun > 0)
            {
                _ticksUntilAutoRun--;
                if (_ticksUntilAutoRun <= 0 && !_isRunning)
                {
                    if (!config.OnlyIfPlayerIdle || IsPlayerIdle())
                    {
                        ExecuteAllTasks();
                    }
                }
            }
        }

        /// <summary>检查玩家是否空闲（没有打开菜单）</summary>
        private bool IsPlayerIdle()
        {
            return Game1.activeClickableMenu == null &&
                   !Game1.player.UsingTool &&
                   !Game1.player.isEating &&
                   !Game1.player.FarmerSprite.isAnimating;
        }

        /// <summary>执行所有已启用的任务</summary>
        public void ExecuteAllTasks()
        {
            if (_isRunning)
            {
                GameHelper.Log("任务正在进行中...", LogLevel.Info);
                return;
            }

            if (Game1.player == null || Game1.currentLocation == null)
                return;

            _isRunning = true;
            int totalProcessed = 0;
            int tasksDone = 0;
            int tasksSkipped = 0;

            var config = ModEntry.Instance?.Config;
            var player = Game1.player;
            var location = Game1.currentLocation;

            if (config?.ShowHudMessages == true)
            {
                Game1.addHUDMessage(new HUDMessage("开始执行日常任务...", 2));
            }

            GameHelper.Log($"=== 开始执行日常任务 ({Game1.Date.TotalDays}) ===", LogLevel.Info);

            int maxTasks = config?.MaxTasksPerRun ?? _tasks.Count;
            int taskCount = 0;

            foreach (var task in _tasks)
            {
                if (taskCount >= maxTasks) break;

                if (!task.IsEnabled(config))
                {
                    GameHelper.Log($"  [跳过] {task.TaskName}: 未启用", LogLevel.Trace);
                    continue;
                }

                if (!task.CanExecute(player, location))
                {
                    GameHelper.Log($"  [跳过] {task.TaskName}: 条件不满足", LogLevel.Trace);
                    continue;
                }

                GameHelper.Log($"  [执行] {task.TaskName}...", LogLevel.Info);

                try
                {
                    var result = task.Execute(player, location);

                    if (result.Skipped)
                    {
                        tasksSkipped++;
                        GameHelper.Log($"  [跳过] {task.TaskName}: {result.Message ?? "无需执行"}", LogLevel.Debug);
                    }
                    else if (result.Success)
                    {
                        totalProcessed += result.ItemsProcessed;
                        tasksDone++;

                        string msg = result.Message ?? $"{task.TaskName} 完成 ({result.ItemsProcessed} 个)";
                        GameHelper.Log($"  [完成] {msg}", LogLevel.Info);

                        if (config?.ShowHudMessages == true && result.ItemsProcessed > 0)
                        {
                            Game1.addHUDMessage(new HUDMessage(msg, 3));
                        }
                    }
                    else
                    {
                        GameHelper.Log($"  [失败] {task.TaskName}: {result.Message}", LogLevel.Warn);
                    }
                }
                catch (Exception ex)
                {
                    GameHelper.Log($"  [错误] {task.TaskName}: {ex.Message}", LogLevel.Error);
                    GameHelper.Log($"    堆栈: {ex.StackTrace}", LogLevel.Trace);
                }

                taskCount++;

                // 在任务之间显示进度
                if (config?.ShowTaskProgress == true && config.ShowHudMessages == true)
                {
                    Game1.addHUDMessage(new HUDMessage($"进度: {tasksDone}/{taskCount}", 3));
                }
            }

            _isRunning = false;

            string summary = $"日常任务完成! 执行 {tasksDone} 个, 跳过 {tasksSkipped} 个, 处理 {totalProcessed} 个物品";
            GameHelper.Log($"=== {summary} ===", LogLevel.Info);

            if (config?.ShowHudMessages == true)
            {
                Game1.addHUDMessage(new HUDMessage(summary, 2));
            }
        }

        /// <summary>重置新日标记 (用于手动重新运行)</summary>
        public void ResetNewDayFlag()
        {
            _newDayHandled = false;
        }
    }
}
