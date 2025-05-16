
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//使用语义角色识别的ActionExtractor
public class SementicActionExtractor : Extractor
{

    //    StartCoroutine( NlpServer.GetSementics("小强 来到 森林里"));

    SementicList sementicList;

    List<List<string>> lastSubList = new List<List<string>>();//记录某个谓词论元结构中的主语列表。当某个短句没有找到素材库中对应的，为空列表
    List<List<string>> lastObjList = new List<List<string>>();//记录某个谓词论元结构中的宾语列表。
    public SementicActionExtractor(List<KeyValuePair<string, string>> pairList, float seqNum, Recorder recorder, float eventsLen, string bkgName, bool bkgChange,SementicList sementicList)
        : base(pairList, seqNum, eventsLen, recorder, bkgChange, bkgName)
    {
        this.sementicList = sementicList;
        
    }
    public override void extractEvent()
    {
        if (sementicList.sementics.Length == 0) return;

        extractShort();
        addDefaultAction();

        //对得到的event作规划处理 
        EventPlanner eventPlanner = new EventPlanner(this.eventList, this.bkgChange);
        eventPlanner.planEvent();
        this.eventList = eventPlanner.eventList;
        //eventNum++;
        setupEventStartTime();
        reCalStartAndDuration();
    }

    void extractShort()
    {

        DispelAllPn(); 

        for(int i = 0; i < sementicList.sementics.Length; i++)
        {

        }
    }

    //消解指代词,存放到dic中，同时记录每个短句的主语和宾语列表 
    void DispelAllPn()
    {
        
    }


    //获取第i个元语的主语对象
    List<string> getISemListSub(int i)
    {
        List<Sementic> subSemList = sementicList.getSubSem(i);
        List<string> subMatList = new List<string>();
        foreach (Sementic subSem in subSemList)
        {
            string subSemForm = subSem.form;

            //从subSemForm中提取动画对象
            subMatList = extractAnimObjFromStr(subSemForm);

            //从subsemForm中提取指代词
            string pnName = extractPnFromStr(subSemForm);
            //消解指代词
            List<string> pnRawNameList = getPnObj(pnName, lastSubList);
            //将指代词消解内容添加到主语列表
            foreach (string pnRawName in pnRawNameList)
            {
                //string matName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { pnRawName });
                string matName = DataMap.GetObjNameMapValue(new List<string> { pnRawName });
                if (!subMatList.Contains(matName))
                {
                    subMatList.Add(matName);
                }
            }
            if (subMatList.Count != 0) break;
        }
       
        return subMatList;
       
    }

    //获取第i个元语的宾语对象
    List<string> getISemListObj(int i)
    {
        List<Sementic> objSemList = sementicList.getObjSem(i);
        List<string> objMatList = new List<string>();
        foreach (Sementic subSem in objSemList)
        {
            string subSemForm = subSem.form;

            //从subSemForm中提取动画对象
            objMatList = extractAnimObjFromStr(subSemForm);

            //从subsemForm中提取指代词
            string pnName = extractPnFromStr(subSemForm);
            //消解指代词
            List<string> pnRawNameList = getPnObj(pnName, lastSubList);
            //将指代词消解内容添加到主语列表
            foreach (string pnRawName in pnRawNameList)
            {
                //string matName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { pnRawName });
                string matName = DataMap.GetObjNameMapValue(new List<string> { pnRawName });
                if (!objMatList.Contains(matName))
                {
                    objMatList.Add(matName);
                }
            }
            if (objMatList.Count != 0) break;
        }

        return objMatList;

    }

    //获取第i个元语的地点标识，地点标识可能在地点状语和宾语中
    string getISemListLocPointFlag(int i)
    {
        string re = "";
        List<Sementic> placeSemList = sementicList.getPlaceSem(i);
        foreach(Sementic sementic in placeSemList)
        {
            string semForm = sementic.form;
            string pointFlag = extractLocPointFlagFromStr(semForm);
            if (pointFlag != "")
            {
                re = pointFlag;
                break;
            }
        }
        return re;
    }

    //获取第i个元语的动作标识，动作地点标识在谓语动词中
    string getISemListActPointFlag(int i)
    {
        string re = "";
        List<Sementic> actSemList = sementicList.getActSem(i);
        foreach (Sementic sementic in actSemList)
        {
            string semForm = sementic.form;
            string pointFlag = extractActPointFlagFromStr(semForm);
            if (pointFlag != "")
            {
                re = pointFlag;
                break;
            }
        }
        return re;
    }

    //获取第i个元语的动作词列表，动作词在谓语动词中
    List<string> getISemListAct(int i)
    {
        List< string> re = new List<string>();
        List<Sementic> actSemList = sementicList.getActSem(i);
        foreach (Sementic sementic in actSemList)
        {
            string semForm = sementic.form;
            List<string> actNameList = extractActFromStr(semForm);
            if (actNameList.Count!=0 )
            {
                re = actNameList;
                break;
            }
        }
        return re;
    }

    //从一个字符串中提取素材库中有的动画对象素材名称
    List<string> extractAnimObjFromStr(string str)
    {
        List<string> re = new List<string>();
        List<string> allAnimObjList = DataMap.convMatMapList.getAllActorAndPropRawName();
        foreach (string name in allAnimObjList)
        {
            if (str.Contains(name))
            {
                //string matName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { name });
                string matName = DataMap.GetObjNameMapValue(new List<string> { name });
                if (!re.Contains(matName)) re.Add(matName);
            }
        }
        return re;
    }

    //从一个字符串中提取指代词
    string extractPnFromStr(string str)
    {
        string re = "";
        List<string> pnList = new List<string>();
        pnList.AddRange(DataMap.pluralPronounList);
        pnList.AddRange(DataMap.singularPronounList);
        foreach (string pn in pnList)
        {
            if (str.Contains(pn))
            {
                re = pn;
            }
        }
        return re;
    }

    //从一个字符串中提取locationpointFlag
    string extractLocPointFlagFromStr(string str)
    {
        string re = "";
        List<string> pointFlagList = DataMap.pointSettingList.getAllLocationFlags();
        foreach(string pointFlag in pointFlagList)
        {
            if (str.Contains(pointFlag))
                re = pointFlag;
        }
        return re;

    }

    //从一个字符串中提取ActionPointFlag
    string extractActPointFlagFromStr(string str)
    {
        string re = "";
        List<string> pointFlagList = DataMap.pointSettingList.getAllActionFlags();
        foreach (string pointFlag in pointFlagList)
        {
            if (str.Contains(pointFlag))
                re = pointFlag;
        }
        return re;

    }


    //从一个字符串中提取Action
    List<string> extractActFromStr(string str)
    {
        List<string> re = new List<string>();
        List<string> actRawNameList = DataMap.convMatMapList.getAllActionRawName();
        actRawNameList.AddRange(DataMap.diyActList.getAllDiyRawName());
        foreach (string actName in actRawNameList)
        {
            if (str.Contains(actName) && !re.Contains(actName))
                re.Add(actName);
        }
        return re;

    }










    void setupEventStartTime()
    {
        Dictionary<string, List<BasicEvent>> tempDic = new Dictionary<string, List<BasicEvent>>();
        foreach (BasicEvent basicEvent in eventList)
        {
            //string tempPrefab = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { basicEvent.subjectName });
            string tempPrefab = DataMap.GetObjNameMapValue(new List<string> { basicEvent.subjectName });
            if (tempDic.ContainsKey(tempPrefab))
            {
                tempDic[tempPrefab].Add(basicEvent);
            }
            else
            {
                tempDic.Add(tempPrefab, new List<BasicEvent> { basicEvent });
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
        foreach (string keyname in tempDic.Keys)
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

        foreach (string keyname in tempDic.Keys)
        {

            float tempActorTotalDuration = 0;
            foreach (BasicEvent basicEvent in tempDic[keyname])
            {
                tempActorTotalDuration += basicEvent.duration;
            }
            float gapDuration = maxDuration - tempActorTotalDuration;

            tempDic[keyname].Last().duration += gapDuration;
        }

        //重新按照timestamp整理eventList
        List<BasicEvent> tempEventList = new List<BasicEvent>();
        foreach (string keyname in tempDic.Keys)
        {
            tempEventList.AddRange(tempDic[keyname]);
        }


        this.eventList.Clear();
        var sorted = tempEventList.OrderBy(a => a.startTime);
        foreach (BasicEvent basicEvent1 in sorted)
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
                }
                else if (basicEvent.eventType.Equals(BasicEvent.BasicEventType.n_action))
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
            if (classList.Contains("prop") || classList.Contains("env_prop"))
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








}
