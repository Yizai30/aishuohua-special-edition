using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BgmAsset", menuName = "ScriptableObjects/BgmAsset", order = 4)]
public class BgmAsset : ScriptableObject
{
    public List<BgmElementAsset> bgmList;

    public BgmElementAsset GetBgmByName(string name)
    {
        foreach (var bgm in bgmList)
        {
            if (bgm.name == name) { 
                return bgm; 
            }
        }
        return null;
    }

}