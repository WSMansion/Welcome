using UnityEngine;
using WelCome.Core;

namespace WelCome.World
{
    /// <summary>
    /// Initializes display case definitions when the standalone shop map preview is played.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShopDemoBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            DisplayCaseConfigurationBootstrap.InitializeForDay(GameManager.Instance.CurrentDay);
        }
    }
}
