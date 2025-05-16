using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Extractor
{
    protected bool bkgChange = false;
    protected string bkgName = "";

    public List<KeyValuePair<string, string>> pairList { set; get; }

    public List<BasicEvent> eventList { set; get; }

    //当前类别的事件开始时间单位
    public float eventSeqNum { set; get; }

    //当前类别的事件数量
    public int eventNum { set; get; }
    //类别事件的总时间单位,在初始化时指定
    public float eventsLen { set; get; }
    protected Recorder recorder { set; get; }


    public Extractor(List<KeyValuePair<string, string>> pairList,float eventSeqNum,float eventsLen,Recorder recorder,bool bkgChange,string bkgName)
    {
        this.bkgChange = bkgChange;
        this.bkgName = bkgName;

        this.pairList = pairList;
        this.eventSeqNum = eventSeqNum;
        this.eventsLen = eventsLen;
        this.recorder = recorder;
        eventList = new List<BasicEvent>();
        this.eventNum = 0;//某个类别的事件数量
    }
    //从pairList中提取事件到eventList
    public abstract void extractEvent();

    //计算每个事件的开始时间和持续时间
    public void reCalStartAndDuration()
    {
        if (eventList.Count == 0) return;
        foreach(BasicEvent basicEvent in eventList)
        {
            //一个时间单位的真实时间
            float perDuration = eventsLen / eventNum;
            //一个事件的真实时间
            basicEvent.duration = perDuration*basicEvent.duration;
            //一个事件的真实开始时间
            basicEvent.startTime = eventSeqNum + basicEvent.startTime * perDuration;
        }
    }

    //获取某些词性的index
    protected List<int> getIndexBySymbol(List<string> symbolList)
    {
        List<int> re = new List<int>();
        for (int i = 0; i < pairList.Count; i++)
        {
            if (symbolList.Contains(pairList[i].Key)) re.Add(i);
        }
        return re;
    }


    protected bool containFlag(int leftIndex,int rightIndex,List<string> flags)
    {
        for(int i = leftIndex; i <= rightIndex; i++)
        {
            if (flags.Contains(pairList[i].Value))
            {
                return true;
            }
        }
        return false;
    }

    //得到所有物体词索引
    protected List<int> getSubjectIndexList()
    {
        List<int> subjectNameList = new List<int>();
        //得到所有nr词性
        List<string> objSymbols = new List<string> { "nr", "n" };
        List<int> objIndexList = getIndexBySymbol(objSymbols);

        if (objIndexList.Count != 0)
        {
            foreach (int index in objIndexList)
            {
                //检查是否在表中
                string rawSubjectName = pairList[index].Value;
               
                if (DataMap.ContainObjNameMapKey(new List<string> { rawSubjectName }))
                {
                    subjectNameList.Add(index);
                }

            }

        }

        return subjectNameList;
    }


    protected string getNearestLocationFlagBySub(int subIndex)
    {
        string locationFlag = getNearestLocationFlagBySub(subIndex, false);
        if (locationFlag.Equals(""))
        {
            locationFlag = getNearestLocationFlagBySub(subIndex, true);
        }
        if (!locationFlag.Equals(""))
        {
            return locationFlag;
        }
        else
        {
            return "";
        }
    }

    protected string getNearestActionFlagBySub(int subIndex)
    {
        string locationFlag = getNearestActionFlagBySub(subIndex, false);
        if (locationFlag.Equals(""))
        {
            locationFlag = getNearestActionFlagBySub(subIndex, true);
        }
        if (!locationFlag.Equals(""))
        {
            return locationFlag;
        }
        else
        {
            return "";
        }
    }

    //获取一个subject前/后最近的一个locationFlag
    protected string getNearestLocationFlagBySub(int subIndex, bool left)
    {
       
        string re = "";
        if (left)
        {
            for (int i = subIndex; i >= 0; i--)
            {
                string name = pairList[i].Value;
                PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByLocationFlag(name);
                if (pointSettingElement != null)
                {
                    re = pointSettingElement.pointType;
                    break;
                }


            }
        }
        else
        {
            for (int i = subIndex; i < pairList.Count; i++)
            {
                string name = pairList[i].Value;
                PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByLocationFlag(name);
                if (pointSettingElement != null)
                {
                    re = pointSettingElement.pointType;
                    break;
                }


            }
        }

        return re;
    }
    //获取一个subject前/后最近的一个locationFlag
    protected string getNearestActionFlagBySub(int subIndex, bool left)
    {

        string re = "";
        if (left)
        {
            for (int i = subIndex; i >= 0; i--)
            {
                string name = pairList[i].Value;
                PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByActionFlag(name);
                if (pointSettingElement != null)
                {
                    re = pointSettingElement.pointType;
                    break;
                }


            }
        }
        else
        {
            for (int i = subIndex; i < pairList.Count; i++)
            {
                string name = pairList[i].Value;
                PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByActionFlag(name);
                if (pointSettingElement != null)
                {
                    re = pointSettingElement.pointType;
                    break;
                }


            }
        }

        return re;
    }

    //查找所有locationFlag,用于背景切换
    protected List<string> getAllLocationFlag()
    {
        List<string> re = new List<string>();
        for (int i = 0; i < pairList.Count; i++)
        {
            string name = pairList[i].Value;
            PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByLocationFlag(name);
            if (pointSettingElement != null)
            {
                re.Add(pointSettingElement.pointType);
               
            }


        }
        return re;
    }


    //查找所有ActionFlag，用于背景切换
    protected List<string> getAllActionFlag()
    {
        List<string> re = new List<string>();
        for (int i = 0; i < pairList.Count; i++)
        {
            string name = pairList[i].Value;
            PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByActionFlag(name);
            if (pointSettingElement != null)
            {
                re.Add(pointSettingElement.pointType);

            }


        }
        return re;
    }


    protected string getLocationFlagByIndex(int index1,int index12)
    {
        string re = "";
        for (int i = index1; i <= index12; i++)
        {
            string name = pairList[i].Value;
            PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByLocationFlag(name);
            if (pointSettingElement != null)
            {
                re = pointSettingElement.pointType;
                break;
            }


        }
        return re;


    }


    protected string getNearestAllFlagBySub(int subIndex)
    {
        //string locationFlag = getNearestLocationFlagBySub(subIndex);

        int lastActI = getLastActI(subIndex);
        int nextActI = getNextActI(subIndex);
        string locationFlag = getLocationFlagByIndex(lastActI, nextActI);
        if (!locationFlag.Equals(""))
        {
            return locationFlag;
        }
        return "";
    }

    protected string getNearestAllFlagByAct(int actI)
    {
        int lastActI = getLastActI(actI);
        int nextActI = getNextActI(actI);
        /*
        string locationFlag = getNearestLocationFlagBySub(subIndex, false);
        if (locationFlag.Equals(""))
        {
            locationFlag = getNearestLocationFlagBySub(subIndex, true);
        }

        string actionFlag = getNearestActionFlagBySub(subIndex, false);
        if (actionFlag.Equals(""))
        {
            actionFlag = getNearestAllFlagBySub(subIndex, true);
        }
        */
        //优先location
        string locationFlag = getLocationFlagByIndex(lastActI, nextActI);
        if (!locationFlag.Equals(""))
        {
            return locationFlag;
        }

        string actionFlag = "";
        string actionName = pairList[actI].Value;
        PointSettingElement pointSettingElement = DataMap.pointSettingList.GetPointSettingElementByActionFlag(actionName);
        if (pointSettingElement != null)
        {
            actionFlag = pointSettingElement.pointType;
            
        }

        return actionFlag;
    }

  

    //获取下一个动词index
    protected int getNextActI(int actI)
    {
        List<string> actSymbols = new List<string> { "v", "i" };
        List<int> actIndexList = getIndexBySymbol(actSymbols);
        List<string> AllActRawNameList = DataMap.convMatMapList.getAllActionRawName();
        List<string> AllDiyRawNameList = DataMap.diyActList.getAllDiyRawName();
        AllActRawNameList.AddRange(AllDiyRawNameList);
        int nextActI = pairList.Count - 1;
        for (int i = actI; i < pairList.Count; i++)
        {
            if (AllActRawNameList.Contains(pairList[i].Value) && i != actI)
            {
                nextActI = i;
                break;
            }
        }

        return nextActI;
    }

    //获取上一个动词index
    protected int getLastActI(int actI)
    {
        List<string> actSymbols = new List<string> { "v", "i" };
        List<int> actIndexList = getIndexBySymbol(actSymbols);
        List<string> AllActRawNameList = DataMap.convMatMapList.getAllActionRawName();
        List<string> AllDiyRawNameList = DataMap.diyActList.getAllDiyRawName();
        AllActRawNameList.AddRange(AllDiyRawNameList);
        int lastActI = 0;
        for (int i = actI; i >= 0; i--)
        {
            if (AllActRawNameList.Contains(pairList[i].Value) && i != actI)
            {
                lastActI = i;
                break;
            }
        }

        return lastActI;
    }

    //找出一个动作之前最近的,可以组合的对象的位置,没有找到则返回-1
    protected int getActionLeftNearestObj(List<KeyValuePair<string, string>> pairList, int actIndex)
    {
        int re = -1;
        string actName = pairList[actIndex].Value;

        for (int i = actIndex; i >= 0; i--)
        {

            string objName = pairList[i].Value;
            if (DataMap.ContainObjNameMapKey(new List<string> { objName }))
            {
                re = i;
                break;
            }
        }
        return re;
    }

    //找出一个index之后最近的角色
    protected int getIndexRightNearestObj(List<KeyValuePair<string, string>> pairList, int mIndex)
    {
        int re = -1;
        
        for (int i = mIndex; i < pairList.Count; i++)
        {
            string objName = pairList[i].Value;
            if (DataMap.ContainObjNameMapKey(new List<string> { objName }))
            {
                re = i;
                break;
            }
        }
        return re;
    }
    //找出一个index之前最近的角色
    protected int getIndexLeftNearestObj(List<KeyValuePair<string, string>> pairList, int mIndex,int leftEdge)
    {
        int re = -1;
        if (mIndex -leftEdge < 0) return -1;
        for (int i = mIndex; i >= leftEdge; i--)
        {
            string objName = pairList[i].Value;
            if (DataMap.ContainObjNameMapKey(new List<string> { objName }))
            {
                re = i;
                break;
            }
        }
        return re;
    }


   

    //找出一个index之前的所有角色
    protected List<int> getAllObjIgnoreP(int leftEdge, int mIndex)
    {
        List<int> re = new List<int>();

        for (int i = leftEdge; i <= mIndex; i++)
        {
            string objName = pairList[i].Value;
            if (DataMap.ContainObjNameMapKey(new List<string> { objName }))
            {
                re.Add(i);
               
            }
        }
        return re;
    }

    public List<int> getAllHandleActIndex(int leftEdge,int rightEdge)
    {
        List<int> re = new List<int>();
        List<string> symbolList = new List<string> { "v", "i" };
        List<string> actRawList = DataMap.convMatMapList.getAllActionRawName();
        for(int i = leftEdge; i <= rightEdge; i++)
        {
            if(symbolList.Contains(pairList[i].Key) && actRawList.Contains(pairList[i].Value))
            {
                re.Add(i);
            }
        }
        return re;

    }
  

    protected int getIndexLeftNearestSymbol(int leftEdge,int masterIndex, List<string> symbolList)
    {
        int re = -1;

        for (int i = masterIndex; i >= leftEdge; i--)
        {
            
            if (symbolList.Contains(pairList[i].Key))
            {
                re = i;
                break;
            }
        }
        return re;
    }

    protected int getIndexRightNearestSymbol(List<KeyValuePair<string, string>> pairList,int rightEdge, int masterIndex, List<string> symbolList)
    {
        int re = -1;

        for (int i = masterIndex; i < rightEdge; i++)
        {

            if (symbolList.Contains(pairList[i].Key))
            {
                re = i;
                break;
            }
        }
        return re;
    }

    protected List<int> getIndexBetweenWord(int leftEdge,int masterIndex,List<string> wordList)
    {
        List<int> re = new List<int>();
        for (int i = masterIndex; i >= leftEdge; i--)
        {

            if (wordList.Contains( pairList[i].Value))
            {
                re.Add(i);
                break;
            }
        }
        return re;
    }

    protected int getIndexRightNearestWord(List<KeyValuePair<string, string>> pairList, int actIndex, string word)
    {
        int re = -1;
        for (int i = actIndex; i < pairList.Count; i++)
        {

            if (pairList[i].Value.Equals(word))
            {
                re = i;
                break;
            }
        }
        return re;
    }

    //根据和与连词/全 助词探查所有的角色列表
    protected List<int> getMultiObjNearAct(int edgeObjIndex,int actIndex)
    {
        List<int> re = new List<int>();
        re.Add(edgeObjIndex);
        //检查有没有连词
        int tempIndex = edgeObjIndex;
        while (tempIndex > 0)
        {

            if (tempIndex - 2 >= 0  && DataMap.actConFlag.Contains(pairList[tempIndex - 1].Value))
            {
                if ( DataMap.ContainObjNameMapKey(new List<string> { pairList[tempIndex-2].Value}))
                {
                    re.Add(tempIndex - 2);
                }
                tempIndex = tempIndex - 2;
            }
            else
            {
                tempIndex--;
            }
        }
        
       
        return re;
    }

    protected int getIndexByValNameFirst(string valName)
    {
        int re = -1;
        for(int i = 0; i < pairList.Count; i++)
        {
            if (pairList[i].Value.Equals(valName))
            {
                re = i;
                break;
            }
        }
        return re;

    }

    //获取两个index之间的所有角色，考虑量词,不考虑代词，即没有检测到角色就返回空列表
    protected Dictionary<string,int> getAllObjIgnorePConM(int leftEdge, int masterIndex, List<string> exceptList)
    {
        if (exceptList == null)
        {
            exceptList = new List<string>();
        }
        List<string> exceptMatList = new List<string>();
        foreach(string exceptRawname in exceptList)
        {
            
           // string exceptMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { exceptRawname });
            string exceptMatName = DataMap.GetObjNameMapValue(new List<string> { exceptRawname });
            exceptMatList.Add(exceptMatName);
        }
        //List<string> re = new List<string>();
        Dictionary<string, int> re = new Dictionary<string, int>();
        List<int> subList = getAllObjIgnoreP(leftEdge, masterIndex);
        for (int i = 0; i < subList.Count; i++)
        {
            string subRawName = pairList[subList[i]].Value;

            if (DataMap.ContainObjNameMapKey(new List<string> { subRawName })
                && !exceptMatList.Contains(DataMap.GetObjNameMapValue(new List<string> { subRawName })))
            {
                string matName = DataMap.GetObjNameMapValue(new List<string> { subRawName });
                MatSettingElement matSetting = DataMap.matSettingList.getMatSettingMap(matName);
                if (matSetting.classList.Contains("prop"))
                {
                    //检测量词
                    if (subList[i] - 1 >= 0 && pairList[subList[i] - 1].Key.Equals("m"))
                    {
                        int num = 1;
                        string mayNum = pairList[subList[i] - 1].Value;
                        foreach (string numStr in DataMap.numberDic.Keys)
                        {
                            if (pairList[subList[i] - 1].Value.Contains(numStr))
                            {
                                num = DataMap.numberDic[numStr];
                                break;
                            }
                        }

                        if (num >= 1)
                        {
                            string tempName = pairList[subList[i]].Value;
                            //加如指定数量的该物品
                            if (re.ContainsKey(tempName))
                            {
                                int newNum = re[tempName] + num;
                                re[tempName] = newNum;
                            }
                            else
                            {
                                re.Add(pairList[subList[i]].Value, num);
                            }
                            
                           
                        }

                    }
                    else
                    {
                        string tempName = pairList[subList[i]].Value;

                        if (!re.ContainsKey(tempName))
                        {
                            re.Add(tempName, 1);
                        }
                        
                    }

                }
                else
                {

                    if (!re.ContainsKey(subRawName))
                    {
                        re.Add(subRawName,1);
                    }
                }

            }
           
        }
        return re;
    }

    //获取上一个场景中的角色带入指代词
    protected List<string> getLastSceneObj(string pnName,List<string> exceptRawNameList)
    {
        List<string> re = new List<string>();
        if (exceptRawNameList == null)
        {
            exceptRawNameList = new List<string>();
        }

        List<string> exceptMatNameList = new List<string>();
        foreach(string rawName in exceptRawNameList)
        {
            //string matName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { rawName });
            string matName = DataMap.GetObjNameMapValue(new List<string> { rawName });
            exceptMatNameList.Add(matName);
        }

        //查找代词
        //int pnI = getIndexLeftNearestSymbol( leftEdge, masterIndex, new List<string> { "r" });
        if (pnName==null ||  pnName=="" || recorder.fileNum==0 ) return re;
        else
        {
            Dictionary<string, int> memDic = RecorderList.getRecordByNum(recorder.fileNum - 1).goMemDic;
            List<string> subRawList = RecorderList.getRecordByNum(recorder.fileNum - 1).subRawNameList;
            if (DataMap.singularPronounList.Contains(pnName))
            {

                string sub = memDic.Last().Key;
                if (subRawList.Count != 0)
                {
                    //sub=DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { subRawList[0] });
                    sub = DataMap.GetObjNameMapValue(new List<string> { subRawList[0] });
                    //sub = subRawList[0];
                }
               
                
                if (!exceptMatNameList.Contains(sub))
                {
                    string rawSubName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(sub);
                    re.Add(rawSubName);
                }
                
            }
            else if (DataMap.pluralPronounList.Contains(pnName))
            {
                foreach(string subName in memDic.Keys)
                {
                    List<string> classList = DataMap.matSettingList.getMatSettingMap(subName).classList;
                    if (!classList.Contains("character")) continue;
                    if (!exceptMatNameList.Contains(subName))
                    {
                        for (int n = 0; n < memDic[subName]; n++)
                        {
                            string rawSubName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(subName);
                            if (rawSubName.Equals("")) continue;
                            re.Add(rawSubName);
                        }
                    }
                   
                                      
                }
            }
            else
            {
                return re;
            }
        }
        return re;
    }


    public List<string> getInitSubBetweenIndex(int lastActIndex, int actI, List<List<string>> lastSubList, List<string> exceptionList)
    {
        if (exceptionList == null)
        {
            exceptionList = new List<string>();
        }
        if (lastSubList == null)
        {
            lastSubList = new List<List<string>>();
        }

        Dictionary<string,int> rawSubDic= getAllObjIgnorePConM(lastActIndex, actI, exceptionList);
        List<string> rawSubList = new List<string>();
        foreach(string key in rawSubDic.Keys)
        {
            for(int i = 0; i < rawSubDic[key]; i++)
            {
                rawSubList.Add(key);
            }
        }

        
        //check 有无代词
        int pnI = getIndexLeftNearestSymbol(lastActIndex, actI, new List<string> { "r" });
        //代入代词的结果
        List<string> pRawList = new List<string>();
        if (pnI != -1)
        {

            List<int> beforePCha = getAllObjIgnoreP(0, pnI);

            if (DataMap.singularPronounList.Contains(pairList[pnI].Value)||
                (DataMap.pluralPronounList.Contains(pairList[pnI].Value)&&beforePCha.Count>1))
            {
                foreach(int chaIndex in beforePCha)
                {
                    pRawList.Add(pairList[chaIndex].Value);
                }
                
            }
              
            //从上一个场景里提取
            if (pRawList.Count == 0)
            {

                List<string> lastSubName = getLastSceneObj(pairList[pnI].Value, rawSubList);
                if (lastSubName.Count != 0)
                {
                    if (DataMap.singularPronounList.Contains(pairList[pnI].Value))
                    {
                        pRawList.Add(lastSubName[0]);
                    }
                    else if (DataMap.pluralPronounList.Contains(pairList[pnI].Value))
                    {
                        pRawList.AddRange(lastSubName);
                    }
                }


            }

        }

        //rawSubList = mergeRawP(lastActIndex, actI, pnI, pRawList, rawSubList, exceptionList);
        
        foreach (string prawname in pRawList)
        {
            if (!containRawName(rawSubList, prawname) && !containRawName(exceptionList, prawname))
            {
                rawSubList.Add(prawname);
            }
        }
        

        
       
        //既没有代词也没有找到角色词,直接带入上个短句的角色
        if (rawSubList.Count == 0)
        {
            if (lastSubList.Count != 0)
            {

                bool isAdded = false;
                for (int i = lastSubList.Count - 1; i >= 0; i--)
                {
                    if (isAdded) break;
                    List<string> pNameList = DataUtil.Clone<List<string>>(lastSubList[i]);

                    for (int j = 0; j < pNameList.Count; j++)
                    {
                        //string tempMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { pNameList[j] });
                        //if (!exceptMatList.Contains(tempMatName) && !rawSubList.Contains(tempMatName))
                        if (!containRawName(exceptionList, pNameList[j]) && !containRawName(rawSubList, pNameList[j]))
                        {
                            rawSubList.Add(pNameList[j]);
                            isAdded = true;
                            break;
                        }
                    }
                }
            }

        }      

        return rawSubList;

    }

    public List<string> getSubBetweenIndex(int lastActIndex, int actI, List<List<string>>lastSubList,List<string> exceptionList)
    {
        if (exceptionList == null)
        {
            exceptionList = new List<string>();
        }
        if (lastSubList == null)
        {
            lastSubList = new List<List<string>>();
        }

        Dictionary<string, int> rawSubDic = getAllObjIgnorePConM(lastActIndex, actI, exceptionList);
        List<string> rawSubList = new List<string>();
        foreach (string key in rawSubDic.Keys)
        {
            for (int i = 0; i < rawSubDic[key]; i++)
            {
                rawSubList.Add(key);
            }
        }
        //check 有无代词
        int pnI = getIndexLeftNearestSymbol(lastActIndex, actI, new List<string> { "r" });
        //代入代词的结果
        List<string> pRawList = new List<string>();
        if (pnI != -1)
        {          
            //从之前的短句中提取
            if (lastSubList.Count != 0)
            {
                
                //单数代词
                if (DataMap.singularPronounList.Contains(pairList[pnI].Value))
                {
                    bool isAdded = false;
                    for (int i = lastSubList.Count - 1; i >= 0; i--)
                    {
                        if (isAdded) break;

                        List<string>pNameList = DataUtil.Clone<List<string>>(lastSubList[i]);
                        for (int j = 0; j < pNameList.Count; j++)
                        {
                            //string tempMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { pNameList[j] });
                            if ( !containRawName(rawSubList,pNameList[j]) && !containRawName(pRawList,pNameList[j]))
                            {
                                pRawList.Add(pNameList[j]);
                                isAdded = true;
                                break;
                            }
                        }
                    }
                   
                }
                //复数代词
                else if (DataMap.pluralPronounList.Contains(pairList[pnI].Value))
                {                   
                    for (int i = lastSubList.Count - 1; i >= 0; i--)
                    {
                        //if (isAdded) break;
                        List<string>pNameList = DataUtil.Clone<List<string>>(lastSubList[i]);
                        //复数代词不能用单个角色带入
                        if (pNameList.Count < 2) continue;
                        List<string> tempAddedList = new List<string>();
                        for (int j = 0; j < pNameList.Count; j++)
                        {
                            if ( !containRawName(rawSubList, pNameList[j]) && !containRawName(pRawList, pNameList[j]))
                            {
                                tempAddedList.Add(pNameList[j]);
                                //rawSubList.Add(pNameList[j]);
                               // isAdded = true;
                               // break;
                            }
                           
                        }
                        if (tempAddedList.Count > 0)
                        {
                            //isAdded = true;
                            pRawList.AddRange(tempAddedList);
                            break;
                        }
                    }
                 
                }

            }
            //从上一个场景里提取
            if(pRawList.Count==0)
            {

                List<string> lastSubName = getLastSceneObj(pairList[pnI].Value, rawSubList);
                if (lastSubName.Count != 0)
                {
                    if (DataMap.singularPronounList.Contains(pairList[pnI].Value))
                    {
                        pRawList.Add(lastSubName[0]);
                    }
                    else if (DataMap.pluralPronounList.Contains(pairList[pnI].Value))
                    {
                        pRawList.AddRange(lastSubName);
                    }
                }
               
                    
            }

        }

        //将pname加入到rawSubList
        //List<string> pMatNameList = new List<string>();
        rawSubList = mergeRawP(lastActIndex, actI, pnI, pRawList, rawSubList, exceptionList);

        //排除被吃的角色

        rawSubList= delEatenObj(rawSubList);


        //既没有代词也没有找到角色词,直接带入上个短句的角色
        if (rawSubList.Count == 0)
        {
            if (lastSubList.Count != 0)
            {

                bool isAdded = false;
                for (int i = lastSubList.Count - 1; i >= 0; i--)
                {
                    if (isAdded) break;
                    List<string> pNameList = DataUtil.Clone<List<string>>(lastSubList[i]);

                    for (int j = 0; j < pNameList.Count; j++)
                    {
                        //string tempMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { pNameList[j] });
                        //if (!exceptMatList.Contains(tempMatName) && !rawSubList.Contains(tempMatName))
                        if (!containRawName(exceptionList,pNameList[j]) && !containRawName(rawSubList,pNameList[j]))
                        {
                            rawSubList.Add(pNameList[j]);
                            isAdded = true;
                            
                        }
                    }
                }
            }
                                                   
        }

        //排除被吃的角色
        rawSubList = delEatenObj(rawSubList);

        return rawSubList;
       
    }


    //消解代词
    public List<string> getPnObj(string pnName, List<List<string>> lastSubObjList)
    {
        
        
        if (lastSubObjList == null)
        {
            lastSubObjList = new List<List<string>>();
        }

      
        List<string> pRawList = new List<string>();

        //从之前短句中提取
        if (lastSubObjList.Count != 0)
        {
            //单数代词
            if (DataMap.singularPronounList.Contains(pnName))
            {
                bool isAdded = false;
                for (int i = lastSubObjList.Count - 1; i >= 0; i--)
                {
                    if (isAdded) break;

                    List<string> pNameList = DataUtil.Clone<List<string>>(lastSubObjList[i]);
                    for (int j = 0; j < pNameList.Count; j++)
                    {
                        //string tempMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { pNameList[j] });
                        if (!containRawName(pRawList, pNameList[j]))
                        {
                            pRawList.Add(pNameList[j]);
                            isAdded = true;
                            break;
                        }
                    }
                }

            }

            //复数代词
            else if (DataMap.pluralPronounList.Contains(pnName))
            {
                for (int i = lastSubObjList.Count - 1; i >= 0; i--)
                {                    
                    List<string> pNameList = DataUtil.Clone<List<string>>(lastSubObjList[i]);
                    //复数代词不能用单个角色带入
                    if (pNameList.Count < 2) continue;
                    List<string> tempAddedList = new List<string>();
                    for (int j = 0; j < pNameList.Count; j++)
                    {
                        if (containRawName(pRawList, pNameList[j]))
                        {
                            tempAddedList.Add(pNameList[j]);                           
                        }

                    }
                    if (tempAddedList.Count > 0)
                    {
                        pRawList.AddRange(tempAddedList);
                        break;
                    }
                }

            }
        }

        //从上一个场景里提取
        if (pRawList.Count == 0)
        {

            List<string> lastSubName = getLastSceneObj(pnName, null);
            if (lastSubName.Count != 0)
            {
                if (DataMap.singularPronounList.Contains(pnName))
                {
                    pRawList.Add(lastSubName[0]);
                }
                else if (DataMap.pluralPronounList.Contains(pnName))
                {
                    pRawList.AddRange(lastSubName);
                }
            }


        }

        //排除被吃的角色
        pRawList = delEatenObj(pRawList);

        return pRawList;

    }


    //排除吃的角色
    List<string> delEatenObj(List<string> rawList)
    {
        List<string> re = new List<string>();
        if (rawList.Count != 0)
        {
            //List<string> newRawSubList = new List<string>();
            foreach (string actorName in rawList)
            {
                if (!isEatObject(actorName))
                {
                    re.Add(actorName);
                }
            }
            
        }
        return re;
    }

    //将pname加入到rawSubList
    //根据有没有连词c决定是否合并
    //如果不这样，无法确定是一句中还是两句中
    //List<string> pMatNameList = new List<string>();
    List<string> mergeRawP(int lastActIndex,int actI,int pIndex,List<string> pRawList,List<string> rawSubList,List<string> exceptionList)
    {
        List<string> re = new List<string>();
        if (pRawList.Count == 0 && rawSubList.Count == 0) return re;
        if (pRawList.Count == 0 && rawSubList.Count != 0)
        {
            re.AddRange(rawSubList);
            return re;
        }
        if (pRawList.Count != 0 && rawSubList.Count == 0)
        {
            re.AddRange(pRawList);
            return re;
        }

        if ( containFlag(lastActIndex, actI, new List<string> { "c" }))
        {
            foreach (string prawname in pRawList)
            {
                if (!containRawName(rawSubList, prawname) && !containRawName(exceptionList, prawname))
                {
                    rawSubList.Add(prawname);
                }
            }
            re.AddRange(rawSubList);
        }
        else
        {
            string firstRawName = rawSubList[0];
            int rawIndex = 0;
            for(int i = 0; i < pairList.Count; i++)
            {
                if (pairList[i].Value == firstRawName)
                {
                    rawIndex = i;
                }
            }
            if (rawIndex > pIndex)
            {
                re = pRawList;
            }
            else
            {
                re = rawSubList;
            }
            //看哪个的index靠前
            
        }
        return re;
    }


    bool containRawAction(int startIndex,int endIndex)
    {
        for(int i = startIndex; i <=endIndex; i++)
        {
            List<string> allRawActionName = DataMap.convMatMapList.getAllActionRawName();
            if (allRawActionName.Contains(pairList[i].Value))
            {
                return true;
            }
        }
        return false;
    }
   

    bool containRawName(List<string> rawNameList,string rawName)
    {
        
        List<string> matnameList = new List<string>();
        foreach(string temprawName in rawNameList)
        {
            //matnameList.Add( DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { temprawName }));
            matnameList.Add(DataMap.GetObjNameMapValue(new List<string> { temprawName }));
        }
        //string matName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { rawName });
        string matName = DataMap.GetObjNameMapValue(new List<string> { rawName });

        if (matnameList.Contains(matName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    

  
    protected List<int> getChangeFlagIndex(int endIndex)
    {
        List<int> re = new List<int>();
        for (int i = 0; i < endIndex; i++)
        {
            
            foreach (string chFlag in DataMap.attrChangeFlag)
            {
                if (this.pairList[i].Value.Contains(chFlag)&& !re.Contains(i))
                {
                    re.Add(i);
                }
            }
        }
        return re;
    }

    //判断一个角色是否被吃或被喝了
    bool isEatObject(string matName)
    {
        List<string>eatRawnameList= DataMap.diyActList.getElemByActionType("n_coupleStaticEat").actRawNameList;
        List<string> drinkRawNameList= DataMap.diyActList.getElemByActionType("n_coupleStaticDrink").actRawNameList;
        List<string> drinkEatList = new List<string>();
        drinkEatList.AddRange(eatRawnameList);
        drinkEatList.AddRange(drinkRawNameList);
        foreach (BasicEvent basicEvent in this.eventList)
        {
            if (drinkEatList.Contains(basicEvent.actionName) && basicEvent.objectName.Equals(matName))
            {
                return true;
            }
        }
        return false;
    }

}
