using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJson : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //MapList mapList = JsonParser.parseObj<MapList>("Assets/Scripts/TellStory/data/conv_mat.json");
        DataMap.initData();
        Debug.Log(DataMap.convMatMapList.map_list.Count);
        //Debug.Log(DataMap.matInitSettingList.matInitSettingList.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
