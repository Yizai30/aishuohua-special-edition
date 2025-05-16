using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionExtractor : Extractor
{
   
    //List<string> extractedSubName = new List<string>();

    List<List<string>> lastSubList = new List<List<string>>();//记录某个基本句式中的主语列表。
    List<List<string>> lastObjList = new List<List<string>>();//记录某个基本句式中的宾语列表。
    public ActionExtractor(List<KeyValuePair<string, string>> pairList, float seqNum, Recorder recorder, float eventsLen, string bkgName, bool bkgChange)
        : base(pairList, seqNum, eventsLen, recorder, bkgChange, bkgName)
    { }
  


    public override void extractEvent()
    {
        
        extractShort();
        addDefaultAction();

        Debug.Log($"提取的动作：{string.Join(", ", this.eventList.Select(e => e.actionName))}");
        //对得到的event作规划处理
        EventPlanner eventPlanner = new EventPlanner(this.eventList,this.bkgChange);
        eventPlanner.planEvent();
        this.eventList = eventPlanner.eventList;
        //eventNum++;
        setupEventStartTime();
        reCalStartAndDuration();



    }
    void extractShort()
    {

        List<string> actSymbols = new List<string> { "v", "i" };
        List<int> actIndexList = getIndexBySymbol(actSymbols);
        int lastActIndex = 0;//上一个动词
        //List<string> rawSubList;
        if (actIndexList.Count != 0)
        {
            foreach (int actI in actIndexList)
            {              
                //
                //lastSubList.Add(rawSubList);
                //判断动词是不是变化词
                bool ischange = false;
                foreach (string chflag in DataMap.attrChangeFlag)
                {
                    if (pairList[actI].Value.Contains(chflag))
                    {
                        ischange = true;
                        break;
                    }
                }
                //List<int> chIndexList = getIndexBetweenWord(lastActIndex,actI,DataMap.attrChangeFlag);
               
                if (ischange)
                {
                    List<string> AttrrawSubList = extractSub(lastActIndex, actI);

                    List<BasicEvent> chEvent =  extractChangeEvent(lastActIndex, actI, AttrrawSubList);
                    if (chEvent.Count != 0)
                    {
                        //lastActIndex = actI;
                        this.eventList.AddRange(chEvent);
                        //lastSubList.Add(rawSubList);
                        continue;
                    }

                }

                //判断是否为指示句
                if (DataMap.isaFlag.Contains(pairList[actI].Value))
                {
                    extractChildMap(actI);
                }

                //判断是不是把字句
                List<int> BaIndexList = getIndexBetweenWord(lastActIndex, actI, new List<string> { "把" });
                if (BaIndexList.Count != 0)
                {
                    int BaIndex = BaIndexList[0];
                    // int objIndex = getIndexLeftNearestObj(pairList, actI, BaIndex);                    
                    List<string> BARawSubList = extractSub(lastActIndex, BaIndex);
                    //List<string> BARawObjList = getSubBetweenIndex(BaIndex, actI, lastSubList,BARawSubList);
                    List<string> BARawObjList = extractObj(BaIndex, actI, BARawSubList);
                    List<BasicEvent> events = genEventBySubObjAct(actI, BARawSubList, BARawObjList);
                    if (events.Count != 0)
                    {
                        //lastActIndex = actI;
                        this.eventList.AddRange(events);
                        //lastSubList.Add(rawSubList);
                        continue;
                    }

                }

                //判断是不是被字句
                List<int> BeiIndexList = getIndexBetweenWord(lastActIndex, actI, new List<string> { "被" });
                if (BeiIndexList.Count != 0)
                {
                    int BeiIndex = BeiIndexList[0];

                    //List<string> BEIRawSubList = getSubBetweenIndex(BeiIndex, actI,lastSubList,null);
                    //List<string> BEIRawObjList = getSubBetweenIndex(lastActIndex, BeiIndex, lastSubList,BEIRawSubList);
                    List<string> BEIRawSubList = extractSub(BeiIndex, actI);
                    List<string> BEIRawObjList = extractObj(lastActIndex, BeiIndex, BEIRawSubList);
                    //List<string> subRawList = getIndexLeftAllObj(BeiIndex,actI)
                    List<BasicEvent> events = genEventBySubObjAct(actI, BEIRawSubList, BEIRawObjList);
                    if (events.Count != 0)
                    {
                        //lastActIndex = actI;
                        this.eventList.AddRange(events);
                        //lastSubList.Add(rawSubList);
                        continue;
                    }
                }

                //判断是不是npnv句型
                int prepIndex = getIndexLeftNearestSymbol(lastActIndex, actI, new List<string> { "p" });
                
                if (prepIndex != -1 && DataMap.toActPrep.Contains(pairList[prepIndex].Value))
                {

                    //List<string> NPNVRawSubList = getSubBetweenIndex(lastActIndex, prepIndex,lastSubList,null);
                    //List<string> NPNVRrawObjList = getSubBetweenIndex(prepIndex, actI, lastSubList,NPNVRawSubList);
                    List<string> NPNVRawSubList = extractSub(lastActIndex, prepIndex);
                    List<string> NPNVRrawObjList = extractObj(prepIndex, actI, NPNVRawSubList);
                    //int objIndex = getIndexLeftNearestObj(pairList, actI, prepIndex);
                    List<BasicEvent> events = genEventBySubObjAct(actI, NPNVRawSubList, NPNVRrawObjList);
                    if (events.Count != 0)
                    {
                        //lastActIndex = actI;
                        this.eventList.AddRange(events);
                        //lastSubList.Add(rawSubList);
                        continue;
                    }
                }

                //nv和nvn句型
                List<string> rawSubList = extractSub(lastActIndex, actI);
                foreach (string subRawName in rawSubList)
                {
                    List<string> objRawNameList = extractObj(actI, getNextActI(actI), rawSubList);
                   
                    string actRawName = pairList[actI].Value;
                    //string subName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { subRawName });
                    string subName = DataMap.GetObjNameMapValue(new List<string> { subRawName });
                    string actName = "";
                    
                    if (DataMap.diyActList.containRawName(actRawName))
                    {
                        List<BasicEvent> n_events = extractN_Action(subRawName, actI, rawSubList,objRawNameList);
                        if (n_events.Count != 0)
                        {
                            this.eventList.AddRange(n_events);
                            //lastActIndex = actI;
                            //lastSubList.Add(rawSubList);
                            continue;
                        }
                        
                        
                    }
                   

                    if (!DataMap.ContainActNameMapKey(new List<string> { subRawName, actRawName }))
                    {
                        continue;
                    }

                    //actName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, actRawName });
                    actName = DataMap.GetActNameMapValue(new List<string> { subRawName, actRawName });
                    string actType = DataMap.actSettingList.getActSettingEle(actName).actType;
                    if (actType.Equals("single_static"))
                    {
                        string pointFlag = getNearestAllFlagByAct(actI);
                        BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, "", actRawName, bkgName, eventNum, 1, pointFlag);
                        //lastActIndex = actI;
                        eventList.Add(actEvent);
                        //lastSubList.Add(rawSubList);

                    }
                    else if (actType.Equals("couple_static"))
                    {
                        List<BasicEvent> csEvents = extractCoupleStatic(actI, subRawName, actRawName, subName, actName, rawSubList);
                        if (csEvents.Count != 0)
                        {
                            //lastActIndex = actI;
                            eventList.AddRange(csEvents);
                        }


                    }
                    else if (actType.Equals("movable"))
                    {
                        List<BasicEvent> mEvents = extractMove(actI, subRawName, actRawName, subName, actName, rawSubList);
                        if (mEvents.Count != 0)
                        {
                            //lastActIndex = actI;
                            eventList.AddRange(mEvents);
                        }


                    }
                    else
                    {
                        throw new System.Exception("没有这种动作类别");
                    }
                }

                //eventNum++;

                lastActIndex = actI;

            }
        }
    }


    void extractChildMap(int indiIndex)
    {
        List<string> valNameList = DataMap.convMatMapList.getAllActorAndPropRawName();
        
        if(indiIndex-2>=0&&indiIndex+1<pairList.Count&&
            pairList[indiIndex-2].Key=="m"&&
            pairList[indiIndex-1].Key=="n"&&
            pairList[indiIndex+1].Key=="nr"&&
            valNameList.Contains(pairList[indiIndex-1].Value)
            )
        {
            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { pairList[indiIndex + 1].Value });
            MapKVPair mapKVPair = new MapKVPair(keyList, pairList[indiIndex - 1].Value);
            DataMap.userConvMatMapList.getConvMapByMapName("ObjectMap").contentList.Add(mapKVPair);
            return;
        }

        if (indiIndex - 1 >= 0 && indiIndex + 2 < pairList.Count &&
            pairList[indiIndex - 1].Key == "m" &&
            pairList[indiIndex + 1].Key == "nr" &&
            pairList[indiIndex + 2].Key == "n" &&
            valNameList.Contains(pairList[indiIndex + 2].Value)
            )
        {
            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { pairList[indiIndex + 1].Value });
            MapKVPair mapKVPair = new MapKVPair(keyList, pairList[indiIndex + 2].Value);
            DataMap.userConvMatMapList.getConvMapByMapName("ObjectMap").contentList.Add(mapKVPair);
            return;
        }

        if (indiIndex - 1 >= 0 && indiIndex + 2 < pairList.Count &&
           pairList[indiIndex - 1].Key == "nr" &&
           pairList[indiIndex + 1].Key == "m" &&
           pairList[indiIndex + 2].Key == "n" &&
           valNameList.Contains(pairList[indiIndex + 2].Value)
           )
        {
            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { pairList[indiIndex - 1].Value });
            MapKVPair mapKVPair = new MapKVPair(keyList, pairList[indiIndex + 2].Value);
            DataMap.userConvMatMapList.getConvMapByMapName("ObjectMap").contentList.Add(mapKVPair);
            return;
        }

        if (indiIndex - 2 >= 0 && indiIndex + 1 < pairList.Count &&
           pairList[indiIndex - 2].Key == "n" &&
           pairList[indiIndex - 1].Value == "的" &&
           pairList[indiIndex + 1].Key == "nr" &&
           valNameList.Contains(pairList[indiIndex - 2].Value)
           )
        {
            List<List<string>> keyList = new List<List<string>>();
            keyList.Add(new List<string> { pairList[indiIndex - 2].Value });
            MapKVPair mapKVPair = new MapKVPair(keyList, pairList[indiIndex + 1].Value);
            DataMap.userConvMatMapList.getConvMapByMapName("ObjectMap").contentList.Add(mapKVPair);
            return;
        }


    }

    List<BasicEvent> extractChangeEvent(int lastActIndex, int actI,List<string> rawSubList)
    {
        //如果是把字句被字句，更正rawSubList
        List<int> BaIndexList = getIndexBetweenWord(lastActIndex, actI, new List<string> { "把" });
        if (BaIndexList.Count != 0)
        {
            int BaIndex = BaIndexList[0];                         
            List<string> BARawSubList = extractSub(lastActIndex, BaIndex);           
            List<string> BARawObjList = extractObj(BaIndex, actI, BARawSubList);
            rawSubList = BARawObjList;

        }
        List<int> BeiIndexList = getIndexBetweenWord(lastActIndex, actI, new List<string> { "被" });
        if (BeiIndexList.Count != 0)
        {
            int BeiIndex = BeiIndexList[0];           
            List<string> BEIRawSubList = extractSub(BeiIndex, actI);
            List<string> BEIRawObjList = extractObj(lastActIndex, BeiIndex, BEIRawSubList);
            rawSubList = BEIRawObjList;
        }


        List<BasicEvent> re = new List<BasicEvent>();
        int chIndex = actI;
        //List<string> rawSubList = getIndexLeftAllObj(0,chIndex[i]);
        foreach (string subjectRawName in rawSubList)
        {
            //获取属性词
            List<AttrElement> attrList = new List<AttrElement>();
            if (chIndex != pairList.Count - 1)
            {
                attrList = getAttr(chIndex, chIndex + 1);
            }
            else
            {
                attrList = getAttr(chIndex, chIndex);
            }


            List<string> attrNameList = new List<string>();
            for (int i = 0; i < attrList.Count; i++)
            {
                attrNameList.Add(attrList[i].attrNameList[0]);
            }

            //如果属性词为空，找change后面的一个角色作为变化对象
            if (attrNameList.Count == 0)
            {
                int objIndex = getIndexRightNearestObj(pairList, chIndex);
                if (objIndex == -1) continue;
                attrNameList.Add(pairList[objIndex].Value);
            }

            if (attrNameList.Count != 0)
            {
                BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.attrChange, subjectRawName, attrNameList, "", "", bkgName, eventNum, 1, "");

                re.Add(actEvent);

            }
        }
        return re;
        
    }

    List<BasicEvent> extractCoupleStatic(int actI, string subRawName, string actRawName, string subName, string actName, List<string> subList)
    {
        List<BasicEvent> re = new List<BasicEvent>();
        //find objectname
        int objIndex = getIndexRightNearestObj(pairList, actI);
        int nextActI = getNextActI(actI);
        //获取所有obj，考虑代词
        //List<string> objRawNameList = getSubBetweenIndex(actI, nextActI,lastSubList,subList);
        List<string> objRawNameList = extractObj(actI, nextActI, subList);
       
        if (objRawNameList.Count != 0)
        {
           
            foreach (string objRawName in objRawNameList)
            {
                BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, objRawName, actRawName, bkgName, eventSeqNum, 1, "");
                //extractedSubName.Add(subRawName);
                re.Add(actEvent);
            }

        }
        //如果没有在句子中找到obj，
        else
        {
            //如果场景中除自身外只有一个对象，此对象为默认客体对象

            Dictionary<string, PathTransform> transDic = this.recorder.bkgPosRecorder.transformDic;
            string tempObject = "";
            if (transDic.Count == 2)
            {
                foreach (string str in transDic.Keys)
                {
                    tempObject = str.Substring(0, str.Length - 1);
                    if (!tempObject.Equals(subName))
                    {

                        BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, tempObject, actRawName, bkgName, eventSeqNum, 1, "");
                        //extractedSubName.Add(subRawName);
                        re.Add(actEvent);
                    }
                }
            }

            //退化为单人静止动作
            else
            {
                BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, "", actRawName, bkgName, eventSeqNum, 1, "");
                //extractedSubName.Add(subRawName);
                re.Add(actEvent);
            }

        }
        return re;
    }



    List<BasicEvent> extractMove(int actI, string subRawName, string actRawName, string subName, string actName, List<string> subList)
    {
        List<BasicEvent> re = new List<BasicEvent>();
        //find flag
        string locationFlag = getNearestAllFlagByAct(actI);   

        if (!locationFlag.Equals(""))
        {
            //找到了方位词
            BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, "", actRawName, bkgName, eventNum, 1, locationFlag);
            //extractedSubName.Add(subRawName);
            re.Add(actEvent);
        }
        else
        {
            // int objIndex = getIndexRightNearestObj(pairList, actI);
            int nextActI = getNextActI(actI);
            List<string> objRawNameList = extractObj(actI, nextActI, subList);
           
            
           
            if (objRawNameList.Count != 0 && containFlag(actI,nextActI,DataMap.aroundFlag))
            {
                foreach (string objRawName in objRawNameList)
                {
                    //string objMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { objRawName });
                    //string subMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { subRawName });
                    string objMatName = DataMap.GetObjNameMapValue(new List<string> { objRawName });
                    string subMatName = DataMap.GetObjNameMapValue(new List<string> { objRawName });
                    if (objMatName != subMatName)
                    {
                        BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, objRawName, actRawName, bkgName, eventNum, 1, "");
                        //extractedSubName.Add(subRawName);
                        re.Add(actEvent);
                    }


                }

            }
            else
            {
                //退化为单人
                BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, "", actRawName, bkgName, eventNum, 1, "");
                //extractedSubName.Add(subRawName);
                re.Add(actEvent);
            }

        }
        return re;

    }


    List<BasicEvent> extractN_Action(string subRawname, int actI, List<string> subList,List<string>objList)
    {
        //bool isExtracted = false;
        List<BasicEvent> re = new List<BasicEvent>();

        DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(pairList[actI].Value);

        string locationFlag = getNearestAllFlagByAct( actI);
           
        if (objList.Count == 0)
        {
            
            BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, subRawname, "", pairList[actI].Value, bkgName, eventNum, 1, locationFlag);
            //extractedSubName.Add(subRawname);
            re.Add(actEvent);
            return re;
        }
        else
        {
            foreach (string objRawName in objList)
            {
                //string objMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { objRawName });
                 //string subMatName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { subRawname });
                string objMatName = DataMap.GetObjNameMapValue(new List<string> { objRawName });
                string subMatName = DataMap.GetObjNameMapValue(new List<string> { subRawname });
                if (objMatName != subMatName)
                {
                    BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, subRawname, objRawName, pairList[actI].Value, bkgName, eventNum, 1, locationFlag);
                    //extractedSubName.Add(subRawname);
                    re.Add(actEvent);
                }


            }
                //isExtracted = true;
        }
                
        return re;
    }
   

    //根据主语列表、宾语列表和谓语动词生成事件
    List<BasicEvent> genEventBySubObjAct(int curActI, List<string> subRawNameList, List<string> objRawNameList)
    {
        List<BasicEvent> re = new List<BasicEvent>();
        if (subRawNameList.Count == 0 || subRawNameList.Count == 0) return re;
        
        foreach (string subRawName in subRawNameList)
        {
            //if (extractedSubName.Contains(subRawName)) continue;
            foreach (string objRawName in objRawNameList)
            {
                string actRawName = pairList[curActI].Value;
                //string objRawName = pairList[objIndex].Value;
                if (subRawName != objRawName)
                { 

                    if (DataMap.diyActList.containRawName(actRawName))
                    { 
                        //actName = "n_action";
                        List<BasicEvent> N_actionEvents = extractN_Action(subRawName, curActI, subRawNameList,objRawNameList);
                        if (N_actionEvents.Count != 0)
                        {
                            re.AddRange(N_actionEvents);
                        }
                    }
                    else
                    {
                        if (DataMap.ContainActNameMapKey(new List<string> { subRawName, actRawName }))
                        {
                            //string actName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, actRawName });
                            string actName = DataMap.GetActNameMapValue(new List<string> { subRawName, actRawName });
                            if (DataMap.actSettingList.getActSettingEle(actName).actType == "single_static")
                            {
                                BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, "", actRawName, bkgName, eventNum, 1, "");
                                re.Add(actEvent);
                            }
                            else
                            {
                                BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.action, subRawName, objRawName, actRawName, bkgName, eventNum, 1, "");                               
                                re.Add(actEvent);
                            }
                           
                        }
                    }
                      
                         
                }
            }


        }
           
        
        return re;
    }
    
    //如果一个角色有两个动作，排布其顺序
    void setupEventStartTime()
    {
        Dictionary<string, List<BasicEvent>> tempDic = new Dictionary<string, List<BasicEvent>>();
        foreach(BasicEvent basicEvent in eventList)
        {
            //string tempPrefab = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { basicEvent.subjectName });
            string tempPrefab = DataMap.GetObjNameMapValue(new List<string> { basicEvent.subjectName });
            if (tempDic.ContainsKey(tempPrefab))
            {
                tempDic[tempPrefab].Add(basicEvent);
            }
            else
            {
                tempDic.Add(tempPrefab, new List<BasicEvent> { basicEvent});
            }
        }
       
        //设定event的持续时间
        foreach (string keyname in tempDic.Keys)
        {
            float tempTimestamp = 0;
            foreach (BasicEvent basicEvent in tempDic[keyname])
            {

                basicEvent.startTime = tempTimestamp;
                DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvent.actionName);
                if ((diyActElement != null && diyActElement.duration.Equals("short")) ||
                    basicEvent.eventType == BasicEvent.BasicEventType.attrChange)
                {
                    basicEvent.duration = 1;
                }
                else
                {
                    basicEvent.duration = 10;
                }
                tempTimestamp += basicEvent.duration;
            }
        }

        //找出最大时间
        float maxDuration = 0;
        foreach(string keyname in tempDic.Keys)
        {
            float tempTimestamp = 0;
            foreach (BasicEvent basicEvent in tempDic[keyname])
            {
                tempTimestamp += basicEvent.duration;
            }
            if (tempTimestamp > maxDuration) maxDuration = tempTimestamp;
        }
        this.eventNum = (int)maxDuration;
        //补全不够时长的duration

        foreach(string keyname in tempDic.Keys)
        {
           
            float tempActorTotalDuration = 0;
            foreach(BasicEvent basicEvent in tempDic[keyname])
            {
                tempActorTotalDuration += basicEvent.duration;
            }
            float gapDuration = maxDuration - tempActorTotalDuration;
           
            tempDic[keyname].Last().duration += gapDuration;
        }

        //重新按照timestamp整理eventList
        List<BasicEvent> tempEventList = new List<BasicEvent>();
        foreach(string keyname in tempDic.Keys)
        {
            tempEventList.AddRange(tempDic[keyname]);
        }


        this.eventList.Clear();
        var sorted = tempEventList.OrderBy(a => a.startTime);
        foreach(BasicEvent basicEvent1 in sorted)
        {
            this.eventList.Add(basicEvent1);
        }
       

    }


    //查找有没有需要加默认动画的角色 
    void addDefaultAction()
    {
        foreach (string matName in recorder.goMemDic.Keys)
        {
            //查看该物体有没有生成动作
            bool isAct = false;

            foreach (BasicEvent basicEvent in eventList)
            {

                if (basicEvent.eventType.Equals(BasicEvent.BasicEventType.action))
                {
                    //string tempPrefab = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { basicEvent.subjectName });
                    string tempPrefab = DataMap.GetObjNameMapValue(new List<string> { basicEvent.subjectName });
                    if (tempPrefab.Equals(matName))
                    {
                        isAct = true;
                        break;
                    }
                } else if (basicEvent.eventType.Equals(BasicEvent.BasicEventType.n_action))
                {
                    //string tempPrefab = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { basicEvent.subjectName });
                    string tempPrefab = DataMap.GetObjNameMapValue(new List<string> { basicEvent.subjectName });
                    DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvent.actionName);
                    if (tempPrefab.Equals(matName) && diyActElement.duration.Equals("long"))
                    {
                        isAct = true;
                        break;
                    }
                }
            }
            if (isAct) continue;

            //查看状态是否晕倒
            if (recorder.hasTumbleGoname(matName + "1"))
            {
                continue;
            }

            //查看是否是prop
            List<string> classList = DataMap.matSettingList.getMatSettingMap(matName).classList;
            if (classList.Contains("prop")||classList.Contains("env_prop"))
            {
                continue;
            }

            //生成这个角色的默认动画事件
            BasicEvent defaultActEvent = genCharacterDefaultActionEvent(matName);

           
            //string actMatName=DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName,})
            if (defaultActEvent != null)
            {
                this.eventList.Add(defaultActEvent);
            }
            
        }
        //List<int> objList=getIndexLeftAllObj
    }


    BasicEvent genCharacterDefaultActionEvent(string matName)
    {
        BasicEvent actEvent = null;
        List<string> classList = DataMap.matSettingList.getMatSettingMap(matName).classList;
        if (classList.Contains("prop")) return null;
        //对一些具体角色的默认动作
        if (matName.Equals("cutePeacock3D"))
        {
            string rawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matName);
            actEvent = new BasicEvent(BasicEvent.BasicEventType.action, rawName, "", "睡", bkgName, eventNum, 1, "");

            return actEvent;
        }

        if (matName.Contains("db_"))
        {
            string rawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matName);
            actEvent = new BasicEvent(BasicEvent.BasicEventType.action, rawName, "", "默认", bkgName, eventNum, 1, "");

            return actEvent;
        }


        if (classList.Contains("water"))
        {
            //看当前场景有没有水
            bool hasWater = false;
            BkgSettingElement bkgSettingElement = DataMap.bkgSettingList.getElementByBkgName(this.bkgName);
            foreach (PointMark pointMark in bkgSettingElement.pointMarks)
            {
                if (pointMark.pointType.Contains("waterPoint"))
                {
                    hasWater = true;
                    break;
                }
            }
            if (hasWater)
            {
                string rawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matName);
                actEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, rawName, "", "游来游去", bkgName, eventNum, 1, "waterPoint");
                
            }
            else
            {
                string rawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matName);
                actEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, rawName, "", "说", bkgName, eventNum, 1, "waterPoint");
                
            }

        }
        else
        {
            string rawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matName);
            actEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, rawName, "", "说", bkgName, eventNum, 1, "");
        }
        return actEvent;
    }

    //得到某个角色的默认动作
    public static string getDefaultRawActionName(string matName)
    {
        string re = "";
        List<string> classList = DataMap.matSettingList.getMatSettingMap(matName).classList;
        if (classList.Contains("prop")|| classList.Contains("env_prop")) return re;
        //对一些具体角色的默认动作
        if (matName.Equals("cutePeacock3D"))
        {
            re = "睡";
        }

        if (matName.Contains("db_"))
        {
            re = "默认";
        }
        else
        {
            re = "说";
        }      
        return re;
    }


    List<AttrElement> getAttr(int subIndex1, int subIndex2)
    {
        
        List<AttrElement> re = new List<AttrElement>();

        if(subIndex1<0 || subIndex2 > pairList.Count)
        {
            return re;
        }
        for (int i = subIndex1; i <= subIndex2; i++)
        {
            string objName = pairList[i].Value;
            foreach (AttrElement attr in DataMap.attrList.attr_List)
            {
                
              
                if (attr.wordContainAttr(objName) && !re.Contains(attr) &&
                   !DataMap.ContainObjNameMapKey(new List<string> { objName}))
                {
                    re.Add(attr);
                }

            }
        }
        return re;
    }

    List<string> extractSub(int leftEdge, int rightEdge)
    {
        List<string> rawSubList = new List<string>();
        rawSubList = getSubBetweenIndex(leftEdge, rightEdge, lastSubList, null);
        if (rawSubList.Count > 0)
        {
            recorder.subRawNameList = DataUtil.Clone<List<string>>(rawSubList);
            lastSubList.Add(rawSubList);
        }
        return rawSubList;
    }

    List<string> extractObj(int leftEdge,int rightEdge,List<string>exceptList)
    {
        List<string> rawObjList = new List<string>();
        rawObjList = getSubBetweenIndex(leftEdge, rightEdge, lastObjList, exceptList);
        if (rawObjList.Count > 0)
        {
            lastObjList.Add(rawObjList);
        }
        return rawObjList;
    }

  


}
