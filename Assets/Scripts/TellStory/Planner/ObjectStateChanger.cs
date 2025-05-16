using AnimGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用于生成改变状态的帧
//消失/动画...
public class ObjectStateChanger
{
    FrameFactory frameFactory;
    PosUpdater posUpdater;
    Recorder recorder;

    public ObjectStateChanger(FrameFactory frameFactory,PosUpdater posUpdater,Recorder recorder)
    {
        this.frameFactory = frameFactory;
        this.posUpdater = posUpdater;
        this.recorder = recorder;
        
    }

    public List<KeyFrame> handleObj(string matname, string goname, float startTime, ref PathTransform pathTransform,string rawActionName)
    {
        List<KeyFrame> re = new List<KeyFrame>();
        DiyActElement diyActElement= DataMap.diyActList.getElemByRawName(rawActionName);
        if (diyActElement == null) return re;
        if(diyActElement.actType== "n_coupleStaticEat")
        {
            re = handleDisappearObj(matname, goname, startTime, ref pathTransform);
        }else if(diyActElement.actType== "n_coupleStaticDrink"||
            diyActElement.actType== "n_coupleStaticOpenBook"||
            diyActElement.actType== "n_coupleStaticCloseBook")
        {
            re = handleChangeAnimObj(matname, goname, startTime, ref pathTransform, rawActionName);
        }
        return re;
    }

     List<KeyFrame> handleDisappearObj(string matname, string goname,float startTime,ref PathTransform pathTransform)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();
        keyFrames.Add( frameFactory.genDisappearFrame(matname, goname, startTime, pathTransform));
        posUpdater.updateAPosInfo(goname, pathTransform,recorder.currBkgName , PosUpdater.UpdateType.disappear);
        return keyFrames;
    }

    //播放改变动画
     List<KeyFrame> handleChangeAnimObj(string matname, string goname, float startTime, ref PathTransform pathTransform,string actionRawName)
    {
        List<KeyFrame> re = new List<KeyFrame>();
        string matRawName = "";
        string actionName = "";
        try
        {
             matRawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matname);
             //actionName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { matRawName, actionRawName });
            actionName = DataMap.GetActNameMapValue(new List<string> { matRawName, actionRawName });
        }catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
        if (actionName == "") return new List<KeyFrame>();
        KeyFrame keyFrameObj = frameFactory.genStaticActFrame(matname, goname,actionName, startTime,0.1f, pathTransform,"");
        re.Add(keyFrameObj);
        return re ;
    }

   


}
