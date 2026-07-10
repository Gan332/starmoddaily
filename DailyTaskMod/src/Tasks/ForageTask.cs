using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Locations;
using DailyTaskMod.Helpers;
using Object = StardewValley.Object;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 采集和清理任务 - 拾取地上物品、清理杂草和碎石
    /// </summary>
    public class ForageTask : IAutomationTask
    {
        public string TaskName => "forage";
        public string Category => "forage";

        public bool IsEnabled(ModConfig config)
            => config.Forage.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null && GameHelper.HasInventorySpace(player);

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int collected = 0;
            var config = ModEntry.Instance?.Config;
            var farm = Game1.getFarm();
            if (farm == null) return TaskResult.SkippedResult();

            // 收集地上的采集物品 (非动物产品)
            if (config?.Forage.CollectForageItems == true)
            {
                foreach (var pair in farm.objects.Pairs)
                {
                    if (!GameHelper.HasInventorySpace(player))
                        break;

                    var obj = pair.Value;
                    if (obj != null && obj.IsSpawnedObject && !IsAnimalProduce(obj.ParentSheetIndex))
                    {
                        obj.performToolAction(null, player);
                        farm.removeObject(pair.Key, false);
                        collected++;
                    }
                }
            }

            // 清理杂草
            if (config?.Forage.ClearWeeds == true)
            {
                var weeds = farm.terrainFeatures.Pairs
                    .Where(p => p.Value is Grass || p.Value is Weed)
                    .ToList();

                foreach (var pair in weeds)
                {
                    if (!GameHelper.HasInventorySpace(player))
                        break;

                    if (config?.Forage.OnlyClearPaths == true)
                    {
                        // 只清理路径附近的 (靠近建筑/围栏)
                        if (!IsNearBuilding(farm, pair.Key))
                            continue;
                    }

                    farm.terrainFeatures.Remove(pair.Key);
                    collected++;
                }
            }

            // 清理碎石/树枝
            if (config?.Forage.ClearStones == true || config?.Forage.ClearDebris == true)
            {
                var debris = farm.objects.Pairs
                    .Where(p => IsDebris(p.Value))
                    .ToList();

                foreach (var pair in debris)
                {
                    if (!GameHelper.HasInventorySpace(player))
                        break;

                    farm.objects.Remove(pair.Key);
                    collected++;
                }
            }

            return collected > 0
                ? TaskResult.Done(collected, $"清理/采集了 {collected} 个物品")
                : TaskResult.Empty();
        }

        private bool IsAnimalProduce(int parentIndex)
        {
            return parentIndex == 430 || parentIndex == 791 || parentIndex == 444;
        }

        private bool IsDebris(Object obj)
        {
            int id = obj.ParentSheetIndex;
            // 碎石, 树枝, 树桩, 杂草
            return id == 0 || id == 1 || id == 2 || id == 3 || id == 4 || id == 5
                || id == 6 || id == 7 || id == 8 || id == 9 || id == 10
                || id == 11 || id == 12 || id == 13 || id == 14 || id == 15
                || id == 313 || id == 314 || id == 315 || id == 316 || id == 317
                || id == 318 || id == 319 || id == 320 || id == 321 || id == 322
                || id == 343 || id == 340 || id == 395 || id == 450;
        }

        private bool IsNearBuilding(GameLocation location, Vector2 tile)
        {
            if (location is Farm farm)
            {
                foreach (var building in farm.buildings)
                {
                    var bounds = new Rectangle(
                        building.tileX.Value - 2,
                        building.tileY.Value - 2,
                        building.tilesWide.Value + 4,
                        building.tilesHigh.Value + 4
                    );
                    if (bounds.Contains((int)tile.X, (int)tile.Y))
                        return true;
                }
            }
            return false;
        }
    }
}
