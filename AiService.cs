using System.Text.Json;

namespace VkCoreRuslBot
{
	public class AiService
	{
		private readonly string _scopeKey; // Сюда передается Authorization Key из appsettings.json
		private string _cachedToken = "";  // Поле для хранения временного токена (кэш)
		private DateTime _tokenExpiresAt;  // Время, когда временный токен протухнет

		public AiService(string scopeKey)
		{
			_scopeKey = scopeKey;
		}

		public async Task<string> GetAccessTokenAsync()
		{
			HttpClientHandler handler = null!;
			if (_cachedToken != string.Empty && DateTime.Now < _tokenExpiresAt)
			{
				return _cachedToken;
			}
			handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (s, c, ch, e) => true
			};

			using (HttpClient client = new HttpClient(handler))
			{
				client.DefaultRequestHeaders.Add("Accept", "application/json");
				client.DefaultRequestHeaders.Add("RqUID", Guid.NewGuid().ToString());
				client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_scopeKey}");
		
				var requestData = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
				});
				var response = await client.PostAsync("https://ngw.devices.sberbank.ru:9443/api/v2/oauth", requestData);
				response.EnsureSuccessStatusCode();

				string json = await response.Content.ReadAsStringAsync();
				using (JsonDocument doc = JsonDocument.Parse(json))
				{
					var root = doc.RootElement;
					_cachedToken = root.GetProperty("access_token").GetString()!;
					long expiresAtUnix = root.GetProperty("expires_at").GetInt64();
					_tokenExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(expiresAtUnix).LocalDateTime;
				}
				return _cachedToken;
			}
		}
	}
}
