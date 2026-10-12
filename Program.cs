using Microsoft.Extensions.Configuration;
using VkCoreRuslBot.Core;
using VkCoreRuslBot.DataBaseDB;
using VkNet;
using VkNet.Model;
using VkNet.Exception;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace VkCoreRuslBot
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
		    IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false)
				.Build();

			string vkToken = config["VkBotSettings:Token"]!;
			string scopeKey = config["AiSettings:ScopeKey"]!;

			if (vkToken is not { Length: >= 50 } || string.IsNullOrWhiteSpace(vkToken))
				ExitWithError("Ошибка! ВК токен не заполнен или слишком короткий.");

			if (!ulong.TryParse(config["VkBotSettings:GroupId"], out ulong groupId))
				ExitWithError("Ошибка! ID группы должен быть числом!");
			
			if(string.IsNullOrWhiteSpace(scopeKey))
				ExitWithError("Ошибка: API-ключ ИИ (ScopeKey) не заполнен!");
				
			try
			{
				DatabaseManager dbManager = new DatabaseManager();
				AiService aiService = new AiService(scopeKey);
				await aiService.GetAccessTokenAsync();
				VkApi vkApi = new VkApi();
				vkApi.Authorize(new ApiAuthParams { AccessToken = vkToken });
				vkApi.Groups.GetById(new [] { groupId.ToString() }, null, null);
				MessageProcessor processor = new MessageProcessor(dbManager, aiService, vkApi);
				VkLongPollReceiver vkLong = new VkLongPollReceiver(vkApi, groupId, processor);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n[УСПЕХ] Бот инициализирован и запустился");
				Console.ResetColor();
				await vkLong.StartLoopAsync();
			}
			catch(HttpRequestException ex)
			{
				ExitWithError($"\n[ОШИБКА] Не удалось авторизоваться в GigaChat!\n" +
					  $"Возможно, ScopeKey неверный или просрочен.\n" +
					  $"Детали: {ex.Message}", isCritical: true);
			}
			catch (UserAuthorizationFailException ex)
			{
				ExitWithError($"Ошибка авторизации ВК: {ex.Message}", isCritical: true);
			}
			catch(Exception ex)
			{
				ExitWithError($"Непредвиденная ошибка при старте: {ex.Message}", isCritical: true);
			}
		}

		private static void ExitWithError(string message, bool isCritical = false)
		{
			Console.ForegroundColor = isCritical ? ConsoleColor.Red : ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ResetColor();
			Console.ReadKey();
			Environment.Exit(0);
		}
	}
}
