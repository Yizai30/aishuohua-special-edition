using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserMapList
{

    public UserMapList()
    {
        map_list = new List<UserMapListElement>();
    }
    public List<UserMapListElement> map_list { set; get; }


    public bool containMapName(string mapName)
    {
        UserMapListElement re = null;
        foreach (UserMapListElement el in map_list)
        {
            if (el.mapName.Equals(mapName))
            {
                return true;
            }
        }
        return false;
    }

    public UserMapListElement getConvMapByMapName(string mapName)
    {
        UserMapListElement re = null;
        foreach(UserMapListElement el in map_list)
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
        foreach (UserMapListElement mapListElement in map_list)
        {
            foreach(UserMapKVPair mapKVPair in mapListElement.contentList)
            {
                re.Add(mapKVPair.valName);
            }
        }
        return re;

    }

    //返回所有动词
    public List<string> getAllActionRawName()
    {
        UserMapListElement actionMap = getConvMapByMapName("ActionMap");
        List<string> re = new List<string>();
        foreach(UserMapKVPair mapKVPair in actionMap.contentList)
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
        UserMapListElement actionMap = getConvMapByMapName("ObjectMap");
        List<string> re = new List<string>();
        foreach (UserMapKVPair mapKVPair in actionMap.contentList)
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
        UserMapListElement actionMap = getConvMapByMapName("BkgMap");
        List<string> re = new List<string>();
        foreach (UserMapKVPair mapKVPair in actionMap.contentList)
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
        UserMapListElement actionMap = getConvMapByMapName("BkgMap");
        List<string> re = new List<string>();
        foreach (UserMapKVPair mapKVPair in actionMap.contentList)
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
public class UserMapListElement
{
    public string mapName { set; get; }
    public List<UserMapKVPair> contentList { set; get; }

    public UserMapListElement(string mapName, List<UserMapKVPair> contentList)
    {
        this.mapName = mapName;
        this.contentList = contentList;
    }

    public bool containKey(List<string> keyList)
    {
        bool re = false;
        int keyCount = keyList.Count;
        foreach (UserMapKVPair el in contentList)
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
        foreach (UserMapKVPair el in contentList)
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
        foreach (UserMapKVPair el in contentList)
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

        foreach (UserMapKVPair el in contentList)
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
public class UserMapKVPair
{
    public string bookName;
    public List<List<string>> keyNameList { get; set; }
    public string valName { get; set; }

    public UserMapKVPair(List<List<string>> keyNameList, string valName,string bookName)
    {
        this.keyNameList = keyNameList;
        this.valName = valName;
        this.bookName = bookName;
    }

    
}