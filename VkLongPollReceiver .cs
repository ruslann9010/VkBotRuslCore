using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VkCoreRuslBot.DataBaseDB;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkCoreRuslBot
{
	internal class VkLongPollReceiver
	{
		private readonly IVkApi _vkApi;
		private readonly MessageProcessor _messageProcessor;
		private readonly ulong _groupId;

		// 1. Приватные поля для VkApi, groupId и MessageProcessor

		// 2. Конструктор для их инициализации
		public VkLongPollReceiver(VkApi vkApi, ulong groupId, MessageProcessor processor)
		{
			_vkApi = vkApi;
			_groupId = groupId;
			_messageProcessor = processor;
		}


		public async Task StartLoopAsync()
		{
			// а) Инициализация LongPoll: получаем Server, Key и начальный Ts через _vkApi.Groups.GetBotsLongPollServer

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



				// б) Запускаем while (true)
				//    Внутри него обязательно используем try-catch (как на строках 31 и 80 вашего кода)

				// в) Делаем запрос обновлений истории: _vkApi.Groups.GetBotsLongPollHistoryAsync
				//    ОБЯЗАТЕЛЬНО передавайте туда актуальный Ts!

				// г) Проверяем pollResponse.Updates на null. 
				//    Если не null — ОБНОВЛЯЕМ переменную Ts новым значением из pollResponse.Ts!

				// д) Бежим циклом foreach по pollResponse.Updates:
				//    - Проверяем, что update.Instance — это MessageNew и message != null.
				//    - Вытаскиваем userId и userText.
				//    - Вызываем: await _processor.ProcessMessageAsync(userId, userText);

				// е) В блоке catch обрабатываем ошибки сети и делаем await Task.Delay(3000);
			}
		}
	}
}
