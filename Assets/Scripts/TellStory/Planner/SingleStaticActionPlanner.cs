using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleStaticActionPlanner : Planner
{
    string rawActionName;
    //string disappearGoName;
    string defaultSubGoname;
    public SingleStaticActionPlanner(BasicEvent basicEvent,Recorder recorder,string rawActionName) : base(basicEvent,recorder)
    {
        this.rawActionName = rawActionName;
        this.basicEvent = basicEvent;
       
       
    }

    public override void planning()
    {
        if (!recorder.goMemDic.ContainsKey(basicEvent.subjectName))
        {
            return;           
        }
      
        this.defaultSubGoname = recorder.getTopGo(basicEvent.subjectName);

        //新方案，原子移动动作
        if (DataMap.actSplitList.containsRawActName(rawActionName))
        {
            ActSplitElement actSplitElement = DataMap.actSplitList.getActElementByRawname(rawActionName);
            planByAtomMove(actSplitElement);
        }
        if (keyFrames.Count != 0) return;

        //原方案，在diyact表中
        //if (DataMap.diyActList.containRawName(rawActionName))
        //{
        //    DiyActElement diyActElement = DataMap.diyActList.getElemByRawName(rawActionName);
        //    planDiyAction(diyActElement);
        //}
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
        //if (diyActElement.actType.Equals("n_singleStaticTurnback"))
        //{
        //    planTransformTurnback(defaultSubGoname);
        //    planKeyframeTurnback(defaultSubGoname);
        //if (diyActElement.actType.Equals("n_singleStaticTumble"))
        //{
        //    planTransformTumble(defaultSubGoname);
        //    planKeyframeTumble(defaultSubGoname);
        //if (diyActElement.actType.Equals("n_singleStaticDisappear"))
        //{
         //   planTransformDisappear(defaultSubGoname);
         //   planKeyframeDisappear(defaultSubGoname);
        //}
        //if (diyActElement.actType.Equals("n_singleStaticTumbleRecover"))
        //{
        //    planTransformTumbleRecover(defaultSubGoname);
        //    planKeyframeTumbleRecover(defaultSubGoname);
        //}
        
    }

    public override void planKeyframe()
    {
        planKeyframeNormal(defaultSubGoname);
    }


    public override void planTransform()
    {
      
        
        planTransformNormal(defaultSubGoname);
        
    }

    void planTransformTumbleRecover(string subgoname)
    {
        PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(this.recorder.bkgPosRecorder.getTransformByGoname(subgoname));
        PathTransformChanger.tumbleRecover( pathTransformSub, basicEvent.subjectName);
        recorder.removeTumbleGoname(subgoname);
        this.transformDic.Add(subgoname, pathTransformSub);

    }

    void planKeyframeTumbleRecover(string subgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransform = this.transformDic[subgoname];

        KeyFrame keyFrame = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime, basicEvent.duration,  pathTransform,"");

       
        keyFrames.Add(keyFrame);

        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        this.keyFrames.AddRange(keyFrames);
    }

    void planTransformTurnback(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(this.recorder.bkgPosRecorder.getTransformByGoname(subgoname));

        PathTransformChanger.turnBack(pathTransform, basicEvent.subjectName);
        this.transformDic.Add(subgoname, pathTransform);
    }

    void planKeyframeTurnback(string subgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransform = this.transformDic[subgoname];

        KeyFrame keyFrame = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime, basicEvent.duration, pathTransform,"");

        /*
        KeyFrame keyFrame = new KeyFrame(2, 2, this.basicEvent.subjectName, subgoname, basicEvent.startTime,
           pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation,
           pathTransform.endRotation, pathTransform.startScale, pathTransform.endScale,
           basicEvent.duration, "", 1, new List<float> { 0, 0, 0 });
        */
        keyFrames.Add(keyFrame);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        this.keyFrames.AddRange(keyFrames);
    }

    void planTransformDisappear(string subgoname)
    {
        PathTransform ePathTransform = this.recorder.bkgPosRecorder.getTransformByGoname(subgoname);
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(ePathTransform);
       // makeTransformStatic(ref pathTransform);

        this.transformDic.Add(subgoname, pathTransform);
    }

    void planKeyframeDisappear(string subgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransform = this.transformDic[subgoname];

        KeyFrame keyFrame = frameFactory.genDisappearFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, pathTransform);
       
        keyFrames.Add(keyFrame);
      
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.disappear);
        this.keyFrames.AddRange(keyFrames);
    }

     void planTransformTumble(string subgoname)
    {
       
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(this.recorder.bkgPosRecorder.getTransformByGoname(subgoname));
        //makeTransformStatic(ref pathTransform);
        PathTransformChanger.tumble( pathTransform);
        recorder.addTumbleGoname(subgoname);
        
        this.transformDic.Add(subgoname, pathTransform);
    }

     void planKeyframeTumble(string subgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransform = this.transformDic[subgoname];    

        //落到地面
        string pointFlag= recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, recorder.bkgPosRecorder.
            getIndexByPos(basicEvent.backgroundName, pathTransform.endPosition));

        if (pointFlag.Contains("sky"))
        {
            List<int> groundIndex = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "defaultPoint");
            List<float> groundPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, groundIndex,2,groundIndex.Count/2);
            keyFrames.Add( frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime, 0.1f,  pathTransform,""));
            pathTransform.endPosition = groundPos;
            keyFrames.Add(frameFactory.genMoveFrame(basicEvent.subjectName,subgoname,basicEvent.startTime+0.1f,basicEvent.duration-0.1f, pathTransform,""));

        }
        else
        {
            keyFrames.Add(frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime, basicEvent.duration,  pathTransform,""));
        }

        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        this.keyFrames.AddRange(keyFrames);
    }

     void planTransformNormal(string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone < PathTransform > (this.recorder.bkgPosRecorder.getTransformByGoname(subgoname));
       // PathTransform pathTransform = DataUtil.Clone<PathTransform>(ePathTransform);
        //makeTransformStatic(ref pathTransform);

        this.transformDic.Add(subgoname, pathTransform);

       
    }

     void planKeyframeNormal(string subgoname)
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();

        PathTransform pathTransform = this.transformDic[subgoname];

        if (basicEvent.pointFlag != "")
        {
            List<int> posList= recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, basicEvent.pointFlag);

            List<float> pos= recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, posList, 2, posList.Count / 2);

            if(pos!=null && pos.Count != 0)
            {
                pathTransform.endPosition = pos;
                keyFrames.Add(frameFactory.genMoveFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, basicEvent.duration/2, pathTransform,""));
                keyFrames.Add(frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, basicEvent.actionName, basicEvent.startTime+basicEvent.duration/2,
                    basicEvent.duration/2,  pathTransform,""));
            }
        }

        if (keyFrames.Count == 0)
        {
            KeyFrame keyFrame = frameFactory.genStaticActFrame(basicEvent.subjectName, subgoname, basicEvent.actionName, basicEvent.startTime, basicEvent.duration,  pathTransform,"");

            keyFrames.Add(keyFrame);
        }

       

        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        this.keyFrames.AddRange(keyFrames);
    }

   
}
