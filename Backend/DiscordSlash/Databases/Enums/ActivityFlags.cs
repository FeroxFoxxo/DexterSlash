namespace DexterSlash.Databases.Enums
{
    /// <summary>
    /// Marks the kind of activity that a GreetFur has completed for a given day.
    /// </summary>

    [Flags]
	public enum ActivityFlags
	{
		/// <summary>
		/// Marks standard activity
		/// </summary>
		None = 0,

		/// <summary>
		/// Marks that the user has muted another user during this record's time.
		/// </summary>
		MutedUser = 1,

		/// <summary>
		/// Marks that the user is exempt from participating during the record's time.
		/// </summary>
		Exempt = 2
	}
}
