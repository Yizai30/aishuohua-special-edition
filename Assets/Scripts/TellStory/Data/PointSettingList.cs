using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSettingList
{
    public List<PointSettingElement> pointSettingList { set; get; }

    public PointSettingList(List<PointSettingElement> pointSettingList)
    {
        this.pointSettingList = pointSettingList;
    }
    public PointSettingList()
    {
        this.pointSettingList = new List<PointSettingElement>();
    }

    public PointSettingElement GetPointSettingElementByAllFlag(string flag)
    {
        foreach (PointSettingElement el in this.pointSettingList)
        {
            if (el.locationFlags.Contains(flag)||el.actionFlags.Contains(flag))
            {
                return el;
            }
        }
        return null;
    }
    public PointSettingElement GetPointSettingElementByLocationFlag(string flag)
    {
        foreach(PointSettingElement el in this.pointSettingList)
        {
            if (el.locationFlags.Contains(flag))
            {
                return el;
            }
        }
        return null;
    }

    public PointSettingElement GetPointSettingElementByActionFlag(string flag)
    {
        foreach (PointSettingElement el in this.pointSettingList)
        {
            if (el.actionFlags.Contains(flag))
            {
                return el;
            }
        }
        return null;
    }

    public List<string> getAllActionFlags()
    {
        List<string> re = new List<string>();
        foreach(PointSettingElement pointSettingElement in pointSettingList)
        {
            re.AddRange(pointSettingElement.actionFlags);
        }
        return re;
    }

    public List<string> getAllLocationFlags()
    {
        List<string> re = new List<string>();
        foreach (PointSettingElement pointSettingElement in pointSettingList)
        {
            re.AddRange(pointSettingElement.locationFlags);
        }
        return re;
    }
}



public class PointSettingElement
{
    public List<string> actionFlags { set; get; }
    public List<string> locationFlags { set; get; }
    public string pointType { set; get; }

    public PointSettingElement(List<string> actionFlags, List<string> locationFlags, string pointType)
    {
        this.actionFlags = actionFlags;
        this.locationFlags = locationFlags;
        this.pointType = pointType;
    }
}



[Serializable]
public class PointMark
{
    public string pointType { set; get; }
    public List<int> pointList { set; get; }

    public PointMark(string pointType, List<int> pointList)
    {
        this.pointType = pointType;
        this.pointList = pointList;
    }
}