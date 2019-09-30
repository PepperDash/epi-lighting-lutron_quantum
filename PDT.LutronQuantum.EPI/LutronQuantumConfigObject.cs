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
	public class LutronQuantumConfigObject
	{
		public CommunicationMonitorConfig CommunicationMonitorProperties { get; set; }
		public ControlPropertiesConfig Control { get; set; }

		public string IntegrationId { get; set; }
        public string ShadeGroup1Id { get; set; }
        public string ShadeGroup2Id { get; set; }
		public List<LightingScene> Scenes { get; set; }

		// Moved to use existing properties in Control object
		// public string Username { get; set; } 
		// public string Password { get; set; }
	}
}