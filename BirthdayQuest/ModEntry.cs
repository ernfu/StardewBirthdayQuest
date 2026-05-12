using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Menus;

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
            helper.Events.GameLoop.DayStarted += this.AllBirthdayNotification;
            helper.Events.Display.MenuChanged += this.OnClosedMenu;

        }

        /*********
        ** Private methods
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
                birthdays[birthSeasonDay].Add("Abigail");

                //this.Monitor.Log($"added npc {npc.Key} and their birthday {birthSeasonDay}!", LogLevel.Info);
            }

            return birthdays;
        }

        private List<string> GetTodayBirthdayNpcs()
        {
            var currDate = SDate.Now();
            var today = (currDate.Season, currDate.Day);

            var birthdays = this.GetAllBirthdays();

            if (birthdays.TryGetValue(today, out var birthdayNpcs))
            {
                return birthdayNpcs;
            }
            return new List<string>();

        }

        private List<string> birthdayNpc =  new List<string>();

        private static void BirthdayNotification(string npcName)
        {
            string message = $"It's {npcName}'s Birthday today! ^Consider giving them something nice.";
            Game1.activeClickableMenu = new DialogueBox(message);
        }

        private void ShowNextBirthdayNotification()
        {
            this.Monitor.Log($"{birthdayNpc[0]}'s birthday", LogLevel.Info);
            BirthdayNotification(birthdayNpc[0]);
            birthdayNpc.RemoveAt(0);
        }

        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void AllBirthdayNotification(object? sender, DayStartedEventArgs e)
        {
            birthdayNpc =  this.GetTodayBirthdayNpcs();
            ShowNextBirthdayNotification();
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