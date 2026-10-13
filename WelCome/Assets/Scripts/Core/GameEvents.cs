namespace WelCome.Core
{
    public struct GameStateChangedEvent
    {
        public GameState OldState;
        public GameState NewState;
    }

    public struct DayStartedEvent { public int DayNumber; }
    public struct NightStartedEvent { public int DayNumber; }

    public struct SettlementCompletedEvent
    {
        public int DayNumber;
        public int GoldEarned;
    }

    public struct ActionPointsChangedEvent
    {
        public int Current;
        public int Max;
    }

    public struct MagicPointsChangedEvent
    {
        public int Current;
        public int Max;
    }

    public struct GoldChangedEvent
    {
        public int Current;
        public int Delta;
    }

    public struct MaterialChangedEvent
    {
        public int MaterialId;
        public int NewCount;
        public int Delta;
    }

    public struct EventCompletedEvent { public int EventId; }
    public struct CustomerArrivedEvent { public int CustomerId; }
    public struct DisplayCasesChangedEvent { public int DayNumber; }

    public struct ProductSoldEvent
    {
        public int ProductId;
        public int Price;
        public int CustomerId;
    }

    public struct AttentionChangedEvent
    {
        public int Current;
        public int Delta;
    }

    public struct SupportChangedEvent
    {
        public int Current;
        public int Delta;
    }

    public struct AVGDialogueStartedEvent { public int StoryId; }
    public struct AVGDialogueEndedEvent { public int StoryId; }
}
