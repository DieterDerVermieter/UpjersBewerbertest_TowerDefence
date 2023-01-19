using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SafeAreaAdjustment : MonoBehaviour
{
    RectTransform m_panel;

    Rect m_lastSafeArea = new Rect(0, 0, 0, 0);
    Vector2Int m_lastScreenSize = new Vector2Int(0, 0);
    ScreenOrientation m_lastOrientation = ScreenOrientation.AutoRotation;

    [SerializeField] bool m_conformX = true;
    [SerializeField] bool m_conformY = true;
    [SerializeField] bool m_logging = false;


    private void Update()
    {
        if (m_panel == null && (m_panel = GetComponent<RectTransform>()) == null)
            return;

        Refresh();
    }


    private void Refresh()
    {
        Rect safeArea = GetSafeArea();

        if (safeArea != m_lastSafeArea
            || Screen.width != m_lastScreenSize.x
            || Screen.height != m_lastScreenSize.y
            || Screen.orientation != m_lastOrientation)
        {
            // Fix for having auto-rotate off and manually forcing a screen orientation.
            // See https://forum.unity.com/threads/569236/#post-4473253 and https://forum.unity.com/threads/569236/page-2#post-5166467
            m_lastScreenSize.x = Screen.width;
            m_lastScreenSize.y = Screen.height;
            m_lastOrientation = Screen.orientation;

            ApplySafeArea(safeArea);
        }
    }


    private Rect GetSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        return safeArea;
    }


    private void ApplySafeArea(Rect safeRect)
    {
        // Ignore x-axis?
        if (!m_conformX)
        {
            safeRect.x = 0;
            safeRect.width = Screen.width;
        }

        // Ignore y-axis?
        if (!m_conformY)
        {
            safeRect.y = 0;
            safeRect.height = Screen.height;
        }

        m_lastSafeArea = safeRect;

        // Check for invalid screen startup state on some Samsung devices (see below)
        if (Screen.width > 0 && Screen.height > 0)
        {
            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = safeRect.position;
            Vector2 anchorMax = safeRect.position + safeRect.size;
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
            name, safeRect.x, safeRect.y, safeRect.width, safeRect.height, Screen.width, Screen.height);
        }
    }
}
