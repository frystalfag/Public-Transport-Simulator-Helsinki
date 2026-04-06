using UnityEngine;

public class TestPoint : MonoBehaviour
{
    public float lat;
    public float lon;
    public RoadRenderer roadRenderer;

    void Start()
    {
        if (roadRenderer == null)
            return;

        Vector2 xy = MapMath.LatLonToXY(
            lat,
            lon,
            roadRenderer.originLat,
            roadRenderer.originLon
        );

        transform.position = new Vector3(xy.x, xy.y, transform.position.z);
    }
}