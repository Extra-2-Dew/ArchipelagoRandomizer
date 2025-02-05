namespace ArchipelagoRandomizer
{
	public class PlayerActionModifier
	{
		private Entity player;
		private StickDisabler stickDisabler;
		private RollDisabler rollDisabler;

		public PlayerActionModifier()
		{
			Events.OnPlayerSpawn += OnPlayerSpawn;
			Events.OnPlayerRespawn += OnPlayerRespawn;
			ItemRandomizer.OnItemReceived += OnItemReceived;
		}

		private bool DisableStick
		{
			get
			{
				return player.GetStateVariable("melee") < 0;
			}
		}

		private bool DisableRoll
		{
			get
			{
				return player.GetStateVariable("canRoll") < 1;
			}
		}

		private void DoModifiyStick(bool disable)
		{
			Attack attack = player.GetAttack("firesword")._attackSwitcher.GetAttack(player);

			if (disable)
			{
				stickDisabler = new StickDisabler();
				attack.owner.LocalMods.RegisterModifier(stickDisabler);
			}
			else
			{
				attack.owner.LocalMods.UnregisterModifier(stickDisabler);
				stickDisabler = null;
			}
		}

		private void DoModifyRoll(bool disable)
		{
			EntityAction action = player.GetAction("roll");

			if (disable)
			{
				rollDisabler = new RollDisabler();
				action.owner.LocalMods.RegisterModifier(rollDisabler);
			}
			else
			{
				action.owner.LocalMods.UnregisterModifier(rollDisabler);
				rollDisabler = null;
			}
		}

		private void OnPlayerSpawn(Entity player, UnityEngine.GameObject camera, PlayerController controller)
		{
			this.player = player;

			if (DisableStick)
				DoModifiyStick(true);

			if (DisableRoll)
				DoModifyRoll(true);
		}

		private void OnPlayerRespawn()
		{
			if (DisableStick)
				DoModifiyStick(true);

			if (DisableRoll)
				DoModifyRoll(true);
		}

		private void OnItemReceived(ItemHandler.ItemData.Item item, string sentFromPlayerName)
		{
			if (stickDisabler != null && item.Type == ItemHandler.ItemTypes.Melee)
				DoModifiyStick(false);
			else if (rollDisabler != null && item.ItemName == "Roll")
				DoModifyRoll(false);
		}

		public class StickDisabler : EntityDataModifier.IAttackPrevent
		{
			public bool CheckPrevent(Attack attack)
			{
				return attack.name.StartsWith("Melee1");
			}
		}

		public class RollDisabler : EntityDataModifier.IActionPrevent
		{
			public bool CheckPrevent(string actionName)
			{
				return actionName == "roll";
			}
		}
	}
}