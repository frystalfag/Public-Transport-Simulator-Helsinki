using UnityEngine;

public class MapLoader : MonoBehaviour
{
    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("map");

        if (jsonFile == null)
        {
            Debug.LogError("map.json not found in Resources!");
            return;
        }

        Debug.Log(jsonFile.text);
    }
}
