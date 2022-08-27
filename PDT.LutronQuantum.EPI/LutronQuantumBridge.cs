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

        public static void LinkToApiExt(this LutronQuantum lightingDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            LutronQuantumJoinMap joinMap = new LutronQuantumJoinMap();

            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);
            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<LutronQuantumJoinMap>(joinMapSerialized);
            joinMap.OffsetJoinNumbers(joinStart);

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));
            Debug.Console(0, "Linking to Lighting Type {0}", lightingDevice.GetType().Name.ToString());

            trilist.SetStringSigAction(joinMap.IntegrationIdSet, id => lightingDevice.SetIntegrationId(id));
            trilist.SetStringSigAction(joinMap.ShadeGroup1IdSet, id => lightingDevice.SetShadeGroup1Id(id));
            trilist.SetStringSigAction(joinMap.ShadeGroup2IdSet, id => lightingDevice.SetShadeGroup2Id(id));
			trilist.SetStringSigAction(joinMap.Commands, (s) => lightingDevice.SendLine(s));

            // GenericLighitng Actions & FeedBack
            trilist.SetUShortSigAction(joinMap.SelectScene, u => 
				{
					if(u <= lightingDevice.LightingScenes.Count)
					{
						lightingDevice.SelectScene(lightingDevice.LightingScenes[u]);
					}
				});
			trilist.SetBoolSigAction(joinMap.Raise, b =>
				{
					if (b)
					{
						lightingDevice.MasterRaise();
					}
					else
					{
						lightingDevice.MasterRaiseLowerStop();
					}
				});
			trilist.SetBoolSigAction(joinMap.Lower, b =>
			{
				if (b)
				{
					lightingDevice.MasterLower();
				}
				else
				{
					lightingDevice.MasterRaiseLowerStop();
				}
			});
            int sceneIndex = 1;
            foreach (var scene in lightingDevice.LightingScenes)
            {
                var tempIndex = sceneIndex - 1;
                trilist.SetSigTrueAction((uint)(joinMap.LightingSceneOffset + sceneIndex), () => lightingDevice.SelectScene(lightingDevice.LightingScenes[tempIndex]));
                scene.IsActiveFeedback.LinkInputSig(trilist.BooleanInput[(uint)(joinMap.LightingSceneOffset + sceneIndex)]);
                trilist.StringInput[(uint)(joinMap.LightingSceneOffset + sceneIndex)].StringValue = scene.Name;
                trilist.BooleanInput[(uint)(joinMap.ButtonVisibilityOffset + sceneIndex)].BoolValue = true;
                sceneIndex++;
            }

            lightingDevice.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
            trilist.SetStringSigAction(joinMap.IntegrationIdSet, s => lightingDevice.IntegrationId = s);
            trilist.SetStringSigAction(joinMap.ShadeGroup1IdSet, s => lightingDevice.ShadeGroup1Id = s);
            trilist.SetStringSigAction(joinMap.ShadeGroup2IdSet, s => lightingDevice.ShadeGroup2Id = s);

            // Shades
            trilist.SetSigTrueAction(joinMap.ShadeGroup1Raise, () =>
            {
                lightingDevice.ShadeGroupRaise(lightingDevice.ShadeGroup1Id);
            });

            trilist.SetSigTrueAction(joinMap.ShadeGroup1Lower, () =>
            {
                lightingDevice.ShadeGroupLower(lightingDevice.ShadeGroup1Id);
            });

            trilist.SetSigTrueAction(joinMap.ShadeGroup2Raise, () =>
            {
                lightingDevice.ShadeGroupRaise(lightingDevice.ShadeGroup2Id);
            });

            trilist.SetSigTrueAction(joinMap.ShadeGroup2Lower, () =>
            {
                lightingDevice.ShadeGroupLower(lightingDevice.ShadeGroup2Id);
            });
        }

        public class LutronQuantumJoinMap : JoinMapBase
        {
            public uint IsOnline { get; set; }
			public uint Raise { get; set; }
			public uint Lower { get; set; }
            public uint SelectScene { get; set; }
            public uint LightingSceneOffset { get; set; }
            public uint ButtonVisibilityOffset { get; set; }
            public uint IntegrationIdSet { get; set; }
            public uint ShadeGroup1IdSet { get; set; }
            public uint ShadeGroup2IdSet { get; set; }
            public uint ShadeGroup1Raise { get; set; }
            public uint ShadeGroup1Lower { get; set; }
            public uint ShadeGroup2Raise { get; set; }
            public uint ShadeGroup2Lower { get; set; }
			public uint Commands { get; set; } 

            public LutronQuantumJoinMap()
            {
                // Digital
                IsOnline = 1;
                SelectScene = 1;
                LightingSceneOffset = 10;
                ButtonVisibilityOffset = 40;
				Raise = 2;
				Lower = 3;
                ShadeGroup1Raise = 60;
                ShadeGroup1Lower = 61;
                ShadeGroup2Raise = 62;
                ShadeGroup2Lower = 63;

                // Analog

                // Serial
                IntegrationIdSet = 1;
                ShadeGroup1IdSet = 2;
                ShadeGroup2IdSet = 3;
				Commands = 4;
            }

            public override void OffsetJoinNumbers(uint joinStart)
            {
                var joinOffset = joinStart - 1;
                IsOnline = IsOnline + joinOffset;
				Raise = Raise + joinOffset;
				Lower = Lower + joinOffset;
                SelectScene = SelectScene + joinOffset;
                LightingSceneOffset = LightingSceneOffset + joinOffset;
                ButtonVisibilityOffset = ButtonVisibilityOffset + joinOffset;
                ShadeGroup1Raise = ShadeGroup1Raise + joinOffset;
                ShadeGroup1Lower = ShadeGroup1Lower + joinOffset;
                ShadeGroup2Raise = ShadeGroup2Raise + joinOffset;
                ShadeGroup2Lower = ShadeGroup2Lower + joinOffset;
                IntegrationIdSet = IntegrationIdSet + joinOffset;
                ShadeGroup1IdSet = ShadeGroup1IdSet + joinOffset;
                ShadeGroup2IdSet = ShadeGroup2IdSet + joinOffset;
				Commands = Commands + joinOffset;
            }
        }
    }
}