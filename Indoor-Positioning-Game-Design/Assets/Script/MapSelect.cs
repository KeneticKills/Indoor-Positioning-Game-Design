using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSelector : MonoBehaviour
{
    public TMP_Dropdown mapDropdown;
    public Image mapDisplay;
    public Component gridMap;
    public Sprite[] mapSprites;

    void Start()
    {
        mapDropdown.onValueChanged.AddListener(OnMapSelected);
        ShowMap(0);
    }

    void OnMapSelected(int index)
    {
        ShowMap(index);
    }

    void ShowMap(int mapIndex)
    {
        if (mapIndex >= 0 && mapIndex < mapSprites.Length)
        {
            mapDisplay.sprite = mapSprites[mapIndex];
            MapDataWithStorage mapData = gridMap.GetComponent<MapDataWithStorage>();
            mapData.floor = mapIndex;
        }
    }
}