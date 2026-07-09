using TMPro;
using UnityEngine;

namespace FantasyIsland
{
    /// A single pooled nameplate on the screen-space UI Canvas. Plain UI - knows
    /// nothing about ECS. NameplateManager creates these from a prefab and drives them.
    public class NameplateElement : MonoBehaviour
    {
        [Tooltip("Text showing the unit's own name.")]
        public TMP_Text NameText;

        [Tooltip("Text showing the unit's current target name (or '-').")]
        public TMP_Text TargetText;

        RectTransform _rect;

        void Awake() => _rect = (RectTransform)transform;

        public void SetName(string value)
        {
            if (NameText != null) NameText.text = value;
        }

        public void SetTarget(string value)
        {
            if (TargetText != null) TargetText.text = value;
        }

        /// Screen-pixel position (from Camera.WorldToScreenPoint). Works with a
        /// Screen Space - Overlay canvas, where RectTransform.position is in pixels.
        public void SetScreenPosition(Vector3 screenPos) => _rect.position = screenPos;

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }
    }
}
