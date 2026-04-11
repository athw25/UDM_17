using System.Collections.Generic;

namespace Caro.Client.UI
{
	public static class MockData
	{
		public static List<string> Players = new List<string>
		{
			"Alice", "Bob", "Charlie", "David"
		};

		public static List<string> MatchHistory = new List<string>
		{
			"Alice vs Bob - Win",
			"Charlie vs David - Lose",
			"Alice vs Charlie - Win"
		};
	}
}