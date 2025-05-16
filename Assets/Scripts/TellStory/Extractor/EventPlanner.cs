using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用于得到eventList后，设定event时间之前
public class EventPlanner 
{
    public List<BasicEvent> eventList { set; get; }
    Dictionary<string, List<BasicEvent>> matEventDic;
    bool bkgChange;
    public EventPlanner(List<BasicEvent> events,bool bkgChange)
    {
        this.bkgChange = bkgChange;
        eventList = events;
        matEventDic = new Dictionary<string, List<BasicEvent>>();

        foreach (BasicEvent basicEvent in eventList)
        {
            //string tempPrefab = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { basicEvent.subjectName });
            string tempPrefab = DataMap.GetObjNameMapValue(new List<string> { basicEvent.subjectName });
            if (tempPrefab.Equals("")) continue;
            if (matEventDic.ContainsKey(tempPrefab))
            {
                matEventDic[tempPrefab].Add(basicEvent);
            }
            else
            {
                matEventDic.Add(tempPrefab, new List<BasicEvent> { basicEvent });
            }
        }

    }

    public void planEvent()
    {
        if (this.eventList.Count == 0) return;
        handleCrossBkg();
    }

    //如果离开事件后,有该对象的到达动作，且bkgChange，删除离开动作
    void handleCrossBkg()
    {
        List<BasicEvent> delEventList = new List<BasicEvent>();
        foreach(string matName in matEventDic.Keys)
        {
            if (!bkgChange) continue;
            List<BasicEvent> basicEvents = matEventDic[matName];
            //找到离开事件
            int leaveIndex = -1;
            for(int i = 0; i < basicEvents.Count; i++)
            {
                DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvents[i].actionName);
                if (diyActElement == null) continue;
               
                string actType=diyActElement.actType;
                if(actType.Equals("n_singleMoveLeave") ||
                    actType.Equals("n_singleMoveEscape"))
                {
                    leaveIndex = i;
                }
            }

            if (leaveIndex == -1) continue;

            //找有无到达事件
            int comeIndex = -1;

            for (int i = leaveIndex; i < basicEvents.Count; i++)
            {
                DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvents[i].actionName);
                if (diyActElement == null) continue;

                string actType = diyActElement.actType;
                if (actType.Equals("n_singleMoveCome"))
                {
                    comeIndex = i;
                }
            }

            if (comeIndex == -1) continue;

            //删除
            delEventList.Add(basicEvents[leaveIndex]);
        }

        if (delEventList.Count != 0)
        {
            List<BasicEvent> newEventList = new List<BasicEvent>();
            foreach(BasicEvent basicEvent in this.eventList)
            {
                if (!delEventList.Contains(basicEvent))
                {
                    newEventList.Add(basicEvent);
                }
            }
            this.eventList = newEventList;
        }
    }
   
}
