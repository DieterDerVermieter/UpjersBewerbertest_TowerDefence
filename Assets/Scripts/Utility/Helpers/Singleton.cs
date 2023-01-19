using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance => m_instance;
    static T m_instance;


    protected virtual void Awake()
    {
        if(m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        m_instance = GetComponent<T>();
    }
}
