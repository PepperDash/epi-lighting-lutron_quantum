using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Devices;
using PepperDash.Core;
using PepperDash.Essentials.Bridges;
using PepperDash.Essentials.Core;
using Crestron.SimplSharpPro.DeviceSupport;


namespace LutronQuantum
{
	public class LutronQseIO : Device, iLutronDevice, IBridge
    {
		private LutronQuantum LutronDevice;
		LutronQseIOConfigObject _Properties; 
		public Dictionary<string, BoolWithFeedback> Feedbacks = new Dictionary<string, BoolWithFeedback>();

		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("LutronQseIO", LutronQseIO.BuildDevice);
		}

		public static LutronQseIO BuildDevice(DeviceConfig dc)
		{
			var config = JsonConvert.DeserializeObject<LutronQseIOConfigObject>(dc.Properties.ToString());

			var newMe = new LutronQseIO(dc, config);
			return newMe;
		}

		public LutronQseIO(DeviceConfig config, LutronQseIOConfigObject props)
			: base(config.Key, config.Name)
		{
			_Properties = props;
			Feedbacks.Add("1", new BoolWithFeedback());
			Feedbacks.Add("2", new BoolWithFeedback());
			Feedbacks.Add("3", new BoolWithFeedback());
			Feedbacks.Add("4", new BoolWithFeedback());
			Feedbacks.Add("5", new BoolWithFeedback());

		}
		public override bool CustomActivate()
		{
			LutronDevice = DeviceManager.GetDeviceForKey(_Properties.LightingDeviceKey) as LutronQuantum;
			if (LutronDevice == null)
			{
				Debug.Console(0, this, "LutronQuantum device {0} does not exist", _Properties.LightingDeviceKey);
			}
			else
			{
				LutronDevice.AddDevice(_Properties.IntegrationId, this);

			}
			AddPostActivationAction(() =>
			{
				Initialize();
			});
			
			
			return true;
		}	
		public void Initialize()
		{
			LutronDevice.SendLine("#MONITORING,2,1");
			LutronDevice.SendLine(string.Format("~DEVICE,{0},1", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("~DEVICE,{0},2", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("~DEVICE,{0},3", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("~DEVICE,{0},4", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("~DEVICE,{0},5", _Properties.IntegrationId));
		}
		public void ParseMessage(string[] message)
		{
			Debug.Console(2, "QseIO Got Message {0} {1} {2} {3} ", message[0],message[1],message[2], message[3]);
			BoolWithFeedback fb;
			if (Feedbacks.TryGetValue(message[2], out fb))
			{
				if (message[3] == "3")
				{
					fb.Value = false; 
				}
				if (message[3] == "4")
				{
					fb.Value = true;
				}
			}
		}


        #region IBridge Members

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }

        #endregion



	}
}