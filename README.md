# Archipelago Randomizer For Ittle Dew 2

## What's an Archipelago Randomizer?

A randomizer is software that rearranges the items in a game, increasing replayability by ensuring you get items in a different order every playthrough. In the case of Ittle Dew 2, your Raft Pieces could be hidden in some random caves, your dynamite could be locked behind a boss fight, and a tracker upgrade could be your reward for Northern End.

[Archipelago](https://archipelago.gg) is a multiworld framework, allowing multiple games to combine their randomizers and send items between players. For example, I could play Ittle Dew 2 while a friend plays Tunic. I find the Tunic player's stick, which allows them to defeat some enemies and find my dynamite. We continue exploring our islands whacking enemies with our sticks and blowing stuff up until we find enough items for us to both beat our games.

## What items are included in the randomizer?

Currently, the randomizer can give you the following:

* Everything in the Loot menu
* All outfits
* All cards
* All dungeon keys
* Crayons
* Lockpicks
* Random status effects (both positive and negative)
* BEES

EFCS is replaced with the Impossible Gates Pass, which automatically opens doors and gates that normally requires EFCS.

## What locations are included in the randomizer?

Currently, the following locations are randomized:

* All Optional Hidden Caves
* All chests and keys in the main eight dungeons
* All chests in the 15 main Portal Worlds (optional)
* All chests and keys in the four secret dungeons (optional)
* All cards and keys in the five Dreamworld dungeons (optional)
* Certain SUPER SECRET LOCATIONS (optional)

## What is the goal of the randomizer?

There are three possible goals:
* **Raft Quest**: Simply find the 8 Raft Pieces and escape the island.
* **Queen of Adventure**: Find the 8 Raft Pieces, but before you can escape the island, you must also defeat Simulacrum and take her Big Ol' Bag of Loot.
* **Queen of Dreams**: Find a Raft Piece to enter the Dreamworld and complete all five Dreamworld dungeons. This will let you escape the island.

## Installation

1. Download the latest version of [Bepinex](https://github.com/BepInEx/BepInEx/releases) (if you're on Linux, you need to use [v5.4.22](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.22)).
2. Download the latest version of [ModCore](https://github.com/Extra-2-Dew/ModCore).
3. Download the latest release (the zip file) from the [Releases page](https://github.com/Extra-2-Dew/ArchipelagoRandomizer/releases).
4. Unzip the release and drop it into your Bepinex plugins folder.

## Getting Started

Starting a randomized playthrough requires two steps: setting up your yaml and connecting in-game.

### Setting up your YAML

The [release](https://github.com/Extra-2-Dew/ArchipelagoRandomizer/releases) includes a .yaml file, which is used for choosing your settings. You can open the YAML file with any text editor. Each setting has a description next to it. If you want to further customize your YAML and you're an advanced user, you may want to check out the [YAML](https://archipelago.gg/tutorial/Archipelago/advanced_settings/en), [Triggers](https://archipelago.gg/tutorial/Archipelago/triggers/en), and [Plando](https://archipelago.gg/tutorial/Archipelago/plando/en) guides.

### Connecting to a multiworld

Once a server has been set up, launch Ittle Dew 2 and start a new game. You should see a new Archipelago settings button, with a gray Archipelago logo.

![image](https://github.com/user-attachments/assets/ecc778b6-c8de-4f89-a470-539b6f667d26)

Click it, and you'll be brought to the Connection Info menu. (Note, none of the custom UI supports controller.)

![Connection Info](https://github.com/Extra-2-Dew/ArchipelagoRandomizer/blob/main/Readme%20Art/ConnectionInfo.png)

* **Server**: The server URL that is hosting the randomizer seed. Do not include the port.
* **Port**: Port of the server to connect to. These are the five numbers after the : in the full server address.
* **Slot Name**: Your player's slot name.
* **Password**: If your server has a password, you'll need to enter it here to connect.
* **Deathlink**: When you die, everyone dies. Of course, the reverse is true too.
* **Equip Outfits**: Automatically equip outfits when you receive them.
* **Stackable Statuses**: Makes it possible to stack multiple of the same status effect on Ittle and enemies, and allows Ittle to receive multiple positive statuses at once.
* **Chests Match Contents**: Makes chests change color to reflect their contents. More details below.

Once you have your info entered, press Back, and the game will attempt to connect to the server (or give you an error if it fails). The Archipelago logo will become colored if a connection was successfully established. Now, simply input your file name (or just leave it as the default "Ittle") and press "Confirm" to start your adventure!

If you need to change your connection info or settings, the Archipelago button can be pressed on the File Summary screen.

### Generating or Hosting?

If you want to generate a seed locally, or host a seed on either your own server or an official Archipelago server, read the [Setup guide](https://archipelago.gg/tutorial/Archipelago/setup/en). You can get the apworld for Ittle Dew 2 with the [release](https://github.com/Extra-2-Dew/ArchipelagoRandomizer/releases).

### Should I have anything else?

Arborelia has been working on a [Poptracker pack](https://github.com/arborelia/ittledew2-poptracker). It's not 100% accurate to the logic yet, but should be largely correct for seeds without phasing logic. This randomizer is also compatible with the [Universal Tracker](https://discord.com/channels/731205301247803413/1170094879142051912).

As the mod is still in develpment, there may be bugs, softlocks, or items not properly granted. If this happens, you may want to use the [Debug Menu Cheats](https://github.com/Extra-2-Dew/DebugMenuCheats) mod to remedy any issues.

### What's this "Chests Match Contents" setting?

If you have the setting on, chests will change their colors to let you know what kind of item is inside them. If they're in a crystal, the crystal will change color too.

![Chests Match Contents chart](https://github.com/Extra-2-Dew/ArchipelagoRandomizer/blob/main/Readme%20Art/ChestsMatchContentsChart.png)

What about traps? Well...you'll figure it out.

### Is there a text client for this?

You can use the ModCore console as an Archipelago text client! Open it with the \` (grave accent) key at any time. All standard Archipelago commands can be entered by using the `ap` command. For example, you can use `ap !hint sword` or `ap !release`. You can also simply chat by preceding your chat message with `ap`; for example `ap Some text` will chat "Some text" to all other text clients.

![Console example](https://github.com/Extra-2-Dew/ArchipelagoRandomizer/blob/main/Readme%20Art/ConsoleExample.png)

## Want to keep up with development?

Join the [Archipelago server](https://discord.gg/8Z65BR2) and visit the [Ittle Dew 2 thread](https://discord.com/channels/731205301247803413/1260030530217574500) or join the [Ludo Modding](https://discord.gg/R2933JkFbp) server to see what we've been up to!
