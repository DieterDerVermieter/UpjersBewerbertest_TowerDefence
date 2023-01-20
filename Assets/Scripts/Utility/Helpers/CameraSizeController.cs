using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraSizeController : MonoBehaviour
{
    [SerializeField] private RectTransform m_captureTransform;
    [SerializeField] private Rect m_targetArea = new Rect(0, 0, 16, 9);

    [SerializeField] private bool m_logging = false;


    private Camera m_camera;

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

        var worldCorners = new Vector3[4];
        m_captureTransform.GetWorldCorners(worldCorners);

        captureArea.min = worldCorners[0];
        captureArea.max = worldCorners[2];

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
        // var width = m_captureArea.width * 0.5f * (Screen.width / safeArea.width) * (Screen.height / Screen.width);
        var width = m_targetArea.width * 0.5f * (Screen.height / captureArea.width);
        var height = m_targetArea.height * 0.5f * (Screen.height / captureArea.height);

        m_camera.orthographicSize = Mathf.Max(width, height);

        var positionZ = transform.position.z;

        transform.position = Vector3.zero;
        var worldCenter = m_camera.ScreenToWorldPoint(captureArea.center);
        Debug.Log($"center={captureArea.center}, worldCenter ={worldCenter}");

        var positionXY = m_targetArea.position - new Vector2(worldCenter.x, worldCenter.y);

        transform.position = new Vector3(positionXY.x, positionXY.y, positionZ);

        if (m_logging)
        {
            Debug.Log($"Updated camera transform.");
        }
    }


    private void OnDrawGizmosSelected()
    {
        var rectColor = Color.blue;

        Gizmos.color = rectColor;
        Gizmos.DrawWireCube(m_targetArea.position, m_targetArea.size);

        rectColor.a = 0.3f;
        Gizmos.color = rectColor;
        Gizmos.DrawCube(m_targetArea.position, m_targetArea.size);
    }
}
