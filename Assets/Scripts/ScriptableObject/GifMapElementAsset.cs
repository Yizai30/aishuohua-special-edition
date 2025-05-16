using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GifMapElementAsset", menuName = "ScriptableObjects/GifMapElementAsset", order = 1)]
public class GifMapElementAsset : ScriptableObject
{
    public string actName;

    public List<string> actRawName;
    
}