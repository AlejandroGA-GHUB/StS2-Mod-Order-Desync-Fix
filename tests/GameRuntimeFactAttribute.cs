namespace ModOrderDesyncFix.Tests;

/// <summary>
/// A test that needs the runtime the game itself uses (.NET 9). The Harmony build the game ships
/// refuses to patch on newer runtimes, so on those the test is reported as skipped, not failed.
/// Install the .NET 9 runtime to run it.
/// </summary>
public sealed class GameRuntimeFactAttribute : FactAttribute
{
    private const int GameRuntimeMajor = 9;

    public GameRuntimeFactAttribute()
    {
        if (Environment.Version.Major != GameRuntimeMajor)
        {
            Skip = $"Needs the .NET {GameRuntimeMajor} runtime the game uses; running on {Environment.Version}.";
        }
    }
}
