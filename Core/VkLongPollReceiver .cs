using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VkCoreRuslBot.DataBaseDB;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkCoreRuslBot.Core
{
	internal class VkLongPollReceiver
	{
		private readonly IVkApi _vkApi;
		private readonly MessageProcessor _messageProcessor;
		private readonly ulong _groupId;

		public VkLongPollReceiver(VkApi vkApi, ulong groupId, MessageProcessor processor)
		{
			_vkApi = vkApi;
			_groupId = groupId;
			_messageProcessor = processor;
		}


		public async Task StartLoopAsync()
		{
			LongPollServerResponse serverResponse = _vkApi.Groups.GetLongPollServer(_groupId);
			ulong currentTs = serverResponse.Ts;
			while (true)
			{
				try
				{
					BotsLongPollHistoryResponse pollResponse = _vkApi.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams
					{
						Server = serverResponse.Server,
						Key = serverResponse.Key,
						Ts = currentTs,
						Wait = 25
					});
					if (pollResponse.Updates != null)
					{
						currentTs = pollResponse.Ts; 
					}
					foreach (GroupUpdate update in pollResponse.Updates)
					{
						string eventType = update.Type?.ToString() ?? "";
						var messageNew = update.Instance as MessageNew;
						if (!eventType.Contains("MessageNew", StringComparison.OrdinalIgnoreCase) &&
							!eventType.Contains("message_new", StringComparison.OrdinalIgnoreCase))
						{
							continue;
						}
						if (messageNew != null && messageNew.Message != null)
						{
							string userText = messageNew.Message.Text;
							long userId = messageNew.Message.FromId.Value;
							await _messageProcessor.ProcessMessageAsync(userId, userText);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка: {ex.Message}");
				}
			}
		}
	}
}
