using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//物体a向物体b移动
public class CoupleMoveActionPlanner : Planner
{
    string rawActionName = "";
    string defaultSubGoname = "";
    string defaultObjGoname = "";
    public CoupleMoveActionPlanner(BasicEvent basicEvent,Recorder recorder,string rawActionName) : base(basicEvent,recorder)
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
        if (recorder.bkgPosRecorder.isChildGoname(defaultSubGoname))
        {
            return;
        }
        this.defaultObjGoname = recorder.getTopGo(basicEvent.objectName);


        if (DataMap.actSplitList.containsRawActName(rawActionName))
        {
            ActSplitElement actSplitElement = DataMap.actSplitList.getActElementByRawname(rawActionName);
            planByAtomMove(actSplitElement);
        }
        if (keyFrames.Count != 0) return;

        //原方案

        if (DataMap.diyActList.containRawName(rawActionName))
        {
            DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(rawActionName);
            planDiyAction(diyActElement);
        }
       if(this.keyFrames.Count==0)
       {
            planTransform();
            planKeyframe();
       }
        

    }

    public void planByAtomMove(ActSplitElement actSplitElement)
    {
        PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[defaultSubGoname]);
        PathTransform pathTransformObj = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[defaultObjGoname]);
        List<KeyFrame> re = atomMoveFactory.getAtomMoveList(actSplitElement.actType, basicEvent, pathTransformSub, pathTransformObj, defaultSubGoname, defaultObjGoname);
        if (re.Count == 0) return;
        this.keyFrames.AddRange(re);
        //posUpdater.updateAPosInfo(defaultSubGoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
    }

    public void planDiyAction(DiyActElement diyActElement)
    {
        if (diyActElement.actType.Equals("n_coupleMoveEscape"))
        {
            planTransformChase(defaultSubGoname,defaultObjGoname);
            planKeyframeChase(defaultSubGoname,defaultObjGoname);
        }

    }
    public override void planKeyframe()
    {
             
        planKeyframeNormal(defaultSubGoname, defaultObjGoname);
        
    }
    public override void planTransform()
    {
       
        
        planTransformNormal(defaultSubGoname, defaultObjGoname);
        
    }


    public void planTransformChase(string subgoname,string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);


        //找到object物体的位置信息
        PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);
        List<float> subNeiPos = new List<float>();
        List<float> objNeiPos = new List<float>();

        if (pathTransformSub.endPosition[0] < pathTransformObj.endPosition[0])
        {
            //同时朝右
            PathTransformChanger.faceToRight( pathTransformSub, basicEvent.subjectName);
            PathTransformChanger.faceToRight( pathTransformObj, basicEvent.objectName);
           

        }
        else
        {
            //同时朝左
            PathTransformChanger.faceToLeft( pathTransformSub, basicEvent.subjectName);
            PathTransformChanger.faceToLeft( pathTransformObj, basicEvent.objectName);

           
         
        }
        int objIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformObj.endPosition);
        int subIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformSub.endPosition);
        //neiPos = recorder.bkgPosRecorder.getNeighborPoint(basicEvent.backgroundName, objIndex, subIndex, 1);
        subNeiPos = frameFactory. getForwardPoint(subgoname, basicEvent.subjectName, pathTransformSub,basicEvent.backgroundName);
        objNeiPos = frameFactory. getForwardPoint(objgoname, basicEvent.objectName, pathTransformObj,basicEvent.backgroundName);
        pathTransformSub.endPosition = subNeiPos;
        pathTransformObj.endPosition = objNeiPos;

        this.transformDic.Add(subgoname, pathTransformSub);

        this.transformDic.Add(objgoname, pathTransformObj);

    }
    public void planKeyframeChase(string subgoname, string objgoname)
    {
        
        PathTransform pathTransformSub = this.transformDic[subgoname];
        PathTransform pathTransformObj = this.transformDic[objgoname];

        string rawSubName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(basicEvent.subjectName);
        //string subActionname = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { rawSubName, "到" });

        string rawObjName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(basicEvent.objectName);
        //string objActionname = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { rawObjName, "到" });

        KeyFrame subMoveKeyFrame = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration, pathTransformSub,"");
        KeyFrame objMoveKeyFrame = frameFactory.genMoveFrame(basicEvent.objectName, objgoname, basicEvent.startTime, basicEvent.duration,  pathTransformObj,"");

        posUpdater.updateAPosInfo(subgoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        posUpdater.updateAPosInfo(objgoname, pathTransformObj, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        this.keyFrames.Add(subMoveKeyFrame);
        this.keyFrames.Add(objMoveKeyFrame);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        //this.recorder.bkgPosRecorder.updateTransform(objgoname, pathTransformObj);
       

        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, pathTransformSub.endPosition);

    }

    //主体走到客体对象旁边
    public void planTransformNormal(string subgoname, string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);


        //找到object物体的位置信息
        PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);

        //调整朝向
        //changeFace(pathTransformObj, ref pathTransformSub, subScale, basicEvent.subjectName + "1");
        PathTransformChanger.faceTo( pathTransformSub,  pathTransformObj, basicEvent.subjectName);
        //设置sub对象的终点位置
        pathTransformSub.startPosition = pathTransformSub.endPosition;


        int objPosIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransformObj.endPosition);
        int subPosIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransformSub.endPosition);
        List<float> distPos = this.recorder.bkgPosRecorder.getNeighborPoint(this.basicEvent.backgroundName, objPosIndex, subPosIndex,subgoname, 1);
        if (distPos.Count == 0)
        {
            //obj没有空余邻位置
            distPos = recorder.bkgPosRecorder.getSameTypeEmptyPoint(basicEvent.backgroundName, objPosIndex);
            //distPos = DataUtil.Clone<List<float>>(pathTransformObj.endPosition);
        }

        int disIndex = this.recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, distPos);

        //避免堆在enterpoint
        if (recorder.bkgPosRecorder.belongType(basicEvent.backgroundName, "enterPoint", disIndex))
        {
            basicEvent.pointFlag = "defaultPoint";
            List<int> pointList = DataUtil.Clone(this.recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName,"defaultPoint"));
            distPos = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, pointList, 2, pointList.Count / 2);
        }

        PathTransformChanger.faceTo( pathTransformSub, basicEvent.subjectName, distPos);
        pathTransformSub.endPosition = distPos;

        //PathTransformChanger.faceTo(ref pathTransformSub, basicEvent.subjectName,distPos);

        this.transformDic.Add(subgoname, pathTransformSub);

        this.transformDic.Add(objgoname, pathTransformObj);
    }

    public void planKeyframeNormal(string subgoname, string objgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransformSub = this.transformDic[subgoname];
        PathTransform pathTransformObj = this.transformDic[objgoname];



        KeyFrame keyFrameMove = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration*9/10, pathTransformSub,"");
        
            
            /*
        KeyFrame keyFrame = new KeyFrame(2, 2, this.basicEvent.subjectName, subgoname, basicEvent.startTime,
            pathTransform.startPosition, pathTransform.endPosition, pathTransform.endRotation,
            pathTransform.endRotation, pathTransform.endScale, pathTransform.endScale,
            basicEvent.duration, basicEvent.actionName, 1, new List<float> { 0, 0, 0 });
        */
        keyFrames.Add(keyFrameMove);

        PathTransform newSubPathTransform = DataUtil.Clone<PathTransform>(pathTransformSub);
        PathTransformChanger.faceTo(newSubPathTransform, basicEvent.subjectName, pathTransformObj.endPosition);

        KeyFrame keyFrameFace = frameFactory.genTurnFrame(basicEvent.subjectName, subgoname, basicEvent.startTime + basicEvent.duration * 9 / 10, basicEvent.duration / 10, newSubPathTransform);

        keyFrames.Add(keyFrameFace);

        posUpdater.updateAPosInfo(subgoname, newSubPathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        this.keyFrames = keyFrames;
    }

   


}
