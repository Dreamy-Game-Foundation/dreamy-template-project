using UnityEngine;

namespace Dreamy.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool applyHorizontal = true;
        [SerializeField] private bool applyVertical = true;

        private Rect lastSafeArea;
        private Vector2 lastScreenSize;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea ||
                lastScreenSize.x != Screen.width ||
                lastScreenSize.y != Screen.height)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
#if UNITY_EDITOR
            // Device Simulator can briefly report device pixel bounds while Screen uses
            // the resized Game View dimensions. Never turn those into anchors above 1.
            if (safeArea.xMin < 0f || safeArea.yMin < 0f ||
                safeArea.xMax > Screen.width || safeArea.yMax > Screen.height)
            {
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            }
#endif
            lastScreenSize = new Vector2(Screen.width, Screen.height);

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            if (!applyHorizontal)
            {
                anchorMin.x = 0f;
                anchorMax.x = 1f;
            }

            if (!applyVertical)
            {
                anchorMin.y = 0f;
                anchorMax.y = 1f;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
