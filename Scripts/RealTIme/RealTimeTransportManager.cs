using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public enum TransportType
{
    Bus = 0,
    Tram = 1,
    Train = 2
}

public class RealTimeTransportManager : MonoBehaviour
{
    public string apiUrl = "http://172.20.10.2:5000/vehicles";
    public RoadRenderer roadRenderer;
    public GameObject vehiclePrefab;

    public float UpdateInterval = 0.5f;
    
    public bool showBuses = true;
    public bool showTrams = true;
    public bool showTrains = true;

    private Dictionary<string, GameObject> vehicles = new Dictionary<string, GameObject>();
    private Dictionary<string, Vector3> vehiclePositions = new Dictionary<string, Vector3>();

    void Start()
    {
        StartCoroutine(UpdateVehicles());
    }

    TransportType GetTransportType(int apiType)
    {
        if (apiType == 0) return TransportType.Bus;
        if (apiType == 1) return TransportType.Tram;
        return TransportType.Train;
    }

    IEnumerator UpdateVehicles()
    {
        while (true)
        {
            UnityWebRequest req = UnityWebRequest.Get(apiUrl);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                VehicleList list = JsonUtility.FromJson<VehicleList>(req.downloadHandler.text);

                if (list == null || list.vehicles == null)
                {
                    yield return new WaitForSeconds(UpdateInterval);
                    continue;
                }

                foreach (var v in list.vehicles)
                {
                    if (!IsInsideMap(v.latitude, v.longitude))
                        continue;

                    Vector3 pos = MapMath.LatLonToXY(
                        v.latitude,
                        v.longitude,
                        roadRenderer.originLat,
                        roadRenderer.originLon
                    );

                    if (!vehicles.ContainsKey(v.id))
                    {
                        GameObject obj = Instantiate(vehiclePrefab, pos, Quaternion.identity);

                        VehicleMeta meta = obj.AddComponent<VehicleMeta>();
                        meta.type = v.type;

                        vehicles[v.id] = obj;
                    }

                    vehiclePositions[v.id] = roadRenderer.FindClosestPointOnRoad(pos);
                }
            }

            yield return new WaitForSeconds(UpdateInterval);
        }
    }

    void Update()
    {
        foreach (var kv in vehicles)
        {
            if (!vehiclePositions.ContainsKey(kv.Key))
                continue;

            GameObject obj = kv.Value;
            VehicleMeta meta = obj.GetComponent<VehicleMeta>();
            if (meta == null)
                continue;

            TransportType t = GetTransportType(meta.type);

            bool visible =
                (t == TransportType.Bus && showBuses) ||
                (t == TransportType.Tram && showTrams) ||
                (t == TransportType.Train && showTrains);

            obj.SetActive(visible);

            if (visible)
            {
                obj.transform.position = Vector3.Lerp(
                    obj.transform.position,
                    vehiclePositions[kv.Key],
                    Time.deltaTime * 5f
                );
            }
        }
    }

    bool IsInsideMap(float lat, float lon)
    {
        return
            lat >= roadRenderer.minLat &&
            lat <= roadRenderer.maxLat &&
            lon >= roadRenderer.minLon &&
            lon <= roadRenderer.maxLon;
    }
}

public class VehicleMeta : MonoBehaviour
{
    public int type;
}

[System.Serializable]
public class VehicleData
{
    public string id;
    public float latitude;
    public float longitude;
    public string route_id;
    public int type;
}

[System.Serializable]
public class VehicleList
{
    public List<VehicleData> vehicles;
}