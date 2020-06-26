﻿using System;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes
using System.Text;
using System.Collections.Generic;
using System.Linq;


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
    public class LutronQuantum : LightingBase, ILightingScenes, ILightingMasterRaiseLower, ICommunicationMonitor, IBridge
	{
		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("LutronQuantum", LutronQuantum.BuildDevice);	
		}

		public static LutronQuantum BuildDevice(DeviceConfig dc)
		{
			Debug.Console(2, "LutronQuantum config is null: {0}", dc == null);
			var comm = CommFactory.CreateCommForDevice(dc);
			Debug.Console(2, "LutronQuantum comm is null: {0}", comm == null);
			var config = JsonConvert.DeserializeObject<LutronQuantumConfigObject>(dc.Properties.ToString());

			var newMe = new LutronQuantum(dc, comm, config);
			return newMe;
		}

        readonly DeviceConfig _config;

		public IBasicCommunication Communication { get; private set; }
        public CommunicationGather PortGather { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }

        CTimer SubscribeAfterLogin;

        public string IntegrationId;
        public string ShadeGroup1Id;
        public string ShadeGroup2Id;
        string Username;
        string Password;

        const string Delimiter = "\x0d\x0a";
        const string Set = "#";
        const string Get = "?";

		public LutronQuantum(DeviceConfig config, IBasicCommunication comm, LutronQuantumConfigObject props)
            : base(config.Key, config.Name)
        {
            _config = config;
            Communication = comm;

            IntegrationId = props.IntegrationId;
            ShadeGroup1Id = props.ShadeGroup1Id;
            ShadeGroup2Id = props.ShadeGroup2Id;

			if (props.Control.Method != eControlMethod.Com)
			{

				Username = props.Control.TcpSshProperties.Username;
				Password = props.Control.TcpSshProperties.Password;
			}

            LightingScenes = props.Scenes;

            var socket = comm as ISocketStatus;
            if (socket != null)
            {
                // IP Control
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }
            else
            {
                // RS-232 Control
            }

            Communication.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(Communication_TextReceived);

            PortGather = new CommunicationGather(Communication, Delimiter);
            PortGather.LineReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(PortGather_LineReceived);

            if (props.CommunicationMonitorProperties != null)
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, props.CommunicationMonitorProperties);
            }
            else
            {
                CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 120000, 120000, 300000, "?ETHERNET,0\x0d\x0a");
            }
        }

        public override bool CustomActivate()
        {
            Communication.Connect();
            CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
            CommunicationMonitor.Start();

            return true;
        }

        void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            Debug.Console(2, this, "Socket Status Change: {0}", e.Client.ClientStatus.ToString());

            if (e.Client.IsConnected)
            {
                // Tasks on connect
            }
        }

        /// <summary>
        /// Checks for responses that do not contain the delimiter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Communication_TextReceived(object sender, GenericCommMethodReceiveTextArgs args)
        {
            Debug.Console(2, this, "Text Received: '{0}'", args.Text);

            if (args.Text.Contains("login:"))
            {
                // Login
                SendLine(Username);
            }
            else if (args.Text.Contains("password:"))
            {
                // Login
                SendLine(Password);
                SubscribeAfterLogin = new CTimer(x => SubscribeToFeedback(), null, 5000);

            }
            else if (args.Text.Contains("Access Granted"))
            {
                if (SubscribeAfterLogin != null)
                {
                    SubscribeAfterLogin.Stop();
                }
                SubscribeToFeedback();
            }
        }

        /// <summary>
        /// Handles all responses that contain the delimiter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void PortGather_LineReceived(object sender, GenericCommMethodReceiveTextArgs args)
        {
            Debug.Console(2, this, "Line Received: '{0}'", args.Text);

            try
            {
                if (args.Text.Contains("~AREA"))
                {
                    var response = args.Text.Split(',');

                    var integrationId = response[1];

                    if (integrationId != IntegrationId)
                    {
                        Debug.Console(2, this, "Response is not for correct Integration ID");
                        return;
                    }
                    else
                    {
                        var action = Int32.Parse(response[2]);

                        switch (action)
                        {
                            case (int)eAction.Scene:
                                {
                                    var scene = response[3];
                                    CurrentLightingScene = LightingScenes.FirstOrDefault(s => s.ID.Equals(scene));

                                    OnLightingSceneChange();

                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Console(2, this, "Error parsing response:\n{0}", e);
            }
        }

        /// <summary>
        /// Subscribes to feedback
        /// </summary>
        public void SubscribeToFeedback()
        {
            Debug.Console(1, "Sending Monitoring Subscriptions");
            SendLine("#MONITORING,6,1");
            SendLine("#MONITORING,8,1");
            SendLine("#MONITORING,5,2");
        }

        /// <summary>
        /// Recalls the specified scene
        /// </summary>
        /// <param name="scene"></param>
		/// 

        public override void SelectScene(LightingScene scene)
        {
            Debug.Console(1, this, "Selecting Scene: '{0}'", scene.Name);
            SendLine(string.Format("{0}AREA,{1},{2},{3}", Set, IntegrationId, (int)eAction.Scene, scene.ID));
        }

        /// <summary>
        /// Begins raising the lights in the area
        /// </summary>
        public void MasterRaise()
        {
            SendLine(string.Format("{0}AREA,{1},{2}", Set, IntegrationId, (int)eAction.Raise));
        }

        /// <summary>
        /// Begins lowering the lights in the area
        /// </summary>
        public void MasterLower()
        {
            SendLine(string.Format("{0}AREA,{1},{2}", Set, IntegrationId, (int)eAction.Lower));
        }

        /// <summary>
        /// Stops the current raise/lower action
        /// </summary>
        public void MasterRaiseLowerStop()
        {
            SendLine(string.Format("{0}AREA,{1},{2}", Set, IntegrationId, (int)eAction.Stop));
        }

        /// <summary>
        /// Begins raising the shades in the group
        /// </summary>
        public void ShadeGroupRaise(string ShadeGroupId)
        {
            SendLine(string.Format("{0}SHADEGRP,{1},{2}", Set, ShadeGroupId, (int)eAction.Raise));
        }

        /// <summary>
        /// Begins lowering the shades in the group
        /// </summary>
        public void ShadeGroupLower(string ShadeGroupId)
        {
            SendLine(string.Format("{0}SHADEGRP,{1},{2}", Set, ShadeGroupId, (int)eAction.Lower));
        }

        public void SetIntegrationId(string id)
        {
            if (String.IsNullOrEmpty(id))
                return;

            var props = JsonConvert.DeserializeObject<LutronQuantumConfigObject>(_config.Properties.ToString());

            if (props.IntegrationId.Equals(id))
                return;

            props.IntegrationId = id;
            IntegrationId = id;

            _config.Properties = JsonConvert.SerializeObject(props);
            ConfigWriter.UpdateDeviceConfig(_config);
        }

        public void SetShadeGroup1Id(string id)
        {
            if (String.IsNullOrEmpty(id))
                return;

            var props = JsonConvert.DeserializeObject<LutronQuantumConfigObject>(_config.Properties.ToString());

            if (props.ShadeGroup1Id.Equals(id))
                return;

            props.ShadeGroup1Id = id;
            ShadeGroup1Id = id;

            _config.Properties = JsonConvert.SerializeObject(props);
            ConfigWriter.UpdateDeviceConfig(_config);
        }

        public void SetShadeGroup2Id(string id)
        {
            if (String.IsNullOrEmpty(id))
                return;

            var props = JsonConvert.DeserializeObject<LutronQuantumConfigObject>(_config.Properties.ToString());

            if (props.ShadeGroup2Id.Equals(id))
                return;

            props.ShadeGroup2Id = id;
            ShadeGroup2Id = id;

            _config.Properties = JsonConvert.SerializeObject(props);
            ConfigWriter.UpdateDeviceConfig(_config);
        }


        /// <summary>
        /// Appends the delimiter and sends the string
        /// </summary>
        /// <param name="s"></param>
        public void SendLine(string s)
        {
            Debug.Console(2, this, "TX: '{0}'", s);
            Communication.SendText(s + Delimiter);
        }
        #region IBridge Members

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }

        #endregion

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, PepperDash.Essentials.Core.Bridges.EiscApiAdvanced bridge)
        {
            this.LinkToApiExt(trilist, joinStart, joinMapKey);
        }
    }

    public enum eAction : int
    {
        SetLevel = 1,
        Raise = 2,
        Lower = 3,
        Stop = 4,
        Scene = 6,
        DaylightMode = 7,
        OccupancyState = 8,
        OccupancyMode = 9,
        OccupiedLevelOrScene = 12,
        UnoccupiedLevelOrScene = 13,
        HyperionShaddowSensorOverrideState = 26,
        HyperionBrightnessSensorOverrideStatue = 27
    }
}