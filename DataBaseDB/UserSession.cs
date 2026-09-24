using System;
using System.ComponentModel.DataAnnotations;



namespace VkCoreRuslBot.DataBaseDB
{
	/// <summary>
	/// Represents a user session entity for tracking state and history in the database.
	/// </summary>
	public class UserSession
	{
		/// <summary>
		/// Gets or sets the unique identifier of the user (Primary Key).
		/// </summary>
		[Key]
		public long UserID { get; set; }
		/// <summary>
		/// Gets or sets the current state of the user in the bot's navigation flow (e.g., "MainPage").
		/// </summary>
		public string CurrentState { get; set; } = "MainPage";
		/// <summary>
		/// Gets or sets the cumulative text log of the conversational history between the user and the AI.
		/// </summary>
		public string ChatHistory { get; set; } = "";
		/// <summary>
		/// Gets or sets the timestamp of the user's last interaction with the bot.
		/// </summary>
		public DateTime LastActivity { get; set; } = DateTime.Now;
		/// <summary>
		/// Gets or sets the total number of interactions performed by the user.
		/// </summary>
		public int InteractionCount { get; set; } = 0;
	}
}
