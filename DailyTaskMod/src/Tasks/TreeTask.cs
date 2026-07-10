using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using DailyTaskMod.Helpers;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 果树与树木任务 - 收集果树果实和树液采集器
    /// </summary>
    public class TreeTask : IAutomationTask
    {
        public string TaskName => "tree-tasks";
        public string Category => "trees";

        public bool IsEnabled(ModConfig config)
            => config.TreeTasks.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null && GameHelper.HasInventorySpace(player);

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int collected = 0;
            var config = ModEntry.Instance?.Config;

            foreach (var loc in GameHelper.GetFarmLocations())
            {
                // 收集果树果实
                if (config?.TreeTasks.HarvestFruitTrees == true)
                {
                    foreach (var pair in loc.terrainFeatures.Pairs)
                    {
                        if (pair.Value is FruitTree fruitTree && fruitTree.fruit.Count > 0)
                        {
                            if (!GameHelper.HasInventorySpace(player))
                                break;

                            // 收获果树果实
                            int fruitCount = fruitTree.fruit.Count;
                            fruitTree.fruit.Clear();
                            collected += fruitCount;
                        }
                    }
                }

                // 收集树液采集器 (tappers on trees)
                if (config?.TreeTasks.CollectTreeTappers == true)
                {
                    foreach (var pair in loc.terrainFeatures.Pairs)
                    {
                        if (!GameHelper.HasInventorySpace(player))
                            break;

                        if (pair.Value is Tree tree && tree.tapped.Value)
                        {
                            if (tree.heldObject.Value != null)
                            {
                                // 尝试使用 tapper 产出收集
                                tree.performUseAction(pair.Key);
                                collected++;
                            }
                        }
                    }
                }

                // 摇树获得种子 (可选)
                if (config?.TreeTasks.ShakeTreesForSeeds == true)
                {
                    foreach (var pair in loc.terrainFeatures.Pairs)
                    {
                        if (!GameHelper.HasInventorySpace(player))
                            break;

                        if (pair.Value is Tree tree && !tree.tapped.Value
                            && tree.growthStage.Value >= Tree.treeStage)
                        {
                            // 摇动树让种子掉落 (模拟摇树逻辑)
                            tree.shake(pair.Key, false);
                            // 种子掉落由游戏内部逻辑处理
                        }
                    }
                }
            }

            return collected > 0
                ? TaskResult.Done(collected, $"收获了 {collected} 个果实/采集物")
                : TaskResult.Empty();
        }
    }
}
