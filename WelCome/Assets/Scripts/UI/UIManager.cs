using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WelCome.Core;

namespace WelCome.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private Canvas _canvas;
        private readonly Dictionary<Type, UIPanel> _panels = new();
        private UIPanel _activePanel;

        protected override void OnInit()
        {
            SetupCanvas();
            RegisterPanels();
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void SetupCanvas()
        {
            _canvas = GetComponentInChildren<Canvas>();
            if (_canvas != null) return;

            var canvasGo = new GameObject("UICanvas");
            canvasGo.transform.SetParent(transform);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
        }

        private void RegisterPanels()
        {
            RegisterPanel<MainMenuPanel>();
            RegisterPanel<DayPhasePanel>();
            RegisterPanel<NightPhasePanel>();
            RegisterPanel<SettlementPanel>();
            RegisterPanel<AVGPanel>();
            RegisterPanel<HUDPanel>();
            RegisterPanel<PopupPanel>();
            RegisterPanel<SettingsPanel>();
            RegisterPanel<SaveSlotPanel>();
            RegisterPanel<DisplayCaseStockPanel>();
        }

        private void RegisterPanel<T>() where T : UIPanel
        {
            var go = new GameObject(typeof(T).Name);
            go.transform.SetParent(_canvas.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var panel = go.AddComponent<T>();
            panel.Hide();
            _panels[typeof(T)] = panel;
        }

        public T GetPanel<T>() where T : UIPanel
        {
            return _panels.TryGetValue(typeof(T), out var panel) ? (T)panel : null;
        }

        public void ShowPanel<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
            {
                _activePanel?.Hide();
                panel.Show();
                _activePanel = panel;
            }
        }

        public void ShowOverlay<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
                panel.Show();
        }

        public void HideOverlay<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
                panel.Hide();
        }

        /// <summary>
        /// Shows a modal popup. If onCancel is null, only an OK button is shown.
        /// If onCancel is provided, both Confirm and Cancel buttons appear.
        /// </summary>
        public void ShowSaveSlots(SaveSlotMode mode, Action onDone = null)
        {
            var panel = GetPanel<SaveSlotPanel>();
            if (panel == null) return;
            panel.SetMode(mode, onDone);
            ShowOverlay<SaveSlotPanel>();
        }

        public void ShowPopup(string title, string body, Action onConfirm = null, Action onCancel = null)
        {
            var popup = GetPanel<PopupPanel>();
            if (popup == null) return;
            popup.SetContent(title, body, onConfirm, onCancel);
            ShowOverlay<PopupPanel>();
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.NewState)
            {
                case GameState.MainMenu:
                    ShowPanel<MainMenuPanel>();
                    HideOverlay<HUDPanel>();
                    break;
                case GameState.DayPhase:
                    ShowPanel<DayPhasePanel>();
                    ShowOverlay<HUDPanel>();
                    break;
                case GameState.NightPhase:
                    ShowPanel<NightPhasePanel>();
                    ShowOverlay<HUDPanel>();
                    break;
                case GameState.NightSettlement:
                    ShowPanel<SettlementPanel>();
                    HideOverlay<HUDPanel>();
                    break;
            }
        }
    }
}
