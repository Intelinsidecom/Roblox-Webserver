using System;

namespace Roblox.Games.Relay
{
	public class Dispatch
	{
		public Guid GameId { get; set; }
		public int ExpirationInSeconds { get; set; }
		public string ScriptName { get; set; }
		public string Script { get; set; }
		public object[] Arguments { get; set; }
	}
}
