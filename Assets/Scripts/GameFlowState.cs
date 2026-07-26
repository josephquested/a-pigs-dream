public static class GameFlowState
{
    static bool shouldPlayTutorialOnNextGameLoad;
    static bool isPigPaused;

    public static void MarkMenuToGameTransition()
    {
        shouldPlayTutorialOnNextGameLoad = true;
    }

    public static void MarkGameToGameReload()
    {
        shouldPlayTutorialOnNextGameLoad = false;
    }

    public static bool ConsumeShouldPlayTutorialOnGameLoad()
    {
        bool shouldPlay = shouldPlayTutorialOnNextGameLoad;
        shouldPlayTutorialOnNextGameLoad = false;
        return shouldPlay;
    }

    public static void SetPigPaused(bool paused)
    {
        isPigPaused = paused;
    }

    public static bool IsPigPaused()
    {
        return isPigPaused;
    }
}