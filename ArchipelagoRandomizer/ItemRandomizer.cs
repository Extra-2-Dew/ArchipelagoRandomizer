using System.Collections.Generic;
using IC = ID2.ItemChanger;

namespace ID2.ArchipelagoRandomizer;

class ItemRandomizer
{
	private readonly struct CustomLocations
	{
		public static readonly List<IC.Location> alwaysOnLocations =
		[
			new FakeChestLocation("Machine Fortress - Bee Chest", IC.Area.MachineFortress, "Dungeon_ChestBees-83--65"),
		];

		public static readonly List<IC.Location> superSecretLocations =
		[
			new OutfitStandLocation("Fluffy Fields Caves - Jenny Berry House Outfit", IC.Area.FluffyFieldsCaves, "OutfitStand-84--26"),
			new OutfitStandLocation("Promised Remedy - That Guy Outfit", IC.Area.FluffyFieldsCaves, "OutfitStand-49--16"),
		];

		public static readonly List<IC.Location> secretSignLocations =
		[
			new SignLocation("Fluffy Fields Caves - Incomplete Sign", IC.Area.FluffyFieldsCaves, "MegaSecretsign2"),
			new SignLocation("Fluffy Fields Caves - Cipher Sign", IC.Area.FluffyFieldsCaves, "SignDeepB1"),
			new SignLocation("Sweetwater Coast Caves - Incomplete Sign", IC.Area.CandyCoastCaves, "MegaSecretsign4"),
			new SignLocation("Pepperpain Caves - Incomplete Sign", IC.Area.VitaminHillsCaves, "MegaSecretsign3"),
			new SignLocation("Frozen Court Caves - Cipher Sign", IC.Area.FrozenCourtCaves, "SignDeepB2"),
			new SignLocation("Lonely Road Caves - Incomplete Sign", IC.Area.LonelyRoadCaves, "MegaSecretsign1"),
			new SignLocation("Maze of Steel - Hint Sign", IC.Area.Deep11, "Deep11Sign1"),
			new SignLocation("Maze of Steel - Cipher Sign", IC.Area.Deep11, "SignDeepA1"),
			new SignLocation("Nowhere - Left Hint Sign", IC.Area.Deep16, "Deep16Sign1"),
			new SignLocation("Nowhere - Right Hint Sign", IC.Area.Deep16, "Deep16Sign2"),
			new SignLocation("Nowhere - Cipher Sign", IC.Area.Deep16, "SignDeepA2"),
			new SignLocation("Ludo City - Cipher Sign", IC.Area.Deep20, "SignDeepC2"),
			new SignLocation("Abyssal Plane - Shard Hint Sign", IC.Area.Deep21, "Deep21Sign1"),
			new SignLocation("Abyssal Plane - Roll Hint Sign", IC.Area.Deep21, "Deep21Sign3"),
			new SignLocation("Abyssal Plane - Drop Table Hint Sign", IC.Area.Deep21, "Deep21Sign2"),
			new SignLocation("Abyssal Plane - Boss Hint Sign", IC.Area.Deep21, "Deep21Sign4"),
			new SignLocation("Place From Younger Days - Cipher Sign", IC.Area.Deep22, "SignDeepC1"),
			new SignLocation("Promised Remedy - E Cipher Sign", IC.Area.Deep19s, "SignDeepG1"),
			new SignLocation("Promised Remedy - F Cipher Sign", IC.Area.Deep19s, "SignDeepE2"),
			new SignLocation("Promised Remedy - G Left Cipher Sign", IC.Area.Deep19s, "SignDeepD1"),
			new SignLocation("Promised Remedy - G Middle Cipher Sign", IC.Area.Deep19s, "SignDeepE1"),
			new SignLocation("Promised Remedy - G Right Cipher Sign", IC.Area.Deep19s, "SignDeepF1"),
			new SignLocation("Promised Remedy - M Cipher Sign", IC.Area.Deep19s, "SignDeepF2"),
			new SignLocation("Promised Remedy - N Cipher Sign", IC.Area.Deep19s, "SignDeepD2"),
		];
	}

	private readonly struct CustomItems
	{
		public static readonly List<IC.ICItem> alwaysOnItems =
		[
			new ImpossibleGatesPassItem("Impossible Gates Pass"),
		];

		public static readonly List<IC.ICItem> weaponUpgradeItems =
		[
			new WeaponUpgradeItem("Forcewand Upgrade") { Icon = "Forcewand1" },
			new WeaponUpgradeItem("Dynamite Upgrade") { Icon = "Dynamite1" },
			new WeaponUpgradeItem("Ice Ring Upgrade") { Icon = "Icering1" },
		];

		public static readonly List<IC.ICItem> keyringItems =
		[
			new KeyringItem("Pillow Fort Keyring", IC.Area.PillowFort),
			new KeyringItem("Sand Castle Keyring", IC.Area.SandCastle),
			new KeyringItem("Art Exhibit Keyring", IC.Area.ArtExhibit),
			new KeyringItem("Trash Cave Keyring", IC.Area.TrashCave),
			new KeyringItem("Flooded Basement Keyring", IC.Area.FloodedBasement),
			new KeyringItem("Potassium Mine Keyring", IC.Area.PotassiumMine),
			new KeyringItem("Boiling Grave Keyring", IC.Area.BoilingGrave),
			new KeyringItem("Grand Library Keyring", IC.Area.BoilingGrave),
			new KeyringItem("Sunken Labyrinth Keyring", IC.Area.SunkenLabyrinth),
			new KeyringItem("Machine Fortress Keyring", IC.Area.MachineFortress),
			new KeyringItem("Dark Hypostyle Keyring", IC.Area.DarkHypostyle),
			new KeyringItem("Tomb of Simulacrum Keyring", IC.Area.TombOfSimulacrum),
			new KeyringItem("Syncope Keyring", IC.Area.DreamDynamite),
			new KeyringItem("Antigram Keyring", IC.Area.DreamFireChain),
			new KeyringItem("Bottomless Tower Keyring", IC.Area.DreamIce),
			new KeyringItem("Quietus Keyring", IC.Area.DreamAll),
		];

		public static readonly List<IC.ICItem> regionConnectorItems =
		[
			new RegionConnnectorItem("Connection - Fluffy Fields To Sweetwater Coast", IC.Area.FluffyFields, IC.Area.CandyCoast) { Flag = "FF_CC" },
			new RegionConnnectorItem("Connection - Fluffy Fields To Fancy Ruins", IC.Area.FluffyFields, IC.Area.FancyRuins) { Flag = "FF_FR" },
			new RegionConnnectorItem("Connection - Fluffy Fields To Star Woods", IC.Area.FluffyFields, IC.Area.StarWoods) { Flag = "FF_SW" },
			new RegionConnnectorItem("Connection - Fluffy Fields To Slippery Slope", IC.Area.FluffyFields, IC.Area.SlipperySlope) { Flag = "FF_SS" },
			new RegionConnnectorItem("Connection - Fluffy Fields To Pepperpain Prairie", IC.Area.FluffyFields, IC.Area.VitaminHills) { Flag = "FF_VH" },
			new RegionConnnectorItem("Connection - Sweetwater Coast To Fancy Ruins", IC.Area.CandyCoast, IC.Area.FancyRuins) { Flag = "CC_FR" },
			new RegionConnnectorItem("Connection - Sweetwater Coast To Star Woods", IC.Area.CandyCoast, IC.Area.StarWoods) { Flag = "CC_SW" },
			new RegionConnnectorItem("Connection - Sweetwater Coast To Slippery Slope", IC.Area.CandyCoast, IC.Area.SlipperySlope) { Flag = "CC_SS" },
			new RegionConnnectorItem("Connection - Fancy Ruins To Star Woods", IC.Area.FancyRuins, IC.Area.StarWoods) { Flag = "FR_SW" },
			new RegionConnnectorItem("Connection - Fancy Ruins To Pepperpain Prairie", IC.Area.FancyRuins, IC.Area.VitaminHills) { Flag = "FR_VH" },
			new RegionConnnectorItem("Connection - Fancy Ruins To Frozen Court", IC.Area.FancyRuins, IC.Area.FrozenCourt) { Flag = "FR_FC" },
			new RegionConnnectorItem("Connection - Star Woods To Frozen Court", IC.Area.StarWoods, IC.Area.FrozenCourt) { Flag = "SW_FC" },
			new RegionConnnectorItem("Connection - Slippery Slope To Pepperpain Prairie", IC.Area.SlipperySlope, IC.Area.VitaminHills) { Flag = "SS_VH" },
			new RegionConnnectorItem("Connection - Slippery Slope To Lonely Road", IC.Area.SlipperySlope, IC.Area.LonelyRoad) { Flag = "SS_LR" },
		];
	}

	private static readonly List<IC.Location> customLocations = [];
	private static readonly List<IC.ICItem> customItems = [];

	public ItemRandomizer()
	{
		Events.OnFileStart += OnFileStart;
	}

	private void OnFileStart(bool newFile)
	{
		AddCustomLocationsAndItems();
		PlaceItems();
		IC.RecentItemsDisplay.Enabled = true;
	}

	private void AddCustomLocationsAndItems()
	{
		customLocations.AddRange(CustomLocations.alwaysOnLocations);
		customItems.AddRange(CustomItems.alwaysOnItems);

		// Modify custom locations/items based on settings
		if (true)
		{
			customLocations.AddRange(CustomLocations.secretSignLocations);
		}
		if (true)
		{
			customLocations.AddRange(CustomLocations.superSecretLocations);
		}
		if (true)
		{
			customItems.AddRange(CustomItems.weaponUpgradeItems);
		}
		if (true)
		{
			customItems.AddRange(CustomItems.keyringItems);
		}
		if (true)
		{
			customItems.AddRange(CustomItems.regionConnectorItems);
		}

		IC.Predefined.AddCustomLocations(customLocations);
		IC.Predefined.AddCustomItems(customItems);
	}

	private void PlaceItems()
	{
		// Temp hardcoded placements for testing. Should get placement data from APWorld
		Dictionary<string, string> placements = new()
		{
			// Pillow Fort
			{ "Pillow Fort - Shellbun Nest Key", "Connection - Fluffy Fields To Sweetwater Coast" },
			{ "Pillow Fort - Crayon Chest", "Tomb of Simulacrum Keyring" },
			{ "Pillow Fort - Safety Jenny Gate Key", "Pillow Fort Key" },
			{ "Pillow Fort - Treasure Chest", "Forcewand" },
			{ "Machine Fortress - Bee Chest", "Raft Piece" },
			{ "Fluffy Fields Caves - Cipher Sign", "Raft Piece" },
		};

		foreach (var placement in placements)
		{
			IC.Replacer.PlaceItem(placement.Key, placement.Value, OnLocationChecked);
		}
	}

	private void OnLocationChecked(IC.Location location, IC.ICItem item)
	{
		Logger.Log($"Checked '{location.Name}' and got '{item.DisplayName}'!");
		IC.NotificationHandler.ShowNotification($"<color=#e004e0>You</color> got a <color=#a793e3>{item.DisplayName}</color> from <color=#f5f5ce>WyrmOW</color>!", item.Icon);
	}
}