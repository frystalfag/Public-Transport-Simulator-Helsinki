using System;
using UnityEngine;
using System.Collections.Generic;

public class MoveAlongRoad : MonoBehaviour
{
    public RoadRenderer roadRenderer;
    public int roadIndex = 0;
    public float speed = 10f;

    private List<Vector2> path;
    private int curPoint = 0;

    void Start()
    {
        path = new List<Vector2>();
        var road = roadRenderer.roads[roadIndex];
        foreach (var p in road)
        {
            Vector2 posPosition = MapMath.LatLonToXY(p[0], p[1], roadRenderer.originLat, roadRenderer.originLon);
            path.Add(posPosition);
        }

        transform.position = path[0];
    }

    void Update()
    {
        if (curPoint >= path.Count)
            curPoint = 0;

        transform.position = Vector3.MoveTowards(
            transform.position,
            path[curPoint],
            speed * Time.deltaTime
        );
        
        Vector2 dir = path[curPoint] - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (Vector3.Distance(transform.position, path[curPoint]) < 0.1f)
            curPoint++;
    }
}