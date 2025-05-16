using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActSettingList
{
   public List<ActSettingElement> actSettingList { get; set; }


    public ActSettingElement getActSettingEle(string actName)
    {
        ActSettingElement tmpElement = new ActSettingElement();
        foreach (ActSettingElement element in actSettingList)
        {
            if (element.actName.Equals(actName))
            {
                tmpElement = element;
            }
        }
        if (tmpElement.actName.Equals(""))
        {
            throw new System.Exception("没有找到动作信息"+actName);
        }
        return tmpElement;
    }

    public ActSettingList(List<ActSettingElement> actList)
    {
        this.actSettingList = actList;
    }
    public ActSettingList()
    {
        this.actSettingList = new List<ActSettingElement>();
    }

}

public class ActSettingElement
{
    public string actName { set; get; }
    public string actType { set; get; }


    public ActSettingElement()
    {
        actName = "";
        actType = "";
    }

    public ActSettingElement(string actName, string actType)
    {
        this.actName = actName;
        this.actType = actType;

    }
}
