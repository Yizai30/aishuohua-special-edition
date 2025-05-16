using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapList
{

    public MapList()
    {
        map_list = new List<MapListElement>();
    }
    public List<MapListElement> map_list { set; get; }


    public bool containMapName(string mapName)
    {
        MapListElement re = null;
        foreach (MapListElement el in map_list)
        {
            if (el.mapName.Equals(mapName))
            {
                return true;
            }
        }
        return false;
    }

    public MapListElement getConvMapByMapName(string mapName)
    {
        MapListElement re = null;
        foreach(MapListElement el in map_list)
        {
            if (el.mapName.Equals(mapName))
            {
                re = el;
            }
        }
        if (re== null){
            throw new System.Exception("没有匹配到MapListElement" + mapName);
        }
        return re;
    }

    public List<string> getAllMatName()
    {
        List<string> re = new List<string>();
        foreach (MapListElement mapListElement in map_list)
        {
            foreach(MapKVPair mapKVPair in mapListElement.contentList)
            {
                re.Add(mapKVPair.valName);
            }
        }
        return re;

    }

    //返回所有动词
    public List<string> getAllActionRawName()
    {
        MapListElement actionMap = getConvMapByMapName("ActionMap");
        List<string> re = new List<string>();
        foreach(MapKVPair mapKVPair in actionMap.contentList)
        {
            foreach(string actRawName in mapKVPair.keyNameList[1])
            {
                if (!re.Contains(actRawName))
                {
                    re.Add(actRawName);
                }
            }
        }
        return re;
    }

    //返回所有角色物品名词
    public List<string> getAllActorAndPropRawName()
    {
        MapListElement actionMap = getConvMapByMapName("ObjectMap");
        List<string> re = new List<string>();
        foreach (MapKVPair mapKVPair in actionMap.contentList)
        {
            foreach (string actRawName in mapKVPair.keyNameList[0])
            {
                if (!re.Contains(actRawName))
                {
                    re.Add(actRawName);
                }
            }
        }
        return re;
    }

    //返回所有背景名词
    public List<string> getAllBackgroundRawName()
    {
        MapListElement actionMap = getConvMapByMapName("BkgMap");
        List<string> re = new List<string>();
        foreach (MapKVPair mapKVPair in actionMap.contentList)
        {
            foreach (string rawName in mapKVPair.keyNameList[0])
            {
                if (!re.Contains(rawName) && !DataUtil.containEn(rawName))
                {
                    re.Add(rawName);
                }
            }

           
        }
        return re;
    }

    //返回所有季节词
    public List<string> getAllSeasonRawName()
    {
        MapListElement actionMap = getConvMapByMapName("BkgMap");
        List<string> re = new List<string>();
        foreach (MapKVPair mapKVPair in actionMap.contentList)
        {
            if (mapKVPair.keyNameList.Count > 1)
            {
                foreach (string rawName in mapKVPair.keyNameList[1])
                {
                    if (!re.Contains(rawName) && !DataUtil.containEn(rawName))
                    {
                        re.Add(rawName);
                    }
                }
            }
           
        }
        return re;
    }


}

[Serializable]
public class MapListElement
{
    public string mapName { set; get; }
    public List<MapKVPair> contentList { set; get; }

    public MapListElement(string mapName, List<MapKVPair> contentList)
    {
        this.mapName = mapName;
        this.contentList = contentList;
    }

    public void removeByKey(List<string> keyList)
    {
        int keyCount = keyList.Count;
        for(int n = contentList.Count-1;n >= 0;n--)
        {
            if (contentList[n].keyNameList.Count != keyList.Count) continue;
            int tempCount = 0;
            for (int i = 0; i < keyCount; i++)
            {
                if (contentList[n].keyNameList[i].Contains(keyList[i]))
                {
                    tempCount++;
                }
            }
            if (tempCount == keyCount)
            {
                contentList.RemoveAt(n);
            }
        }
    }

    public bool containKey(List<string> keyList)
    {
        bool re = false;
        int keyCount = keyList.Count;
        foreach (MapKVPair el in contentList)
        {
            if (el.keyNameList.Count != keyList.Count) continue;
            int tempCount = 0;
            for (int i = 0; i < keyCount; i++)
            {
                if (el.keyNameList[i].Contains(keyList[i]))
                {
                    tempCount++;
                }
            }
            if (tempCount == keyCount)
            {
                re = true;
                break;
            }
        }
        return re;
    }

    //根据keyList查找value，一个key匹配一个列表中的一项。
    public string getValByKeyList(List<string> keyList)
    {
        string re = "";
        int keyCount = keyList.Count;
        foreach (MapKVPair el in contentList)
        {
            if (el.keyNameList.Count != keyList.Count) continue;
            int tempCount = 0;
            for (int i = 0; i < keyCount; i++)
            {
                if (el.keyNameList[i].Contains(keyList[i]))
                {
                    tempCount++;
                }
            }
            if (tempCount == keyCount)
            {
                re = el.valName;
                break;
            }
        }

        if (re == "")
        {
            string reText = "";
            foreach (string key in keyList)
            {
                reText += key;
            }
            throw new System.Exception("匹配失败" + reText);
        }
        
        return re;
    }

    public string getKeyNameByVal(string valName)
    {
        string re = "";
        foreach (MapKVPair el in contentList)
        {
            if (el.valName.Equals(valName))
            {
                re = el.keyNameList[0][0];
            }
        }
        return re;
    }

    public List<string> getKeyNameListByvalName(string valName)
    {
        List<string> re = new List<string>();

        foreach (MapKVPair el in contentList)
        {
            if (el.valName.Equals(valName))
            {
                re = el.keyNameList[0];
            }
        }


        return re;
    }

}

[Serializable]
public class MapKVPair
{
    private List<List<string>> list;

    public List<List<string>> keyNameList { get; set; }
    public string valName { get; set; }

    public MapKVPair(List<List<string>> keyNameList, string valName)
    {
        this.keyNameList = keyNameList;
        this.valName = valName;
    }

    
}