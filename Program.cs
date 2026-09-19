using Microsoft.Extensions.Configuration;
using VkNet;


namespace VkCoreRuslBot
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var config = new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", optional: false)
		.Build();

		string scopeKey = config["AiSettings:ScopeKey"]!;
		AiService aiservice = new AiService(scopeKey);
		Console.WriteLine(await aiservice.GetAccessTokenAsync());
		Console.ReadKey();


			
		}
	}
}
