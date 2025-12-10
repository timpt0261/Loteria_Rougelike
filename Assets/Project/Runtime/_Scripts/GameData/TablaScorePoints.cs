public static class TablaScorePoints
{
	private const int INIT_SINGLE = 1;
	private const int INIT_HORIZONTAL = 4;
	private const int INIT_VERTICAL = 4;
	private const int INIT_DIAGONAL = 8;
	private const int INIT_FULL = 16;

	public static int SingleMultiplier { get; set; } = INIT_SINGLE;
	public static int HorizontalMultiplier { get; set; } = INIT_HORIZONTAL;
	public static int VerticalMultiplier { get; set; } = INIT_VERTICAL;
	public static int DiagonalMultiplier { get; set; } = INIT_DIAGONAL;
	public static int FullMultiplier { get; set; } = INIT_FULL;

	// Call this if you want to reset to default values
	public static void ResetToDefaults()
	{
		SingleMultiplier = INIT_SINGLE;
		HorizontalMultiplier = INIT_HORIZONTAL;
		VerticalMultiplier = INIT_VERTICAL;
		DiagonalMultiplier = INIT_DIAGONAL;
		FullMultiplier = INIT_FULL;
	}
}
