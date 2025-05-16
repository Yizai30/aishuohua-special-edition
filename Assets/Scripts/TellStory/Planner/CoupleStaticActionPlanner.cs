using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoupleStaticActionPlanner : Planner
{
    private string rawActionName;
    //string disappearGoname;
    string defaultSubGoname;
    string defaultObjGoname;
    
    public CoupleStaticActionPlanner(BasicEvent basicEvent,Recorder recorder,string rawActionName) : base(basicEvent,recorder)
    {
        this.basicEvent = basicEvent;
        this.rawActionName = rawActionName;
        

        
    }
    public override void planning()
    {
        if (!recorder.goMemDic.ContainsKey(basicEvent.subjectName))
        {
            return;
        }
        this.defaultSubGoname = recorder.getTopGo(basicEvent.subjectName);
        if (!recorder.goMemDic.ContainsKey(basicEvent.objectName))
        {
            this.defaultObjGoname = "";
        }
        else
        {
            this.defaultObjGoname = recorder.getTopGo(basicEvent.objectName);
        }
        //新方案，原子移动动作
        if (DataMap.actSplitList.containsRawActName(rawActionName))
        {
            ActSplitElement actSplitElement = DataMap.actSplitList.getActElementByRawname(rawActionName);
            planByAtomMove(actSplitElement);
        }
        if (keyFrames.Count != 0) return;

        //原方案，在diyact表中
        if (DataMap.diyActList.containRawName(rawActionName))
        {
            DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(rawActionName);
            planDiyAction(diyActElement);
        }
        if (this.keyFrames.Count == 0)
        {
            planTransform();
            planKeyframe();
        }
        


    }


    public void planByAtomMove(ActSplitElement actSplitElement)
    {
        PathTransform pathTransformSub = recorder.getPathTransformCopy(defaultSubGoname);
        PathTransform pathTransformObj = recorder.getPathTransformCopy(defaultObjGoname);
        List<KeyFrame> re = atomMoveFactory.getAtomMoveList(actSplitElement.actType, basicEvent, pathTransformSub, pathTransformObj, defaultSubGoname, defaultObjGoname);
        if (re.Count == 0) return;
        this.keyFrames.AddRange(re);
        //posUpdater.updateAPosInfo(defaultSubGoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
    }

    public void planDiyAction(DiyActElement diyActElement)
    {
        //if (diyActElement.actType.Equals("n_coupleStaticSee"))
        //{
        //    planTransformFaceToObj(defaultSubGoname, defaultObjGoname);
        //    planKeyframeSee(defaultSubGoname, defaultObjGoname);
        // if (diyActElement.actType.Equals("n_coupleStaticFaceToFace"))
        //{
        //    planTransformFaceToFace(defaultSubGoname, defaultObjGoname);
        //    planKeyframeFaceToFace(defaultSubGoname, defaultObjGoname, "");
        //}else if (diyActElement.actType.Equals("n_coupleStaticSpeak"))
        //{
        //    planTransformFaceToFace(defaultSubGoname, defaultObjGoname);
        //    planKeyframeFaceToFace(defaultSubGoname, defaultObjGoname, basicEvent.actionName);
        //if (diyActElement.actType.Equals("n_coupleStaticEat")||
        //    diyActElement.actType.Equals("n_coupleStaticDrink")||
        //    diyActElement.actType.Equals("n_coupleStaticOpenBook")||
        //    diyActElement.actType.Equals("n_coupleStaticCloseBook"))
        //{
        //    planTransformFaceToObj(defaultSubGoname, defaultObjGoname);
        //    planKeyframeOperateObj(defaultSubGoname, defaultObjGoname);
        //}
       
    }

    public override void planKeyframe()
    {
              
    }

    public override void planTransform()
    {
    }

    /*
    private void planKeyframeOperateObj(string subgoname,string objgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();
        //int keyframeActionType = 2;
        PathTransform pathTransformSub = this.transformDic[subgoname];
       

        if (objgoname != "")
        {
            PathTransform pathTransformObj = this.transformDic[objgoname];
          
            //取得obj的临边
            int subIndex = recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransformSub.endPosition);
            int objIndex = recorder.bkgPosRecorder.getIndexByPos( this.basicEvent.backgroundName, pathTransformObj.endPosition);
            List<float> objNei = recorder.bkgPosRecorder.getNeighborPoint(basicEvent.backgroundName, objIndex, subIndex,subgoname, 1);
            if (objNei.Count == 0)
            {
                //obj没有邻位置
                //objNei = DataUtil.Clone<List<float>>(pathTransformObj.endPosition);
                objNei = recorder.bkgPosRecorder.getSameTypeEmptyPoint(basicEvent.backgroundName, objIndex);
            }
            if (!DataUtil.compareList<float>(objNei, pathTransformSub.endPosition))
            {
                pathTransformSub.endPosition = objNei;
                KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration/2, pathTransformSub,"");                                                                                       
                keyFrames.Add(keyFrameGo);


                //转向
                PathTransform newPathTransformSub = DataUtil.Clone<PathTransform>(pathTransformSub);
                PathTransformChanger.faceTo( newPathTransformSub, basicEvent.subjectName,pathTransformObj.endPosition);
                pathTransformSub = newPathTransformSub;

                KeyFrame keyFrameEat = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, basicEvent.actionName, basicEvent.startTime + basicEvent.duration / 2, basicEvent.duration / 2, pathTransformSub,"");                                  
                keyFrames.Add(keyFrameEat);

               //处理对object的状态改变
                List<KeyFrame> keyFrameObjOp = objectStateChanger.handleObj(basicEvent.objectName, objgoname, basicEvent.startTime + basicEvent.duration, ref pathTransformObj,rawActionName);
                keyFrames.AddRange(keyFrameObjOp);
            }
            else
            {

                KeyFrame keyFrameEat = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, basicEvent.actionName, basicEvent.startTime , basicEvent.duration, pathTransformSub);               
                keyFrames.Add(keyFrameEat);


                List<KeyFrame> keyFrameObjOp = objectStateChanger.handleObj(basicEvent.objectName, objgoname, basicEvent.startTime + basicEvent.duration, ref pathTransformObj, rawActionName);
                keyFrames.AddRange(keyFrameObjOp);
            }
            
            //this.recorder.removeGo(basicEvent.objectName);
           
        }

        else
        {
            KeyFrame keyFrameEat = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, basicEvent.actionName, basicEvent.startTime + basicEvent.duration / 2, basicEvent.duration / 2, pathTransformSub);          
            keyFrames.Add(keyFrameEat);

        }
        //更新recorder      
        posUpdater.updateAPosInfo(subgoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        this.keyFrames = keyFrames;

    }

    

    private void planKeyframeFaceToFace(string subgoname, string objgoname,string actionName)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();
        //int keyframeActionType = 2;
        PathTransform pathTransformSub = this.transformDic[subgoname];

        string subRawName= DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(basicEvent.subjectName);

        string speakAction = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, "说" });

        KeyFrame keyFrameSub = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, speakAction, basicEvent.startTime, basicEvent.duration, pathTransformSub);

       
        keyFrames.Add(keyFrameSub);

        if (objgoname != "")
        {
            PathTransform pathTransformObj = this.transformDic[objgoname];
            KeyFrame keyFrameObj = frameFactory.genStaticActFrame(basicEvent.objectName, objgoname, "", basicEvent.startTime, basicEvent.duration, pathTransformObj);


            keyFrames.Add(keyFrameObj);
            //this.recorder.bkgPosRecorder.updateTransform(objgoname, pathTransformObj);
            posUpdater.updateAPosInfo(objgoname, pathTransformObj, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        }
        posUpdater.updateAPosInfo(subgoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        

        this.keyFrames = keyFrames;
    }

    //主体走向面对客体，客体消失
    private void planTransformFaceToObj(string subgoname,string objgoname)
    {
        //找到subject物体的位置信息
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);

        if (objgoname == "")
        {
            //this.makeTransformStatic(ref pathTransformSub);
            this.transformDic.Add(subgoname, pathTransformSub);

        }
        else
        {
            //找到object物体的位置信息
            PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);
            PathTransformChanger.faceTo( pathTransformSub,  pathTransformObj, basicEvent.subjectName);
            this.transformDic.Add(subgoname, pathTransformSub);
            this.transformDic.Add(objgoname, pathTransformObj);
        }     
    }

    //两个物体面对面
    private void planTransformFaceToFace(string subgoname, string objgoname)
    {
        PathTransform pathTransformSub = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);
        int subPosIndex = recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, pathTransformSub.endPosition);
       
        if (objgoname == "")
        {
            List<string> rightObjList = recorder.bkgPosRecorder.getIndexRightGoname(basicEvent.backgroundName, subPosIndex);
            List<string> leftObjList = recorder.bkgPosRecorder.getIndexLeftGoname(basicEvent.backgroundName, subPosIndex);
            bool isSubFaceRight = PathTransformChanger.isFaceRight(pathTransformSub,DataUtil.getMatnameByGoname( subgoname));
            if (isSubFaceRight && rightObjList.Count==0 && leftObjList.Count!=0)
            {
                objgoname = leftObjList[0];
                
            }

            if (!isSubFaceRight && leftObjList.Count == 0 && rightObjList.Count != 0)
            {
                objgoname = rightObjList[0];
                
            }
        }

        if (objgoname == "")
        {
            this.transformDic.Add(subgoname, pathTransformSub);
        }

        else
        {
            //找到object物体的位置信息
            PathTransform pathTransformObj = this.recorder.bkgPosRecorder.getTransformByGoname(objgoname);
            basicEvent.objectName = DataUtil.getMatnameByGoname(objgoname);
            float objScale = Mathf.Abs(pathTransformObj.endScale[0]);
            float subScale = Mathf.Abs(pathTransformSub.endScale[0]);

            PathTransformChanger.faceToFace(ref pathTransformSub, ref pathTransformObj, basicEvent.subjectName, basicEvent.objectName, subScale, objScale);

            //changeFace(ref pathTransformSub, ref pathTransformObj, subScale, objScale, basicEvent.subjectName+"1", basicEvent.objectName+"1");

            //this.makeTransformStatic(ref pathTransformSub);
            this.transformDic.Add(subgoname, pathTransformSub);

            this.transformDic.Add(objgoname, pathTransformObj);
        }
       


    }

    
    private void planKeyframeSee(string subgoname, string objgoname)
    {
        PathTransform pathTransformSub = this.transformDic[subgoname];

        KeyFrame keyFrameSub = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime, basicEvent.duration, pathTransformSub);

      
        keyFrames.Add(keyFrameSub);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransformSub);
        posUpdater.updateAPosInfo(subgoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);

    }

    */

}

