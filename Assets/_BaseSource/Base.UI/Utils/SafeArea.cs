using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour {
    RectTransform rectTransform;
    Rect lastSafeArea = Rect.zero;
    Vector2Int lastScreenSize = Vector2Int.zero;
    ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update() {
        if (Screen.safeArea != lastSafeArea
            || Screen.width != lastScreenSize.x
            || Screen.height != lastScreenSize.y
            || Screen.orientation != lastOrientation) {
            ApplySafeArea();
        }
    }

    void ApplySafeArea() {
        Rect safeArea = Screen.safeArea;

        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

    }
}