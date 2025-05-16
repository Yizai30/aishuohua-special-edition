using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;

public class FrameFactoryFollow
{


    float startTime;
    float duration;
    string subgoname;
    //string childGoName;
    Recorder recorder;
    PosUpdater posUpdater;
    PathTransform pathTransformSub;

    public FrameFactoryFollow(string subgoname, float startTime,float duration,Recorder recorder,PathTransform pathTransformSub)
    {
        this.subgoname = subgoname;
        this.startTime = startTime;
        this.duration = duration;
        this.recorder = recorder;
        this.pathTransformSub = pathTransformSub;
        posUpdater = new PosUpdater(recorder);
    }


    public List<KeyFrame> getInitFollowFrame(List<float> subStartPos, List<float> subEndPos)
    {
        List<KeyFrame> re = new List<KeyFrame>();
        Dictionary<string, List<string>> childDic = recorder.bkgPosRecorder.childDic;
        string childGoName = "";
        if (!childDic.ContainsKey(subgoname) || childDic[subgoname].Count == 0) return re;
        else
        {
            childGoName = childDic[subgoname][0];

        }
        string childname = DataUtil.getMatnameByGoname(childGoName);
        string subname = DataUtil.getMatnameByGoname(subgoname);
        //PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        PathTransform pathTransformObj = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[childGoName]);

        //当前场景没有子物体
        if (!recorder.goMemDic.ContainsKey(childname))
        {
            List<float> objStartPosGap = recorder.bkgPosRecorder.childParentGapDic[childGoName];
            List<float> objStartPos = DataUtil.listSub(pathTransformSub.endPosition, objStartPosGap);
            pathTransformObj.startPosition = DataUtil.Clone<List<float>>(objStartPos);
            pathTransformObj.endPosition = DataUtil.Clone<List<float>>(objStartPos);
            recorder.goMemDic.Add(childname, 1);

            KeyFrame initkeyFrame = new KeyFrame(0, 2, childname, childGoName, startTime,
           pathTransformObj.startPosition, pathTransformObj.endPosition, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           0, "", 1, new List<float> { 0, 0, 0 });
            re.Add(initkeyFrame);
        }
        return re;
    }


    public  List<KeyFrame> getMoveFollowFrame(List<float> subStartPos,List<float>subEndPos)
    {

        List<KeyFrame> re = new List<KeyFrame>();
        Dictionary<string, List<string>> childDic = recorder.bkgPosRecorder.childDic;


        string childGoName = "";
        if (!childDic.ContainsKey(subgoname) || childDic[subgoname].Count == 0) return re;
        else
        {
            childGoName = childDic[subgoname][0];

        }
        string childname = DataUtil.getMatnameByGoname(childGoName);
        string subname = DataUtil.getMatnameByGoname(subgoname);
        //PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        PathTransform pathTransformObj = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[childGoName]);

        //当前场景没有子物体
        if (!recorder.goMemDic.ContainsKey(childname))
        {
            List<float> objStartPosGap = recorder.bkgPosRecorder.childParentGapDic[childGoName];
            List<float> objStartPos = DataUtil.listSub(pathTransformSub.endPosition, objStartPosGap);
            pathTransformObj.startPosition =DataUtil.Clone<List<float>>( objStartPos);
            pathTransformObj.endPosition= DataUtil.Clone<List<float>>(objStartPos);
            recorder.goMemDic.Add(childname, 1);

            KeyFrame initkeyFrame = new KeyFrame(0, 2, childname, childGoName, startTime,
           pathTransformObj.startPosition, pathTransformObj.endPosition, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           0, "", 1, new List<float> { 0, 0, 0 });
            re.Add(initkeyFrame);
        }

        PathTransformChanger.movePositionTogether(pathTransformSub, ref pathTransformObj);
        PathTransformChanger.faceSameDirection(pathTransformSub, ref pathTransformObj, subname, childname);
        
        KeyFrame keyFrame = new KeyFrame(2, 2, childname, childGoName, startTime,
           pathTransformObj.startPosition, pathTransformObj.endPosition, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           duration, "", 1, new List<float> { 0, 0, 0 });
        re.Add(keyFrame);
        PathTransform newPathTransform = DataUtil.Clone<PathTransform>(pathTransformObj);
        PathTransformChanger.makeTransformStatic( newPathTransform);
        this.recorder.bkgPosRecorder.addTransform(childGoName, newPathTransform);
        //this.recorder.bkgPosRecorder.addPos(recorder.currBkgName, newPathTransform.endPosition, childGoName);
        //recorder.bkgPosRecorder.updateTransform(childGoName, newPathTransform);
        return re;
    }

    public List<KeyFrame> getTurnFollowFrame()
    {

        List<KeyFrame> re = new List<KeyFrame>();
        Dictionary<string, List<string>> childDic = recorder.bkgPosRecorder.childDic;


        string childGoName = "";
        if (!childDic.ContainsKey(subgoname) || childDic[subgoname].Count == 0) return re;
        else
        {
            childGoName = childDic[subgoname][0];

        }
        string childname = DataUtil.getMatnameByGoname(childGoName);
        string subname = DataUtil.getMatnameByGoname(subgoname);
        //PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        PathTransform pathTransformObj = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[childGoName]);

        
        PathTransformChanger.faceSameDirection(pathTransformSub, ref pathTransformObj, subname, childname);

        KeyFrame keyFrame = new KeyFrame(2, 2, childname, childGoName, startTime,
           pathTransformObj.startPosition, pathTransformObj.endPosition, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           duration, "", 1, new List<float> { 0, 0, 0 });
        re.Add(keyFrame);
        PathTransformChanger.makeTransformStatic( pathTransformObj);
        
        //recorder.bkgPosRecorder.updateTransform(childGoName, pathTransformObj);
        posUpdater.updateAPosInfo(childGoName, pathTransformObj, recorder.currBkgName, PosUpdater.UpdateType.update);
        return re;
    }

    //主体消失时，客体占据主体位置
    public List<KeyFrame> getSubDisppearDettachFrame()
    {

        List<KeyFrame> re = new List<KeyFrame>();
        Dictionary<string, List<string>> childDic = recorder.bkgPosRecorder.childDic;


        string childGoName = "";
        if (!childDic.ContainsKey(subgoname) || childDic[subgoname].Count == 0) return re;
        else
        {
            childGoName = childDic[subgoname][0];

        }
        string childname = DataUtil.getMatnameByGoname(childGoName);
        string subname = DataUtil.getMatnameByGoname(subgoname);
        //PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        PathTransform pathTransformObj = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[childGoName]);
        pathTransformObj.endPosition = DataUtil.Clone<List<float>>(pathTransformSub.endPosition);

        KeyFrame keyFrame = new KeyFrame(2, 2, childname, childGoName, startTime,
           pathTransformObj.startPosition, pathTransformObj.endPosition, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           duration, "", 1, new List<float> { 0, 0, 0 });
        re.Add(keyFrame);
        PathTransformChanger.makeTransformStatic( pathTransformObj);
        //recorder.bkgPosRecorder.updateTransform(childGoName, pathTransformObj);
        posUpdater.updateAPosInfo(childGoName, pathTransformObj, recorder.currBkgName, PosUpdater.UpdateType.update);
        recorder.bkgPosRecorder.changePos(recorder.currBkgName, childGoName, pathTransformObj.endPosition);
        recorder.bkgPosRecorder.unsetChild(subgoname, childGoName);
        return re;
    }

    //主体消失时，客体也消失
    public List<KeyFrame> getSubDisppearDettachFrameAllDis()
    {

        List<KeyFrame> re = new List<KeyFrame>();
        Dictionary<string, List<string>> childDic = recorder.bkgPosRecorder.childDic;


        string childGoName = "";
        if (!childDic.ContainsKey(subgoname) || childDic[subgoname].Count == 0) return re;
        else
        {
            childGoName = childDic[subgoname][0];

        }
        string childname = DataUtil.getMatnameByGoname(childGoName);
        string subname = DataUtil.getMatnameByGoname(subgoname);
        //PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        PathTransform pathTransformObj = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[childGoName]);
        pathTransformObj.endPosition = DataUtil.Clone<List<float>>(pathTransformSub.endPosition);

        KeyFrame keyFrame = new KeyFrame(1, 2, childname, childGoName, startTime,
           pathTransformObj.startPosition, pathTransformObj.endPosition, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           duration, "", 1, new List<float> { 0, 0, 0 });
        re.Add(keyFrame);
        PathTransformChanger.makeTransformStatic( pathTransformObj);
        recorder.bkgPosRecorder.unsetChild(subgoname, childGoName);
        posUpdater.updateAPosInfo(childGoName, pathTransformObj, recorder.currBkgName, PosUpdater.UpdateType.disappear);
        //recorder.removeGo(childname);
        //recorder.bkgPosRecorder.updateTransform(childGoName, pathTransformObj);
        //recorder.bkgPosRecorder.changePos(recorder.currBkgName, childGoName, pathTransformObj.endPosition);
        
        return re;
    }
}
