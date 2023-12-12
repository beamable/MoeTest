using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Server;
using UnityEngine;

namespace Beamable.Microservices
{
	[Microservice("GameServer")]
	public class GameServer : Microservice
	{
		[ClientCallable]
		public async Task SetPowerStat(string stat, string value)
		{
			var statsToSend = new Dictionary<string, string>
			{
				{stat, value}
			};
			await Services.Stats.SetStats("public", statsToSend);
			await Services.Stats.SetStats("private", statsToSend);
		}
	}
}
