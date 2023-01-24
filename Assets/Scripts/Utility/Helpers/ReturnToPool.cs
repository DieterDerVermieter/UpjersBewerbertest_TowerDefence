using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// This component returns the particle system to the pool when the OnParticleSystemStopped event is received.
/// https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ReturnToPool : MonoBehaviour
{
    public ParticleSystem System;
    public IObjectPool<ParticleSystem> Pool;


    private void Start()
    {
        System = GetComponent<ParticleSystem>();

        var main = System.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }


    private void OnParticleSystemStopped()
    {
        // Return to the pool
        Pool.Release(System);
    }
}
