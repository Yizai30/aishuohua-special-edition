using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiyActList
{
    public List<DiyActElement> diy_act_list { get; set; }

    public DiyActList(List<DiyActElement> diy_act_list)
    {
        this.diy_act_list = diy_act_list;
    }

    public DiyActList()
    {
        this.diy_act_list = new List<DiyActElement>();
    }


    public bool containRawName(string actRawName)
    {
        foreach(DiyActElement diyActElement in diy_act_list)
        {
            if (diyActElement.actRawNameList.Contains(actRawName))
            {
                return true;
            }
        }
        return false;
    }

    public DiyActElement getElemByActionType(string actType)
    {
        DiyActElement re = null;
        foreach (DiyActElement diyActElement in diy_act_list)
        {
            if (diyActElement.actType.Equals(actType))
            {
                re = diyActElement;
            }
        }
        return re;
    }

    public DiyActElement getElemByRawName(string actRawname)
    {
        DiyActElement re = null;
        foreach (DiyActElement diyActElement in diy_act_list)
        {
            if (diyActElement.actRawNameList.Contains(actRawname))
            {
                re = diyActElement;
            }
        }
        return re;
    }

    public List<string> getAllDiyRawName()
    {
        List<string> re = new List<string>();
        foreach (DiyActElement diyActElement in diy_act_list)
        {
            foreach(string actRawName in diyActElement.actRawNameList)
            {
                if (!re.Contains(actRawName))
                {
                    re.Add(actRawName);
                }
            }
            
           // re.AddRange(diyActElement.actRawNameList);
           
        }
        return re;
    }

    
}


public class DiyActElement
{
    public string level { get; }
    public string actType { get; }

    public string duration { get; }
    public string defaultPlanner { get; }
    public List<string> actRawNameList { get; }

    public DiyActElement(string level, string actType, string duration, string defaultPlanner, List<string> actRawNameList)
    {
        this.level = level;
        this.actType = actType;
        this.duration = duration;
        this.defaultPlanner = defaultPlanner;
        this.actRawNameList = actRawNameList;
    }
}
