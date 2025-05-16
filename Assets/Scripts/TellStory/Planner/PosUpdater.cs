using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosUpdater
{
    Recorder recorder;

    public PosUpdater(Recorder recorder)
    {
        this.recorder = recorder;
    }

    public  enum UpdateType
    {
        appear,
        disappear,
        update
    }

    public void updateAPosInfo(string goname,PathTransform pathTransform,string bkgName,UpdateType updateType)
    {
        if (updateType == UpdateType.disappear)
        {
            recorder.removeGo(DataUtil.getMatnameByGoname(goname));
        }else if (updateType == UpdateType.appear)
        {
            recorder.bkgPosRecorder.addTransform(goname, pathTransform);
            recorder.bkgPosRecorder.addPos(bkgName, pathTransform.endPosition, goname);
        }else if (updateType == UpdateType.update)
        {
            this.recorder.bkgPosRecorder.updateTransform(goname, pathTransform);            
            this.recorder.bkgPosRecorder.changePos(bkgName, goname, pathTransform.endPosition);
        }
        else
        {
            throw new System.Exception("未知的更新位置操作类型"+updateType);
        }
    }
    
}
