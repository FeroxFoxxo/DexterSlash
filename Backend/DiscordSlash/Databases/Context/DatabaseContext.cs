using DexterSlash.Databases.Models;
using Microsoft.EntityFrameworkCore;

namespace DexterSlash.Databases.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

		/// <summary>
		/// Holds every individual event that has been suggested into the system.
		/// </summary>

		public DbSet<CommunityEvent> Events { get; set; }

		/// <summary>
		/// The set of all managed cooldowns in the corresponding database.
		/// </summary>

		public DbSet<Cooldown> Cooldowns { get; set; }

		/// <summary>
		/// A table of the custom commands in the CustomCommandDB database.
		/// </summary>

		public DbSet<CustomCommand> CustomCommands { get; set; }

		/// <summary>
		/// The set of all managed event timers in the corresponding database.
		/// </summary>

		public DbSet<EventTimer> EventTimers { get; set; }

		/// <summary>
		/// The set of final warns stored in the database, accessed by the ID of the user who has been warned.
		/// </summary>

		public DbSet<FinalWarn> FinalWarns { get; set; }

		/// <summary>
		/// A table of the topics in the FunTopicsDB database.
		/// </summary>

		public DbSet<FunTopic> Topics { get; set; }

		/// <summary>
		/// Holds game-specific data, a set of GameInstances (or sessions).
		/// </summary>

		public DbSet<GameInstance> Games { get; set; }

		/// <summary>
		/// Holds player-specific data, like what game they're playing or their score.
		/// </summary>

		public DbSet<Player> Players { get; set; }

		/// <summary>
		/// The set of all records for days GreetFurs have been active.
		/// </summary>

		public DbSet<GreetFurRecord> Records { get; set; }

		/// <summary>
		/// A table of the warnings issued in the InfractionsDB database.
		/// </summary>

		public DbSet<Infraction> Infractions { get; set; }

		/// <summary>
		/// The set of Dexter Profiles in the corresponding database.
		/// </summary>

		public DbSet<DexterProfile> DexterProfiles { get; set; }

		/// <summary>
		/// The data structure containing information for user XP.
		/// </summary>

		public DbSet<UserLevel> Levels { get; set; }

		/// <summary>
		/// Stores a collection of user preferences for rank card display.
		/// </summary>

		public DbSet<LevelPreferences> Prefs { get; set; }

		/// <summary>
		/// A table of the sent modmail messages.
		/// </summary>

		public DbSet<ModMail> ModMail { get; set; }

		/// <summary>
		/// A table of the proposals issued in the SuggestionDB database.
		/// </summary>

		public DbSet<Proposal> Proposals { get; set; }

		/// <summary>
		/// A table of suggestions for voting in the SuggestionDB database.
		/// </summary>

		public DbSet<Suggestion> Suggestions { get; set; }

		/// <summary>
		/// A table of confirmations for admin approval in the SuggestionDB database.
		/// </summary>

		public DbSet<AdminConfirmation> AdminConfirmations { get; set; }

		/// <summary>
		/// A table of relays that will repeat every x amount of messages.
		/// </summary>

		public DbSet<Relay> Relays { get; set; }

		/// <summary>
		/// Holds all individual reminders and their related information for processing.
		/// </summary>

		public DbSet<Reminder> Reminders { get; set; }

		/// <summary>
		/// A table of the borkday times in the BorkdayDB database.
		/// </summary>

		public DbSet<UserProfile> Profiles { get; set; }

		/// <summary>
		/// Holds all recorded nickname and username changes.
		/// </summary>

		public DbSet<NameRecord> Names { get; set; }

		/// <summary>
		/// Holds all information related to user interactions.
		/// </summary>

		public DbSet<UserLink> Links { get; set; }

		/// <summary>
		/// A collection of user-specific restrictions.
		/// </summary>

		public DbSet<UserRestriction> UserRestrictions { get; set; }


		public DbSet<GuildConfig> GuildConfigs { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<ulong[]>().HaveConversion<ULAConverter>();
        }

        public async Task<bool> CanConnectAsync()
        {
            return await Database.CanConnectAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ULAComparer ulongArrayComparer = new();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                    if (property.ClrType == typeof(ulong[]))
                        property.SetValueComparer(ulongArrayComparer);
        }
    }

}
