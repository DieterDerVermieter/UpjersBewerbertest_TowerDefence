using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Responsible for spawing vfx and managing.
/// </summary>
public class EffectManager : Singleton<EffectManager>
{
    [SerializeField] private Transform m_VFXContainer;

    [SerializeField] private ParticleSystem m_enemyDeathVFXPrefab;


    private IObjectPool<ParticleSystem> m_enemyVFXPool;


    protected override void Awake()
    {
        base.Awake();

        // Setup vfx pools
        m_enemyVFXPool = new ObjectPool<ParticleSystem>(EnemyVFXPoolCreate, EnemyVFXPoolGet, EnemyVFXPoolRelease, EnemyVFXPoolDestroy, true, 20, 1000);
    }


    /// <summary>
    /// Plays vfx for an enemy death at a given point.
    /// </summary>
    /// <param name="position">Effect position</param>
    public void PlayEnemyDeathEffect(Vector3 position)
    {
        // Play vfx
        var effect = m_enemyVFXPool.Get();
        effect.transform.position = position;
        effect.Play();
    }


    private ParticleSystem EnemyVFXPoolCreate()
    {
        var system = Instantiate(m_enemyDeathVFXPrefab, m_VFXContainer);
        system.Stop(transform, ParticleSystemStopBehavior.StopEmittingAndClear);

        // This is used to return ParticleSystems to the pool when they have stopped.
        var returnToPool = system.gameObject.AddComponent<ReturnToPool>();
        returnToPool.Pool = m_enemyVFXPool;

        return system;
    }

    private void EnemyVFXPoolGet(ParticleSystem system)
    {
        system.gameObject.SetActive(true);
    }

    private void EnemyVFXPoolRelease(ParticleSystem system)
    {
        system.gameObject.SetActive(false);
    }

    private void EnemyVFXPoolDestroy(ParticleSystem system)
    {
        Destroy(system.gameObject);
    }
}
