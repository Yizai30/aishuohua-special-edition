
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


//使用语义角色识别的ActionExtractor
public class AMRActionExtractor : Extractor
{

    //    StartCoroutine( NlpServer.GetSementics("小强 来到 森林里"));


    AMRGraph amrGraph;
    List<ActionPattern> actionPatternList;
    List<List<string>> lastSubList = new List<List<string>>();//记录某个谓词论元结构中的主语列表。当某个短句没有找到素材库中对应的，为空列表
    List<List<string>> lastObjList = new List<List<string>>();//记录某个谓词论元结构中的宾语列表。
    public AMRActionExtractor(List<KeyValuePair<string, string>> pairList, float seqNum, Recorder recorder, float eventsLen, string bkgName, bool bkgChange,AMRGraph amrGraph)
        : base(pairList, seqNum, eventsLen, recorder, bkgChange, bkgName)
    {
        this.amrGraph = amrGraph;
        actionPatternList = new List<ActionPattern>();
        
    }
    public override void extractEvent()
    {
        if (amrGraph.nodes.Count == 0) return;
        //if (sementicList.sementics.Length == 0) return;

        ExtractPattern();

        if (this.actionPatternList.Count == 0) return;

        ExtractEvent();
        
        if (this.eventList.Count == 0) return;

        addDefaultAction();

        //对得到的event作规划处理 
        EventPlanner eventPlanner = new EventPlanner(this.eventList, this.bkgChange);
        eventPlanner.planEvent();
        this.eventList = eventPlanner.eventList;
        //eventNum++;
        setupEventStartTime();
        reCalStartAndDuration();
    }

    #region 以pattern为输入，结合动作类型，生成basicevent
    void ExtractEvent()
    {

        foreach(ActionPattern actionPattern in this.actionPatternList)
        {
            string actionRawName = actionPattern.actionName;
            BasicEvent basicEvent = null;
            //动词为变化词
            if (isChangeWord(actionRawName))
            {
                basicEvent = ExtractEventChangeAction(actionPattern);
                continue;
            }
            //动词为diy
            else if (DataMap.diyActList.containRawName(actionRawName))
            {
                basicEvent = ExtractEventDiyAction(actionPattern);
                continue;
            }
            //动词为非diy
            else
            {
                basicEvent = ExtractEventNormalAction(actionPattern);
                
            }
            if (basicEvent != null)
            {
                this.eventList.Add(basicEvent);
            }
        }

    }

    BasicEvent ExtractEventChangeAction(ActionPattern actionPattern)
    {
        return null;
    }

    BasicEvent ExtractEventDiyAction(ActionPattern actionPattern)
    {
        BasicEvent genEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, actionPattern.subName, actionPattern.objName, 
            actionPattern.actionName, bkgName, eventNum, 1, actionPattern.locName);
        //extractedSubName.Add(subRawname);

        return genEvent;
    }

    BasicEvent ExtractEventNormalAction(ActionPattern actionPattern)
    {

        string actName = DataMap.GetActNameMapValue(new List<string> { actionPattern.subName, actionPattern.actionName });
        

        BasicEvent genEvent = new BasicEvent(BasicEvent.BasicEventType.action, actionPattern.subName, actionPattern.objName,
           actionPattern.actionName, bkgName, eventNum, 1, actionPattern.locName);
        //extractedSubName.Add(subRawname);

        return genEvent;
    }

    //判断是否 为变化词
    bool isChangeWord(string rawAction)
    {
        foreach (string chflag in DataMap.attrChangeFlag)
        {
            if (rawAction.Contains(chflag))
            {
                return true;
            }
        }
        return false;
    }


    #endregion


    #region 以动作为中心查找action pattern各个元素
    //获取动作为核心的元组,不考虑动作类型，只根据amr
    void ExtractPattern()
    {
        List<int> actIndexList = getActionNode();
        if (actIndexList.Count == 0) return;

        //对每个动作节点，寻找其action pattern元素
        foreach(int actIndex in actIndexList)
        {
            int subIndex = getSubFromAMR(actIndex);
            int objIndex = getObjFormAMR(actIndex);
            int locIndex = getLocFlagFormAMR(actIndex);
            ActionPattern actionPattern = new ActionPattern();

            if (subIndex == -1) continue;
            actionPattern.subName = amrGraph.getNodeById(subIndex).label;
            if (objIndex != -1)
            {
                actionPattern.objName = amrGraph.getNodeById(objIndex).label;
            }
            if (locIndex != -1)
            {
                actionPattern.locName = amrGraph.getNodeById(locIndex).label;
            }
            actionPatternList.Add(actionPattern);

        }

    }

    int getLocFlagFormAMR(int actIndex)
    {
        List<string> locLabel = new List<string> { "arg1","location" };
        int locIndex = getNeighborByLabel(actIndex, locLabel[0]);
        if (locIndex == -1)
        {
            locIndex= getNeighborByLabel(actIndex, locLabel[1]);
        }
        AMRNode tempNode = amrGraph.getNodeById(locIndex);
        if (tempNode == null) return -1;

        string nodeName = tempNode.label;
        if (DataMap.pointSettingList.getAllLocationFlags().Contains(nodeName))
        {
            return locIndex;
        }
        return -1;
    }


    int getObjFormAMR(int actIndex)
    {
        string objLabel = "arg1";
        int objIndex = getNeighborByLabel(actIndex, objLabel);
        AMRNode tempNode = amrGraph.getNodeById(objIndex);
        if (tempNode == null) return -1;

        string nodeName = tempNode.label;
        if (DataMap.convMatMapList.getAllActorAndPropRawName().Contains(nodeName) ||
            DataMap.convMatMapListChild.getAllActorAndPropRawName().Contains(nodeName))
        {
            return objIndex;
        }
        return -1;
    }

    int getSubFromAMR(int actIndex)
    {
        string subLabel = "arg0";
        int subIndex = getNeighborByLabel(actIndex, subLabel);
        AMRNode tempNode = amrGraph.getNodeById(subIndex);
        if (tempNode == null) return -1;

        string nodeName = tempNode.label;
        if (DataMap.convMatMapList.getAllActorAndPropRawName().Contains(nodeName)||
            DataMap.convMatMapListChild.getAllActorAndPropRawName().Contains(nodeName))
        {
            return subIndex;
        }
        return -1;
    }

    //查找index某个label边的另一个点
    int getNeighborByLabel(int sourceNode,string label)
    {
        int targetNode = -1;
        foreach(AMREdge aMREdge in amrGraph.edges)
        {
            if(aMREdge.label.Equals(label) && aMREdge.source.Equals(sourceNode))
            {
                targetNode = aMREdge.target;
            }
        }
        return targetNode;
    }

    #endregion


    #region 查找amr图中的动作
    //查找在数据库中的动作，返回其node index
    List<int> getActionNode()
    {
        List<int> actNode = new List<int>();
        List<AMRNode> amrNodeList = amrGraph.nodes;
        foreach(AMRNode aMRNode in amrNodeList)
        {
            string labelName = aMRNode.label;
            if (ActionExists(labelName))
            {
                actNode.Add(aMRNode.id);
            }
        }
        return actNode;
    }

    //判断nodeName是否在动作库中
    bool ActionExists(string labelName)
    {
        string rawName = labelName;
        int cutPos = labelName.IndexOf('-');
        if (cutPos != -1)
        {
            rawName = labelName.Substring(0, cutPos + 1);
        }
        if(DataMap.convMatMapList.getAllActionRawName().Contains(rawName)||
            DataMap.convMatMapListChild.getAllActionRawName().Contains(rawName)||
            DataMap.attrChangeFlag.Contains(rawName))
        {
            return true;
        }
        return false;
    }

    #endregion









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
