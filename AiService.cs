using System.Text;
using System.Text.Json;


namespace VkCoreRuslBot
{
	/// <summary>
	/// Provides services for interacting with the GigaChat AI API, 
	/// handling authentication token caching and text generation.
	/// </summary>
	public class AiService
	{
		/// <summary>
		/// The authorization credentials key loaded from application settings.
		/// </summary>
		private readonly string _scopeKey;
		/// <summary>
		/// Cached access token used to authenticate API requests.
		/// </summary>
		private string _cachedToken = "";
		/// <summary>
		/// Expiration date and time for the cached authentication token.
		/// </summary>
		private DateTime _tokenExpiresAt;

		/// <summary>
		/// Initializes a new instance of the <see cref="AiService"/> class.
		/// </summary>
		/// <param name="scopeKey">The Sber OAuth authorization key (Client Secret/Credentials).</param>
		public AiService(string scopeKey)
		{
			_scopeKey = scopeKey;
		}

		/// <summary>
		/// Retrieves a valid access token, utilizing a cached token if it has not expired.
		/// </summary>
		/// <returns>A task that represents the asynchronous operation. The task result contains the active access token string.</returns>
		private async Task<string> GetAccessTokenAsync()
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



		/// <summary>
		/// Generates a response from the AI model based on the user's message and chat history.
		/// </summary>
		/// <param name="userMessage">The new text message received from the VK user.</param>
		/// <param name="historyContext">The previous conversation history retrieved from the database.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the generated text response from the AI.</returns>
		public async Task<string> GenerateResponseAsync(string userMessage, string historyContext)
		{
			string token = await GetAccessTokenAsync();
			HttpClientHandler handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (s, c, ch, e) => true };
			using (HttpClient client = new HttpClient(handler))
			{
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
				string fullPrompt = $"История: {historyContext}\nНовое сообщение: {userMessage}";

				var requestBody = new {
					model = "GigaChat-3-Ultra",
					messages = new[] {
					 new { role = "system", content = "Ты ИИ-ассистент в группе ВК." },
					 new { role = "user", content = fullPrompt }
				 },
					temperature = 0.7 };
			
				string jsonPayload = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
				var response = await client.PostAsync("https://api.giga.chat/v1/chat/completions", content);
				response.EnsureSuccessStatusCode();
				string responseString = await response.Content.ReadAsStringAsync();
				using (JsonDocument doc = JsonDocument.Parse(responseString))
				{
					JsonElement root = doc.RootElement;
					string generatedText = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;
					return generatedText ?? string.Empty;
				}
			}
		}
	}
}
