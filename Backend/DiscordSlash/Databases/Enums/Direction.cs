namespace DexterSlash.Databases.Enums
{
    /// <summary>
    /// Indicates how to interpret the direction attached to an effect.
    /// </summary>

    [Flags]
	public enum Direction
	{
		/// <summary>
		/// Neither user is affected
		/// </summary>
		None,

		/// <summary>
		/// Sender is affected
		/// </summary>
		Sender,

		/// <summary>
		/// Sendee is affected
		/// </summary>
		Sendee,

		/// <summary>
		/// Both users are affected
		/// </summary>
		Both
	}
}
