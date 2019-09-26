using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;
using Newtonsoft.Json;

namespace LutronQuantum
{
	public static class LutronQuantumTemplateBridge
	{
		public static void LinkToApiExt(this LutronQuantum DspDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{

			LutronQuantumConfigObject joinMap = new LutronQuantumConfigObject();

			var JoinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(JoinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<LutronQuantumConfigObject>(JoinMapSerialized);

		}
	}
	public class EssentialsPluginTemplateBridgeJoinMap : JoinMapBase
	{
		public EssentialsPluginTemplateBridgeJoinMap()
		{
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
		}
	}
}