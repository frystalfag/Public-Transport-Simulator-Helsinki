import zipfile
import csv
import io
import requests
from flask import Flask, jsonify
from google.transit import gtfs_realtime_pb2

app = Flask(__name__)

GTFS_ZIP_PATH = "gtfs.zip"


def load_stops(zip_path: str) -> dict:
    stops = {}
    with zipfile.ZipFile(zip_path, "r") as z:
        with z.open("stops.txt") as f:
            text = io.TextIOWrapper(f, encoding="utf-8")
            reader = csv.DictReader(text)

            for row in reader:
                sid = row.get("stop_id")
                name = row.get("stop_name")
                lat = row.get("stop_lat")
                lon = row.get("stop_lon")
                if sid is None or lat is None or lon is None:
                    continue
                try:
                    stops[sid] = (name, float(lat), float(lon))
                except:
                    continue
    return stops


STOPS = load_stops(GTFS_ZIP_PATH)
print("Loaded stops:", len(STOPS))


@app.route("/stops")
def stops():
    out = []

    for sid, (name, lat, lon) in STOPS.items():
        out.append({
            "id": sid,
            "name": name,
            "latitude": lat,
            "longitude": lon
        })

    return jsonify({"stops": out})


def load_route_type_map(zip_path: str) -> dict:
    route_map = {}

    with zipfile.ZipFile(zip_path, "r") as z:
        with z.open("routes.txt") as f:
            text = io.TextIOWrapper(f, encoding="utf-8")
            reader = csv.DictReader(text)

            for row in reader:
                rid = row.get("route_id")
                rtype = row.get("route_type")
                if rid is None or rtype is None:
                    continue
                try:
                    route_map[rid] = int(rtype)
                except:
                    continue

    return route_map


ROUTE_TYPE_MAP = load_route_type_map(GTFS_ZIP_PATH)
print("Loaded route types:", len(ROUTE_TYPE_MAP))


HSL_URL = "https://realtime.hsl.fi/realtime/vehicle-positions/v2/hsl"


@app.route("/vehicles")
def vehicles():
    resp = requests.get(HSL_URL, timeout=10)

    if resp.status_code != 200:
        return jsonify({"error": "HSL failed", "status": resp.status_code}), 502

    feed = gtfs_realtime_pb2.FeedMessage()
    feed.ParseFromString(resp.content)

    out = []

    for e in feed.entity:
        if not e.HasField("vehicle"):
            continue

        v = e.vehicle

        if not v.HasField("position"):
            continue

        route_id = None
        if v.HasField("trip"):
            route_id = v.trip.route_id

        route_type = ROUTE_TYPE_MAP.get(route_id, -1)

        out.append({
            "id": v.vehicle.id if v.vehicle.id else e.id,
            "latitude": v.position.latitude,
            "longitude": v.position.longitude,
            "route_id": route_id,
            "type": route_type
        })

    return jsonify({"vehicles": out})


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
