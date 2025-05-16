using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FixMatSetting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string filePath = Application.streamingAssetsPath + "/data/JsonFiles/mat_setting.json";
        string jsonStr = File.ReadAllText(filePath);
        MatSettingList matSettingList = JsonOperator.parseObjFormStr<MatSettingList>(jsonStr);
        foreach(MatSettingElement matSetting in matSettingList.matSettingList)
        {
            matSetting.initScale[0] *= 80f;
            matSetting.initScale[1] *= 80f;
            matSetting.initScale[2] *= 80f;
        }
        JsonOperator.Obj2Json<MatSettingList>(matSettingList, filePath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
