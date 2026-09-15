using UnityEngine;

/// <summary>
/// Simple Timeline/UnityEvent controller for the H2 delivery effect.
/// </summary>
public class H2GasDeliveryController : MonoBehaviour
{
    public ParticleSystem travellingGas;
    public bool playOnStart = false;

    void Start()
    {
        if (travellingGas != null)
        {
            travellingGas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (playOnStart) StartGasFlow();
        }
    }

    public void StartGasFlow()
    {
        if (travellingGas == null) return;
        travellingGas.Clear(true);
        travellingGas.Play(true);
    }

    public void StopGasFlow()
    {
        if (travellingGas == null) return;
        travellingGas.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ResetGasFlow()
    {
        if (travellingGas == null) return;
        travellingGas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        travellingGas.Clear(true);
    }
}
