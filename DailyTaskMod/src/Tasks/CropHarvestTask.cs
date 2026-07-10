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
                bool isRegrowable = dirt.crop.RegrowsAfterHarvest();
                if (isRegrowable && config?.CropHarvest.HarvestRegrowable == false)
                    continue;

                if (!GameHelper.HasInventorySpace(player))
                    break;

                // 收获作物
                try
                {
                    // SDV 1.6: harvest 需要完整参数
                    // 对于自动收获，使用 isForcedScytheHarvest=false，junimoHarvester=null
                    bool autoHarvest = dirt.crop.GetHarvestMethod() == 0; // 0 = harvest method (not scythe)
                    if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt, null, false))
                    {
                        harvested++;

                        // 再生作物不摧毁植株
                        if (!dirt.crop.RegrowsAfterHarvest())
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
            // SDV 1.6: indexOfHarvest 是 NetString (string)
            string id = crop.indexOfHarvest.Value;
            // 常见花朵 id 转 string
            return id == "288" || id == "424" || id == "521" || id == "595"
                || id == "597" || id == "591" || id == "376"; // 番红花
        }
    }
}
