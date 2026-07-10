using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DailyTaskMod
{
    /// <summary>
    /// 模组配置模型 - 支持 Generic Mod Config Menu 集成
    /// </summary>
    public class ModConfig
    {
        // ============= 全局设置 =============
        public bool EnableAutoRun { get; set; } = true;
        public int AutoRunDelayTicks { get; set; } = 120;
        public bool AutoRunOnNewDay { get; set; } = true;
        public bool OnlyIfPlayerIdle { get; set; } = true;
        public bool ShowHudMessages { get; set; } = true;
        public bool ShowTaskProgress { get; set; } = true;
        public int MaxTasksPerRun { get; set; } = 30;

        public SButton TaskToggleKey { get; set; } = SButton.K;

        // ============= 任务分类设置 =============
        public WateringConfig Watering { get; set; } = new();
        public CropHarvestConfig CropHarvest { get; set; } = new();
        public AnimalPettingConfig AnimalPetting { get; set; } = new();
        public AnimalProductsConfig AnimalProducts { get; set; } = new();
        public AnimalFeedingConfig AnimalFeeding { get; set; } = new();
        public MachineHarvestConfig MachineHarvest { get; set; } = new();
        public TreeTasksConfig TreeTasks { get; set; } = new();
        public ForageConfig Forage { get; set; } = new();
        public SocialConfig Social { get; set; } = new();
        public MiscConfig Misc { get; set; } = new();
    }

    public class WateringConfig
    {
        public bool Enabled { get; set; } = true;
        public bool OnlyUnwatered { get; set; } = true;
        public bool RespectSprinklers { get; set; } = true;
        public bool UseWaterFromInventory { get; set; } = true;
        public int MaxWateringCanLevel { get; set; } = 4;
    }

    public class CropHarvestConfig
    {
        public bool Enabled { get; set; } = true;
        public bool HarvestFlowers { get; set; } = true;
        public bool HarvestRegrowable { get; set; } = true;
    }

    public class AnimalPettingConfig
    {
        public bool Enabled { get; set; } = true;
        public bool PetAllAnimals { get; set; } = true;
    }

    public class AnimalProductsConfig
    {
        public bool Enabled { get; set; } = true;
        public bool CollectMilk { get; set; } = true;
        public bool CollectEggs { get; set; } = true;
        public bool CollectWool { get; set; } = true;
        public bool CollectTruffles { get; set; } = true;
        public bool CollectFeathers { get; set; } = true;
    }

    public class AnimalFeedingConfig
    {
        public bool Enabled { get; set; } = true;
        public bool PlaceHayInBarn { get; set; } = true;
        public bool OpenGatesInGoodWeather { get; set; } = true;
        public bool CloseGatesInRain { get; set; } = true;
    }

    public class MachineHarvestConfig
    {
        public bool Enabled { get; set; } = true;
        public bool CollectKegs { get; set; } = true;
        public bool CollectPreserveJars { get; set; } = true;
        public bool CollectCheesePresses { get; set; } = true;
        public bool CollectMayonnaiseMachines { get; set; } = true;
        public bool CollectLoom { get; set; } = true;
        public bool CollectOilMakers { get; set; } = true;
        public bool CollectFurnaces { get; set; } = true;
        public bool CollectCharcoalKilns { get; set; } = true;
        public bool CollectCrystalariums { get; set; } = true;
        public bool CollectSeedMakers { get; set; } = true;
        public bool CollectSluices { get; set; } = true;
        public bool CollectBoneMills { get; set; } = true;
        public bool CollectTappers { get; set; } = true;
        public bool CollectBeeHouses { get; set; } = true;
        public bool CollectDehydrators { get; set; } = true;
        public bool CollectFishSmokers { get; set; } = true;
    }

    public class TreeTasksConfig
    {
        public bool Enabled { get; set; } = true;
        public bool HarvestFruitTrees { get; set; } = true;
        public bool CollectTreeTappers { get; set; } = true;
        public bool ShakeTreesForSeeds { get; set; } = false;
    }

    public class ForageConfig
    {
        public bool Enabled { get; set; } = true;
        public bool CollectForageItems { get; set; } = true;
        public bool ClearDebris { get; set; } = true;
        public bool ClearWeeds { get; set; } = true;
        public bool ClearStones { get; set; } = true;
        public bool OnlyClearPaths { get; set; } = true;
        public int MaxClearRadius { get; set; } = 50;
    }

    public class SocialConfig
    {
        public bool Enabled { get; set; } = true;
        public bool GiveBirthdayGifts { get; set; } = true;
        public string BirthdayGiftQuality { get; set; } = "gold";
        public bool TalkToSpouse { get; set; } = true;
        public bool PetDogOrCat { get; set; } = true;
    }

    public class MiscConfig
    {
        public bool Enabled { get; set; } = true;
        public bool CheckTV { get; set; } = true;
        public bool ProcessGeodes { get; set; } = false;
        public bool CollectCaveMushrooms { get; set; } = true;
        public bool CheckMail { get; set; } = true;
        public bool CollectFishPonds { get; set; } = true;
        public bool CollectSlimeBalls { get; set; } = true;
    }
}
