using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Locations;
using DailyTaskMod.Helpers;
using Object = StardewValley.Object;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 机器收获任务 - 收集所有已完成的机器产出
    /// 包括: 小桶、罐头瓶、熔炉、晶球破开器、蜂房等所有生产设备
    /// </summary>
    public class MachineHarvestTask : IAutomationTask
    {
        public string TaskName => "machine-harvest";
        public string Category => "machines";

        public bool IsEnabled(ModConfig config)
            => config.MachineHarvest.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null && GameHelper.HasInventorySpace(player);

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int collected = 0;
            var config = ModEntry.Instance?.Config;

            foreach (var loc in GameHelper.GetFarmLocations())
            {
                // 收集机器产出
                foreach (var pair in loc.objects.Pairs)
                {
                    var pos = pair.Key;
                    var obj = pair.Value;

                    if (!obj.bigCraftable.Value) continue;
                    if (!GameHelper.MachineHasOutput(obj)) continue;

                    if (!GameHelper.HasInventorySpace(player))
                        break;

                    if (ShouldCollectMachine(obj, config))
                    {
                        obj.checkForAction(player);
                        collected++;
                    }
                }

                // 收集 tappers (SDV 1.6: tree.heldObject 已移除，改为直接查找 tapper Object)
                if (config?.TreeTasks.CollectTreeTappers == true)
                {
                    foreach (var feature in loc.terrainFeatures.Pairs)
                    {
                        if (feature.Value is Tree tree && tree.tapped.Value)
                        {
                            if (!GameHelper.HasInventorySpace(player))
                                break;

                            // 查找该位置的 tapper Object
                            if (loc.objects.TryGetValue(feature.Key, out Object tapperObj)
                                && tapperObj != null && tapperObj.ParentSheetIndex == 254)
                            {
                                if (tapperObj.heldObject.Value != null && tapperObj.readyForHarvest.Value)
                                {
                                    tapperObj.checkForAction(player);
                                    collected++;
                                }
                            }
                        }
                    }
                }
            }

            return collected > 0
                ? TaskResult.Done(collected, $"收集 {collected} 个机器产出")
                : TaskResult.Empty();
        }

        private bool ShouldCollectMachine(Object obj, ModConfig config)
        {
            if (config == null) return true;
            var mh = config.MachineHarvest;
            int id = obj.ParentSheetIndex;

            return id switch
            {
                12  => mh.CollectKegs,            // 小桶
                15  => mh.CollectPreserveJars,     // 罐头瓶
                16  => mh.CollectCheesePresses,    // 压酪机
                17  => mh.CollectMayonnaiseMachines,// 蛋黄酱机
                24  => mh.CollectLoom,            // 织布机
                19  => mh.CollectOilMakers,       // 产油机
                10  => mh.CollectFurnaces,        // 熔炉
                13  => mh.CollectCharcoalKilns,   // 炭窑
                26  => mh.CollectCrystalariums,   // 晶球破开器
                25  => mh.CollectSeedMakers,      // 种子生产器
                231 => mh.CollectSluices,         // 淘金锅
                455 => mh.CollectBoneMills,       // 碎骨机
                264 => mh.CollectBeeHouses,       // 蜂房
                800 => mh.CollectDehydrators,     // 烘干机
                813 => mh.CollectFishSmokers,     // 熏鱼机
                _   => true
            };
        }
    }
}
