using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic singleton base class.
/// Makes sure, there is only one instance of that class at any time.
/// </summary>
/// <typeparam name="T">Type of the singleton class</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance => m_instance;
    static T m_instance;


    protected virtual void Awake()
    {
        // Make sure, there is only one instace of the class
        if(m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set this instance to the current instance
        m_instance = GetComponent<T>();
    }
}
