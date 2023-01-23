using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple helper class to load a scene from a unityEvent.
/// </summary>
public class LoadScene : MonoBehaviour
{
    [SerializeField] private SceneIndex m_sceneIndex;


    public void Load()
    {
        SceneManager.LoadScene((int)m_sceneIndex);
    }
}
