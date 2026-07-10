using StardewValley;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 自动化任务接口 - 所有日常任务实现此接口
    /// </summary>
    public interface IAutomationTask
    {
        /// <summary>任务唯一名称标识</summary>
        string TaskName { get; }

        /// <summary>任务所属分类</summary>
        string Category { get; }

        /// <summary>任务是否启用</summary>
        bool IsEnabled(ModConfig config);

        /// <summary>判断任务今天是否可执行 (避免重复执行或条件不满足)</summary>
        bool CanExecute(Farmer player, GameLocation location);

        /// <summary>执行任务，返回执行结果摘要</summary>
        TaskResult Execute(Farmer player, GameLocation location);
    }

    /// <summary>
    /// 任务执行结果
    /// </summary>
    public class TaskResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ItemsProcessed { get; set; }
    public bool Skipped { get; set; }

    public static TaskResult Done(int count, string message = null)
        => new() { Success = true, ItemsProcessed = count, Message = message };

    public static TaskResult SkippedResult(string reason = null)
        => new() { Skipped = true, Message = reason, Success = false };

    public static TaskResult Fail(string message)
        => new() { Success = false, Message = message };

        public static TaskResult Empty()
            => new() { Success = true, ItemsProcessed = 0, Message = null };
    }
}
