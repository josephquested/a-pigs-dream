public static class GameFlowState
{
    static bool shouldPlayTutorialOnNextGameLoad;

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
}