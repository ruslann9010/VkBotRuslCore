using System;
using System.Collections.Generic;
using System.Text;
using VkCoreRuslBot.DataBaseDB;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkCoreRuslBot.Core
{
	internal class MessageProcessor
	{
		private static readonly Random _random = new Random();
		private DatabaseManager _dbManager;
		private AiService _aiService;
		private VkApi _vkApi;

		public MessageProcessor(DatabaseManager dbManager, AiService aiService, VkApi vkApi)
		{
			_dbManager = dbManager;
			_aiService = aiService;
			_vkApi = vkApi;
		}


		public async Task ProcessMessageAsync(long userId, string userText)
		{
			if (userId <= 0 || string.IsNullOrWhiteSpace(userText)) return;
			Console.WriteLine($"[Новое сообщение] От пользователя {userId}: {userText}");
			UserSession session = _dbManager.GetOrCreateSession(userId);
			string historyContext = session.ChatHistory;
			string aiResponse = await _aiService.GenerateResponseAsync(userText, historyContext);
			string updatedHistory = historyContext + $"User: {userText}\nAI: {aiResponse}\n";
			_dbManager.UpdateChatHistory(userId, updatedHistory);
			await _vkApi.Messages.SendAsync(new MessagesSendParams
			{
				RandomId = _random.Next(),
				PeerId = userId,
				Message = aiResponse
			});
			Console.WriteLine($"[Ответ отправлен] Пользователю {userId}");
		}
	}
}
