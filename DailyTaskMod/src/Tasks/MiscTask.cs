using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Buildings;
using StardewValley.Characters;
using Microsoft.Xna.Framework;
using DailyTaskMod.Helpers;
using Object = StardewValley.Object;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 杂项日常任务 - 电视、鱼塘、史莱姆球、山洞蘑菇、邮件
    /// </summary>
    public class MiscTask : IAutomationTask
    {
        public string TaskName => "misc";
        public string Category => "misc";

        public bool IsEnabled(ModConfig config)
            => config.Misc.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null;

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int actions = 0;
            var config = ModEntry.Instance?.Config;

            // 检查电视
            if (config?.Misc.CheckTV == true)
            {
                var farmHouse = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                if (farmHouse != null)
                {
                    var tv = farmHouse.furniture?.FirstOrDefault(f => f.Name.Contains("TV") || f.Name.Contains("Television"));
                    if (tv != null)
                    {
                        tv.checkForAction(player);
                        actions++;
                    }
                }
            }

            // 收集山洞蘑菇
            if (config?.Misc.CollectCaveMushrooms == true)
            {
                var farmCave = Game1.getLocationFromName("FarmCave") as FarmCave;
                if (farmCave != null)
                {
                    foreach (var pair in farmCave.objects.Pairs)
                    {
                        var obj = pair.Value;
                        if (obj != null && obj.bigCraftable.Value)
                        {
                            if (GameHelper.MachineHasOutput(obj))
                            {
                                obj.checkForAction(player);
                                actions++;
                            }
                        }
                    }
                }
            }

            // 收集鱼塘产出 (SDV 1.6: hasOutput → output.Value, grabFarmItem → CatchFish)
            if (config?.Misc.CollectFishPonds == true)
            {
                var farm = Game1.getFarm();
                foreach (var building in farm.buildings)
                {
                    if (building is FishPond pond)
                    {
                        if (pond.output.Value != null && GameHelper.HasInventorySpace(player))
                        {
                            pond.CatchFish();
                            actions++;
                        }
                    }
                }
            }

            // 收集史莱姆球
            if (config?.Misc.CollectSlimeBalls == true)
            {
                foreach (var loc in GameHelper.GetFarmLocations())
                {
                    foreach (var pair in loc.objects.Pairs)
                    {
                        var obj = pair.Value;
                        if (obj != null && obj.ParentSheetIndex == 56 && GameHelper.MachineHasOutput(obj))
                        {
                            obj.checkForAction(player);
                            actions++;
                        }
                    }
                }
            }

            // 检查邮件 (由游戏自动处理，但我们可以确保今天已检查)
            if (config?.Misc.CheckMail == true)
            {
                Game1.mailbox?.Clear();
                actions++;
            }

            return actions > 0
                ? TaskResult.Done(actions, $"完成了 {actions} 个杂项任务")
                : TaskResult.Empty();
        }
    }
}
