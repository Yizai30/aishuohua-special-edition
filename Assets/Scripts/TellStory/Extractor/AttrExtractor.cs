using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class AttrExtractor : Extractor
{
    
    public AttrExtractor(List<KeyValuePair<string, string>> pairList, float seqNum,Recorder recorder,float eventsLen) : base(pairList, seqNum,recorder)
    {
        this.eventsLen = eventsLen;
        
    }
    public override void extractEvent()
    {
        List<int> chIndex = getChangeFlagIndex(pairList.Count);
        if (chIndex.Count != 0)
        {
            for (int i = 0; i < chIndex.Count; i++)
            {
                //获取之前最近的subject
                List<string> rawSubList = new List<string>();
                if (i == 0)
                {
                    //rawSubList = getIndexLeftAllObj(0, chIndex[i]);
                    rawSubList = getSubBetweenIndex(0, chIndex[i], null, null);
                }
                else
                {
                    //rawSubList = getIndexLeftAllObj(chIndex[i - 1] + 1, chIndex[i]);
                    rawSubList = getSubBetweenIndex(0, chIndex[i], null, null);
                }
                //List<string> rawSubList = getIndexLeftAllObj(0,chIndex[i]);
                foreach(string subjectRawName in rawSubList)
                {
                    //获取属性词
                    List<string> attrList = new List<string>();
                    if (i == chIndex.Count - 1)
                    {
                        attrList = getAttr(chIndex[i], pairList.Count - 1);
                    }
                    else
                    {
                        attrList = getAttr(chIndex[i], chIndex[i + 1]-1);
                    }
                    //如果属性词为空，找change后面的一个角色作为变化对象
                    if (attrList.Count == 0)
                    {
                        int objIndex = getIndexRightNearestObj(pairList, chIndex[i]);
                        if (objIndex == -1) continue;
                        attrList.Add(pairList[objIndex].Value);
                    }
                    
                    BasicEvent actEvent = new BasicEvent(BasicEvent.BasicEventType.attrChange, subjectRawName, attrList, "", "", "", eventNum, 1, "");
                   
                    eventList.Add(actEvent);
                }
               
            }
            
        }
        eventNum++;
        reCalStartAndDuration();
    }

    //获取变化后的属性
    List<string> getAttr(int subIndex1, int subIndex2)
    {
        List<string> re = new List<string>();


        for (int i = subIndex1; i <= subIndex2; i++)
        {
            string objName = pairList[i].Value;
            foreach (AttrElement attr in DataMap.attrList.attrList)
            {

                if (objName.Contains(attr.attrName) && !re.Contains(attr.attrName) &&
                    !DataMap.convMatMapList.getConvMapByMapName("ObjectMap").containKey(new List<string> { objName}))
                {
                    re.Add(attr.attrName);
                }
            }
        }
        return re;
    }



}
*/
