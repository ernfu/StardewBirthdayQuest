# Stardew Valley Birthday Quest Mod

A mod for Stardew Valley that reminds you when it's someone's birthday and adds 1-day "birthday gift" task to you quest borad.

Perfect for people who always forget to give birthday presents!

![Birthday quest](screenshots/quest2.png)

![Wake-up birthday reminder](screenshots/wake_msg.png)

e.g. on Spring 14 (Haley's birthday)...
1. when you wake up, the game shows a dialoge box reminding you it's Haley's birthday
2. adds a "birthday gift for Haley" limited time quest to your active quest log


## Downloads
Download it here: https://www.nexusmods.com/stardewvalley/mods/46184 or go to releases for the zip.

## Config (optional)

You can use config to set 

The config file is created by SMAPI the first time you launch the game with the mod installed. If you do not see it yet, start the game once, then close it.

After that, edit:

```text
Mods/BirthdayQuest/config.json
```

Restart the game after changing the config.

Available options:

- `BirthdayNotification`: shows a wake-up message when today is an NPC's birthday. Default: `false`.
- `BirthdayQuest`: adds a one-day birthday gift quest to your quest log. Default: `false`.
- `LovedGiftsHint`: adds a list of loved gifts to the birthday quest text. Default: `false`.


### TODOs
[x] add recommended gift (by taste) to dialoge/ quest - added toggle on from config.json
    [] add support for Generic Mod Config Menu (GMCM)
- fix pronouns
- add npc schedule to quest
- add cross mod compatibility
- add translation compatibility
