using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using DailyTaskMod.Helpers;

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
                if (pet != null && !pet.wasPetToday.Value)
                {
                    pet.pet(player);
                    actions++;
                }
            }

            // 与配偶对话
            if (config?.Social.TalkToSpouse == true && player.spouse != null)
            {
                var spouse = player.getSpouse();
                if (spouse != null && !spouse.todayAtFarmHouse.Value)
                {
                    spouse.todayAtFarmHouse.Set(true);
                    spouse.dialogueQuestionsAsked.Add("spouse_" + Game1.Date.TotalDays);
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
                    if (npc.IsVillager && GameHelper.IsBirthday(npc) && !npc.Giftermail.Contains(player.UniqueMultiplayerID.ToString()))
                    {
                        var gift = GameHelper.GetLovedGift(npc, player, config?.Social.BirthdayGiftQuality);
                        if (gift != null)
                        {
                            try
                            {
                                // 模拟送礼
                                player.Items.Remove(gift);
                                npc.receiveGift(gift, player, true);
                                npc.Giftermail.Add(player.UniqueMultiplayerID.ToString());
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
