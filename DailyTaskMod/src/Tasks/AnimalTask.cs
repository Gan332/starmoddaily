using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Buildings;
using StardewValley.Locations;
using DailyTaskMod.Helpers;
using Object = StardewValley.Object;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 动物照料任务 - 抚摸动物、收集动物产品
    /// </summary>
    public class AnimalPettingTask : IAutomationTask
    {
        public string TaskName => "animal-petting";
        public string Category => "animals";

        public bool IsEnabled(ModConfig config)
            => config.AnimalPetting.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null;

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int petted = 0;
            int buildingCount = 0;

            var farm = Game1.getFarm();
            if (farm == null) return TaskResult.SkippedResult();

            // 抚摸室外动物
            foreach (var animal in farm.Animals.Values)
            {
                if (!GameHelper.WasPetToday(animal))
                {
                    animal.pet(player, false);
                    petted++;
                }
            }

            // 遍历建筑内动物
            foreach (var building in farm.buildings)
            {
                if (building.indoors.Value is AnimalHouse house)
                {
                    buildingCount++;
                    foreach (var animal in house.Animals.Values)
                    {
                        if (!GameHelper.WasPetToday(animal))
                        {
                            animal.pet(player, false);
                            petted++;
                        }
                    }
                }
            }

            if (buildingCount == 0 && petted == 0)
                return TaskResult.Empty();

            return petted > 0
                ? TaskResult.Done(petted, $"抚摸 {petted} 只动物")
                : TaskResult.SkippedResult("所有动物今天已抚摸了");
        }
    }

    /// <summary>
    /// 收集动物产品任务 (蛋、奶、毛等)
    /// </summary>
    public class AnimalProductTask : IAutomationTask
    {
        public string TaskName => "animal-products";
        public string Category => "animals";

        public bool IsEnabled(ModConfig config)
            => config.AnimalProducts.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null && GameHelper.HasInventorySpace(player);

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int collected = 0;
            var config = ModEntry.Instance?.Config;

            var farm = Game1.getFarm();
            if (farm == null) return TaskResult.SkippedResult();

            // 收集地上的动物产品 (松露等)
            foreach (var obj in GameHelper.GetAnimalProduceOnGround(farm))
            {
                if (!GameHelper.HasInventorySpace(player))
                    break;
                if (ShouldCollect(obj.ParentSheetIndex, config))
                {
                    // 直接添加到背包并移除地面物品 (替代旧版 performToolAction)
                    if (player.addItemToInventoryBool(obj, true))
                    {
                        farm.removeObject(obj.TileLocation, false);
                        collected++;
                    }
                }
            }

            // 收集建筑内的产品
            foreach (var building in farm.buildings)
            {
                if (!GameHelper.HasInventorySpace(player))
                    break;

                var indoors = building.indoors.Value;
                if (indoors is AnimalHouse house)
                {
                    foreach (var obj in GameHelper.GetCollectibleAnimalObjects(building))
                    {
                        if (!GameHelper.HasInventorySpace(player))
                            break;
                        if (ShouldCollect(obj.ParentSheetIndex, config))
                        {
                            if (player.addItemToInventoryBool(obj, true))
                            {
                                house.removeObject(obj.TileLocation, false);
                                collected++;
                            }
                        }
                    }
                }
            }

            return collected > 0
                ? TaskResult.Done(collected, $"收集 {collected} 个动物产品")
                : TaskResult.Empty();
        }

        private bool ShouldCollect(int parentIndex, ModConfig config)
        {
            if (config == null) return true;
            var ap = config.AnimalProducts;

            return parentIndex switch
            {
                184 => ap.CollectMilk,    // 牛奶
                186 => ap.CollectMilk,    // 羊奶 (大瓶)
                176 => ap.CollectEggs,    // 鸡蛋
                180 => ap.CollectEggs,    // 棕色鸡蛋
                107 => ap.CollectEggs,    // 恐龙蛋黄
                174 => ap.CollectEggs,    // 大鸡蛋
                182 => ap.CollectEggs,    // 大棕色鸡蛋
                440 => ap.CollectWool,    // 兔毛/羊毛
                442 => ap.CollectWool,    // 动物毛
                430 => ap.CollectTruffles,// 松露
                444 => ap.CollectFeathers,// 羽毛
                791 => ap.CollectEggs,    // 鸵鸟蛋
                446 => ap.CollectWool,    // 羊毛布料?
                _ => true
            };
        }
    }

    /// <summary>
    /// 动物喂养任务 - 放干草、开关门
    /// </summary>
    public class AnimalFeedingTask : IAutomationTask
    {
        public string TaskName => "animal-feeding";
        public string Category => "animals";

        public bool IsEnabled(ModConfig config)
            => config.AnimalFeeding.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null;

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int hayPlaced = 0;
            var config = ModEntry.Instance?.Config;
            var farm = Game1.getFarm();

            foreach (var building in farm.buildings)
            {
                if (building.indoors.Value is AnimalHouse house)
                {
                    // 放置干草到喂料斗
                    if (config?.AnimalFeeding.PlaceHayInBarn == true &&
                        GameHelper.GetHayCount() > 0)
                    {
                        int origHay = house.numberOfObjectsWithName("Hay");
                        GameHelper.PlaceHayInFeed(house);
                        int newHay = house.numberOfObjectsWithName("Hay");
                        hayPlaced += (newHay - origHay);
                    }

                    // 开门/关门
                    if (config?.AnimalFeeding.OpenGatesInGoodWeather == true &&
                        !Game1.isRaining && !Game1.isSnowing && !Game1.isLightning)
                    {
                        building.animalDoorOpen.Value = true;
                    }
                    else if (config?.AnimalFeeding.CloseGatesInRain == true &&
                             (Game1.isRaining || Game1.isSnowing || Game1.isLightning))
                    {
                        building.animalDoorOpen.Value = false;
                    }
                }
            }

            return hayPlaced > 0
                ? TaskResult.Done(hayPlaced, $"放置 {hayPlaced} 份干草")
                : TaskResult.Empty();
        }
    }
}
