using UnityEngine;

namespace InteractiveGlobeKit
{
    public static class GlobeCoordinateUtility
    {
        public static Vector3 LatLonToLocal(float latitude, float longitude, float radius, float longitudeOffset = 0f)
        {
            float lat = latitude * Mathf.Deg2Rad;
            float lon = (longitude + longitudeOffset) * Mathf.Deg2Rad;
            float cosLat = Mathf.Cos(lat);
            return new Vector3(cosLat * Mathf.Sin(lon), Mathf.Sin(lat), cosLat * Mathf.Cos(lon)) * radius;
        }

        public static float GreatCircleDistanceKm(float latA, float lonA, float latB, float lonB)
        {
            float a1 = latA * Mathf.Deg2Rad;
            float a2 = latB * Mathf.Deg2Rad;
            float dLat = (latB - latA) * Mathf.Deg2Rad;
            float dLon = (lonB - lonA) * Mathf.Deg2Rad;
            float h = Mathf.Sin(dLat * .5f) * Mathf.Sin(dLat * .5f) +
                      Mathf.Cos(a1) * Mathf.Cos(a2) * Mathf.Sin(dLon * .5f) * Mathf.Sin(dLon * .5f);
            return 6371f * 2f * Mathf.Atan2(Mathf.Sqrt(h), Mathf.Sqrt(1f - h));
        }
    }
}
