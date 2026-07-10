using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using DailyTaskMod.Helpers;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 浇灌作物任务 - 自动浇灌未洒水器覆盖的未浇水作物
    /// </summary>
    public class WateringTask : IAutomationTask
    {
        public string TaskName => "watering";
        public string Category => "crops";

        public bool IsEnabled(ModConfig config)
            => config.Watering.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
        {
            if (player == null) return false;
            var wateringCan = GameHelper.FindTool<WateringCan>(player);
            return wateringCan != null && wateringCan.WaterLeft > 0;
        }

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            if (!CanExecute(player, location))
                return TaskResult.SkippedResult("没有水壶或水量不足");

            int watered = 0;
            var config = ModEntry.Instance?.Config;
            var wateringCan = GameHelper.FindTool<WateringCan>(player);
            if (wateringCan == null) return TaskResult.SkippedResult();

            int powerLevel = wateringCan.UpgradeLevel;
            int maxPower = config?.Watering.MaxWateringCanLevel ?? 4;
            int effectivePower = System.Math.Min(powerLevel, maxPower);

            foreach (var pair in GameHelper.GetCrops(location))
            {
                var tile = pair.Key;
                var dirt = pair.Value;

                if (!GameHelper.NeedsWater(dirt))
                    continue;

                // 跳过洒水器覆盖区域
                if (config?.Watering.RespectSprinklers == true &&
                    GameHelper.IsCoveredBySprinkler(location, tile))
                    continue;

                // 浇水
                if (wateringCan.WaterLeft > 0)
                {
                    dirt.state.Value = HoeDirt.watered;
                    wateringCan.WaterLeft--;
                    watered++;
                }
                else
                {
                    break;
                }
            }

            return watered > 0
                ? TaskResult.Done(watered, $"浇灌了 {watered} 块土地")
                : TaskResult.Empty();
        }
    }
}
