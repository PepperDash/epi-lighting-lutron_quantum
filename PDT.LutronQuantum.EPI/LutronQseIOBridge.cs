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
    public static class LutronQseIOBridge
    {

        public static void LinkToApiExt(this LutronQseIO device, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
			LutronQseIOJoinMap joinMap = new LutronQseIOJoinMap();
			joinMap.OffsetJoinNumbers(joinStart);
			var x = 0;
			foreach (var cc in device.Feedbacks)
			{
	
				Debug.Console(2, "Linking {0} {1}", cc.Key, joinMap.ContactClosureStart + x);
				cc.Value.Feedback.LinkInputSig(trilist.BooleanInput[(uint)(joinMap.ContactClosureStart + x)]);
				x++;
			}

        }

        public class LutronQseIOJoinMap : JoinMapBase
        {
            public uint ContactClosureStart { get; set; }

			public LutronQseIOJoinMap()
            {
                // Digital
				ContactClosureStart = 1;


                // Analog

                // Serial

            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;
				ContactClosureStart = ContactClosureStart + joinOffset;
            }
        }
    }
}