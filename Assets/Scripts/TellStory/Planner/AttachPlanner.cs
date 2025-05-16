using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachPlanner : Planner
{

    string rawActionName = "";
    string defaultSubGoname = "";
    string defaultObjGoname = "";

    public AttachPlanner(BasicEvent basicEvent, Recorder recorder, string rawActionName) : base(basicEvent, recorder)
    {
        this.basicEvent = basicEvent;
        this.rawActionName = rawActionName;
    }


    public override void planning()
    {
        if (!(recorder.goMemDic.ContainsKey(basicEvent.subjectName) && recorder.goMemDic.ContainsKey(basicEvent.objectName)))
        {
            return;

        }
        this.defaultSubGoname = recorder.getTopGo(basicEvent.subjectName);
        this.defaultObjGoname = recorder.getTopGo(basicEvent.objectName);
        if (DataMap.diyActList.containRawName(basicEvent.actionName))
        {
            DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvent.actionName);
            planDiyAction(diyActElement);
        }
    }

    

    public void planDiyAction(DiyActElement diyActElement)
    {
        if (diyActElement.actType.Equals("n_attachCarryBack"))
        {
            planTransformCarryBack(defaultSubGoname, defaultObjGoname);
            planKeyframeBound(defaultSubGoname,defaultObjGoname);
        }
        if (diyActElement.actType.Equals("n_attachCatch"))
        {
            planTransformCatch(defaultSubGoname, defaultObjGoname);
            planKeyframeBound(defaultSubGoname, defaultObjGoname);
        }
        if (diyActElement.actType.Equals("n_attachCatchAndEscape"))
        {
            planTransformCatchAndEscape(defaultSubGoname, defaultObjGoname);
            planKeyframeCatchAndEscape(defaultSubGoname, defaultObjGoname);
        }
    }

    public void planTransformCatchAndEscape(string subgoname, string objgoname)
    {
        planTransformCatch(defaultSubGoname, defaultObjGoname);
    }

    public void planKeyframeCatchAndEscape(string subgoname, string objgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransformSub = this.transformDic[subgoname];
        PathTransform pathTransformObj = this.transformDic[objgoname];


        //string subRawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(basicEvent.subjectName);
        //string subRawName = DataMap.GetObjNameMapValue(new List<string> { basicEvent.subjectName });
       //string actionName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, "到" });

        KeyFrame subFrame = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration, pathTransformSub,"");

        KeyFrame objFrame = frameFactory.genMoveFrame(basicEvent.objectName, objgoname, basicEvent.startTime, basicEvent.duration * 0.9f,pathTransformObj,"");

        PathTransformChanger.faceSameDirection(pathTransformSub, ref pathTransformObj, basicEvent.subjectName, basicEvent.objectName);

        KeyFrame turnkeyFrame = frameFactory.genMoveFrame(basicEvent.objectName, objgoname, basicEvent.startTime + basicEvent.duration * 0.1f, basicEvent.duration / 2, pathTransformObj,"");

        keyFrames.Add(subFrame);
        keyFrames.Add(objFrame);
        keyFrames.Add(turnkeyFrame);

        //更新Recorder
        this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        this.recorder.bkgPosRecorder.updateTransform(objgoname, pathTransformObj);
        //int subIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransform.endPosition);
        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, subIndex);
        this.recorder.bkgPosRecorder.removePos(objgoname);
        this.recorder.bkgPosRecorder.setChild(subgoname, objgoname, pathTransformObj.endPosition, pathTransformSub.endPosition);

        //走
       // BasicEvent SubEscapebasicEvent = new BasicEvent(BasicEvent.BasicEventType.n_action, basicEvent.subjectName, "", "", basicEvent.backgroundName, basicEvent.startTime+0.5f, basicEvent.duration * 0.5f, "");
        //SingleMoveActionPlanner singleMoveActionPlanner = new SingleMoveActionPlanner(SubEscapebasicEvent, recorder,"走");
        //singleMoveActionPlanner.planning();
        //keyFrames.AddRange(singleMoveActionPlanner.keyFrames);

        

       

        this.keyFrames.AddRange(keyFrames);
    }

    //父物体走到子物体点位。子物体走到父物体下
    public void planTransformCatch(string subgoname, string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);
        //找到object物体的位置信息
        PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);

        //makeTransformStatic(ref pathTransformObj);
        //makeTransformStatic(ref pathTransformSub);
        float subScale = Mathf.Abs(pathTransformSub.endScale[0]);
        PathTransformChanger.faceTo( pathTransformSub,  pathTransformObj, basicEvent.subjectName);

        //父物体终点为子物体坐标
        pathTransformSub.endPosition = DataUtil.Clone<List<float>>(pathTransformObj.endPosition);
        //子物体终点为父物体下


        List<float> subUp = getAttachPos(objgoname, pathTransformSub, pathTransformObj, "bottomOffset");

        pathTransformObj.endPosition = DataUtil.Clone<List<float>>(subUp);

        //pathTransformSub.startPosition = DataUtil.Clone<List<float>>(pathTransformObj.endPosition);
        //pathTransformSub.endPosition= DataUtil.Clone<List<float>>(pathTransformObj.endPosition);

        this.transformDic.Add(subgoname, pathTransformSub);

        this.transformDic.Add(objgoname, pathTransformObj);
    }

   

    //子物体走到父物体上
    public void planTransformCarryBack(string subgoname, string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);
        //找到object物体的位置信息
        PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);

        //makeTransformStatic(ref pathTransformObj);
        //makeTransformStatic(ref pathTransformSub);
        float subScale = Mathf.Abs(pathTransformSub.endScale[0]);
        PathTransformChanger.faceTo( pathTransformSub, pathTransformObj, basicEvent.subjectName);

        //父物体终点为子物体坐标
        pathTransformSub.endPosition =DataUtil.Clone<List<float>>( pathTransformObj.endPosition);
        //子物体终点为父物体上侧
        

        List<float> subUp = getAttachPos(objgoname, pathTransformSub, pathTransformObj, "upOffset");

        pathTransformObj.endPosition = DataUtil.Clone<List<float>>(subUp);

       //pathTransformSub.startPosition = DataUtil.Clone<List<float>>(pathTransformObj.endPosition);
        //pathTransformSub.endPosition= DataUtil.Clone<List<float>>(pathTransformObj.endPosition);

        this.transformDic.Add(subgoname, pathTransformSub);

        this.transformDic.Add(objgoname, pathTransformObj);

    }

    public void planKeyframeBound(string subgoname,string objgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransformSub = this.transformDic[subgoname];
        PathTransform pathTransformObj = this.transformDic[objgoname];


        //string subRawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(basicEvent.subjectName);
        //string actionName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, "到" });

        KeyFrame subFrame = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration,pathTransformSub,"");

        KeyFrame objFrame = frameFactory.genMoveFrame(basicEvent.objectName, objgoname, basicEvent.startTime, basicEvent.duration*0.9f,pathTransformObj,"");
       
        PathTransformChanger.faceSameDirection( pathTransformSub, ref pathTransformObj, basicEvent.subjectName, basicEvent.objectName);

        KeyFrame turnkeyFrame = frameFactory.genMoveFrame(basicEvent.objectName, objgoname, basicEvent.startTime + basicEvent.duration*0.1f, basicEvent.duration / 2,pathTransformObj,"");

        keyFrames.Add(subFrame);
        keyFrames.Add(objFrame);
        keyFrames.Add(turnkeyFrame);

        //更新Recorder
        this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        this.recorder.bkgPosRecorder.updateTransform(objgoname, pathTransformObj);
        //int subIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransform.endPosition);
        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, subIndex);
        this.recorder.bkgPosRecorder.removePos(objgoname);
        this.recorder.bkgPosRecorder.setChild(subgoname, objgoname,pathTransformObj.endPosition,pathTransformSub.endPosition);

        this.keyFrames.AddRange(keyFrames);
    }



    public override void planKeyframe()
    {

    }



    public override void planTransform()
    {

    }


    List<float> getAttachPos(string objgoname, PathTransform subTransform,PathTransform objTransform, string relationName)
    {
        List<float> subUp = RelatedPosManager.getRelatedPos(basicEvent.subjectName, basicEvent.objectName, subTransform.endPosition,
            subTransform.endScale, objTransform.endScale, relationName);

        //List<float> subUp = RelatedPosManager.getRelatedPos(basicEvent.subjectName,subTransform.endScale,relationName);

        List<float> gap = DataUtil.listSub(subTransform.endPosition, subUp);
        recorder.bkgPosRecorder.childParentGapDic.Add(objgoname, gap);
        return subUp;
    }

}
