using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Frames a rectTransform, so that is is fully inside the safeRect provided by unity
/// </summary>
[ExecuteInEditMode]
public class SafeAreaFraming : MonoBehaviour
{
    [SerializeField] private bool m_conformX = true;
    [SerializeField] private bool m_conformY = true;

    [SerializeField] private bool m_logging = false;


    private RectTransform m_panel;

    // Last updated values
    private Rect m_lastSafeArea;
    private Vector2Int m_lastScreenSize;
    private ScreenOrientation m_lastOrientation = ScreenOrientation.AutoRotation;


    private void Update()
    {
        if (m_panel == null && (m_panel = GetComponent<RectTransform>()) == null)
            return;

        Refresh();
    }


    private void Refresh()
    {
        var safeArea = Screen.safeArea;

        // Only update the area, if values have changed
        if (safeArea != m_lastSafeArea
            || Screen.width != m_lastScreenSize.x
            || Screen.height != m_lastScreenSize.y
            || Screen.orientation != m_lastOrientation)
        {
            m_lastSafeArea = safeArea;
            // Fix for having auto-rotate off and manually forcing a screen orientation.
            // See https://forum.unity.com/threads/569236/#post-4473253 and https://forum.unity.com/threads/569236/page-2#post-5166467
            m_lastScreenSize.x = Screen.width;
            m_lastScreenSize.y = Screen.height;
            m_lastOrientation = Screen.orientation;

            ApplySafeArea(safeArea);
        }
    }


    private void ApplySafeArea(Rect safeArea)
    {
        // Ignore x-axis?
        if (!m_conformX)
        {
            safeArea.x = 0;
            safeArea.width = Screen.width;
        }

        // Ignore y-axis?
        if (!m_conformY)
        {
            safeArea.y = 0;
            safeArea.height = Screen.height;
        }

        // Check for invalid screen startup state on some Samsung devices (see below)
        if (Screen.width > 0 && Screen.height > 0)
        {
            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Fix for some Samsung devices (e.g. Note 10+, A71, S20) where Refresh gets called twice and the first time returns NaN anchor coordinates
            // See https://forum.unity.com/threads/569236/page-2#post-6199352
            if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
            {
                m_panel.anchorMin = anchorMin;
                m_panel.anchorMax = anchorMax;
            }
        }

        if (m_logging)
        {
            Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            name, safeArea.x, safeArea.y, safeArea.width, safeArea.height, Screen.width, Screen.height);
        }
    }
}
