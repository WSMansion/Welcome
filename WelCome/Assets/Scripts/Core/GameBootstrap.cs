using UnityEngine;
using WelCome.Data;
using WelCome.Systems;
using WelCome.UI;

namespace WelCome.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[GameBootstrap] Initializing game systems...");
            InitSystems();
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }

        private void InitSystems()
        {
            _ = ConfigManager.Instance;
            _ = ResourceSystem.Instance;
            _ = TimeSystem.Instance;
            _ = DayEventSystem.Instance;
            _ = WorkshopSystem.Instance;
            _ = CustomerSystem.Instance;
            _ = SettlementSystem.Instance;
            _ = AVGSystem.Instance;
            _ = UIManager.Instance;
            _ = GameManager.Instance;

            Debug.Log("[GameBootstrap] All systems initialized.");
        }
    }
}
