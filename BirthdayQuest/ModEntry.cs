using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Characters;

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
            helper.Events.GameLoop.DayStarted += this.BirthdayMessage;
        }


        /*********
        ** Private methods
        *********/
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void BirthdayMessage(object? sender, DayStartedEventArgs e)
        {
            this.Monitor.Log("WOW, new day!", LogLevel.Info);

            // get list of birthdays

            var allCharacterData = this.Helper.GameContent.Load<Dictionary<string, CharacterData>>("Data/Characters");

            var birthdays = new Dictionary< (Season season, int Day), List<string>>();

            foreach (var npc in allCharacterData)
            {
                
                CharacterData data = npc.Value;

                if (data.BirthSeason is null)
                {
                    continue;
                }

                var birthSeasonDay = (data.BirthSeason.Value, data.BirthDay);

                if (!birthdays.ContainsKey(birthSeasonDay)){
                    birthdays[birthSeasonDay] = new List<string>();
                }

                birthdays[birthSeasonDay].Add(npc.Key);

                //this.Monitor.Log($"added npc {npc.Key} and their birthday {birthSeasonDay}!", LogLevel.Info);
            }

            // get today's date

            var currDate = SDate.Now();
            var today = (currDate.Season, currDate.Day);

            List<string>? birthdayNpcs = null;

            // printing first
            if (birthdays.ContainsKey(today))
            {
                birthdayNpcs = birthdays[today];
            }

            if (birthdayNpcs is null)
            {
                return;
            }

            foreach (var npcName in birthdayNpcs){
                this.Monitor.Log($"Today is {npcName}'s birthday! Consider give them a gift.", LogLevel.Info);
            }
        }
    }
}