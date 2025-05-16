using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPinyin;
public class WordInspector
{
    

    //key为汉字，value为拼音
    Dictionary<string, string> pinyinDic = new Dictionary<string, string>();

    public WordInspector()
    {
        initPinyinDic();
    }

    public string getSamePinyinWord(string word)
    {
        string re = "";
        string inputPinyin = Pinyin.GetPinyin(word);
        foreach(string wordKey in this.pinyinDic.Keys)
        {
            if (pinyinDic[wordKey].Equals(inputPinyin))
            {
                re = wordKey;
            }
        }
        return re;
    }

    private void initPinyinDic()
    {
        List<MapKVPair> mapListElementsObj = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").contentList;
        List<MapKVPair> mapListElementsBkg = DataMap.convMatMapList.getConvMapByMapName("BkgMap").contentList;
        List<UserMapKVPair> mapListElementObjChild = DataMap.convMatMapListChild.getConvMapByMapName("ObjectMap").contentList;
        List<UserMapKVPair> mapListElementBkgChild = DataMap.convMatMapListChild.getConvMapByMapName("BkgMap").contentList;
        //mapListElements.AddRange( DataMap.convMatMapList.getConvMapByMapName("BkgMap").contentList);
        List<MapKVPair> mapListElements = new List<MapKVPair>();
        List<UserMapKVPair> userMapListElements = new List<UserMapKVPair>();
        mapListElements.AddRange(mapListElementsObj);
        mapListElements.AddRange(mapListElementsBkg);
        userMapListElements.AddRange(mapListElementBkgChild);
        userMapListElements.AddRange(mapListElementObjChild);
        foreach (MapKVPair mapKVPair in mapListElements)
        {
            foreach (List<string> keynameList in mapKVPair.keyNameList)
            {
                foreach (string keyname in keynameList)
                {
                    if (!pinyinDic.ContainsKey(keyname) && !DataUtil.containEn(keyname))
                    {
                        string pinyin = Pinyin.GetPinyin(keyname);
                        pinyinDic.Add(keyname, pinyin);
                    }
                }
            }
        }

       foreach(PointSettingElement pointSettingElement in DataMap.pointSettingList.pointSettingList)
        {
            foreach(string locName in pointSettingElement.locationFlags)
            {
                if (!pinyinDic.ContainsKey(locName))
                {
                    pinyinDic.Add(locName, Pinyin.GetPinyin(locName));
                }
               
            }
            
        }

        foreach (UserMapKVPair mapKVPair in userMapListElements)
        {
            foreach (List<string> keynameList in mapKVPair.keyNameList)
            {
                foreach (string keyname in keynameList)
                {
                    if (!pinyinDic.ContainsKey(keyname) && !DataUtil.containEn(keyname))
                    {
                        string pinyin = Pinyin.GetPinyin(keyname);
                        pinyinDic.Add(keyname, pinyin);
                    }
                }
            }
        }

    }

}


