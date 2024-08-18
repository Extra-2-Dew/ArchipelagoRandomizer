using System.Collections.Generic;
using UnityEngine;

namespace ArchipelagoRandomizer
{
	public class HintHandler : MonoBehaviour
	{
		private List<string> requiredDungeons;

		public static HintHandler Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
			requiredDungeons = RandomizerSettings.Instance.RequiredDungeons;
		}

		private void OnEnable()
		{
			Events.OnEntitySpawn += OnSpawnEntity;
		}

		private void OnDisable()
		{
			requiredDungeons = null;
			Events.OnEntitySpawn -= OnSpawnEntity;
		}

		public void ShowSyncopePianoPuzzleHint(Transform doodads, string puzzleKeys)
		{
			Sign noteSign = doodads.transform.Find("GrandLibrary_Bookpile7").GetComponentInChildren<Sign>();
			string puzzleWithSharps = "";

			// Add sharps to string
			foreach (char c in puzzleKeys)
			{
				if (char.IsLower(c))
				{
					puzzleWithSharps += char.ToUpper(c) + "#";
					continue;
				}

				puzzleWithSharps += c;
			}

			// Force Sign to use our custom text
			noteSign._configString = null;

			string ittleComment = "It's gotta be important.";

			switch (puzzleKeys.ToLower())
			{
				case "add":
					ittleComment = "Don't tell me I'm going to\nhave to do math...";
					break;
				case "age":
				case "aged":
					ittleComment = "Good thing I'm forever young.";
					break;
				case "baa":
					ittleComment = "Is there a Jenny Lamb around here?";
					break;
				case "bad":
					ittleComment = "Harsh critic.";
					break;
				case "bed":
					ittleComment = "Yeah, those Deadbeets could\nprobably use some sleep.";
					break;
				case "bee":
					ittleComment = "I've had enough of those,\nthank you.";
					break;
				case "beef":
					ittleComment = "I could use a good hamburger.";
					break;
				case "cab":
					ittleComment = "It would be nice not to have to\nwalk around everywhere.";
					break;
				case "cafe":
					ittleComment = "Do they serve health potions?";
					break;
				case "cabbage":
					ittleComment = "Cabbage cabbage cabbage.";
					break;
				case "dab":
				case "dabbed":
					ittleComment = "Cringe.";
					break;
				case "dace":
					ittleComment = "Apparently the author loved fish?";
					break;
				case "decaf":
					ittleComment = "What's the point of health potion\nwithout the buzz though?";
					break;
				case "egg":
					ittleComment = "Egg.";
					break;
				case "edge":
					ittleComment = "I'm more of a Firefox fan though.";
					break;
				case "fad":
					ittleComment = "I agree. Haunted mansions are\nso 1996.";
					break;
				case "fee":
					ittleComment = "You aren't getting any treasure\nout of me.";
					break;
			}

			noteSign._text = $"\"{puzzleWithSharps}...\"\nThe word is repeated on every page.\n{ittleComment}";
		}

		private void OnSpawnEntity(Entity entity)
		{
			if (requiredDungeons == null || requiredDungeons.Count == 0)
				return;

			if (entity.name == "NPCJennyMole")
			{
				Sign dialogue = entity.GetComponentInChildren<Sign>();
				dialogue._altStrings = null;
				dialogue._configString = null;
				dialogue._exprData = null;
				dialogue._text = GetDungeonsHint();
			}
		}

		private string GetDungeonsHint()
		{
			string hint = "Going on an adventure? You should check out\n";

			for (int i = 0; i < requiredDungeons.Count; i++)
			{
				if (requiredDungeons.Count > 1 && i == requiredDungeons.Count - 1)
					hint += "and ";

				hint += requiredDungeons[i];

				if (requiredDungeons.Count >= 3 && i < requiredDungeons.Count - 1)
					hint += ", ";

				if (i > 0 && i % 3 == 2 && i < requiredDungeons.Count - 1)
					hint += "\n";
			}

			hint += ".\nI bet there's some good loot at the end of them.";

			return hint;
		}
	}
}