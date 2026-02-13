using UnityEngine;

/// <summary>
/// Manages safe area for responsive UI across all devices
/// Handles notches, different screen sizes, and aspect ratios
/// Properly supports all devices including iPhone notches and Android safe zones
/// </summary>
public class SafeAreaManager : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform safeAreaPanel;

    private Rect lastSafeArea = Rect.zero;
    private ScreenOrientation lastOrientation;

    void Awake()
    {
        // Get Canvas from this GameObject or parent
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }

        // Get safeAreaPanel from assignment or use this transform
        if (safeAreaPanel == null)
            safeAreaPanel = GetComponent<RectTransform>();

        if (canvas == null)
        {
            Debug.LogError("SafeAreaManager: No Canvas found!");
            return;
        }

        if (safeAreaPanel == null)
        {
            Debug.LogError("SafeAreaManager: No RectTransform found for safe area panel!");
            return;
        }

        ApplySafeArea();
    }

    void OnEnable()
    {
        // Reapply safe area when becoming active
        ApplySafeArea();
    }

    void Update()
    {
        // Check for screen changes (orientation, size)
        if (Screen.safeArea != lastSafeArea || Screen.orientation != lastOrientation)
        {
            ApplySafeArea();
        }
    }

    /// <summary>
    /// Apply safe area margins to the panel, handling all device types
    /// </summary>
    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // No change needed
        if (safeArea == lastSafeArea && Screen.orientation == lastOrientation)
            return;

        lastSafeArea = safeArea;
        lastOrientation = Screen.orientation;

        if (safeAreaPanel == null)
            return;

        // Normalize safe area to 0-1 range
        // This converts pixel coordinates to normalized viewport coordinates
        float leftPercent = safeArea.x / Screen.width;
        float bottomPercent = safeArea.y / Screen.height;
        float rightPercent = 1f - ((Screen.width - (safeArea.x + safeArea.width)) / Screen.width);
        float topPercent = 1f - ((Screen.height - (safeArea.y + safeArea.height)) / Screen.height);

        // Clamp to valid range
        leftPercent = Mathf.Clamp01(leftPercent);
        bottomPercent = Mathf.Clamp01(bottomPercent);
        rightPercent = Mathf.Clamp01(rightPercent);
        topPercent = Mathf.Clamp01(topPercent);

        // Apply anchors to panel
        safeAreaPanel.anchorMin = new Vector2(leftPercent, bottomPercent);
        safeAreaPanel.anchorMax = new Vector2(rightPercent, topPercent);

        // Clear any offset min/max to allow anchors to work properly
        safeAreaPanel.offsetMin = Vector2.zero;
        safeAreaPanel.offsetMax = Vector2.zero;

        // Log for debugging
        Vector4 insets = GetSafeAreaInsets();

    }

    /// <summary>
    /// Manually trigger safe area recalculation
    /// </summary>
    public void RefreshSafeArea()
    {
        lastSafeArea = Rect.zero; // Force update
        lastOrientation = ScreenOrientation.AutoRotation; // Force update
        ApplySafeArea();
    }

    /// <summary>
    /// Get safe area insets in pixels (useful for debugging or manual layout)
    /// Returns (left, bottom, right, top) insets
    /// </summary>
    public static Vector4 GetSafeAreaInsets()
    {
        Rect safeArea = Screen.safeArea;
        float leftInset = safeArea.x;
        float bottomInset = safeArea.y;
        float rightInset = Screen.width - (safeArea.x + safeArea.width);
        float topInset = Screen.height - (safeArea.y + safeArea.height);

        return new Vector4(leftInset, bottomInset, rightInset, topInset);
    }

    /// <summary>
    /// Get safe area as percentage of screen (0-1 range)
    /// Returns (left%, bottom%, right%, top%) percentages
    /// </summary>
    public static Vector4 GetSafeAreaPercentage()
    {
        Rect safeArea = Screen.safeArea;
        float leftPercent = safeArea.x / Screen.width;
        float bottomPercent = safeArea.y / Screen.height;
        float rightPercent = (Screen.width - (safeArea.x + safeArea.width)) / Screen.width;
        float topPercent = (Screen.height - (safeArea.y + safeArea.height)) / Screen.height;

        return new Vector4(leftPercent, bottomPercent, rightPercent, topPercent);
    }

    /// <summary>
    /// Get current safe area rectangle
    /// </summary>
    public static Rect GetSafeAreaRect()
    {
        return Screen.safeArea;
    }

    /// <summary>
    /// Check if device has a notch or safe area
    /// </summary>
    public static bool HasNotch()
    {
        Rect safeArea = Screen.safeArea;

        // Check if safe area is smaller than screen (indicates notch or safe zone)
        bool hasHorizontalNotch = safeArea.x > 0 || (Screen.width - (safeArea.x + safeArea.width)) > 0;
        bool hasVerticalNotch = safeArea.y > 0 || (Screen.height - (safeArea.y + safeArea.height)) > 0;

        return hasHorizontalNotch || hasVerticalNotch;
    }
}
