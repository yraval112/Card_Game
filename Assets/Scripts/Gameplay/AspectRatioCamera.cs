using UnityEngine;

/// <summary>
/// Handles viewport/camera adjustments for different aspect ratios
/// Ensures gameplay is readable on all devices
/// </summary>
[RequireComponent(typeof(Camera))]
public class AspectRatioCamera : MonoBehaviour
{
    private Camera targetCamera;

    [Header("Letter-boxing")]
    [SerializeField] private bool useLetterboxing = false;
    [SerializeField] private Color letterboxColor = Color.black;

    [Header("Target Aspect")]
    [SerializeField] private float targetAspectRatio = 16f / 9f;

    private float lastAspect;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
    }

    void Start()
    {
        UpdateAspect();
    }

    void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        if (Mathf.Abs(currentAspect - lastAspect) > 0.01f)
        {
            UpdateAspect();
        }
    }

    /// <summary>
    /// Adjust camera viewport based on screen aspect ratio
    /// </summary>
    void UpdateAspect()
    {
        if (targetCamera == null)
            return;

        float currentAspect = (float)Screen.width / Screen.height;
        lastAspect = currentAspect;

        if (useLetterboxing)
        {
            ApplyLetterboxing(currentAspect);
        }
        else
        {
            ApplyPillarboxing(currentAspect);
        }

        Debug.Log($"Aspect Ratio Camera Updated - Current: {currentAspect:F2}, Target: {targetAspectRatio:F2}");
    }

    /// <summary>
    /// Apply letterboxing (black bars on top/bottom)
    /// </summary>
    void ApplyLetterboxing(float currentAspect)
    {
        if (currentAspect >= targetAspectRatio)
        {
            // Screen is wider than target - letterbox
            float scaleHeight = currentAspect / targetAspectRatio;
            Rect rect = targetCamera.rect;

            rect.height = 1f / scaleHeight;
            rect.y = (1f - rect.height) / 2f;

            targetCamera.rect = rect;
        }
        else
        {
            // Screen is taller than target - pillarbox
            float scaleWidth = targetAspectRatio / currentAspect;
            Rect rect = targetCamera.rect;

            rect.width = 1f / scaleWidth;
            rect.x = (1f - rect.width) / 2f;

            targetCamera.rect = rect;
        }
    }

    /// <summary>
    /// Apply pillarboxing (black bars on sides)
    /// </summary>
    void ApplyPillarboxing(float currentAspect)
    {
        if (currentAspect >= targetAspectRatio)
        {
            // Screen is wider - add pillarboxing
            float scaleWidth = targetAspectRatio / currentAspect;
            Rect rect = targetCamera.rect;

            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;

            targetCamera.rect = rect;
        }
        else
        {
            // Screen is taller - add letterboxing
            float scaleHeight = currentAspect / targetAspectRatio;
            Rect rect = targetCamera.rect;

            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;

            targetCamera.rect = rect;
        }
    }

    /// <summary>
    /// Get current viewport aspect ratio
    /// </summary>
    public float GetCurrentViewportAspect()
    {
        return (float)Screen.width / Screen.height;
    }

    /// <summary>
    /// Reset camera to full viewport
    /// </summary>
    public void ResetViewport()
    {
        if (targetCamera != null)
        {
            targetCamera.rect = new Rect(0, 0, 1, 1);
        }
    }
}
