using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//提取init事件
public class InitExtractor : Extractor
{
    //public List<int> subjectNameList { get; private set; }
    public List<string> allSubjectNameList { get; private set; }

    public InitExtractor(List<KeyValuePair<string, string>> pairList, float seqNum, Recorder recorder, float eventsLen, string bkgName, bool bkgChange)
        : base(pairList, seqNum, eventsLen, recorder, bkgChange, bkgName)
    { }

    bool hasNeighbor(string matName, List<string> memList)
    {
        bool re = false;
        for(int i = 0; i < memList.Count; i++)
        {
            if (memList[i].Equals(matName))
            {
                if(i+1<memList.Count && memList[i + 1].Equals(matName))
                {
                    re = true;
                }
               
            }
        }
        return re;
    }

    //生成默认出现的环境物品
    void genDefaultEnvProp()
    {
        List<EnvPointSettingElement> defaultPropList = new List<EnvPointSettingElement>();
        EnvPointSettingList envPointSettingList = DataMap.envPointSettingList;
        foreach(EnvPointSettingElement envPointSettingElement in envPointSettingList.envPointSettingList)
        {
            List<string> defaultBkgName = envPointSettingElement.defaultBackgroundList;
            if (defaultBkgName.Contains(bkgName) && envPointSettingElement.appear=="1")
            {
                defaultPropList.Add(envPointSettingElement);
            }
        }

        foreach(EnvPointSettingElement envPoint in defaultPropList)
        {
            foreach(string propName in envPoint.envPropList)
            {
                if (this.recorder.goMemDic.ContainsKey(propName)) continue;
                BasicEvent initEvent = new BasicEvent(BasicEvent.BasicEventType.init, propName, "", "", bkgName, eventNum, 1, envPoint.pointNameList[0]);
                this.recorder.CreateGoMem(propName);

                this.eventList.Add(initEvent);
            }
        }
      
    }

    public override void extractEvent()
    {
        genDefaultEnvProp();       
        List<string> rawSubList = getInitSubBetweenIndex(0,pairList.Count - 1,null,null);
       
              
        allSubjectNameList = rawSubList;
        foreach(string rawSubName in rawSubList)
        {
            //string fName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { rawSubName });
            string fName = DataMap.GetObjNameMapValue(new List<string> { rawSubName });
            BasicEvent initEvent;
            int subNameIndex = getIndexByValNameFirst(rawSubName);
            //如果是代词，用代词位置替换subIndex
            if (subNameIndex == -1) subNameIndex = getIndexBySymbol(new List<string> { "r" })[0];

            MatSettingElement element = DataMap.matSettingList.getMatSettingMap(fName);

            //如果上一个场景最后还有，不生成
            if (recorder.fileNum > 0 && RecorderList.getRecordByNum(recorder.fileNum - 1).goMemDic.ContainsKey(fName) && !bkgChange
                || recorder.bkgPosRecorder.isChildMatname(fName))
            {
                continue;
            }
            if (element.classList.Contains("prop") && hasNeighbor(rawSubName, rawSubList) && recorder.goMemDic.ContainsKey(fName)
                || !recorder.goMemDic.ContainsKey(fName))
            {
                //新增临时
                if (element.classList.Contains("dis") && recorder.fileNum > 0)
                {
                    foreach (string lastmatName in RecorderList.getRecordByNum(recorder.fileNum - 1).goMemDic.Keys)
                    {
                        if (lastmatName == fName) continue;
                        MatSettingElement elementTemp = DataMap.matSettingList.getMatSettingMap(lastmatName);
                        if (elementTemp.classList.Contains("dis"))
                        {
                            BasicEvent disEvent = new BasicEvent(BasicEvent.BasicEventType.disappear, lastmatName, "", "", bkgName, eventNum, 1, "");
                            this.eventList.Add(disEvent);
                        }
                    }
                }

                //找sub左右最近的方位词
                //string locFlag = getLocationFlag(subNameIndex);
                string locFlag = getNearestAllFlagBySub(subNameIndex);
                if (locFlag != "")
                {

                    initEvent = new BasicEvent(BasicEvent.BasicEventType.init, rawSubName, "", "", bkgName, eventNum, 1, locFlag);

                    this.recorder.CreateGoMem(fName);
                    this.eventList.Add(initEvent);
                    continue;
                }
               
                initEvent = new BasicEvent(BasicEvent.BasicEventType.init, rawSubName, "", "", bkgName, eventNum, 1, "");
                this.recorder.CreateGoMem(fName);
                //this.recorder.goMemDic.Add(fName,0);
                this.eventList.Add(initEvent);
            }         
        }


        eventNum++;

        reCalStartAndDuration();
    }





   
}
