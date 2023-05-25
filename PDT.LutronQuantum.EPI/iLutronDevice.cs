
namespace LutronQuantum
{
	public interface iLutronDevice
	{
		void Initialize();
		void ParseMessage(string[] message);
	}
}