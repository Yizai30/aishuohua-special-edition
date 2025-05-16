using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;
using System;
using System.Reflection;

public class AtomMoveFactory 
{
    //原子动作
    //移动到某个区域：MoveToRegion
    //移动到某个物体旁：MoveToObject/MoveToSubject
    //向前一步：MoveForward
    //向后一步：MoveBackward
    //转向某个区域方向：TurnToRegion
    //转向某个物体方向：TurnToObject/TurnToSubject
    //转头：TurnAround
    //播放动画（保持不动）：StayStatic
    
    Recorder recorder;
    FrameFactory frameFactory;
    PosUpdater posUpdater;


    public AtomMoveFactory(Recorder recorder,FrameFactory frameFactory)
    {
        this.recorder = recorder;
        this.frameFactory = frameFactory;
        this.posUpdater = new PosUpdater(recorder);
    }


    KeyFrame Stay(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove,int flag)
    {
        string indicateAnim = "";
        string matname = "";
        if (flag == 0)
        {
            indicateAnim = atomMove.subAnimation;
            matname = basicEvent.subjectName;
        }
        else
        {
            indicateAnim = atomMove.objAnimation;
            matname = basicEvent.objectName;
        }
        KeyFrame re = frameFactory.genStaticActFrame(matname, maingoname, basicEvent.actionName, startTime, duration, pathTransformMain, indicateAnim);
        return re;
    }

    //转头
     KeyFrame TurnBack(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        PathTransformChanger.turnBack(pathTransformMain, DataUtil.getMatnameByGoname(maingoname));
        
        KeyFrame re = frameFactory.genTurnFrame(DataUtil.getMatnameByGoname(maingoname), maingoname, startTime, duration, pathTransformMain);
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        return re;
    }

     KeyFrame TurnToObject(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        if (pathTransformAttach == null || pathTransformMain == null) return null;
        PathTransformChanger.faceTo(pathTransformMain, pathTransformAttach, DataUtil.getMatnameByGoname(maingoname));
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        KeyFrame re = frameFactory.genTurnFrame(DataUtil.getMatnameByGoname(maingoname), maingoname, startTime,duration, pathTransformMain);
        return re;
    }

     KeyFrame TurnToRegion(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        string pf;
        if (atomMove.subPointFlag != "")
        {
            pf = atomMove.subPointFlag;
        }
        else
        {
            pf = basicEvent.pointFlag;
        }
        if (pf == "") pf = "defaultPoint";
        List<int> pointIndexList= recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, pf);
        List<float> pos = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, pointIndexList[0]);
        PathTransformChanger.faceTo(pathTransformMain, DataUtil.getMatnameByGoname(maingoname), pos);
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        KeyFrame re = frameFactory.genTurnFrame(DataUtil.getMatnameByGoname(maingoname), maingoname, startTime,duration, pathTransformMain);
        return re;
    }

     KeyFrame MoveToRegion(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        string rPointflag = "";
        string indicateAnim = "";
        if (flag == 0)
        {
            rPointflag = atomMove.subPointFlag;
            indicateAnim = atomMove.subAnimation;
        }
        else
        {
            rPointflag = atomMove.objPointFlag;
            indicateAnim = atomMove.objAnimation;
        }
        string pf;
        if (atomMove.subPointFlag != "")
        {
            pf = rPointflag;
        }
        else
        {
            pf = basicEvent.pointFlag;
        }

        string matName = DataUtil.getMatnameByGoname(maingoname);
        //如果已经在区域中，向前一步
        int curIndex = recorder.bkgPosRecorder.getIndexByPos( basicEvent.backgroundName, pathTransformMain.endPosition);
        string curFlag = recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curIndex);

        List<float> endPos = new List<float>();

        if (pf != "")
        {
            if (curFlag.Equals(pf))
            {
                //向前一步
                endPos = frameFactory.getForwardPoint(maingoname, matName, pathTransformMain, basicEvent.backgroundName);
            }
            else
            {
        
                //走到别的点集
                List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName ,pf);
                //endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, eventPointList, 1, eventPointList.Count / 2);
                endPos = recorder.bkgPosRecorder.getEmptyNearestPos(basicEvent.backgroundName, eventPointList, pathTransformMain.endPosition);
            }
        }
        else
        {
            endPos = frameFactory.getForwardPoint(maingoname, matName, pathTransformMain, basicEvent.backgroundName);
        }
        pathTransformMain.endPosition = endPos;
        
        KeyFrame keyFrame= frameFactory.genMoveFrame(matName, maingoname, startTime, duration, pathTransformMain, indicateAnim);
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        return keyFrame;

    }


    KeyFrame MoveToObject(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        if (pathTransformAttach == null) return null;
        //取得obj的临边
        int subIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformMain.endPosition);
        int objIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformAttach.endPosition);
        List<float> objNei = recorder.bkgPosRecorder.getNeighborPoint(basicEvent.backgroundName, objIndex, subIndex, maingoname, 1);
        if (objNei.Count == 0)
        {
            //obj没有邻位置
            //objNei = DataUtil.Clone<List<float>>(pathTransformObj.endPosition);
            objNei = recorder.bkgPosRecorder.getSameTypeEmptyPoint(basicEvent.backgroundName, objIndex);
        }
        pathTransformMain.endPosition = objNei;

        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, maingoname, basicEvent.startTime, basicEvent.duration / 2, pathTransformMain, "");
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        return keyFrameGo;
    }

    //晕倒
     KeyFrame Tumble(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        PathTransformChanger.tumble(pathTransformMain);
        recorder.addTumbleGoname(maingoname);

        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        KeyFrame re = frameFactory.genTurnFrame(DataUtil.getMatnameByGoname(maingoname), maingoname, startTime, duration, pathTransformMain);
        return re;
    }

    //消失
     KeyFrame Disappear(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
       

        KeyFrame keyFrame = frameFactory.genDisappearFrame(basicEvent.subjectName, maingoname, startTime, pathTransformMain);

   
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.disappear);
        return keyFrame;
    }

    //爬起来
     KeyFrame TumbleRecover(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        PathTransformChanger.tumbleRecover(pathTransformMain, basicEvent.subjectName);
        recorder.removeTumbleGoname(maingoname);

        KeyFrame keyFrame = frameFactory.genTurnFrame(basicEvent.subjectName, maingoname, startTime, duration, pathTransformMain);

        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        return keyFrame;
    }

    //向前进
    KeyFrame MoveForward(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        //如果在入口，先出来
        int curIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformMain.endPosition);
        string curFlag = recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curIndex);
        if (curFlag.Contains("enterPoint"))
        {
            if (curFlag.Equals("ground_enterPoint"))
            {
                List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "defaultPoint");
                pathTransformMain.endPosition = recorder.bkgPosRecorder.getEmptyRightFirst(basicEvent.backgroundName, eventPointList, 1);                
            }
            if (curFlag.Equals("sky_enterPoint"))
            {
                List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "skyPoint");
                pathTransformMain.endPosition = recorder.bkgPosRecorder.getEmptyRightFirst(basicEvent.backgroundName, eventPointList, 1);              
            }
        }
        else
        {
            if (curFlag != basicEvent.pointFlag && basicEvent.pointFlag!="")
            {
                List<int> eventPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, basicEvent.pointFlag);
                pathTransformMain.endPosition = recorder.bkgPosRecorder.getEmptyRightFirst(basicEvent.backgroundName, eventPointList, 1);
            }
            else
            {
                pathTransformMain.endPosition = frameFactory.getForwardPoint(maingoname, basicEvent.subjectName, pathTransformMain, basicEvent.backgroundName);
            }
            
        }              
        KeyFrame keyFrame = frameFactory.genMoveFrame(basicEvent.subjectName, maingoname, startTime, duration, pathTransformMain, "");
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        return keyFrame;

    }



    //特殊动作（不易拆分的封装动作）
    //飞
    KeyFrame Fly(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {     
        int curPointIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformMain.endPosition);
        string curPointFlag = recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curPointIndex);
        List<int> curPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, curPointFlag);
        List<float> endPos = new List<float>();
        if (basicEvent.pointFlag != "")
        {

            List<int> pointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, basicEvent.pointFlag);
            endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, pointList, 2, pointList.Count / 2);
            
            
        }
        else
        {
            if (curPointFlag.Equals("skyPoint"))
            {
                endPos = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, curPointList, 2, curPointList.Count / 2);
               
            }
            else
            {
                //查看其正上方点位是不是为空,
                float curX_pos = pathTransformMain.endPosition[0];

                int targetPosIndex = recorder.bkgPosRecorder.getPointIndexByX("skyPoint", basicEvent.backgroundName, curX_pos, 3);

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
                
            }

        }

        pathTransformMain.endPosition = endPos;
        KeyFrame keyFrame = frameFactory.genMoveFrame(basicEvent.subjectName, maingoname, startTime, duration, pathTransformMain, "");
        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        return keyFrame;
    }


    //降落
    KeyFrame Land(BasicEvent basicEvent, PathTransform pathTransformMain, PathTransform pathTransformAttach, string maingoname, string attchgoname,
       float startTime, float duration, AtomMove atomMove, int flag)
    {
        
        int curPointIndex = recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformMain.endPosition);
        string curPointFlag = recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, curPointIndex);
        List<int> curPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, curPointFlag);
        if (!curPointFlag.Equals("defaultPoint"))
        {
            //查看其正下方点位是不是为空,
            float curX_pos = pathTransformMain.endPosition[0];

            int targetPosIndex = recorder.bkgPosRecorder.getPointIndexByX("defaultPoint", basicEvent.backgroundName, curX_pos, 3);

            if (targetPosIndex != -1)
            {
                if (recorder.bkgPosRecorder.isIndexEmpty(basicEvent.backgroundName, targetPosIndex))
                {
                    pathTransformMain.endPosition = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, targetPosIndex);
                }
            }
            else
            {
                List<int> defaultPointList = recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName, "defaultPoint");
                pathTransformMain.endPosition = recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, defaultPointList, 2, defaultPointList.Count / 2);
            }
        }

        KeyFrame keyFrameGo = frameFactory.genMoveFrame(basicEvent.subjectName, maingoname, startTime,duration, pathTransformMain, "");
        

        posUpdater.updateAPosInfo(maingoname, pathTransformMain, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        return keyFrameGo;
    }


   
    //从split文件中读取轨迹
    public List<KeyFrame> getAtomMoveList(string actType,BasicEvent basicEvent,PathTransform pathTransformSub,PathTransform pathTransformObj,string subgoname,string objgoname)
    {
        List<KeyFrame> re = new List<KeyFrame>();
        ActSplitElement actSplitElement= DataMap.actSplitList.getActElementByTypeName(actType);
        if (actSplitElement == null) return re;

        //根据条件选择轨迹
        MoveState moveState = getMoveStateFromEvent(basicEvent, pathTransformSub, pathTransformObj);
        SplitCondition targetCondition = null;
        foreach(SplitCondition splitCondition in actSplitElement.SplitConditionList)
        {
            string conditionStr = splitCondition.condition;
            //string  TrimConditionStr= System.Text.RegularExpressions.Regex.Unescape(conditionStr);
            conditionStr = conditionStr.Replace(@"\", "");
            if (conditionStr == "")
            {
                targetCondition = splitCondition;
                break;
            }

            bool boolRe = false;
            DataUtil.TestCondition(conditionStr, moveState,ref boolRe);
            if (boolRe == true)
            {
                targetCondition = splitCondition;
                break;
            }

        }
        //没有找到对应的条件
        if (targetCondition == null) return re;
        //var result = DataUtil.Eval("curPointFlag.contains(sky),", "curPointFlag=\"skyPoint\";className=\"ground\";");
        List<AtomMove> atomMoves = targetCondition.atomMoveList;

        float eventStartTime = basicEvent.startTime;
        float eventDuration = basicEvent.duration;
        int seqNum = atomMoves.Count;
        float eachDuration = 0;
        float eachStartTime = 0;
        if (seqNum != 0)
        {
            eachDuration = basicEvent.duration / seqNum;
        }

        Dictionary<int, List<float>> durationDic = setupAtomMoveStartAndDuration(targetCondition.atomMoveList,eventStartTime, eventDuration);

        foreach (AtomMove atomAct in targetCondition.atomMoveList)
        {
            string subAtomMoveName = atomAct.subAtomMove;
            string subActName = atomAct.subAnimation;
            string objAtomMoveName = atomAct.objAtomMove;
            string objActName = atomAct.objAnimation;

            eachStartTime = durationDic[atomAct.seqNum][0];
            eachDuration = durationDic[atomAct.seqNum][1];
            //sub的原子动作
            if (atomAct.subAtomMove != "")
            {
                KeyFrame subFrame = getFramesFromAtomMove(atomAct.subAtomMove,atomAct, basicEvent, pathTransformSub, pathTransformObj, subgoname, objgoname,
              eachStartTime, eachDuration,0);
                if (subFrame != null)
                {
                    re.Add(subFrame);
                }
            }


            //obj的原子动作
            if (atomAct.objAtomMove != "")
            {
                KeyFrame objFrame = getFramesFromAtomMove(atomAct.objAtomMove,atomAct, basicEvent, pathTransformObj, pathTransformSub, objgoname, subgoname,
              eachStartTime, eachDuration,1);

                if (objFrame != null)
                {
                    re.Add(objFrame);
                }
            }
           

        }
        
        return re;
    }

    //设置每个原子移动动作的时间
    //列表第一项为开始时间，第二项为持续时间
    Dictionary<int, List<float>> setupAtomMoveStartAndDuration(List<AtomMove>atomActs,float startTime, float totalDuration)
    {
        
        Dictionary<int, List<float>> durationDic = new Dictionary<int, List<float>>();
        int totalTimeGap = 0;//总时间单位
        for (int i = 0; i < atomActs.Count; i++)
        {
            if (atomActs[i].duration == "short")
            {
                totalTimeGap += 1;
            }
            else if (atomActs[i].duration == "long")
            {
                totalTimeGap += 10;
            }
            else
            {
                totalTimeGap += 1;
            }
        }

        float startTimeCurrent = startTime;
        for (int i = 0; i < atomActs.Count; i++)
        {
            float currentDuration = 0;
            if (atomActs[i].duration == "short")
            {
                List<float> temp = new List<float>();
                temp.Add(startTimeCurrent);
                currentDuration = totalDuration / totalTimeGap;
                temp.Add(currentDuration);
                durationDic.Add(i + 1, temp);
                
            }
            else if (atomActs[i].duration == "long")
            {
                List<float> temp = new List<float>();
                temp.Add(startTimeCurrent);
                currentDuration = totalDuration * 10 / totalTimeGap;
                temp.Add(currentDuration);
                durationDic.Add(i + 1, temp);
                
            }
            else
            {

                List<float> temp = new List<float>();
                temp.Add(startTimeCurrent);
                currentDuration = totalDuration / totalTimeGap;
                temp.Add(currentDuration);
                durationDic.Add(i + 1, temp);
            }
            startTimeCurrent += currentDuration;
        }
        return durationDic;
    }

    //用反射机制调用split文件中对应的原子动作函数
    //flag=0:main=sub flag=1:main=obj
     KeyFrame getFramesFromAtomMove(string atomMoveName, AtomMove atomMove, BasicEvent basicEvent,PathTransform mainPathTransform,PathTransform attachPathTransform,
         string mainGoname,string attachGoname,float eachStartTime,float eachDuration,int flag)
     {

        
        if(mainGoname!=null && mainGoname != "")
        {
            mainPathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[mainGoname]);
        }

        if (attachGoname != null && attachGoname != "")
        {
            attachPathTransform = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[attachGoname]);
        }
        


        KeyFrame re = null;
        
        if (atomMoveName == "MoveForward")
        {
            re = MoveForward(basicEvent, mainPathTransform, attachPathTransform, mainGoname, attachGoname, eachStartTime, eachDuration, atomMove, flag);
            return re;
        }else if (atomMoveName == "TurnBack")
        {
            re = TurnBack(basicEvent, mainPathTransform, attachPathTransform, mainGoname, attachGoname, eachStartTime, eachDuration, atomMove, flag);
            return re;
        }
        

        //if (atomMoveName == "MoveToRegion")
        //{
        //    re = MoveToRegion(basicEvent, mainPathTransform, attachPathTransform, mainGoname, attachGoname, eachStartTime, eachDuration, atomMove,flag);
        //    return re;
        //}
        
        
        
        object[] paramsList = new object[] { basicEvent, mainPathTransform, attachPathTransform, mainGoname, attachGoname, eachStartTime, eachDuration, atomMove,flag };

        Type thisType = this.GetType();
        MethodInfo theMethod = thisType
            .GetMethod(atomMoveName, BindingFlags.NonPublic | BindingFlags.Instance);
        re= (KeyFrame)theMethod.Invoke(this,paramsList);

        return re;
    }



    public MoveState getMoveStateFromEvent(BasicEvent basicEvent, PathTransform pathTransformSub, PathTransform pathTransformObj)
    {
        List<string> subMatClass = new List<string>();
        List<string> objMatClass = new List<string>();
        string subPointFlag = "";
        string objPointFlag = "";

        string subMatName = basicEvent.subjectName;
        string objMatName = basicEvent.objectName;
        if (subMatName != "")
        {
            subMatClass = DataMap.matSettingList.getMatSettingMap(subMatName).classList;

        }
        if (objMatName != "")
        {
            objMatClass = DataMap.matSettingList.getMatSettingMap(objMatName).classList;
        }

        if (pathTransformSub != null)
        {
            int pointIndex = this.recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformSub.endPosition);
            subPointFlag = this.recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, pointIndex);
        }

        if (pathTransformObj != null)
        {
            int pointIndex = this.recorder.bkgPosRecorder.getIndexByPos(basicEvent.backgroundName, pathTransformSub.endPosition);
            objPointFlag = this.recorder.bkgPosRecorder.getPointFlag(basicEvent.backgroundName, pointIndex);
        }

        MoveState moveState = new MoveState(subPointFlag, subMatClass, objPointFlag, objMatClass);
        return moveState;

    }


}


//条件参数
public class MoveState
{
    public string subCurPointFlag { set; get; }
    public List<string> subMatClass { set; get; }

    public string objCurPointFlag { set; get; }

    public List<string> objMatClass { set; get; }

    public MoveState(string subCurPointFlag, List<string> subMatClass, string objCurPointFlag, List<string> objMatClass)
    {
        this.subCurPointFlag = subCurPointFlag;
        this.subMatClass = subMatClass;
        this.objCurPointFlag = objCurPointFlag;
        this.objMatClass = objMatClass;
    }

    public string ConvertStateToStr()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("string subCurPointFlag=\"" + subCurPointFlag + "\";" +
            "string objCurPointFlag=\"" + objCurPointFlag + "\";"+
            "List<string>subMatClass=new List<string> {");
        foreach(string subClass in this.subMatClass)
        {
            sb.Append("\""+subClass+"\""+",");
        }
        sb.Append("};");

        sb.Append(" List<string> objMatClass = new List<string> {");
        foreach (string objClass in this.objMatClass)
        {
            sb.Append("\""+objClass+"\""+",");
        }
        sb.Append("};");

        return sb.ToString();


    }
   
}

