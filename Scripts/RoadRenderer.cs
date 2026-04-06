using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class RoadRenderer : MonoBehaviour
{
    public Material roadMaterial;
    public float roadWidth = 3f;

    public float originLat;
    public float originLon;

    public List<List<List<float>>> roads;
    public List<List<Vector3>> worldRoads;

    public float minLat, maxLat;
    public float minLon, maxLon;

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("map");
        if (jsonFile == null)
        {
            Debug.LogError("map.json не найден");
            return;
        }

        roads = JsonConvert.DeserializeObject<List<List<List<float>>>>(jsonFile.text);

        originLat = roads[0][0][0];
        originLon = roads[0][0][1];
        
        minLat = float.MaxValue;
        maxLat = float.MinValue;
        minLon = float.MaxValue;
        maxLon = float.MinValue;

        foreach (var road in roads)
        {
            foreach (var p in road)
            {
                float lat = p[0];
                float lon = p[1];

                if (lat < minLat) minLat = lat;
                if (lat > maxLat) maxLat = lat;
                if (lon < minLon) minLon = lon;
                if (lon > maxLon) maxLon = lon;
            }
        }

        Debug.Log($"Map bounds: lat {minLat}..{maxLat}, lon {minLon}..{maxLon}");
        
        worldRoads = new List<List<Vector3>>();

        List<Vector3> waypoints = new List<Vector3>();

        for (int i = 0; i < roads.Count; i++)
        {
            var worldRoad = MapMath.ConvertRoadToWorld(
                roads[i],
                originLat,
                originLon
            );
            
            worldRoads.Add(worldRoad);
            DrawRoad(worldRoad, i);
        }
    }

    public Vector3 FindClosestPointOnRoad(Vector3 pos)
    {
        Vector3 closestPoint = pos;
        float minDistance = float.MaxValue;
        foreach (var i in worldRoads)
        {
            foreach (var p in i)
            {
                float distance = Vector3.Distance(pos, p);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = p;
                }
            }    
        }
        return closestPoint;
    } 
        
    void DrawRoad(List<Vector3> road, int index)
    {
        GameObject roadObj = new GameObject("Road_" + index);
        roadObj.transform.parent = transform;

        LineRenderer lr = roadObj.AddComponent<LineRenderer>();
        lr.material = roadMaterial;
        lr.useWorldSpace = true;
        lr.positionCount = road.Count;
        lr.startWidth = roadWidth;
        lr.endWidth = roadWidth;
        lr.sortingOrder = 10;

        lr.SetPositions(road.ToArray());
    }
}
