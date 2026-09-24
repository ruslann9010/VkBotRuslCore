using Microsoft.EntityFrameworkCore;



namespace VkCoreRuslBot.DataBaseDB
{
	/// <summary>
	/// Database context for the bot, handling entity mapping and database connections.
	/// </summary>
	internal class BotDbContext : DbContext
	{
		/// <summary>
		/// Gets or sets the database set for managing user sessions.
		/// </summary>
		public DbSet<UserSession> Users { get; set; }
		/// <summary>
		/// Configures the database connection context, specifying the SQLite provider and database file path.
		/// </summary>
		/// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=bot.db");
		}
	}
}
