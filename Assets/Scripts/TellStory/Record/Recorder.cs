using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;



//记录场景缓存信息
[Serializable]
public class Recorder
{
    //记录当前场景有什么物体
    //public  List<string> goMemList { set; get; }
    public Dictionary<string, int> goMemDic { set; get; }


    public List<string> subRawNameList { set; get; }

    //记录游戏物体的位置
    public BkgPosRecorder bkgPosRecorder;
    //public static Dictionary<string,List<string>> bkgPosUseDic { set; get; }
    public string currBkgName { set; get; }

    public int fileNum { set; get; }

    //记录当前句子里的追逐主语
    //public string chaseSubjectName { set; get; }

    //如果有属性更改，在此记录
    Dictionary<string, AttrRecord> attrRecordDic { set; get; }

    //记录昏倒的角色
    List<string> tumbleList { set; get; }

    //记录出场角色
    public List<string> comeActor { set; get; }
    public List<KeyValuePair<string, string>> pairList { set; get; }

    private KeyFrameList keyFrameList;

    // public Dictionary<string,string> defaultActionDic { get; set; }

    public Recorder()
    {
        goMemDic = new Dictionary<string, int>();
        //goTransform=new Dictionary<string, PathTransform>();
        bkgPosRecorder = new BkgPosRecorder();
        subRawNameList = new List<string>();
        attrRecordDic = new Dictionary<string, AttrRecord>();
        //defaultActionDic = new Dictionary<string, string>(); 
        keyFrameList = new KeyFrameList();
        tumbleList = new List<string>();
        comeActor = new List<string>();
        currBkgName = "";

        //chaseSubjectName = "";
        fileNum = 0;
    }



    public void setKeyframeList(List<KeyFrame> keyFrames)
    {
        this.keyFrameList.keyFrames = keyFrames;
    }

    public List<KeyFrame> getKeyframeList()
    {
        List<KeyFrame> re = new List<KeyFrame>();
        if (this.keyFrameList.keyFrames.Count > 0)
        {
            re = keyFrameList.keyFrames;
        }
        return re;
    }

    public void clearRecord()
    {
        goMemDic = new Dictionary<string, int>();
        //goTransform=new Dictionary<string, PathTransform>();
        bkgPosRecorder = new BkgPosRecorder();
        subRawNameList = new List<string>();
        attrRecordDic = new Dictionary<string, AttrRecord>();

        currBkgName = "";
        fileNum = 0;
    }

    //加入昏倒角色
    public void addTumbleGoname(string goname)
    {
        if (!tumbleList.Contains(goname))
        {
            tumbleList.Add(goname);
        }
    }
    //删除昏倒角色
    public void removeTumbleGoname(string goname)
    {
        if (tumbleList.Contains(goname))
        {
            tumbleList.Remove(goname);
        }
    }

    //是否有昏倒角色
    public bool hasTumbleGoname(string goname)
    {
        if (tumbleList.Contains(goname)) return true;
        else return false;
    }

    public string genGoName(string objName)
    {
        if (!goMemDic.ContainsKey(objName))
        {
            throw new Exception("没有这个角色");
        }
        goMemDic[objName]++;

        //goMemList.Add(objName);
        return objName + goMemDic[objName];

    }

    //extractor阶段用，不管数量只管有没有
    public void CreateGoMem(string objName)
    {
        if (!goMemDic.ContainsKey(objName))
        {
            goMemDic.Add(objName, 0);
        }

    }

    public string getTopGo(string matName)
    {
        string re = matName + this.goMemDic[matName];
        if (goMemDic[matName] == 0)
        {
            re = matName + "1";
        }
        return re;
    }
    //delete top go
    public string removeGo(string matName)
    {
        if (!goMemDic.ContainsKey(matName)) return "";
        string name = "";
        int num = goMemDic[matName];
        if (num >= 1)
        {
            this.bkgPosRecorder.removeTransform(matName + num);
            this.bkgPosRecorder.removePos(matName + num);
            this.removeTumbleGoname(matName + num);
            this.removeAttrDicEle(matName + num);

        }
        else
        {
            return "";
        }


        if (this.goMemDic[matName] > 1)
        {
            this.goMemDic[matName]--;
        }
        else
        {
            this.goMemDic.Remove(matName);
        }

        //if (attrRecordDic.ContainsKey(matName))
        //{
        //    attrRecordDic.Remove(matName);
        //}

        name = matName + num;
        //removeTumbleGoname(name);
        return name;

    }

    public void addAttrDicEle(string goname, AttrRecord attrRecord)
    {
        if (attrRecordDic.ContainsKey(goname))
        {
            attrRecordDic.Remove(goname);

        }
        attrRecordDic.Add(goname, attrRecord);
    }

    public void removeAttrDicEle(string goname)
    {
        if (attrRecordDic.ContainsKey(goname))
        {
            attrRecordDic.Remove(goname);

        }

    }

    public AttrRecord getAttrDicEle(string goname)
    {
        if (this.attrRecordDic.ContainsKey(goname))
        {
            return attrRecordDic[goname];
        }
        else
        {
            return null;
        }
    }

    public bool containAttrDicEle(string goname)
    {
        if (this.attrRecordDic.ContainsKey(goname))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isNewActor(string actorMatName)
    {
        bool re = false;
        if (this.fileNum == 0) re = true;
        else
        {
            Dictionary<string, int> goDic = RecorderList.getRecordByNum(this.fileNum - 1).goMemDic;
            string lastBkg = RecorderList.getRecordByNum(this.fileNum - 1).currBkgName;
            if (goDic.ContainsKey(actorMatName) && lastBkg == this.currBkgName)
            {
                re = false;
            }
            else
            {
                re = true;
            }
        }
        return re;

    }

    public PathTransform getPathTransformCopy(string goname)
    {
        PathTransform re = null;
        if (this.bkgPosRecorder.transformDic.ContainsKey(goname))
        {
            re = DataUtil.Clone<PathTransform>(this.bkgPosRecorder.transformDic[goname]);
        }
        return re;
    }


}