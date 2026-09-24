using Microsoft.Extensions.Configuration;
using VkCoreRuslBot.DataBaseDB;
using VkNet;
using VkNet.Model;


namespace VkCoreRuslBot
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false)
				.Build();

				string vkToken = config["VkBotSettings:Token"]!;
				ulong groupId = ulong.Parse(config["VkBotSettings:GroupId"]!);
				string scopeKey = config["AiSettings:ScopeKey"]!;

				DatabaseManager dbManager = new DatabaseManager();
				AiService aiService = new AiService(scopeKey);
				VkApi vkApi = new VkApi();
				vkApi.Authorize(new ApiAuthParams { AccessToken = vkToken });

				while (true)
				{
					try
					{
						var serverResponse = vkApi.Groups.GetLongPollServer(groupId);
						var pollResponse = vkApi.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams
						{
							Server = serverResponse.Server,
							Key = serverResponse.Key,
							Ts = serverResponse.Ts,
							Wait = 25
						});
						if (pollResponse.Updates == null) 
							continue;
						long userId = 0;
						string userText = "";
						foreach (var update in pollResponse.Updates)
						{
							string eventType = update.Type?.ToString() ?? "";
							var messageNew = update.Instance as MessageNew;
							if (messageNew != null && messageNew.Message != null)
							{
								userText = messageNew.Message.Text;
								userId = messageNew.Message.FromId.Value;
							}
							if (!eventType.Contains("MessageNew", StringComparison.OrdinalIgnoreCase) &&
								!eventType.Contains("message_new", StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}	
							if (userId <= 0 || string.IsNullOrWhiteSpace(userText)) continue;
							Console.WriteLine($"[Новое сообщение] От пользователя {userId}: {userText}");	
							UserSession session = dbManager.GetOrCreateSession(userId);
							string historyContext = session.ChatHistory;
							string aiResponse = await aiService.GenerateResponseAsync(userText, historyContext);
							string updatedHistory = historyContext + $"User: {userText}\nAI: {aiResponse}\n";
							dbManager.UpdateChatHistory(userId, updatedHistory);
							vkApi.Messages.Send(new MessagesSendParams
							{
								RandomId = new Random().Next(), 
								PeerId = userId,                
								Message = aiResponse            
							});
							Console.WriteLine($"[Ответ отправлен] Пользователю {userId}");
						}
					}
					catch(Exception ex)
					{	
						Console.WriteLine($"Ошибка при получении обновлений ВК:{ex.Message + " \r\n " + ex.ToString() }  ");
						await Task.Delay(3000); 
					}
				}
			}
			catch (Exception criticalEx)
			{ 
				Console.WriteLine($"Критическая ошибка при старте приложения: {criticalEx.Message}");
			}
		}
	}
}
