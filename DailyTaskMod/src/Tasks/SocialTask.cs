using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using DailyTaskMod.Helpers;
using Object = StardewValley.Object;

namespace DailyTaskMod.Tasks
{
    /// <summary>
    /// 社交任务 - 送生日礼物、与配偶对话、摸宠物
    /// </summary>
    public class SocialTask : IAutomationTask
    {
        public string TaskName => "social";
        public string Category => "social";

        public bool IsEnabled(ModConfig config)
            => config.Social.Enabled;

        public bool CanExecute(Farmer player, GameLocation location)
            => player != null;

        public TaskResult Execute(Farmer player, GameLocation location)
        {
            int actions = 0;
            var config = ModEntry.Instance?.Config;
            var farm = Game1.getFarm();

            // 宠物猫/狗
            if (config?.Social.PetDogOrCat == true)
            {
                var pet = GameHelper.GetPet();
                if (pet != null && !pet.hasBeenKissedToday.Value)
                {
                    // SDV 1.6: 使用 OnPetPush 替代旧版 pet() 方法
                    pet.OnPetPush(player.UniqueMultiplayerID);
                    actions++;
                }
            }

            // 与配偶对话 (SDV 1.6: todayAtFarmHouse → hasBeenKissedToday)
            if (config?.Social.TalkToSpouse == true && player.spouse != null)
            {
                var spouse = player.getSpouse();
                if (spouse != null && !spouse.hasBeenKissedToday.Value)
                {
                    spouse.hasBeenKissedToday.Value = true;
                    actions++;
                }
            }

            // 生日礼物
            if (config?.Social.GiveBirthdayGifts == true)
            {
                actions += GiveBirthdayGifts(player, config);
            }

            return actions > 0
                ? TaskResult.Done(actions, $"完成了 {actions} 个社交互动")
                : TaskResult.Empty();
        }

        private int GiveBirthdayGifts(Farmer player, ModConfig config)
        {
            int given = 0;

            foreach (var loc in GameHelper.GetAllLocations())
            {
                foreach (var npc in loc.characters.OfType<NPC>())
                {
                    if (npc.IsVillager && GameHelper.IsBirthday(npc) && npc.CanReceiveGifts())
                    {
                        var gift = GameHelper.GetLovedGift(npc, player, config?.Social.BirthdayGiftQuality);
                        if (gift != null)
                        {
                            try
                            {
                                // SDV 1.6: receiveGift 需要 Object, Farmer, bool, float, bool
                                player.Items.Remove(gift);
                                npc.receiveGift((Object)gift, player, true, 1.0f, false);
                                given++;
                            }
                            catch { }
                        }
                    }
                }
            }

            return given;
        }
    }
}
