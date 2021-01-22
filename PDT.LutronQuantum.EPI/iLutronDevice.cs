using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace LutronQuantum
{
	public interface iLutronDevice
	{
		void Initialize();
		void ParseMessage(string message);
	}
}