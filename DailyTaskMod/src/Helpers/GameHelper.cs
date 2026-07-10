using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace DailyTaskMod.Helpers
{
    /// <summary>
    /// 游戏助手类 - 提供各种游戏对象查找和操作工具方法
    /// </summary>
    public static class GameHelper
    {
        private static IMonitor _monitor;

        public static void Init(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>获取所有可访问的游戏地点</summary>
        public static IEnumerable<GameLocation> GetAllLocations()
        {
            return Game1.locations
                .Concat(Game1.locations.OfType<Farm>()
                    .SelectMany(farm => farm.buildings
                        .Where(b => b.indoors.Value != null)
                        .Select(b => b.indoors.Value)));
        }

        /// <summary>获取农场及所有建筑内部地点</summary>
        public static IEnumerable<GameLocation> GetFarmLocations()
        {
            var farm = Game1.getFarm();
            yield return farm;

            foreach (var building in farm.buildings)
            {
                if (building.indoors.Value != null)
                    yield return building.indoors.Value;
            }
        }

        /// <summary>检查物品是否是特定类型的机器</summary>
        public static bool IsMachine(Object obj, out string machineType)
        {
            machineType = null;
            if (obj == null || obj.bigCraftable.Value == false)
                return false;

            int parentId = obj.ParentSheetIndex;

            machineType = parentId switch
            {
                12  => "Keg",              // 小桶
                15  => "PreserveJar",       // 罐头瓶
                16  => "CheesePress",       // 压酪机
                17  => "MayonnaiseMachine",  // 蛋黄酱机
                24  => "Loom",             // 织布机
                19  => "OilMaker",         // 产油机
                10  => "Furnace",          // 熔炉
                13  => "CharcoalKiln",     // 炭窑
                21  => "SlimeIncubator",   // 史莱姆孵化器
                20  => "RecyclingMachine", // 回收机
                25  => "SeedMaker",        // 种子生产器
                26  => "Crystalarium",     // 晶球破开器
                90  => "SlimePress",       // 史莱姆压蛋器
                96  => "WormBin",          // 蚯蚓箱
                231 => "Sluice",           // 淘金锅
                246 => "Incubator",        // 孵化器 (地上)
                254 => "Tapper",           // 树液采集器
                264 => "BeeHouse",         // 蜂房
                265 => "MushroomBox",      // 蘑菇箱
                343 => "StatueOfPerfection",// 完美雕像
                499 => "StatueOfTruePerfection",// 真正完美雕像
                455 => "BoneMill",         // 碎骨机
                710 => "CoffeeMaker",      // 咖啡机
                800 => "Dehydrator",       // 烘干机
                813 => "FishSmoker",       // 熏鱼机
                _    => null
            };

            return machineType != null;
        }

        /// <summary>检查机器是否有产出可收集</summary>
        public static bool MachineHasOutput(Object obj)
        {
            return obj.heldObject.Value != null && obj.readyForHarvest.Value;
        }

        /// <summary>查找地点中的所有庄稼</summary>
        public static IEnumerable<KeyValuePair<Vector2, HoeDirt>> GetCrops(GameLocation location)
        {
            foreach (var pair in location.terrainFeatures.Pairs)
            {
                if (pair.Value is HoeDirt dirt && dirt.crop != null)
                    yield return pair;
            }
        }

        /// <summary>检查作物是否可收获</summary>
        public static bool CanHarvestCrop(Crop crop)
        {
            return crop != null && crop.currentPhase.Value >= crop.phaseDays.Count - 1
                   && crop.dayOfCurrentPhase.Value <= 0 && !crop.dead.Value;
        }

        /// <summary>检查庄稼是否需要浇水</summary>
        public static bool NeedsWater(HoeDirt dirt)
        {
            return dirt.crop != null && dirt.state.Value != HoeDirt.watered
                   && !dirt.crop.dead.Value;
        }

        /// <summary>检查地块是否在洒水器范围内</summary>
        public static bool IsCoveredBySprinkler(GameLocation location, Vector2 tile)
        {
            foreach (var obj in location.objects.Pairs)
            {
                if (obj.Value is Object o && o.IsSprinkler() &&
                    o.IsSprinklerRange(tile))
                    return true;
            }
            return false;
        }

        /// <summary>获取玩家可用的浇水能量 (基于水壶)</summary>
        public static int GetWateringCanPower(Farmer player)
        {
            var wateringCan = player.Items.FirstOrDefault(i => i is WateringCan) as WateringCan;
            if (wateringCan == null) return 0;
            return wateringCan.UpgradeLevel + 1;
        }

        /// <summary>查找农场中的所有动物</summary>
        public static IEnumerable<FarmAnimal> GetAllAnimals()
        {
            var farm = Game1.getFarm();
            foreach (var animal in farm.Animals.Values)
                yield return animal;

            foreach (var building in farm.buildings)
            {
                if (building.indoors.Value is AnimalHouse house)
                {
                    foreach (var animal in house.Animals.Values)
                        yield return animal;
                }
            }
        }

        /// <summary>检查动物是否今天已抚摸</summary>
        public static bool WasPetToday(FarmAnimal animal)
        {
            return animal.wasPet.Value;
        }

        /// <summary>查找所有 ready 的动物产品 (在地面上)</summary>
        public static IEnumerable<Object> GetAnimalProduceOnGround(GameLocation location)
        {
            foreach (var pair in location.objects.Pairs)
            {
                var obj = pair.Value;
                if (obj != null && obj.IsSpawnedObject)
                {
                    int id = obj.ParentSheetIndex;
                    // 松露 430, 鸵鸟蛋 791, 羽毛 444 等动物产品
                    if (id == 430 || id == 791 || id == 444 ||
                        id == 107 || id == 174 || id == 182 || id == 440 ||
                        id == 442 || id == 176 || id == 180 || id == 184 ||
                        id == 186 || id == 438 || id == 446)
                        yield return obj;
                }
            }
        }

        /// <summary>查找地点中所有 ready 的机器</summary>
        public static IEnumerable<KeyValuePair<Vector2, Object>> GetReadyMachines(GameLocation location)
        {
            foreach (var pair in location.objects.Pairs)
            {
                var obj = pair.Value;
                if (obj.bigCraftable.Value && MachineHasOutput(obj))
                    yield return pair;
            }
        }

        /// <summary>获取工具从工具栏 (或可用物品中)</summary>
        public static T FindTool<T>(Farmer player) where T : Tool
        {
            return player.Items.OfType<T>().FirstOrDefault();
        }

        /// <summary>检查玩家背包是否有空间</summary>
        public static bool HasInventorySpace(Farmer player, int slotsNeeded = 1)
        {
            return player.freeSpotsInInventory() >= slotsNeeded;
        }

        /// <summary>找到特定 NPC</summary>
        public static NPC GetNpc(string name)
        {
            return Game1.getCharacterFromName(name);
        }

        /// <summary>判断今天是否是某个 NPC 的生日</summary>
        public static bool IsBirthday(NPC npc)
        {
            var date = Game1.Date;
            return npc.Birthday_Season == date.Season && npc.Birthday_Day == date.DayOfMonth;
        }

        /// <summary>获取 NPC 最爱的礼物 (有库存的情况下)</summary>
        public static Item GetLovedGift(NPC npc, Farmer player, string quality = "gold")
        {
            // 检查赠送者是否能找到
            var lovedItems = npc.GiftTastes?.Where(t => t switch
            {
                2 or 3 or 4 or 5 or 6 or 7 or 8 => true, // Loved gifts in Stardew Valley
                _ => false
            }).Select(t => t.Item1).ToList();

            if (lovedItems == null || lovedItems.Count == 0)
                return null;

            // 在背包中找最爱的礼物
            foreach (var item in player.Items)
            {
                if (item == null) continue;
                if (lovedItems.Contains(item.ParentSheetIndex))
                    return item;
            }

            return null;
        }

        /// <summary>获取宠物</summary>
        public static Pet GetPet()
        {
            return Game1.getFarm().characters.OfType<Pet>().FirstOrDefault();
        }

        /// <summary>获取鸡舍/畜棚中可收集的动物产品 (蛋桶/牛奶桶等)</summary>
        public static IEnumerable<Object> GetCollectibleAnimalObjects(Building building)
        {
            if (building.indoors.Value is AnimalHouse house)
            {
                foreach (var pair in house.objects.Pairs)
                {
                    var obj = pair.Value;
                    if (obj == null || obj.bigCraftable.Value) continue;

                    // 牛奶桶 (parentSheetIndex 184), 羊奶桶 (186), 蛋 (各种)
                    int pid = obj.ParentSheetIndex;
                    if (pid == 184 || pid == 186 || pid == 440 || pid == 442 ||
                        pid == 176 || pid == 180 || pid == 104 || pid == 174 ||
                        pid == 182 || pid == 438 || pid == 446)
                        yield return obj;
                }
            }
        }

        /// <summary>获取干草库存 (从筒仓)</summary>
        public static int GetHayCount()
        {
            return Game1.getFarm().piecesOfHay.Value + Utility.numSilos() * 240;
        }

        /// <summary>往喂料斗放干草</summary>
        public static bool PlaceHayInFeed(AnimalHouse house)
        {
            if (house.numberOfObjectsWithName("Hay") >= house.animalLimit)
                return false;

            // 检查是否有干草库存
            if (Game1.getFarm().piecesOfHay.Value <= 0)
                return false;

            // 使用游戏内置的加干草逻辑
            int placed = house.numberOfObjectsWithName("Hay");
            int maxFeeders = house.animalLimit;

            for (int i = placed; i < maxFeeders; i++)
            {
                if (Game1.getFarm().piecesOfHay.Value > 0)
                {
                    Game1.getFarm().piecesOfHay.Value--;
                    var hay = new Object(178, 1);
                    house.objects.Add(new Vector2(6 + i, 3), hay);
                }
                else break;
            }

            return true;
        }

        /// <summary>获取鱼塘的产出</summary>
        public static bool TryCollectFishPond(FishPond pond)
        {
            if (pond == null) return false;
            return pond.needsWatering.Value || pond.hasUnlockedFinalPopulationCap.Value;
        }

        /// <summary>尝试安全地执行动作并捕获异常</summary>
        public static bool TryAction(Action action, string actionName = "")
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                _monitor?.Log($"执行 '{actionName}' 时出错: {ex.Message}", LogLevel.Warn);
                return false;
            }
        }

        /// <summary>记录日志</summary>
        public static void Log(string message, LogLevel level = LogLevel.Debug)
        {
            _monitor?.Log(message, level);
        }
    }
}
