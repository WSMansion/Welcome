namespace WelCome.Core
{
    public enum GameState
    {
        None,
        MainMenu,
        DayPhase,
        DayToNightTransition,
        NightPhase,
        NightSettlement,
        GameOver
    }

    public enum TimePhase
    {
        Dawn,
        Day,
        Dusk,
        Night
    }
}
