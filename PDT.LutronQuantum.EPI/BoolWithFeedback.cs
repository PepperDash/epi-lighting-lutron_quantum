using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;
using PepperDash.Core;

namespace LutronQuantum
{
	public class BoolWithFeedback
	{
		public BoolFeedback Feedback;
		private bool _Value;
		public bool Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				Feedback.FireUpdate();
				
			}
		}
		public BoolWithFeedback()
		{

			Feedback = new BoolFeedback(() => { 
				return Value;
			});
		}
	}
}