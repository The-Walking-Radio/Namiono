using System;

namespace Namiono
{
	class Program
	{
		static void Main(string[] args)
		{
			var namiono = new Namiono();
			namiono.Start();

			var x = "exit";
			while (x != "exit!")
				x = Console.ReadLine();

			namiono.Close();
		}
	}
}
