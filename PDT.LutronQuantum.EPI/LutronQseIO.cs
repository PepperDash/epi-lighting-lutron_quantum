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
using Crestron.SimplSharpPro.DeviceSupport;


namespace LutronQuantum
{
	public class LutronQseIO : Device, iLutronDevice, IBridge
    {
		private LutronQuantum LutronDevice;
		LutronQseIOConfigObject _Properties; 

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

			return true;
		}
		public void Initialize()
		{
			LutronDevice.SendLine(string.Format("#DEVICE,{0},1,3", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("#DEVICE,{0},1,4", _Properties.IntegrationId));

			LutronDevice.SendLine(string.Format("#DEVICE,{0},2,3", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("#DEVICE,{0},2,4", _Properties.IntegrationId));

			LutronDevice.SendLine(string.Format("#DEVICE,{0},3,3", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("#DEVICE,{0},3,4", _Properties.IntegrationId));

			LutronDevice.SendLine(string.Format("#DEVICE,{0},4,3", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("#DEVICE,{0},4,4", _Properties.IntegrationId));

			LutronDevice.SendLine(string.Format("#DEVICE,{0},5,3", _Properties.IntegrationId));
			LutronDevice.SendLine(string.Format("#DEVICE,{0},5,4", _Properties.IntegrationId));

		}
		public void ParseMessage(string message)
		{

		}


		#region IBridge Members

		public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}