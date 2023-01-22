using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes sure, that a target area is contained within the area of a specified rectTransform.
/// </summary>
[ExecuteInEditMode]
public class CameraFraming : MonoBehaviour
{
    [SerializeField] private RectTransform m_captureTransform;
    [SerializeField] private Rect m_targetArea = new Rect(0, 0, 16, 9);

    [SerializeField] private bool m_logging = false;


    private Camera m_camera;

    // World corners of the rectTransform
    private Vector3[] m_worldCorners = new Vector3[4];

    // Last updated rects
    private Rect m_lastCaptureArea;
    private Rect m_lastTargetArea;


    private void Update()
    {
        if (m_camera == null && (m_camera = GetComponent<Camera>()) == null)
            return;

        if (m_captureTransform == null)
            return;

        Refresh();
    }


    private void Refresh()
    {
        var captureArea = new Rect();

        m_captureTransform.GetWorldCorners(m_worldCorners);

        // Create the capture rect based on the corners of the rectTransform
        captureArea.min = m_worldCorners[0];
        captureArea.max = m_worldCorners[2];

        // Sometimes the capture area was zero, so the framing didn't work
        if (captureArea.size == Vector2.zero)
            return;

        // Only change the framing, if the rects have changed
        if (captureArea != m_lastCaptureArea
            || m_targetArea != m_lastTargetArea)
        {
            m_lastCaptureArea = captureArea;
            m_lastTargetArea = m_targetArea;

            ApplyCaptureArea(captureArea);
        }
    }


    private void ApplyCaptureArea(Rect captureArea)
    {
        // Calculate the minimum width and height of the camera
        var width = m_targetArea.width * 0.5f * (Screen.height / captureArea.width);
        var height = m_targetArea.height * 0.5f * (Screen.height / captureArea.height);

        // Set the camera size to the biggest dimension that needs to be captured
        m_camera.orthographicSize = Mathf.Max(width, height);

        // Don't change the z position
        var positionZ = transform.position.z;

        // Reset the camera to the origin and calculate the center of the capture area
        transform.position = Vector3.zero;
        var worldCenter = m_camera.ScreenToWorldPoint(captureArea.center);

        // Calculate the camera position on the xy plane
        var positionXY = m_targetArea.position - new Vector2(worldCenter.x, worldCenter.y);

        transform.position = new Vector3(positionXY.x, positionXY.y, positionZ);

        if (m_logging)
        {
            Debug.Log($"Updated camera transform.");
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var rectColor = Color.blue;

        Gizmos.color = rectColor;
        Gizmos.DrawWireCube(m_targetArea.position, m_targetArea.size);

        rectColor.a = 0.3f;
        Gizmos.color = rectColor;
        Gizmos.DrawCube(m_targetArea.position, m_targetArea.size);
    }
#endif
}
