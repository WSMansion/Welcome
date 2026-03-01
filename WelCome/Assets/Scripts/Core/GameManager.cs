using System.Collections;
using UnityEngine;

namespace WelCome.Core
{
    public class GameManager : Singleton<GameManager>
    {
        public GameState CurrentState { get; private set; } = GameState.None;
        public int CurrentDay { get; private set; } = 1;

        protected override void OnInit()
        {
            Application.targetFrameRate = 60;
        }

        public void StartGame()
        {
            CurrentDay = 1;
            Systems.ResourceSystem.Instance.ResetAll();
            Systems.DayEventSystem.Instance.ClearAll();
            Systems.WorkshopSystem.Instance.ClearInventory();
            ChangeState(GameState.DayPhase);
        }

        public void LoadGame(int day)
        {
            CurrentDay = day;
            ChangeState(GameState.DayPhase);
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            var oldState = CurrentState;
            OnExitState(oldState);
            CurrentState = newState;
            OnEnterState(newState);

            EventBus.Publish(new GameStateChangedEvent { OldState = oldState, NewState = newState });
        }

        private void OnExitState(GameState state)
        {
            switch (state)
            {
                case GameState.DayPhase:
                    break;
                case GameState.NightPhase:
                    break;
            }
        }

        private void OnEnterState(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    break;

                case GameState.DayPhase:
                    Systems.TimeSystem.Instance.StartDay(CurrentDay);
                    EventBus.Publish(new DayStartedEvent { DayNumber = CurrentDay });
                    break;

                case GameState.DayToNightTransition:
                    StartCoroutine(DelayedStateChange(GameState.NightPhase));
                    break;

                case GameState.NightPhase:
                    Systems.TimeSystem.Instance.StartNight(CurrentDay);
                    EventBus.Publish(new NightStartedEvent { DayNumber = CurrentDay });
                    break;

                case GameState.NightSettlement:
                    break;

                case GameState.GameOver:
                    break;
            }
        }

        public void EndDayPhase()
        {
            if (CurrentState == GameState.DayPhase)
                ChangeState(GameState.DayToNightTransition);
        }

        public void EndNightPhase()
        {
            if (CurrentState == GameState.NightPhase)
                ChangeState(GameState.NightSettlement);
        }

        public void AdvanceToNextDay()
        {
            CurrentDay++;
            ChangeState(GameState.DayPhase);
        }

        private IEnumerator DelayedStateChange(GameState target)
        {
            yield return null;
            ChangeState(target);
        }
    }
}
