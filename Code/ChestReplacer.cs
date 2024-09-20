using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArchipelagoRandomizer
{
	class ChestReplacer
	{
		private static ChestReplacer instance;
		private const string assetPath = $"{PluginInfo.PLUGIN_NAME}/Assets/";
		private static readonly List<ChestCrystalColorData> chestCrystalColors = new()
		{
			{ new("Key", true,
				new ChestCrystalColorData.ChestColors("Yellow", "DarkGrey", "Grey"),
				new ChestCrystalColorData.CrystalColors("Gold", "Gold", "Yellow"))
			},
			{ new("Macguffin", true,
				new ChestCrystalColorData.ChestColors("Purple", "LightGrey", "Pearl"),
				new ChestCrystalColorData.CrystalColors("Purple", "Purple", "White"))
			},
			{ new("Minor", true,
				new ChestCrystalColorData.ChestColors("Brown", "DarkYellow", "Gold"),
				new ChestCrystalColorData.CrystalColors("Brown", "Brown", "Burgundy"))
			},
			{ new("Shard", true,
				new ChestCrystalColorData.ChestColors("DarkGrey", "Grey", "Grey"),
				new ChestCrystalColorData.CrystalColors("DarkGrey", "DarkGrey", "LightGrey"))
			},
			{ new("Junk", true,
				new ChestCrystalColorData.ChestColors("LightGrey", "Grey", "Grey"),
				new ChestCrystalColorData.CrystalColors("Silver", "Silver", "Grey"))
			},
			{ new("Filler", false,
				new ChestCrystalColorData.ChestColors("Cyan", "Grey", "Grey"),
				new ChestCrystalColorData.CrystalColors("Cyan", "Cyan", "Cyan"))
			},
			{ new("Useful", false,
				new ChestCrystalColorData.ChestColors("Blue", "DarkYellow", "Gold"),
				new ChestCrystalColorData.CrystalColors("Blue", "Blue", "Blue"))
			},
			{ new("NeverExclude", false,
				new ChestCrystalColorData.ChestColors("Orange", "Grey", "Grey"),
				new ChestCrystalColorData.CrystalColors("Orange", "Orange", "Orange"))
			},
			{ new("Advancement", false,
				new ChestCrystalColorData.ChestColors("Orange", "DarkYellow", "Gold"),
				new ChestCrystalColorData.CrystalColors("Orange", "Orange", "Yellow"))
			},
			{ new("RegionConnector", false,
				new ChestCrystalColorData.ChestColors("Orange", "Grey", "Grey"),
				new ChestCrystalColorData.CrystalColors("Orange", "Orange", "Orange"))
			},
			{ new("Empty", false,
				new ChestCrystalColorData.ChestColors("LightGrey", "Grey", "Grey"),
				new ChestCrystalColorData.CrystalColors("Silver", "Silver", "Grey"))
			}
		};
		private static List<ChestCrystalColorData> trapChestColors;
		private static readonly Dictionary<string, Texture2D> cachedTextures = new();

		public static ChestReplacer Instance
		{
			get
			{
				if (instance == null)
					instance = new();

				return instance;
			}
		}

		public ChestReplacer()
		{
			if (trapChestColors == null)
				trapChestColors = chestCrystalColors.FindAll(x => x.useForTraps);
		}

		public void ReplaceChestTextures(DummyAction dummyAction, Renderer chestMesh, Renderer crystalMesh)
		{
			ItemHandler.ItemData.Item item = ItemRandomizer.Instance.GetItemForLocation(SceneManager.GetActiveScene().name, dummyAction._saveName, out var scoutedItemInfo);

			// Leave vanila if major
			if (item != null && CheckItemFlags(item, ItemHandler.ItemFlags.Major))
				return;

			ChestCrystalColorData colors = null;

			if (scoutedItemInfo != null)
			{
				// If trap
				if (CheckItemFlags(scoutedItemInfo, Archipelago.MultiClient.Net.Enums.ItemFlags.Trap))
				{
					// Get random colors
					int randIndex = Random.Range(0, trapChestColors.Count);

					// Major item color
					if (randIndex >= trapChestColors.Count)
						return;

					colors = trapChestColors[randIndex];
					Vector3 rot = dummyAction.transform.localEulerAngles;
					dummyAction.transform.localEulerAngles = new Vector3(rot.x, rot.y + 180, rot.z);
				}
				// If ID2 item
				else if (item != null)
				{
					if (item.Type == ItemHandler.ItemTypes.Key || item.Type == ItemHandler.ItemTypes.Keyring)
						colors = chestCrystalColors.Find(x => x.flag == "Key");
					else if (item.Type == ItemHandler.ItemTypes.Shard)
						colors = chestCrystalColors.Find(x => x.flag == item.Type.ToString());
					else if (item.Type == ItemHandler.ItemTypes.RegionConnector)
						colors = chestCrystalColors.Find(x => x.flag == item.Type.ToString());
					else
						colors = chestCrystalColors.Find(x => x.flag == item.Flag.ToString());
				}
				// If item for another game or is a trap
				else
				{
					if (CheckItemFlags(scoutedItemInfo, Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement))
						colors = chestCrystalColors.Find(x => x.flag == scoutedItemInfo.Flags.ToString());
					else if (CheckItemFlags(scoutedItemInfo, Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude))
						colors = chestCrystalColors.Find(x => x.flag == "Useful");
				}
			}
			// If no item, that means it's not randomized
			else
				colors = chestCrystalColors.Find(x => x.flag == "Empty");

			// If item exists, but has no flags/type, it's filler
			if (colors == null)
				colors = chestCrystalColors.Find(x => x.flag == "Filler");

			SetChestTextures(chestMesh, colors.chestColors);

			if (crystalMesh != null)
				SetCrystalTextures(crystalMesh, colors.crystalColors);
		}

		private void SetChestTextures(Renderer mesh, ChestCrystalColorData.ChestColors colors)
		{
			Material chestMaterial = mesh.materials[2];
			Material trimMaterial = mesh.materials[1];
			Texture2D chestTexture = GetCachedTexture(colors.color);
			Texture2D trimTexture = GetCachedTexture(colors.trimColor);
			Texture2D shineTexture = GetCachedTexture(colors.shineColor);

			chestMaterial.SetTexture("_MainTex", chestTexture);
			trimMaterial.SetTexture("_MainTex", trimTexture);
			trimMaterial.SetTexture("_SpecularRamp", shineTexture);
		}

		private void SetCrystalTextures(Renderer mesh, ChestCrystalColorData.CrystalColors colors)
		{
			Material faceMaterial = mesh.materials[0];
			Material edgeMaterial = mesh.materials[1];
			Texture2D faceRampTexture = GetCachedTexture(colors.faceRamp);
			Texture2D faceRimTexture = GetCachedTexture(colors.faceRim);
			Texture2D edgeTexture = GetCachedTexture(colors.edgeColor);

			edgeMaterial.shader = Shader.Find("Unlit/Texture");
			edgeMaterial.SetTexture("_MainTex", edgeTexture);
			faceMaterial.SetTexture("_SpecularRamp", faceRimTexture);
			faceMaterial.SetTexture("_RimRamp", faceRampTexture);
		}

		private Texture2D GetCachedTexture(string color)
		{
			if (!cachedTextures.TryGetValue(color, out Texture2D texture))
			{
				// Load & cache texture
				string path = $"{assetPath}Chest{color}.png";
				texture = ModCore.Utility.GetTextureFromFile(path);
				cachedTextures.Add(color, texture);
			}

			return texture;
		}

		private bool CheckItemFlags(ItemHandler.ItemData.Item item, ItemHandler.ItemFlags flag)
		{
			return (item.Flag & flag) == flag;
		}

		private bool CheckItemFlags(Archipelago.MultiClient.Net.Models.ScoutedItemInfo item, Archipelago.MultiClient.Net.Enums.ItemFlags flag)
		{
			return (item.Flags & flag) == flag;
		}

		private class ChestCrystalColorData
		{
			public readonly string flag;
			public readonly bool useForTraps;
			public readonly ChestColors chestColors;
			public readonly CrystalColors crystalColors;

			public ChestCrystalColorData(string flag, bool useForTraps, ChestColors chestColors, CrystalColors crystalColors)
			{
				this.flag = flag;
				this.useForTraps = useForTraps;
				this.chestColors = chestColors;
				this.crystalColors = crystalColors;
			}

			public readonly struct ChestColors
			{
				public readonly string color;
				public readonly string trimColor;
				public readonly string shineColor;

				public ChestColors(string color, string trimColor, string shineColor)
				{
					this.color = color;
					this.trimColor = trimColor;
					this.shineColor = "Rim" + shineColor;
				}
			}

			public readonly struct CrystalColors
			{
				public readonly string faceRamp;
				public readonly string faceRim;
				public readonly string edgeColor;

				public CrystalColors(string faceRamp, string faceRim, string edgeColor)
				{
					this.faceRamp = "Jewel" + faceRamp;
					this.faceRim = "CrystalRim" + faceRim;
					this.edgeColor = edgeColor;
				}
			}
		}
	}
}