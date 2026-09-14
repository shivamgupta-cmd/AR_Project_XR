using UnityEngine;
using System.Collections;

public class CO2DeliveryFXController : MonoBehaviour
{
    public CO2WorldPathFollower travellingBubbles;
    public ParticleSystem jarCollection;
    public float collectionStartDelay = 2.6f;

    public void StartCO2()
    {
        ResetCO2();
        if (travellingBubbles) travellingBubbles.PlayCO2();
        StartCoroutine(StartCollection());
    }

    IEnumerator StartCollection()
    {
        yield return new WaitForSeconds(collectionStartDelay);
        if (jarCollection) jarCollection.Play(true);
    }

    public void StopCO2()
    {
        StopAllCoroutines();
        if (travellingBubbles) travellingBubbles.StopCO2();
        if (jarCollection) jarCollection.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ResetCO2()
    {
        StopAllCoroutines();
        if (travellingBubbles) travellingBubbles.ClearCO2();
        if (jarCollection) jarCollection.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
