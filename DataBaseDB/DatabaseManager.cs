
namespace VkCoreRuslBot.DataBaseDB
{
	/// <summary>
	/// Manager class responsible for high-level database operations, decoupling business logic from EF Core context.
	/// </summary>
	public class DatabaseManager
	{
		public DatabaseManager()
		{
			using (BotDbContext db = new BotDbContext())
			{
				db.Database.EnsureCreated();
			}
		}

		/// <summary>
		/// Retrieves an existing user session from the database. If it does not exist, creates and saves a new one.
		/// </summary>
		/// <param name="userId">The unique ID of the user.</param>
		/// <returns>The found or newly created <see cref="UserSession"/> object.</returns>
		public UserSession GetOrCreateSession(long userId)
		{
			using (BotDbContext db = new BotDbContext())
			{
				UserSession user = db.Users.FirstOrDefault(u => u.UserID == userId)!;
				if (user == null)
				{
					UserSession newuser = new UserSession 
					{
						 UserID = userId,
						 CurrentState = "MainPage",
						 ChatHistory = ""
					};
					db.Users.Add(newuser);
					db.SaveChanges();
					return newuser;
				}
				return user;	
			}
		}

		/// <summary>
		/// Updates the chat history context for a specific user and records the interaction timestamp.
		/// </summary>
		/// <param name="userId">The unique ID of the user.</param>
		/// <param name="newHistory">The updated full string of the conversation history.</param>
		public void UpdateChatHistory(long userId, string newHistory)
		{
			using (BotDbContext db = new BotDbContext())
			{
				UserSession user = db.Users.FirstOrDefault(u => u.UserID == userId)!;
				if(user != null)
				{
					user.ChatHistory = newHistory;
					user.LastActivity = DateTime.Now;
					db.SaveChanges();
				}

			}
		}

		/// <summary>
		/// Updates the current navigation state (screen/menu context) for a specific user.
		/// </summary>
		/// <param name="userId">The unique ID of the user.</param>
		/// <param name="newState">The new state name to apply.</param>
		public void UpdateState(long userId, string newState)
		{
			using (BotDbContext db = new BotDbContext())
			{
				UserSession user = db.Users.FirstOrDefault(u => u.UserID == userId)!;
				if (user != null)
				{
					user.CurrentState = newState;
					db.SaveChanges();
				}
			}
		}
	}
}
