using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//基本事件
using AnimGenerator;
public class BasicEvent
{
    public string subjectName { get; set; }
    public List<string> attrList { get; set; }
    public string objectName { get; set; }
    public string actionName { get; set; }
    public string backgroundName { get; set; }

    public float startTime { get; set; }

    public float duration { get; set; }

    public enum BasicEventType
    {
        attrChange,
        action,
        n_action,
        disappear,
        init,
        background,
        subtitle,
        audio
    }

    public BasicEventType eventType { set; get; }

    //用于标定地图位置
    public string pointFlag { set; get; }

    public BasicEvent(BasicEventType eventType, string subName,string objName,string actName,string bkgName,float seqNum,float duration,string pointFlag)
    {
        this.pointFlag = pointFlag;
        this.attrList = new List<string>();
        this.subjectName = subName;
        this.objectName = objName;
        this.actionName = actName;
        this.backgroundName = bkgName;
        this.startTime = seqNum;//在一个句子中发生的顺序
        this.eventType = eventType;
        this.duration = duration;
    }

    public BasicEvent(BasicEventType eventType, string subName,List<string> attrList, string objName, string actName, string bkgName, float seqNum, float duration,string pointFlag)
    {
        this.pointFlag = pointFlag;
        this.attrList = attrList;
        this.subjectName = subName;
        this.objectName = objName;
        this.actionName = actName;
        this.backgroundName = bkgName;
        this.startTime = seqNum;//在一个句子中发生的顺序
        this.eventType = eventType;
        this.duration = duration;
    }





    //将原始的event名称映射到素材上
    public void mapElementName()
    {
        List<string> matNameList = DataMap.convMatMapList.getAllMatName();
        if (this.actionName != "" && !matNameList.Contains(actionName))
        {
            
            this.actionName = DataMap.GetActNameMapValue(new List<string>{this.subjectName, this.actionName});
           
        }
        if (this.subjectName!="" && !matNameList.Contains(subjectName))
        {
            
            this.subjectName = DataMap.GetObjNameMapValue(new List<string> { this.subjectName });
        }
        if (this.objectName != "" && !matNameList.Contains(objectName))
        {
            this.objectName = DataMap.GetObjNameMapValue(new List<string> { this.objectName });

        }
        
        if (this.backgroundName != "" && !matNameList.Contains(backgroundName))
        {
            
            this.backgroundName = DataMap.GetBkgNameMapValue(new List<string> { this.backgroundName});
        }

       
    }


}
