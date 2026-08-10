using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
    private const float CHECK_RATE = 1f;

    public bool stopManuallyAfterDelay;
    [ShowIf(nameof(stopManuallyAfterDelay))][Min(0f)] public float delay;

    private void Start()
    {
        this.InvokeRepeating(nameof(this.CheckParticleSystemFinished), CHECK_RATE, CHECK_RATE);
        if (this.stopManuallyAfterDelay)
            this.Invoke(nameof(this.StopAfterDelay), this.delay);
    }

    private void CheckParticleSystemFinished()
    {
        if (this.transform.childCount == 0)
            Destroy(this.gameObject);
    }

    private void StopAfterDelay()
    {
        this.StopParticles();
    }
}
