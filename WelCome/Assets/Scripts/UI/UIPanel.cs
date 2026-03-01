using UnityEngine;

namespace WelCome.UI
{
    public abstract class UIPanel : MonoBehaviour
    {
        protected bool IsVisible { get; private set; }
        private bool _isBuilt;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            if (!_isBuilt)
            {
                BuildUI();
                _isBuilt = true;
            }
            OnShow();
        }

        public virtual void Hide()
        {
            IsVisible = false;
            OnHide();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Build the panel's UI hierarchy. Called only once, on first Show().
        /// Override this to create the panel's visual structure.
        /// </summary>
        protected virtual void BuildUI() { }

        /// <summary>
        /// Called every time the panel is shown (after BuildUI on first show).
        /// Override this to refresh data-driven content.
        /// </summary>
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        /// <summary>
        /// Forces a full UI rebuild on next Show(). Use sparingly.
        /// </summary>
        protected void InvalidateUI()
        {
            if (_isBuilt)
            {
                foreach (Transform child in transform)
                    Destroy(child.gameObject);
                _isBuilt = false;
            }
        }
    }
}
