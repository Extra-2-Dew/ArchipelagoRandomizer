using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	class ChestReplacer
	{
		private static ChestReplacer instance;
		private const string assetPath = $"{PluginInfo.PLUGIN_NAME}/Assets/";
		private static readonly List<ItemFlagColors> itemFlagColors = new()
		{
			{ new("Useful", "Blue", "DarkYellow", "Gold") },
			{ new("Minor", "Brown", "DarkYellow", "Gold") },
			{ new("Card", "Burgundy", "Grey", "Grey") },
			{ new("Shard", "DarkGrey", "Grey", "Grey") },
			{ new("EvilKey", "DarkGrey", "Grey", "Grey") },
			{ new("Advancement", "Orange", "DarkYellow", "Gold") },
			{ new("NeverExclude", "Orange", "Grey", "Grey") },
			{ new("Filler", "TealGreen", "Grey", "Grey") }
		};
		private static readonly Dictionary<string, Texture2D> cachedChestTextures = new();
		private static readonly Dictionary<string, Texture2D> cachedTrimTextures = new();
		private static readonly Dictionary<string, Texture2D> cachedShineTextures = new();

		public static ChestReplacer Instance
		{
			get
			{
				if (instance == null)
					instance = new();

				return instance;
			}
		}

		public void ReplaceChestTextures(DummyAction dummyAction, SkinnedMeshRenderer mesh)
		{
			ItemHandler.ItemData.Item item = ItemRandomizer.Instance.GetItemForLocation(dummyAction._saveName, out var scoutedItemInfo);

			// Leave vanila if major
			if (CheckItemType(item, ItemHandler.ItemFlags.Major))
				return;

			ItemFlagColors colors;

			if (item.Type == ItemHandler.ItemTypes.Card)
				colors = itemFlagColors.Find(x => x.flag == item.Type.ToString());
			else if (item.Type == ItemHandler.ItemTypes.Shard || item.Type == ItemHandler.ItemTypes.EvilKey)
				colors = itemFlagColors.Find(x => x.flag == item.Type.ToString());
			else if (CheckItemType(item, ItemHandler.ItemFlags.Minor))
				colors = itemFlagColors.Find(x => x.flag == item.Flag.ToString());
			else if (CheckItemType(item, ItemHandler.ItemFlags.Useful))
				colors = itemFlagColors.Find(x => x.flag == item.Flag.ToString());
			else if (CheckItemType(scoutedItemInfo, Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement))
				colors = itemFlagColors.Find(x => x.flag == scoutedItemInfo.Flags.ToString());
			else if (CheckItemType(scoutedItemInfo, Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude))
				colors = itemFlagColors.Find(x => x.flag == scoutedItemInfo.Flags.ToString());
			else if (CheckItemType(scoutedItemInfo, Archipelago.MultiClient.Net.Enums.ItemFlags.Trap))
			{
				// Get random colors
				int randIndex = Random.Range(0, itemFlagColors.Count);

				// Major item color
				if (randIndex >= itemFlagColors.Count)
					return;

				colors = itemFlagColors[randIndex];
				dummyAction.transform.localScale = new(1, 1, -1);
			}
			else
				colors = itemFlagColors.Find(x => x.flag == "Filler");

			SetTextures(mesh, colors);
		}

		private void SetTextures(SkinnedMeshRenderer mesh, ItemFlagColors colors)
		{
			Material chestMaterial = mesh.materials[2];
			Material trimMaterial = mesh.materials[1];
			Texture2D chestTexture = GetCachedChestTexture(colors.chestColor);
			Texture2D trimTexture = GetCachedTrimTexture(colors.trimColor);
			Texture2D shineTexture = GetCachedShineTexture(colors.shineColor);

			chestMaterial.SetTexture("_MainTex", chestTexture);
			trimMaterial.SetTexture("_MainTex", trimTexture);
			trimMaterial.SetTexture("_SpecularRamp", shineTexture);
		}

		private Texture2D GetCachedChestTexture(string chestColor)
		{
			if (!cachedChestTextures.TryGetValue(chestColor, out Texture2D chestTexture))
			{
				chestTexture = GetTexture(chestColor);
				cachedChestTextures.Add(chestColor, chestTexture);
			}

			return chestTexture;
		}

		private Texture2D GetCachedTrimTexture(string trimColor)
		{
			if (!cachedTrimTextures.TryGetValue(trimColor, out Texture2D trimTexture))
			{
				trimTexture = GetTexture(trimColor);
				cachedTrimTextures.Add(trimColor, trimTexture);
			}

			return trimTexture;
		}

		private Texture2D GetCachedShineTexture(string shineColor)
		{
			if (!cachedShineTextures.TryGetValue(shineColor, out Texture2D shineTexture))
			{
				shineTexture = GetTexture("Rim" + shineColor);
				cachedShineTextures.Add(shineColor, shineTexture);
			}

			return shineTexture;
		}

		private Texture2D GetTexture(string textureName)
		{
			string path = $"{assetPath}Chest{textureName}.png";
			return ModCore.Utility.GetTextureFromFile(path);
		}

		private bool CheckItemType(ItemHandler.ItemData.Item item, ItemHandler.ItemFlags flag)
		{
			return (item.Flag & flag) == flag;
		}

		private bool CheckItemType(Archipelago.MultiClient.Net.Models.ScoutedItemInfo item, Archipelago.MultiClient.Net.Enums.ItemFlags flag)
		{
			return (item.Flags & flag) == flag;
		}

		private readonly struct ItemFlagColors
		{
			public readonly string flag;
			public readonly string chestColor;
			public readonly string trimColor;
			public readonly string shineColor;

			public ItemFlagColors(string flag, string chestColor, string trimColor, string shineColor)
			{
				this.flag = flag;
				this.chestColor = chestColor;
				this.trimColor = trimColor;
				this.shineColor = shineColor;
			}
		}
	}
}