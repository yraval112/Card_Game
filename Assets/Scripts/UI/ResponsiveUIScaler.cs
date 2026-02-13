using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages responsive UI scaling for different devices and orientations
/// Should be attached to Canvas or CanvasScaler
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveUIScaler : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;

    [Header("Reference Resolution")]
    [SerializeField] private Vector2 referenceResolution = new(1920, 1080);

    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;

    // [Header("Orientation")]
    // [SerializeField] private bool autoDetectOrientation = true;

    private ScreenOrientation lastOrientation;
    private Vector2 lastScreenSize;

    void Awake()
    {
        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();

        InitializeScaler();
    }

    void Update()
    {
        // Check for screen size or orientation change
        if (Screen.orientation != lastOrientation ||
            new Vector2(Screen.width, Screen.height) != lastScreenSize)
        {
            lastOrientation = Screen.orientation;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            UpdateScaling();
        }
    }

    /// <summary>
    /// Initialize canvas scaler with reference resolution
    /// </summary>
    void InitializeScaler()
    {
        if (canvasScaler == null)
            return;

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // Set match value based on aspect ratio
        float screenAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceResolution.x / referenceResolution.y;

        // If screen is wider, match height; if taller, match width
        canvasScaler.matchWidthOrHeight = screenAspect > referenceAspect ? 1 : 0;

        Debug.Log($"UI Scaler Initialized - Screen: {Screen.width}x{Screen.height}, " +
            $"Reference: {referenceResolution.x}x{referenceResolution.y}, " +
            $"Match: {canvasScaler.matchWidthOrHeight}");
    }

    /// <summary>
    /// Update scaling based on current screen
    /// </summary>
    void UpdateScaling()
    {
        if (canvasScaler == null)
            return;

        float screenAspect = (float)Screen.width / Screen.height;
        float referenceAspect = referenceResolution.x / referenceResolution.y;

        // Adjust match value for current aspect ratio
        canvasScaler.matchWidthOrHeight = screenAspect > referenceAspect ? 1 : 0;

        Debug.Log($"UI Scaled for {Screen.width}x{Screen.height} - Aspect: {screenAspect:F2}");
    }

    /// <summary>
    /// Get current UI scale factor
    /// </summary>
    public float GetCurrentScale()
    {
        if (canvasScaler == null)
            return 1f;

        float scale = Mathf.Clamp(canvasScaler.scaleFactor, minScale, maxScale);
        return scale;
    }

    /// <summary>
    /// Check if device is in portrait mode
    /// </summary>
    public bool IsPortraitMode()
    {
        return Screen.height > Screen.width;
    }

    /// <summary>
    /// Check if device is in landscape mode
    /// </summary>
    public bool IsLandscapeMode()
    {
        return Screen.width > Screen.height;
    }

    /// <summary>
    /// Get screen aspect ratio
    /// </summary>
    public float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
}
