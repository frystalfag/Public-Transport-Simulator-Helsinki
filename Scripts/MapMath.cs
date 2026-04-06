using UnityEngine;
using System.Collections.Generic;

public static class MapMath
{
    public const float METERS_PER_DEG_LAT = 111320f;

    public static Vector2 LatLonToXY(
        float lat,
        float lon,
        float originLat,
        float originLon
    )
    {
        float metersPerDegLon =
            METERS_PER_DEG_LAT * Mathf.Cos(originLat * Mathf.Deg2Rad);

        float x = (lon - originLon) * metersPerDegLon;
        float y = (lat - originLat) * METERS_PER_DEG_LAT;

        return new Vector2(x, y);
    }

    public static float DistanceLatLon(
        float lat1, float lon1,
        float lat2, float lon2
    )
    {
        Vector2 p1 = LatLonToXY(lat1, lon1, lat1, lon1);
        Vector2 p2 = LatLonToXY(lat2, lon2, lat1, lon1);
        return Vector2.Distance(p1, p2);
    }

    public static List<Vector3> ConvertRoadToWorld(
        List<List<float>> road,
        float originLat,
        float originLon,
        float z = 0f
    )
    {
        List<Vector3> result = new List<Vector3>(road.Count);

        foreach (var p in road)
        {
            float lat = p[0];
            float lon = p[1];

            Vector2 xy = LatLonToXY(lat, lon, originLat, originLon);
            result.Add(new Vector3(xy.x, xy.y, z));
        }

        return result;
    }
}