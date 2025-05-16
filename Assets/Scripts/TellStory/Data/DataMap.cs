using Assets.Scripts.TellStory.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;
using Interfaces;
public class DataMap
{

    //素材的原始名称映射表
    public static MapList convMatMapListRaw = new MapList();
    //素材的名称映射表
    public static MapList convMatMapList = new MapList();
    //二级映射
    public static UserMapList convMatMapListChild = new UserMapList();
    //用户指定的二级映射
    public static MapList userConvMatMapList = new MapList();
    //动画动作类型设置
    public static ActSettingList actSettingList = new ActSettingList();
    //素材信息设置
    public static MatSettingList matSettingList = new MatSettingList();

    public static EnvPointSettingList envPointSettingList = new EnvPointSettingList();
    //改变属性命令的信息
    public static AttrList attrList = new AttrList();
    public static PointSettingList pointSettingList = new PointSettingList();
    //public static MatInitSettingList matInitSettingList;//素材在不同背景中的初始化信息
    public static BkgSettingList bkgSettingList = new BkgSettingList();
    public static CorrectList correctList = new CorrectList();
    //复杂动作分类表
    public static DiyActList diyActList = new DiyActList();
    //复杂动作拆分表
    public static ActSplitList actSplitList = new ActSplitList();
    //相对位置
    public static RelatedPosList relatedPosList = new RelatedPosList();
    
   

    public static PrivateActorList privateActorList = new PrivateActorList();
    public static PrivatePropList privatePropList = new PrivatePropList();
    public static PrivateBackgroundList privateBackgroundList = new PrivateBackgroundList();

    public static PublicActorList publicActorList = new PublicActorList();
    public static PublicBackgroundList publicBackgroundList = new PublicBackgroundList();

    public static StyleList styleList = new StyleList();


    //解码的gif数据，用于播放gif动画
    public static MatGifCollection matGifCollection = new MatGifCollection();
    //gif动作映射
    public static GifMapAsset gifMapAsset;

    static string gifMapAssetPath = "ScriptableObject/GifMapAsset";




    //多角色指示连词
    public static List<string> actConFlag = new List<string> { "和", "与" ,"跟"};
    //属性变化动词
    public static List<string> attrChangeFlag = new List<string> { "变成", "变", "恢复", "变成了", "恢复了" };
    //出场词标识
    public static List<string> groundComeFlag = new List<string> { "回来","回到", "来到", "来", "到", "走到", "跑到", "走进", "走来", "跑进", "跳到", "跑了过来", "走了过来", "跑了进来","跑了出来", "走了进来","走了出来", "走过来", "跑过来", "跑来", "进来", "过来", "窜出来", "窜了出来", "冲进来", "冲了进来","冲了出来", "冲进", "窜过来", "窜了过来", "冲过来", "冲了过来", "跳进", "跳了进来","跳了出来", "跳过来", "跳了过来" };
    public static List<string> skyComeFlag = new List<string> { "飞到", "飞来", "飞了过来", "飞过来", "飞进来", "飞了进来","飞了出来" };
    public static List<string> waterComeFlag = new List<string> { "游到", "游来", "游了过来", "游过来", "游进来", "游了进来","游了出来" };
    public static List<string> comeFlag;
    //附近修饰词
    public static List<string> aroundFlag = new List<string> { "旁边", "身边", "身旁", "边上" };
    //代词
    public static List<string> singularPronounList = new List<string> { "他", "她", "它" };
    public static List<string> pluralPronounList = new List<string> { "他们", "她们", "它们", "大家", "大伙" };
    //截断词    
    public static List<string> speakFlag = new List<string> { "说", "想","想念", "问", "说道", "想道", "问道", "邀请", "请", "想要", "告诉" };
    public static List<string> denyFlag = new List<string> { "没", "无", "没有", "忘记", "忘", "不知道", "不能", "无法" };
    public static List<string> cutFlag;

    //指示句
    public static List<string> isaFlag = new List<string> { "叫", "名叫", "叫作","是","名字是" };

    //数量词映射
    public static Dictionary<string,int> numberDic = new Dictionary<string, int>
        {
            { "一",1},{ "二",2},{ "两",2},{ "三",3},{ "四",4},{ "五",5},{ "六",6},{ "七",7},{ "八",8},{ "九",9},{ "十",10}
        };
    //动作介词
    public static List<string> toActPrep = new List<string> { "向", "对", "朝" };


    public static float panleWidth = 0;
    public static float panelHeight = 0;

    public static bool posChanged = false;

    public static List<string> allRawNameList = new List<string>();

  

    //在得到json信息表后调用，进一步处理
    public static void initData()
    {
        
      
        comeFlag = new List<string>();
        comeFlag.AddRange(groundComeFlag);
        comeFlag.AddRange(waterComeFlag);
        comeFlag.AddRange(skyComeFlag);

        cutFlag = new List<string>();
        cutFlag.AddRange(speakFlag);
        cutFlag.AddRange(denyFlag);


        if (!userConvMatMapList.containMapName("ObjectMap"))
        {
            userConvMatMapList.map_list.Add(new MapListElement("ObjectMap", new List<MapKVPair>()));
        }

        if (!userConvMatMapList.containMapName("BkgMap"))
        {
            userConvMatMapList.map_list.Add(new MapListElement("BkgMap", new List<MapKVPair>()));
        }

        convMatMapList = DataUtil.Clone<MapList>(convMatMapListRaw);

        gifMapAsset = Resources.Load<GifMapAsset>(gifMapAssetPath);

        mergePrivateActorInfo();
        mergePrivatePropInfo();
        mergeBkgActorInfo();

        //mergePrivateActionInfo();

        genAllUserBkgPoint();

        allRawNameList.AddRange(convMatMapList.getAllActorAndPropRawName());
        allRawNameList.AddRange(convMatMapList.getAllBackgroundRawName());
        allRawNameList.AddRange(convMatMapList.getAllActionRawName());
        allRawNameList.AddRange(convMatMapListChild.getAllActionRawName());
        allRawNameList.AddRange(convMatMapListChild.getAllActionRawName());
        allRawNameList.AddRange(convMatMapListChild.getAllActionRawName());

        

    }


    static void  genAllUserBkgPoint()
    {
        foreach(PrivateBackground privateBackground in privateBackgroundList.privateBackgroundList)
        {
            string matName = privateBackground.Name;
            //自动生成背景信息
            if (DataMap.bkgSettingList.getElementByBkgName(matName) != null)
            {
                DataMap.bkgSettingList.delElementByBkgName(matName);
            }

            BkgSettingElement bkgSettingElement = new BkgSettingElement(matName);
            UserBkgPointGenerator.genAllDefaultPoint(bkgSettingElement);
            //转换坐标
            DataMap.changeAnElementPos(bkgSettingElement);
            DataMap.bkgSettingList.bkgSettingList.Add(bkgSettingElement);
        }
       
    }


    public static void mergeOnePrivateGifMap(string matName,string matActionName)
    {
        MapListElement mapListElement = convMatMapList.getConvMapByMapName("ActionMap");
        List<MapKVPair> mapKVPairs = mapListElement.contentList;

        string matRawName = privateActorList.FindPrivateActorByMatName(matName).RawName;
        List<string> actRawName = gifMapAsset.GetRawActName(matActionName);
        string actName = Server.USERNAME + "_" + matRawName + "_"+matActionName;

        List<List<string>> keyList = new List<List<string>>
            {
                new List<string> { matRawName },
                actRawName
            };
        if (mapListElement.containKey(new List<string> { matRawName, actRawName[0] }))
        {
            mapListElement.removeByKey(new List<string> { matRawName, actRawName[0] });
            
        }
        mapKVPairs.Add(new MapKVPair(keyList, actName));
        actSettingList.actSettingList.Add(new ActSettingElement(actName, "single_static"));


    }

    //将gif信息与本地素材库动作信息融合 需要将信息调整到
    public static void mergePrivateActionInfo()
    {
            
        foreach(PrivateActor privateActor in privateActorList.privateActorList)
        {
            foreach(GifMapElementAsset gifMapElementAsset in gifMapAsset.GifMapList)
            {
                string matActionName = gifMapElementAsset.actName;
                mergeOnePrivateGifMap(privateActor.Name, matActionName);
            }
                           
            
        }

    }


    //将privateActor的素材信息与公共素材信息合并
    public static void mergePrivateActorInfo()
    {
        
       // if (privateActorList == null) return;
        MapListElement mapListElement= convMatMapList.getConvMapByMapName("ObjectMap");
        List<MapKVPair> mapKVPairs = mapListElement.contentList;
        List<MatSettingElement> matSettingElements = matSettingList.matSettingList;
       

        foreach(PrivateActor privateActor in privateActorList.privateActorList)
        {
            //添加到名称映射表中
            string matRawName = privateActor.RawName;
            for (int i = mapKVPairs.Count - 1; i > 0; i--)
            {
                List<List<string>> tempkeyList = mapKVPairs[i].keyNameList;
                foreach (List<string> keys in tempkeyList)
                {
                    if (keys.Contains(matRawName))
                    {
                        mapKVPairs.RemoveAt(i);
                    }
                }
            }
            //AndroidUtils.ShowToast("已添加名称" + matRawName + "--" + privateActor.Name);
            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { privateActor.RawName });
            mapKVPairs.Add(new MapKVPair(keyList, privateActor.Name));
            
            //添加到素材设置表中
            MatSettingElement matSettingElement = new MatSettingElement(privateActor.Name, new List<float> { 30f, 30f, 30f },
               new List<float> { 0, 0, 0 }, new List<string> { "character", "ground" }, 1, 1, 1);

            matSettingElements.RemoveAll(data => (data.matName.Equals(matSettingElement.matName)));
            matSettingElements.Add(matSettingElement);
        }

    }


    public static void mergePrivatePropInfo()
    {
        // if (privateActorList == null) return;
        MapListElement mapListElement = convMatMapList.getConvMapByMapName("ObjectMap");
        List<MapKVPair> mapKVPairs = mapListElement.contentList;
        List<MatSettingElement> matSettingElements = matSettingList.matSettingList;

        foreach (PrivateProp privateProp in privatePropList.privatePropList)
        {
            //添加到名称映射表中
            string matRawName = privateProp.RawName;
            for (int i = mapKVPairs.Count - 1; i > 0; i--)
            {
                List<List<string>> tempkeyList = mapKVPairs[i].keyNameList;
                foreach (List<string> keys in tempkeyList)
                {
                    if (keys.Contains(matRawName))
                    {
                        mapKVPairs.RemoveAt(i);
                    }
                }
            }

            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { privateProp.RawName });
            mapKVPairs.Add(new MapKVPair(keyList, privateProp.Name));

            //添加到素材设置表中
            MatSettingElement matSettingElement = new MatSettingElement(privateProp.Name, new List<float> { 40f, 40f, 40f },
               new List<float> { 0, 0, 0 }, new List<string> { "prop" }, 1, 1, 1);

            matSettingElements.RemoveAll(data => (data.matName.Equals(matSettingElement.matName)));
            matSettingElements.Add(matSettingElement);
        }



    }


    public static void mergeBkgActorInfo()
    {
        // if (privateActorList == null) return;
        MapListElement mapListElement = convMatMapList.getConvMapByMapName("BkgMap");
        List<MapKVPair> mapKVPairs = mapListElement.contentList;
        

        foreach (PrivateBackground privateBackground in privateBackgroundList.privateBackgroundList)
        {
            //添加到名称映射表中
            string matRawName = privateBackground.RawName;
            for (int i = mapKVPairs.Count - 1; i > 0; i--)
            {
                List<List<string>> tempkeyList = mapKVPairs[i].keyNameList;
                foreach (List<string> keys in tempkeyList)
                {
                    if (keys.Contains(matRawName))
                    {
                        mapKVPairs.RemoveAt(i);
                    }
                }
            }
            //AndroidUtils.ShowToast("已添加名称" + matRawName + "--" + privateActor.Name);
            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { privateBackground.RawName });
            mapKVPairs.Add(new MapKVPair(keyList, privateBackground.Name));

          
        }

    }

    //转换坐标
    public static void changeAllPos(float screenHeight, float screenWidth, float pivotX, float pivotY)
    {
        panelHeight = screenHeight;
        panleWidth = screenWidth;
        if (posChanged) return;

        posChanged = true;
        foreach (BkgSettingElement bkgSettingElement in bkgSettingList.bkgSettingList)
        {
            foreach (List<float> pos in bkgSettingElement.initPositionList)
            {
                pos[0] = pos[0] * screenWidth / 2f + pivotX;
                pos[1] = pos[1] * screenHeight / 2f + pivotY;

            }
        }      

    }

    public static bool changeAnElementPos(BkgSettingElement bkgSettingElement)
    {

        if (panelHeight == 0 || panleWidth == 0) return false;

        foreach (List<float> pos in bkgSettingElement.initPositionList)
        {
            pos[0] = pos[0] * panleWidth / 2f;
            pos[1] = pos[1] * panelHeight / 2f;

        }
        return true;
    }


    //查询名称映射，先二级再一级
    public static string GetObjNameMapValue(List<string>keyList)
    {
        string re = "";

        MapListElement firstMap = convMatMapList.getConvMapByMapName("ObjectMap");
        UserMapListElement secondMap = convMatMapListChild.getConvMapByMapName("ObjectMap");
       
        //二级映射表存在key
        if (secondMap.containKey(keyList))
        {
            string secondKeyName = secondMap.getValByKeyList(keyList);
            if (firstMap.containKey(new List<string> { secondKeyName}))
            {
                re = firstMap.getValByKeyList(new List<string> { secondKeyName });
            }
        }

        if (re == "")
        {
            if (firstMap.containKey(keyList))
            {
                re = firstMap.getValByKeyList(keyList); 
            }
        }
      
        return re;
    }

    public static bool ContainObjNameMapKey(List<string>keyList)
    {
        MapListElement firstMap = convMatMapList.getConvMapByMapName("ObjectMap");
        UserMapListElement secondMap = convMatMapListChild.getConvMapByMapName("ObjectMap");
        //二级映射表存在key
        if (secondMap.containKey(keyList))
        {
            string secondKeyName = secondMap.getValByKeyList(keyList);
            if (firstMap.containKey(new List<string> { secondKeyName }))
            {
                return true;
            }
        }
        if (firstMap.containKey(keyList))
        {
            return true;
        }


        return false ;
    }

    public static string GetActNameMapValue(List<string> keyList)
    {
        string objName = keyList[0];
        if(convMatMapListChild.getConvMapByMapName("ObjectMap").containKey(new List<string> { objName }))
        {
            keyList[0] = convMatMapListChild.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { keyList[0] });
        }
        string re = "";
        MapListElement firstMap = convMatMapList.getConvMapByMapName("ActionMap");
        if (firstMap.containKey(keyList))
        {
            re = firstMap.getValByKeyList(keyList);
        }
        return re;
    }

    public static bool ContainActNameMapKey(List<string> keyList)
    {
        string objName = keyList[0];
        if (convMatMapListChild.getConvMapByMapName("ObjectMap").containKey(new List<string> { objName }))
        {
            keyList[0] = convMatMapListChild.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { keyList[0] });
        }
        MapListElement firstMap = convMatMapList.getConvMapByMapName("ActionMap");
        if (firstMap.containKey(keyList))
        {
            return true;
        }
        return false;
    }


    public static string GetBkgNameMapValue(List<string> keyList)
    {
        string re = "";

        MapListElement firstMap = convMatMapList.getConvMapByMapName("BkgMap");
        UserMapListElement secondMap = convMatMapListChild.getConvMapByMapName("BkgMap");
        //二级映射表存在key
        if (secondMap.containKey(keyList))
        {
            string secondKeyName = secondMap.getValByKeyList(keyList);
            if (firstMap.containKey(new List<string> { secondKeyName }))
            {
                re = firstMap.getValByKeyList(new List<string> { secondKeyName });
            }
        }

        if (re == "")
        {
            if (firstMap.containKey(keyList))
            {
                re = firstMap.getValByKeyList(keyList);
            }
        }

        return re;
    }

    public static bool ContainBkgNameMapKey(List<string> keyList)
    {
        MapListElement firstMap = convMatMapList.getConvMapByMapName("BkgMap");
        UserMapListElement secondMap = convMatMapListChild.getConvMapByMapName("BkgMap");
        //二级映射表存在key
        if (secondMap.containKey(keyList))
        {
            string secondKeyName = secondMap.getValByKeyList(keyList);
            if (firstMap.containKey(new List<string> { secondKeyName }))
            {
                return true;
            }
        }
        if (firstMap.containKey(keyList))
        {
            return true;
        }


        return false;
    }






}
