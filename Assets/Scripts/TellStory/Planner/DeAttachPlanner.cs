using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeAttachPlanner : Planner
{

    string rawActionName = "";
    string defaultSubGoname = "";
    string defaultObjGoname = "";

    public DeAttachPlanner(BasicEvent basicEvent, Recorder recorder, string rawActionName) : base(basicEvent, recorder)
    {
        this.basicEvent = basicEvent;
        this.rawActionName = rawActionName;
    }
  

    public override void planning()
    {
        if (!recorder.goMemDic.ContainsKey(basicEvent.subjectName) )
        {
            return;

        }
        this.defaultSubGoname = recorder.getTopGo(basicEvent.subjectName);
        if (!recorder.goMemDic.ContainsKey(basicEvent.objectName))
        {
            Dictionary<string, List<string>> childDic = recorder.bkgPosRecorder.childDic;
            if (childDic.ContainsKey(defaultSubGoname))
            {
                this.defaultObjGoname = childDic[defaultSubGoname][0];
            }
            else
            {
                foreach(string str in childDic.Keys)
                {
                    if (childDic[str].Contains(defaultSubGoname))
                    {
                        this.defaultObjGoname = str;
                    }
                }
            }
        }
        else
        {
            this.defaultObjGoname = recorder.getTopGo(basicEvent.objectName);
        }

        if (this.defaultObjGoname == "") return;
        
        //
        if (DataMap.diyActList.containRawName(basicEvent.actionName))
        {
            DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(basicEvent.actionName);
            planDiyAction(diyActElement);
        }
        if (keyFrames.Count == 0)
        {
            planTransform();
            planKeyframe();
        }
    }

    public void planDiyAction(DiyActElement diyActElement)
    {
        if (diyActElement.actType.Equals("n_DeAttachLetFree"))
        {
            planTransformLetFree(defaultSubGoname, defaultObjGoname);
            planKeyframeLetFree(defaultSubGoname, defaultObjGoname);
        }
        else if (diyActElement.actType.Equals("n_DeAttachBeFree"))
        {
            planTransformBeFree(defaultSubGoname, defaultObjGoname);
            planKeyframeBeFree(defaultSubGoname, defaultObjGoname);
        }

    }
    //主语是子物体
    //子物体移动到父物体邻位
    public void planTransformBeFree(string subgoname,string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);
        //找到object物体的位置信息
        PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);
        int objIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformObj.endPosition);
        List<float> objNei = new List<float>();

        objNei = recorder.bkgPosRecorder.getNeighborPoint(this.basicEvent.backgroundName, objIndex, objIndex + 1,"", 1);
        if (objNei.Count == 0)
        {
            //没有邻位置，找空位
            objNei = recorder.bkgPosRecorder.getSameTypeEmptyPoint(basicEvent.backgroundName, objIndex);
        }

        PathTransformChanger.moveToDis(basicEvent.subjectName,  pathTransformSub, objNei);

        this.transformDic.Add(subgoname, pathTransformSub);

        this.transformDic.Add(objgoname, pathTransformObj);
    }

    //子物体移动到父物体邻位
    public void planKeyframeBeFree(string subgoname, string objgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransformSub = this.transformDic[subgoname];
        PathTransform pathTransformObj = this.transformDic[objgoname];

        //子物体移动

        KeyFrame subGoFrame = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration,  pathTransformSub,"");


        keyFrames.Add(subGoFrame);

        //更新Recorder
        this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        this.recorder.bkgPosRecorder.updateTransform(objgoname, pathTransformObj);
        
        //int subIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransform.endPosition);
        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, subIndex);
        int subIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformSub.endPosition);
        int objIndex = recorder.bkgPosRecorder.getIndexByPos( basicEvent.backgroundName, pathTransformObj.endPosition);

        this.recorder.bkgPosRecorder.unsetChild(objgoname, subgoname);
        this.recorder.bkgPosRecorder.childParentGapDic.Remove(subgoname);

        //this.recorder.bkgPosRecorder.changePos(basicEvent.backgroundName,objgoname,objIndex);
        this.recorder.bkgPosRecorder.addPos(basicEvent.backgroundName, pathTransformSub.endPosition, subgoname);
        //this.recorder.bkgPosRecorder.changePos(basicEvent.backgroundName, objgoname, objIndex);


        this.keyFrames = keyFrames;
    }

    //子物体移动到父物体邻位
    public void planTransformLetFree(string subgoname, string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);
        //找到object物体的位置信息
        PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);

        //makeTransformStatic(ref pathTransformObj);
        //makeTransformStatic(ref pathTransformSub);

        
        int subIndex = recorder.bkgPosRecorder.getIndexByPos( basicEvent.backgroundName, pathTransformSub.endPosition);
        //int objIndex= recorder.bkgPosRecorder.getIndexByPos(basicEvent.objectName, basicEvent.backgroundName, pathTransformObj.endPosition);
        List<float> subNei = new List<float>();
        //List<float> subPos = new List<float>();
        //List<float> objNei = new List<float>();
        subNei = recorder.bkgPosRecorder.getNeighborPoint(this.basicEvent.backgroundName, subIndex, subIndex + 1,"", 1);
        if (subNei.Count == 0)
        {
            //没有邻位置，找空位
            subNei=recorder.bkgPosRecorder. getSameTypeEmptyPoint(basicEvent.backgroundName, subIndex);
        }
        //子物体向邻位移动
        //PathTransformChanger.moveToDis(basicEvent.subjectName, ref pathTransformSub, subNei);
        //客体向主体原位置移动
        
        //List<float> subPos = DataUtil.Clone<List<float>>(pathTransformSub.startPosition);
        PathTransformChanger.moveToDis(basicEvent.objectName,  pathTransformObj, subNei);      

        this.transformDic.Add(subgoname, pathTransformSub);

        this.transformDic.Add(objgoname, pathTransformObj);

    }

    //子物体移动到父物体邻位
    public void planKeyframeLetFree(string subgoname, string objgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransformSub = this.transformDic[subgoname];
        PathTransform pathTransformObj = this.transformDic[objgoname];

        //子物体移动

        KeyFrame objGoFrame = frameFactory.genMoveFrame(basicEvent.objectName, objgoname, basicEvent.startTime, basicEvent.duration, pathTransformObj,"");

       
        keyFrames.Add(objGoFrame);

        //更新Recorder
        this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        this.recorder.bkgPosRecorder.updateTransform(objgoname, pathTransformObj);
        //int subIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransform.endPosition);
        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, subIndex);
        int subIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformSub.endPosition);
        int objIndex= recorder.bkgPosRecorder.getIndexByPos( basicEvent.backgroundName, pathTransformObj.endPosition);

        this.recorder.bkgPosRecorder.unsetChild(subgoname, objgoname);
        this.recorder.bkgPosRecorder.childParentGapDic.Remove(objgoname);

        //this.recorder.bkgPosRecorder.changePos(basicEvent.backgroundName,objgoname,objIndex);
        this.recorder.bkgPosRecorder.addPos(basicEvent.backgroundName, pathTransformObj.endPosition, objgoname);
        //this.recorder.bkgPosRecorder.changePos(basicEvent.backgroundName, objgoname, objIndex);
        

        this.keyFrames = keyFrames;
    }

    public override void planKeyframe()
    {
        
    }
    public override void planTransform()
    {
        
    }
}
