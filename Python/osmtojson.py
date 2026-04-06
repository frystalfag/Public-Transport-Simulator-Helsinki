import json
import xml.etree.ElementTree as ET

tree = ET.parse('map.osm')
root = tree.getroot()
nodes = {}
lines = []

for node in root.findall("node"):
    node_id = node.get("id")
    lat = float(node.get("lat"))
    lon = float(node.get("lon"))
    nodes[node_id] = [lat, lon]

for line in root.findall("way"):
    is_road = False
    for tag in line.findall("tag"):
        if tag.get("k") == "highway":
            is_road = True
    if not is_road:
        continue
    points = []
    for nd in line.findall("nd"):
        ref = nd.get("ref")
        if ref in nodes:
            points.append(nodes[ref])
    if len(points) > 1:
        lines.append(points)
with open('map.json', 'w') as f:
    json.dump(lines, f)

