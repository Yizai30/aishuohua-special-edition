using AnimGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleMoveActionPlanner : Planner
{

    string actRawName;
    string defaultSubGoname = "";
    public SingleMoveActionPlanner(BasicEvent basicEvent,Recorder recorder,string actRawName) : base(basicEvent,recorder)
    {
      
        this.basicEvent = basicEvent;
        this.actRawName = actRawName;
        
    }

    public override void planning()
    {
        
        if (!recorder.goMemDic.ContainsKey(basicEvent.subjectName))
        {
            return;
        }
        
        this.defaultSubGoname = recorder.getTopGo(basicEvent.subjectName);
        //if (recorder.bkgPosRecorder.isChildGoname(defaultSubGoname))
        //{
        //    return;
        //}

        //新方案，原子移动动作
        if (DataMap.actSplitList.containsRawActName(actRawName))
        {
            ActSplitElement actSplitElement = DataMap.actSplitList.getActElementByRawname(actRawName);
            planByAtomMove(actSplitElement);
        }
        if (keyFrames.Count != 0) return;

        //原方案
        if (DataMap.diyActList.containRawName(actRawName))
        {
            DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(actRawName);
            planDiyAction(diyActElement);
        }
        if(keyFrames.Count==0)
        {
            planTransform();
            planKeyframe();
        } 

       
    }

   
    public void planByAtomMove(ActSplitElement actSplitElement)
    {
        PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[defaultSubGoname]);
         List<KeyFrame> re = atomMoveFactory.getAtomMoveList(actSplitElement.actType, basicEvent, pathTransformSub, null, defaultSubGoname, "");
        if (re.Count == 0) return;
        this.keyFrames.AddRange(re);
        //posUpdater.updateAPosInfo(defaultSubGoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
    }

    public void planDiyAction(DiyActElement diyActElement)
    {
        //if (diyActElement.actType.Equals("n_singleMoveLeave"))
        //{
        //    planTransformLeave(defaultSubGoname);
        //    planKeyframeLeave(defaultSubGoname);
        //if (diyActElement.actType.Equals("n_singleMoveEscape"))
        //{
        //    planTransformEscape(defaultSubGoname);
        //    planKeyframeEscape(defaultSubGoname);
        //}
        //if (diyActElement.actType.Equals("n_singleMoveWander"))
        //{
        //    planTransformWander(defaultSubGoname);
        //    planKeyframeWander(defaultSubGoname);
        //if (diyActElement.actType.Equals("n_singleMoveFly"))
        //{
        //    planTransformFly(defaultSubGoname);
        //    planKeyframeFly(defaultSubGoname);
        //if (diyActElement.actType.Equals("n_singleMoveTakeoff"))
        //{
        //    planTransformTakeoff(defaultSubGoname);
        //    planKeyframeTakeoff(defaultSubGoname);
        //}
        //if (diyActElement.actType.Equals("n_singleMoveSit"))
        //{
            //走到 
        //    planTransformNormal(defaultSubGoname);
        //    planKeyframeNormal(defaultSubGoname); 
        //}
        //过桥
         if (diyActElement.actType.Equals("n_singleCrossBridge"))
        {
            if (!this.basicEvent.backgroundName.Contains("bridge")) return;
            planTransformCrossBridge(defaultSubGoname);
            planKeyframeCrossBridge(defaultSubGoname);
        }

        //移动到某个区域      
        //else if (diyActElement.actType.Equals("n_singleMoveToRegion"))
        //{
        //    PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[defaultSubGoname]);
        //    List<KeyFrame> re = atomMoveFactory.getAtomMoveList("n_singleMoveToRegion", basicEvent, pathTransformSub, null, defaultSubGoname, "");
        //    this.keyFrames.AddRange(re);
        //    posUpdater.updateAPosInfo(defaultSubGoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        //
        //}

                //要特殊处理，根据上一句话是否有此物体。
                //如果有，"来到"不是入场词
                //如果没有，是入场词
        else if (diyActElement.actType.Equals("n_singleMoveCome"))
        {

           
            if (recorder.isNewActor(basicEvent.subjectName))
            {
                planTransformCome(defaultSubGoname);
                planKeyframeCome(defaultSubGoname);
                recorder.comeActor.Add(defaultSubGoname);
            }
            else
            {
                planTransform();
                planKeyframe();
            }           
            
        }

    }


    private void planKeyframeCrossBridge(string subgoname)
    {
        PathTransform pathTransformSub = transformDic["subgoname"];
        //获取桥的出入口点位集

        //找出竖直方向，距离当前位置最近的点，作为入口
        int bridgeGateIndex = recorder.bkgPosRecorder.getNeareastBridgeGatePoint(basicEvent.backgroundName, pathTransformSub.endPosition);
        if (bridgeGateIndex == -1) return;

        List<float> bridgeGatePos = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, bridgeGateIndex);
        pathTransformSub.endPosition = bridgeGatePos;
        this.keyFrames.Add(frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration / 3, pathTransformSub,""));

        //找出对岸桥出口
        int bridgeGate2Index = recorder.bkgPosRecorder.getOtherBridgeGatePoint(basicEvent.backgroundName, bridgeGatePos);
        List<float> bridgeGate2Pos = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, bridgeGate2Index);
        pathTransformSub.endPosition = bridgeGate2Pos;
        this.keyFrames.Add(frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime+basicEvent.duration/3, basicEvent.duration / 3,  pathTransformSub,""));

        //在另一侧找一个空余点
        int otherSideIndex = recorder.bkgPosRecorder.getBridgeSidePos(basicEvent.backgroundName, bridgeGate2Pos);
        List<float> otherSidePos = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, otherSideIndex);
        pathTransformSub.endPosition = otherSidePos;
        this.keyFrames.Add(frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime + basicEvent.duration / 3*2, basicEvent.duration / 3,  pathTransformSub,""));

        posUpdater.updateAPosInfo(subgoname, pathTransformSub, basicEvent.backgroundName, PosUpdater.UpdateType.update);
    }

    private void planTransformCrossBridge(string subgoname)
    {
        PathTransform pathTransformSub= DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        this.transformDic.Add("subgoname", pathTransformSub);
    }

    public override void planKeyframe()
    {
        planKeyframeNormal(defaultSubGoname);      
    }

    public override void planTransform()
    {
        planTransformNormal(defaultSubGoname);
       
    }
    //如果有flag，且当前在flag内，向前方移动一个单位
    //如果有flag，且不在flag内，移动到flag
    //如果没有flag，向前方移动一个单位
    void planTransformNormal(string subgoname)
    {
             
        PathTransform pathTransform = DataUtil.Clone<PathTransform>( recorder.bkgPosRecorder.transformDic[subgoname]);              
        BkgSettingElement BkgEl = DataMap.bkgSettingList.getElementByBkgName(basicEvent.backgroundName);

        //如果是新角色，要移动，先朝右。（因为initplanner会调整角色避免其面向空）
        if (recorder.isNewActor(basicEvent.subjectName))
        {
            PathTransformChanger.faceToRight( pathTransform, basicEvent.subjectName);
        }

        //确认终点
        List<float> endPos = new List<float>();
        List<int> pointList = new List<int>();
        int curIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransform.endPosition);
        string curFlag = recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curIndex);
       
        if (basicEvent.pointFlag != "")
        {
            if (curFlag.Equals(basicEvent.pointFlag))
            {                
                //向前一步
                endPos = frameFactory. getForwardPoint(subgoname, basicEvent.subjectName, pathTransform,basicEvent.backgroundName);                               
            }
            else
            {
                //走到别的点集
                List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, basicEvent.pointFlag);
                endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, eventPointList,1,eventPointList.Count/2);
            }
        }
        else
        {
            endPos =frameFactory. getForwardPoint(subgoname, basicEvent.subjectName, pathTransform,basicEvent.backgroundName);
        }

        //如果在入口，先出来d

        if (curFlag.Equals("ground_enterPoint"))
        {
            List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "defaultPoint");
            endPos = recorder.bkgPosRecorder.getEmptyRightFirst(basicEvent.backgroundName, eventPointList, 1);

        }
        if (curFlag.Equals("sky_enterPoint"))
        {
            List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "skyPoint");
            endPos = recorder.bkgPosRecorder.getEmptyRightFirst(basicEvent.backgroundName, eventPointList, 1);

        }

        PathTransformChanger.moveToDis(basicEvent.subjectName,  pathTransform, endPos);
        this.transformDic.Add(subgoname, pathTransform);
     
    }

  

    void planKeyframeNormal(string subgoname)
    {
        List<KeyFrame> re = new List<KeyFrame>();


        PathTransform pathTransform = this.transformDic[subgoname];

        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration*0.99f, pathTransform,"");

        re.Add(keyFrameGo);
        PathTransform newPathtransform = DataUtil.Clone<PathTransform>(pathTransform);
        //如果是pointFlag是起点，转向
        if (basicEvent.pointFlag.Equals("ground_enterPoint"))
        {
            if (pathTransform.endPosition[0] < 0)
            {
                //向右转向
                
                PathTransformChanger.faceToRight( newPathtransform, basicEvent.subjectName);
                KeyFrame keyframeTurn = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime + basicEvent.duration * 0.99f, basicEvent.duration * 0.01f, newPathtransform,"");
                re.Add(keyframeTurn);
            }
            else
            {
                //向左转向
                
                PathTransformChanger.faceToLeft( newPathtransform, basicEvent.subjectName);
                KeyFrame keyframeTurn = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime + basicEvent.duration * 0.99f, basicEvent.duration * 0.01f,  newPathtransform,"");
                re.Add(keyframeTurn);
            }
        }  

        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, newPathtransform.endPosition);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, newPathtransform);
        posUpdater.updateAPosInfo(subgoname, newPathtransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        this.keyFrames .AddRange(re);
    }

    void planTransformCome(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);

        BkgSettingElement BkgEl = DataMap.bkgSettingList.getElementByBkgName(basicEvent.backgroundName);


        //确认终点
        List<float> endPos = new List<float>();
        List<int> pointList = new List<int>();
        if (basicEvent.pointFlag != "")
        {
            pointList = DataUtil.Clone(BkgEl.getPointListByPointType(basicEvent.pointFlag));
        }
        if (pointList.Count == 0)
        {
            List<string> classList = DataMap.matSettingList.getMatSettingMap(basicEvent.subjectName).classList;
            if (classList.Contains("sky"))
            {
                basicEvent.pointFlag = "skyPoint";
            }
            else if (classList.Contains("water"))
            {
                basicEvent.pointFlag = "waterPoint";
            }
            else
            {
                basicEvent.pointFlag = "defaultPoint";
            }
            pointList = DataUtil.Clone(BkgEl.getPointListByPointType(basicEvent.pointFlag));
            if (pointList.Count == 0)
            {
                basicEvent.pointFlag = "defaultPoint";
            }
            pointList = DataUtil.Clone(BkgEl.getPointListByPointType(basicEvent.pointFlag));

        }

        pathTransform.startPosition = this.recorder.bkgPosRecorder.getPosByGoname(basicEvent.backgroundName, subgoname);

       
        endPos = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, pointList, 2, pointList.Count / 2);


        if (endPos.Count == 0) throw new System.Exception("singleMoveActionPlanner移动目标没有空余点");

        //根据路径方向改变朝向
        //PathTransformChanger.faceTo(ref pathTransform, basicEvent.subjectName, endPos);       
        PathTransformChanger.moveToDis(basicEvent.subjectName,  pathTransform, endPos);
        this.transformDic.Add(subgoname, pathTransform);
    }

    void planKeyframeCome(string subgoname)
    {
        List<KeyFrame> re = new List<KeyFrame>();


        PathTransform pathTransform = this.transformDic[subgoname];

        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration * 0.99f,pathTransform,"");

        re.Add(keyFrameGo);

        //改变朝向 移动后，朝向中心位置

        PathTransform newPathTransform = DataUtil.Clone<PathTransform>(pathTransform);
        KeyFrame keyFrameTurn =frameFactory.turnToCenterFrame( newPathTransform, subgoname,basicEvent);      

        re.Add(keyFrameTurn);

        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, newPathTransform.endPosition);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, newPathTransform);

        posUpdater.updateAPosInfo(subgoname, newPathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        this.keyFrames = re;
    }

    void planTransformLeave(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);

        BkgSettingElement BkgEl = DataMap.bkgSettingList.getElementByBkgName(basicEvent.backgroundName);

        List<float> endPos = recorder.bkgPosRecorder.getExitPos(basicEvent.backgroundName, pathTransform.endPosition);
        PathTransformChanger.moveToDis(basicEvent.subjectName, pathTransform, endPos);
        this.transformDic.Add(subgoname, pathTransform);
    }

    void planTransformTakeoff(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        int curPointIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransform.endPosition);
        string curPointFlag = recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curPointIndex);
        List<int> curPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, curPointFlag);
        if (!curPointFlag.Equals("defaultPoint"))
        {
            //查看其正下方点位是不是为空,
            float curX_pos = pathTransform.endPosition[0];

            int targetPosIndex = recorder.bkgPosRecorder.getPointIndexByX("defaultPoint", basicEvent.backgroundName, curX_pos,3);

            if (targetPosIndex != -1)
            {
                if (recorder.bkgPosRecorder.isIndexEmpty(basicEvent.backgroundName, targetPosIndex))
                {
                    pathTransform.endPosition = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, targetPosIndex);
                }
            }
            else
            {
                List<int> defaultPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "defaultPoint");
                pathTransform.endPosition = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, defaultPointList, 2, defaultPointList.Count / 2);
            }
        }
       
        this.transformDic.Add(subgoname, pathTransform);
    }

    void planKeyframeTakeoff(string subgoname)
    {
        PathTransform pathTransform = this.transformDic[subgoname];


        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration, pathTransform,"");
        this.keyFrames.Add(keyFrameGo);

        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);

    }

    void planTransformFly(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        int curPointIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransform.endPosition);
        string curPointFlag= recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curPointIndex);
        List<int> curPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, curPointFlag);
        List<float> endPos = new List<float>();
        if (basicEvent.pointFlag != "" )
        {

            List<int> pointList= recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, basicEvent.pointFlag);
            endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, pointList, 2, pointList.Count / 2);
            PathTransformChanger.moveToDis(basicEvent.subjectName,  pathTransform, endPos);

            PathTransformChanger.faceTo( pathTransform, basicEvent.subjectName, pathTransform.endPosition);
        }
        else
        {
            if (curPointFlag.Equals("skyPoint"))
            {
                endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, curPointList, 2, curPointList.Count / 2);
                PathTransformChanger.moveToDis(basicEvent.subjectName,  pathTransform, endPos);

                PathTransformChanger.faceTo( pathTransform, basicEvent.subjectName, pathTransform.endPosition);
            }
            else
            {
                //查看其正上方点位是不是为空,
                float curX_pos = pathTransform.endPosition[0];

                int targetPosIndex = recorder.bkgPosRecorder.getPointIndexByX("skyPoint", basicEvent.backgroundName, curX_pos, 1);

                if (targetPosIndex != -1)
                {
                    if (recorder.bkgPosRecorder.isIndexEmpty(basicEvent.backgroundName, targetPosIndex))
                    {
                        endPos = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, targetPosIndex);
                    }
                    else
                    {
                        List<int> skyPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "skyPoint");
                        endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, skyPointList, 2, curPointList.Count / 2);
                    }
                }
                else
                {
                    List<int> skyPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "skyPoint");
                    endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, skyPointList, 2, curPointList.Count / 2);
                }
                pathTransform.endPosition = endPos;
                PathTransformChanger.moveToDis(basicEvent.subjectName,  pathTransform, endPos);
                PathTransformChanger.faceTo( pathTransform, basicEvent.subjectName, endPos);
            }              
           
        }
       

        this.transformDic.Add(subgoname, pathTransform);

    }

    void planKeyframeFly(string subgoname)
    {
        PathTransform pathTransform = this.transformDic[subgoname];


        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration, pathTransform,"");
        
        
        this.keyFrames.Add(keyFrameGo);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);       
        //this.recorder.bkgPosRecorder.changePos(basicEvent.backgroundName, subgoname, pathTransform.endPosition);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);
    }
    void planKeyframeLeave(string subgoname)
    {
        //planKeyframeNormal(subgoname);
        PathTransform pathTransform = this.transformDic[subgoname];
        PathTransform hidePathTransform = DataUtil.Clone<PathTransform>(pathTransform);

        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration * 0.99f, pathTransform,"");
       
        this.keyFrames.Add(keyFrameGo);

        KeyFrame hideKeyframe = frameFactory.genDisappearFrame(basicEvent.subjectName, subgoname, basicEvent.startTime + basicEvent.duration * 0.99f, hidePathTransform);
       
        this.keyFrames.Add(hideKeyframe);
        //this.recorder.removeGo(basicEvent.subjectName);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.disappear);

    }

    void planTransformEscape(string subgoname)
    {
        //planTransformNormal();
        PathTransform subTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        //找到主语的朝向信息
        bool faceRight = true;
        if (PathTransformChanger.isFaceRight(subTransform, basicEvent.subjectName))
        {
            faceRight = true;
        }
        else
        {
            faceRight = false;
        }       
       
        BkgSettingElement BkgEl = DataMap.bkgSettingList.getElementByBkgName(basicEvent.backgroundName);
        int startIndex = this.recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, subTransform.startPosition);
        List<int> allPointList = DataUtil.Clone(BkgEl.getPointListByPointType("escapePoint"));
        List<int> pointList = new List<int>();
        for (int i = 0; i < allPointList.Count; i++)
        {
            if (faceRight)
            {
                //取大于零的出口点
                if (BkgEl.initPositionList[allPointList[i]][0] > 0)
                {
                    pointList.Add(allPointList[i]);
                }
            }
            else
            {
                //取小于零的出口点
                if (BkgEl.initPositionList[allPointList[i]][0] <= 0)
                {
                    pointList.Add(allPointList[i]);
                }
            }

        }
        if (pointList.Count == 0)
        {
            pointList = allPointList;
        }

        List<float> endPos = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, pointList, 1,pointList.Count/2);

        PathTransformChanger.moveToDis(basicEvent.subjectName,  subTransform, endPos);

        this.transformDic[subgoname] = subTransform;

        subTransform.endPosition = endPos;
        //更新Recorder
        //int newIndex = this.recorder.bkgPosRecorder.getIndexByPos("", this.basicEvent.backgroundName, endPos);
        this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, endPos);
        posUpdater.updateAPosInfo(subgoname, subTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);
    }

    void planKeyframeEscape(string subgoname)
    {
        planKeyframeLeave(subgoname);
    }

    void planTransformWander(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        
        this.transformDic.Add(subgoname, pathTransform);
        //planTransformNormal(subgoname);

    }

    void planKeyframeWander(string subgoname)
    {

        //PathTransform pathTransform = this.recorder.bkgPosRecorder.transformDic[basicEvent.subjectName + "1"];
        PathTransform pathTransform = this.transformDic[subgoname];
        //PathTransform pathTransform = this.transformDic[basicEvent.subjectName + "1"];
        List<float> startPos = DataUtil.Clone<List<float>>(pathTransform.endPosition);
        int startIndex = recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, startPos);
        List<float> endPos= DataUtil.Clone<List<float>>( recorder.bkgPosRecorder.getNeighborPoint(this.basicEvent.backgroundName, startIndex,0,"",2));
        if (endPos.Count == 0)
        {
            endPos= DataUtil.Clone<List<float>>(recorder.bkgPosRecorder.getNeighborPoint(this.basicEvent.backgroundName, startIndex, 0,"", 1));
            if (endPos.Count == 0)
            {
                endPos = DataUtil.Clone<List<float>>(pathTransform.endPosition);
            }
        }

        PathTransform transformGo = DataUtil.Clone<PathTransform>(pathTransform);
        transformGo.endPosition = endPos;
        transformGo.startPosition = startPos;
        PathTransformChanger.faceTo( transformGo, basicEvent.subjectName, transformGo.endPosition);

        PathTransform transformBack = DataUtil.Clone<PathTransform>(transformGo);
        
        transformBack.endPosition = startPos;
        transformBack.startPosition = endPos;
       

        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration / 2, transformGo,"");


        KeyFrame keyFrameBack = frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime + basicEvent.duration / 2, basicEvent.duration / 2, transformBack,"");

       
        this.keyFrames.Add(keyFrameGo);
        this.keyFrames.Add(keyFrameBack);

        //this.recorder.bkgPosRecorder.changeTransform
        //this.makeTransformStatic(ref transformBack);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, transformBack);
        //int subIndex = this.recorder.bkgPosRecorder.getIndexByPos(subgoname, this.basicEvent.backgroundName, transformBack.endPosition);
        //this.recorder.bkgPosRecorder.changePos(this.basicEvent.backgroundName, subgoname, transformBack.endPosition);
        posUpdater.updateAPosInfo(subgoname, transformBack, basicEvent.backgroundName, PosUpdater.UpdateType.update);

    }

   

}
