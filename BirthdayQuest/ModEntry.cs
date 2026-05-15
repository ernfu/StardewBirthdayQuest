using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Menus;
using StardewValley.GameData.SpecialOrders;
using StardewValley.GameData.Objects;
using Microsoft.Xna.Framework.Graphics;

namespace BirthdayQuest
{
    /// <summary>The mod entry point.</summary>
    internal sealed class MyMod : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            helper.Events.GameLoop.DayStarted += this.OnDayStarted;

            helper.Events.Display.MenuChanged += this.OnClosedMenu;

            helper.Events.Content.AssetRequested += this.OnAssetRequested;

        }

        /*********
        ** Private methods
        *********/

        /*********
        ** helper funcs - getting birthdays
        *********/

        // get birthdays - key: season/ day and value: npc names
        private Dictionary< (Season season, int Day), List<string>> GetAllBirthdays()
        {
            var allCharacterData = this.Helper.GameContent.Load<Dictionary<string, CharacterData>>("Data/Characters");
            var birthdays = new Dictionary< (Season season, int Day), List<string>>();

            foreach (var npc in allCharacterData)
            {
                
                CharacterData data = npc.Value;

                // mod compatible way - BirthSeason could be null
                if (data.BirthSeason is null)
                {
                    continue;
                }

                var birthSeasonDay = (data.BirthSeason.Value, data.BirthDay);

                // list to guard against mod NPCs have the same birthday as original NPCss
                if (!birthdays.ContainsKey(birthSeasonDay)){
                    birthdays[birthSeasonDay] = new List<string>();
                }

                birthdays[birthSeasonDay].Add(npc.Key);
            }

            return birthdays;
        }

        /*********
        ** helper funcs - getting NPC gift taste
        *********/

        private List<string> GetAllItems()
        {
            return this.Helper.GameContent.Load<Dictionary<string, ObjectData>>("Data/Objects").Keys.ToList();
        }
        private List<string> GetItemByTaste(string npcName, string taste)
        {

            int tasteNum = taste switch
            {
                "love" => 0,
                "like" => 2,
                _ => 4
            };

            var tasteItems = new List<string>();

            var npc = Game1.getCharacterFromName(npcName);
            if (npc is null){
                return tasteItems;
            }

            foreach (var item in GetAllItems()){
                var itemId = "(O)" + item;
                Item itemObject = ItemRegistry.Create(itemId);
                int itemTaste = npc.getGiftTasteForThisItem(itemObject);

                if (itemTaste == tasteNum){
                    tasteItems.Add(itemObject.DisplayName);
                }
            }

            return tasteItems;
        }

        /*********
        ** load save - load all birthdays + all object items
        *********/

        private Dictionary< (Season season, int Day), List<string>> allBirthday = new();
        private List<string> allItems = new();

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            allBirthday = this.GetAllBirthdays();
            allItems = this.GetAllItems();
        }

        /*********
        ** day starts - load today's birthday npcs & add quest and notifications
        *********/

        private List<string> GetTodayBirthdayNpcs()
        {
            var currDate = SDate.Now();
            var today = (currDate.Season, currDate.Day);

            var birthdays = this.allBirthday;

            if (birthdays.TryGetValue(today, out var birthdayNpcs))
            {
                return birthdayNpcs;
            }
            return new List<string>();

        }

        private List<string> birthdayNpc =  new List<string>();

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            birthdayNpc =  this.GetTodayBirthdayNpcs();

            foreach (var npc in birthdayNpc){
                AddBirthdayQuest(npc);
            }

            ShowNextBirthdayNotification();
        }

        /*********
        ** Quests
        *********/

        // Register all birthday quests to special order data
        private SpecialOrderData BuildBirthdaySpecialOrderData(string npc){
            
            var newSpecialOrder = new SpecialOrderData();
            newSpecialOrder.Name = $"{npc}'s birthday";
            newSpecialOrder.Requester = npc;
            newSpecialOrder.Duration = QuestDuration.OneDay;

            var baseText =  $"It's {npc}'s Birthday today! \nGive them something nice. ";

            var lovedItems = this.GetItemByTaste(npc, "love");
            var lovedItemsText = "\n\nThey love " + string.Join(", ", lovedItems) + ".";

            //var likedItems = this.GetItemByTaste(npc, "like");
            //var likedItemsText = "\n\nThey like " + string.Join(", ", likedItems) + ".";

            newSpecialOrder.Text = baseText + lovedItemsText;

            // add objective to order; need SpecialOrderObjectiveData
            var newObjective = new SpecialOrderObjectiveData();
            newObjective.Type = "Deliver";
            newObjective.Text = $"Give {npc} a birthday gift.";
            newObjective.RequiredCount = "1";
            newObjective.Data = new Dictionary<string, string>{{"TargetName", npc}};
            newSpecialOrder.Objectives = new List<SpecialOrderObjectiveData> {newObjective};

            // add rewards to order; need SpecialOrderRewardData
            var newRewards = new SpecialOrderRewardData();
            newRewards.Type = "Money";
            newRewards.Data = new Dictionary<string, string>{{"Amount", "1"}, {"Multiplier", "0"}};
            newSpecialOrder.Rewards = new List<SpecialOrderRewardData> {newRewards};

            return newSpecialOrder;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            //here
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/SpecialOrders"))
            {
                return;
            }

            var allBirthdays = this.GetAllBirthdays();

            foreach (var birthday in allBirthdays){
                foreach (var npc in birthday.Value)

                    e.Edit(asset =>
                    {
                        var data = asset.AsDictionary<string, SpecialOrderData>().Data;
                        string orderId = $"BirthdayQuest.{npc}.BirthdayGift";

                        data[orderId] = this.BuildBirthdaySpecialOrderData(npc);

                    }
                    );
            }
        }

        // add quest to active quests for birthday npcs
        private void AddBirthdayQuest(string npc)
        {
            var orderId = $"BirthdayQuest.{npc}.BirthdayGift";

            this.Monitor.Log($"added {orderId} to active!", LogLevel.Info);

            Game1.player.team.AddSpecialOrder(orderId, forceRepeatable: true);

        }

        /*********
        ** Notifications
        *********/
        private static void BirthdayNotification(string npcName)
        {
            string message = $"It's {npcName}'s Birthday today! ^Consider giving them something nice.";
            Game1.activeClickableMenu = new DialogueBox(message);
        }

        private void ShowNextBirthdayNotification()
        {
            if (birthdayNpc.Count == 0)
            {
                return;
            }

            this.Monitor.Log($"{birthdayNpc[0]}'s birthday", LogLevel.Info);
            BirthdayNotification(birthdayNpc[0]);
            birthdayNpc.RemoveAt(0);
        }

        private void OnClosedMenu(object? sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not null)
            {
                return;
            }

            if (birthdayNpc.Count == 0)
            {
                return;
            }

            ShowNextBirthdayNotification();

        }
    }
}