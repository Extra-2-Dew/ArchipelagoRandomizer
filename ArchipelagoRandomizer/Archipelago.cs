using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;

namespace ID2.ArchipelagoRandomizer;

class Archipelago
{
	private static readonly Archipelago instance = new();
	private bool isConnected;

	public static Archipelago Instance => instance;
	public ArchipelagoSession Session { get; private set; }

	private Archipelago() { }

	public LoginSuccessful Connect(APSaveData apSaveData)
	{
		Session = ArchipelagoSessionFactory.CreateSession(apSaveData.URL, apSaveData.Port);
		string message;

		LoginResult loginResult = Session.TryConnectAndLogin(
			"Ittle Dew 2",
			apSaveData.SlotName,
			ItemsHandlingFlags.AllItems,
			password: apSaveData.Password,
			requestSlotData: true
		);

		switch (loginResult)
		{
			case LoginFailure failure:
				string errors = string.Join(", ", failure.Errors);
				message = $"Failed to connect to Archipelago: {errors}";
				throw new LoginValidationException(message);
			case LoginSuccessful success:
				Logger.Log($"Successfully connected to Archipelago at {apSaveData.URL}:{apSaveData.Port} as {apSaveData.SlotName} on team {success.Team}. Have fun!");
				return success;
			default:
				message = $"Unexpected LoginResult type when connecting to Archipelago: {loginResult}";
				throw new LoginValidationException(message);
		}
	}

	public void Disconnect()
	{
		if (!isConnected)
		{
			return;
		}

		Session.Socket.Disconnect();
	}

	public APSaveData GetAPSaveData()
	{
		return null;
	}

	public class APSaveData
	{
		public string URL { get; set; }
		public int Port { get; set; }
		public string SlotName { get; set; }
		public string Password { get; set; }
	}
}