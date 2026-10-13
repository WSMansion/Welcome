using UnityEngine;

namespace WelCome.World
{
    /// <summary>Scene-view marker and line for an editable visitor route point.</summary>
    public sealed class ShopWaypointGizmo : MonoBehaviour
    {
        public Color routeColor = new Color(1f, 0.82f, 0.32f, 0.9f);

        private void OnDrawGizmos()
        {
            Gizmos.color = routeColor;
            Gizmos.DrawSphere(transform.position, 0.09f);

            Transform parent = transform.parent;
            if (parent == null || parent.childCount < 2)
                return;

            int nextIndex = (transform.GetSiblingIndex() + 1) % parent.childCount;
            Gizmos.DrawLine(transform.position, parent.GetChild(nextIndex).position);
        }
    }
}
