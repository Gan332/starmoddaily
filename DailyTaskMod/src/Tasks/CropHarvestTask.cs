using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Tools;
using DailyTaskMod.Helpers;
using Object = StardewValley.Object;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 作物收获任务 - 自动收获可收获的作物
    /// </summary>
    public class CropHarvestTask : IAutomationTask
    {
        public string TaskName => "crop-harvest";
        public string Category => "crops";

        public bool IsEnabled(ModConfig config)
            => config.CropHarvest.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
        {
            return player != null && GameHelper.HasInventorySpace(player);
        }

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int harvested = 0;
            var config = ModEntry.Instance?.Config;

            foreach (var pair in GameHelper.GetCrops(location))
            {
                var tile = pair.Key;
                var dirt = pair.Value;

                if (dirt.crop == null || !GameHelper.CanHarvestCrop(dirt.crop))
                    continue;

                // 跳过花朵 (可选)
                bool isFlower = dirt.crop.programColored.Value || IsFlowerCrop(dirt.crop);
                if (isFlower && config?.CropHarvest.HarvestFlowers == false)
                    continue;

                // 是否是再生作物 (蓝莓/蔓越莓等)
                bool isRegrowable = dirt.crop.regrowAfterHarvest.Value >= 0;
                if (isRegrowable && config?.CropHarvest.HarvestRegrowable == false)
                    continue;

                if (!GameHelper.HasInventorySpace(player))
                    break;

                // 收获作物 - 模拟右键点击
                try
                {
                    bool autoHarvest = dirt.crop.harvestMethod.Value == Crop.sickleHarvest;
                    if (dirt.crop.harvest())
                    {
                        harvested++;

                        // 再生作物不摧毁植株
                        if (dirt.crop.regrowAfterHarvest.Value <= 0)
                        {
                            dirt.crop = null;
                        }
                    }
                }
                catch
                {
                    // 忽略单个作物的收获错误
                }
            }

            return harvested > 0
                ? TaskResult.Done(harvested, $"收获了 {harvested} 份作物")
                : TaskResult.Empty();
        }

        private bool IsFlowerCrop(Crop crop)
        {
            int id = crop.indexOfHarvest.Value;
            // 常见花朵：288 (郁金香), 424 (虞美人), 521 (蓝爵), 595 (玫瑰仙子)
            // 597 (向日葵), 591 (蜜汁山茶花)
            return id == 288 || id == 424 || id == 521 || id == 595
                || id == 597 || id == 591 || id == 376; // 番红花
        }
    }
}
