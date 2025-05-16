using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GifMapAsset", menuName = "ScriptableObjects/GifMapAsset", order = 2)]
public class GifMapAsset : ScriptableObject
{
    public List<GifMapElementAsset> GifMapList;

    public List<string> GetRawActName(string matActName)
    {
        
        foreach(GifMapElementAsset gifMapElementAsset in GifMapList)
        {
            if (gifMapElementAsset.actName == matActName)
            {
                return gifMapElementAsset.actRawName;
            }
        }
        return null;
    } 

    public List<string> GetActionNameList()
    {
        List<string> list = new List<string>();
        foreach (GifMapElementAsset gifMapElementAsset in GifMapList)
        {
            list.Add(gifMapElementAsset.actName);
        }
        return list;
    }

    public List<string> GetActionRawNameList()
    {
        List<string> list = new List<string>();
        foreach (GifMapElementAsset gifMapElementAsset in GifMapList)
        {
            list.Add(gifMapElementAsset.actRawName[0]);
        }
        return list;
    }


}