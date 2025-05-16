using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundExtractor : Extractor
{


    public BackgroundExtractor(List<KeyValuePair<string, string>> pairList, float seqNum, Recorder recorder, float eventsLen, string bkgName = "", bool bkgChange = false)
       : base(pairList, seqNum, eventsLen, recorder, bkgChange, bkgName)
    { }

    public override void extractEvent()
    {
        string oldBkgName = "";
        if (recorder.fileNum > 0)
        {
            oldBkgName = RecorderList.getRecordByNum(recorder.fileNum - 1).currBkgName;
        }
        else
        {
            oldBkgName = "defaultBkg";
        }
        this.bkgChange = getBackground();


        if (bkgChange && !isSeasonBkg( this.bkgName))
        {
            //如果更换背景，生成消失事件
            //清除当前recorder的所有物体
            if (this.recorder != null && recorder.fileNum > 0)
            {
                foreach (string objName in recorder.goMemDic.Keys)
                {
                    BasicEvent disEvent = new BasicEvent(BasicEvent.BasicEventType.disappear, objName, "", "", "", eventNum, 0, "");
                    this.eventList.Add(disEvent);
                }
            }
            /*
            if (this.recorder != null && recorder.fileNum > 0)
            {
                int charNum = 0;
                foreach (string objName in recorder.goMemDic.Keys)
                {
                    //BasicEvent disEvent = new BasicEvent(BasicEvent.BasicEventType.disappear, objName, "", "", "", eventNum, 1, "");

                    List<string> classList = DataMap.matSettingList.getMatSettingMap(objName).classList;
                    if (classList.Contains("character"))
                    {
                        BasicEvent leaveEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, objName, "", "离开", oldBkgName, eventNum, 10, "");
                        this.eventList.Add(leaveEvent);
                        charNum++;
                    }
                    else
                    {
                        BasicEvent disEvent = new BasicEvent(BasicEvent.BasicEventType.disappear, objName, "", "", "", eventNum + 10, 1, "");
                        this.eventList.Add(disEvent);
                    }

                }
                if (charNum > 0)
                {
                    base.eventsLen = 1;
                    eventNum = eventNum + 10;
                }

            }

            */
            recorder.goMemDic.Clear();

        }

        //生成背景帧
        BasicEvent bkgEvent = new BasicEvent(BasicEvent.BasicEventType.background, "", "", "", this.bkgName, eventNum, 1, "");
        this.eventList.Add(bkgEvent);
        eventNum++;
        reCalStartAndDuration();
    }


    bool isSeasonBkg(string bkgName)
    {
        if(bkgName.Contains("autumn")||
            bkgName.Contains("winter")||
            bkgName.Contains("summer")||
            bkgName.Contains("spring"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //得到这句话的背景信息,并对记录的背景进行维护
    private bool getBackground()
    {

        bool bkgChange = false;
        List<string> bkgSymbols = new List<string> { "s", "n", "nr" };
        List<int> bkgIndex = getIndexBySymbol(bkgSymbols);
        //找出这句话的背景词
        string tmpBkgName = "";
        string tmpBkgRawName = "";
        foreach (int bkgTempIndex in bkgIndex)
        {
            string tmpName = pairList[bkgTempIndex].Value;
            if (DataMap.ContainBkgNameMapKey(new List<string> { tmpName }))
            {
                //tmpBkgName = DataMap.convMatMapList.getConvMapByMapName("BkgMap").getValByKeyList(new List<string> { tmpName });
                tmpBkgName = DataMap.GetBkgNameMapValue(new List<string> { tmpName });
                tmpBkgRawName = tmpName;
            }
        }
        if (!tmpBkgName.Equals(""))
        {
            //查看当前提取出的背景名词是不是
            //当前已有背景名字的一部分，
            //如果是，则不改变背景
            if (isExtractPart(tmpBkgRawName, recorder.currBkgName))
            {
                bkgChange = false;
                bkgName = recorder.currBkgName;
                return bkgChange;
            }

            if (tmpBkgName != this.recorder.currBkgName)
            {

                bkgChange = true;
                this.recorder.currBkgName = tmpBkgName;

            }
            bkgName = tmpBkgName;

        }
        //没有背景信息
        else
        {
            if (DataMap.privateBackgroundList.GetAllName().Contains(this.recorder.currBkgName))
            {
                bkgChange = false;
                bkgName = recorder.currBkgName;
                return bkgChange;
            }

            //查找有没有包含唯一flag的背景
            string locationBkg = "";
            List<string> locationFlagList = getAllLocationFlag();
            List<string> actionFlagList = getAllActionFlag();
            List<string> flagList = new List<string>();
            //优先location
            if (locationFlagList.Count != 0)
            {
                flagList.AddRange(locationFlagList);
            }
            else
            {
                flagList.AddRange(actionFlagList);
            }
            if (flagList.Count != 0)
            {
                foreach (string locFlag in flagList)
                {
                    foreach (BkgSettingElement bkgSettingElement in DataMap.bkgSettingList.bkgSettingList)
                    {
                        locationBkg = DataMap.bkgSettingList.getBkgNameByPointType(locFlag, this.recorder.currBkgName);
                    }
                }
            }
            if (locationBkg != "")
            {
                if (locationBkg != this.recorder.currBkgName)
                {
                    bkgChange = true;
                    this.recorder.currBkgName = locationBkg;

                }
                bkgName = locationBkg;
            }
            else
            {
                if (this.recorder.currBkgName.Equals(""))
                {
                    bkgChange = true;
                    this.bkgName = "defaultBkg";
                    this.recorder.currBkgName = "defaultBkg";
                }
                else
                {
                    bkgName = this.recorder.currBkgName;
                }
            }

        }

        //找出其他背景约束词
        List<MapKVPair> mapKVPairs = DataMap.convMatMapList.getConvMapByMapName("BkgMap").contentList;
        string newBkgName = "";
        foreach (MapKVPair mapKV in mapKVPairs)
        {
            if (mapKV.keyNameList[0].Contains(this.bkgName) && mapKV.keyNameList.Count > 1)
            {
                foreach (KeyValuePair<string, string> kvPair in pairList)
                {
                    if (mapKV.keyNameList[1].Contains(kvPair.Value))
                    {
                        newBkgName = mapKV.valName;
                    }
                }
            }
        }
        if (newBkgName != "" && !newBkgName.Equals(this.bkgName))
        {
            this.bkgName = newBkgName;
            this.recorder.currBkgName = newBkgName;
            bkgChange = true;
        }


        return bkgChange;
    }

    public string getCurBkgName()
    {

        return this.bkgName;
    }

    public bool getCurBkgIsChanged()
    {
        return this.bkgChange;
    }

    //查看当前提取出的背景名词是不是
    //当前已有背景名字的一部分，
    //如果是，则不改变背景
    bool isExtractPart(string extractedRawName, string currBkgName)
    {
        List<string> bkgNameList = DataMap.convMatMapList.getConvMapByMapName("BkgMap").getKeyNameListByvalName(currBkgName);
        foreach (string bkgName in bkgNameList)
        {
            if (bkgName.Contains(extractedRawName)) return true;
        }
        return false;
    }

}