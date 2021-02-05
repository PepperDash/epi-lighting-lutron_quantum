using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Lighting;

namespace LutronQuantum
{
	public class LutronQseIOConfigObject
	{
		public string IntegrationId { get; set; }
		public string LightingDeviceKey { get; set; } 

	}
}