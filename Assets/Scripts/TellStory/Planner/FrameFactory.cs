using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;
using System;

//生成帧
public class FrameFactory
{
    Recorder recorder;
    PosUpdater posUpdater;

    public FrameFactory(Recorder recorder)
    {
        this.recorder = recorder;
        posUpdater = new PosUpdater(recorder);
    }



    public KeyFrame genMoveFrame(string matname, string goname,float startTime,float duration, PathTransform pathTransform,string indicateAnim)
    {

        PathTransformChanger.faceTo( pathTransform, matname, pathTransform.endPosition);

        List<KeyFrame> re = new List<KeyFrame>();
        //移动的动画 可以统一得到
        string subRawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matname);
        string actionName = "";
        try
        {
           
            //actionName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, "到" });
            actionName = DataMap.GetActNameMapValue(new List<string> { subRawName, "到" });
            if (indicateAnim != "")
            {
                if (indicateAnim == "static")
                {
                    actionName = "";
                }
                else
                {
                    //actionName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, indicateAnim });
                    actionName = DataMap.GetActNameMapValue(new List<string> { subRawName, indicateAnim });
                }
            }

        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
       

        
        KeyFrame moveFrame = new KeyFrame(2, 2, matname, goname, startTime,
            pathTransform.startPosition, pathTransform.endPosition, pathTransform.endRotation,
            pathTransform.endRotation, pathTransform.endScale, pathTransform.endScale,
            duration, actionName, 1, new List<float> { 0, 0, 0 });

        re.Add(moveFrame);
     
        PathTransformChanger.makeTransformStatic( pathTransform);
       

        return moveFrame;
    }

    public KeyFrame genTurnFrame(string matname, string goname, float startTime, float duration, PathTransform pathTransform)
    {

        List<KeyFrame> re = new List<KeyFrame>();

        KeyFrame moveFrame = new KeyFrame(2, 2, matname, goname, startTime,
            pathTransform.endPosition, pathTransform.endPosition, pathTransform.startRotation,
            pathTransform.endRotation, pathTransform.endScale, pathTransform.endScale,
            duration, "", 1, new List<float> { 0, 0, 0 });

        re.Add(moveFrame);
      
        PathTransformChanger.makeTransformStatic(pathTransform);
    
     
        return moveFrame;
    }



    public KeyFrame  genStaticActFrame(string matname, string goname, string actionName, float startTime, float duration, PathTransform pathTransform,string indicateAnim)
    {

        string tempActName = "";
        if (indicateAnim != "")
        {
            string subRawName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getKeyNameByVal(matname);
            //tempActName = DataMap.convMatMapList.getConvMapByMapName("ActionMap").getValByKeyList(new List<string> { subRawName, indicateAnim });
            tempActName = DataMap.GetActNameMapValue(new List<string> { subRawName, indicateAnim });
        }
        if(tempActName!=null&& tempActName != "")
        {
            actionName = tempActName;
        }
        KeyFrame re = new KeyFrame(2, 2, matname, goname, startTime,
            pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation,
            pathTransform.endRotation, pathTransform.endScale, pathTransform.endScale,
            duration, actionName, 1, new List<float> { 0, 0, 0 });

       
       
        PathTransformChanger.makeTransformStatic( pathTransform);
        return re;
    }


    public  KeyFrame genChangeScaleFrame(string matname, string goname, float startTime,ref PathTransform pathTransform)
    {
        KeyFrame keyFrame = new KeyFrame(2, 2, matname,goname, startTime,
                        pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                        pathTransform.startScale, pathTransform.endScale, 0, "", 0, new List<float> { 0, 0, 0 });
        PathTransformChanger.makeTransformStatic( pathTransform);
        return keyFrame;
    } 


    public  KeyFrame genChangeColorFrame(string matname, string goname, float startTime,ref PathTransform pathTransform,List<float>color)
    {
        KeyFrame keyFrame = new KeyFrame(7, 2, matname, goname, startTime,
                      pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                      pathTransform.startScale, pathTransform.endScale, 0, "", 0, color);
        PathTransformChanger.makeTransformStatic( pathTransform);
        return keyFrame;
    }

    //消失帧
    //若有子物体
    public KeyFrame genDisappearFrame(string matname, string goname, float startTime,PathTransform pathTransform)
    {
        List<KeyFrame> re = new List<KeyFrame>();

        KeyFrame keyFrameDisappear = new KeyFrame(1, 2, matname, goname, startTime,
                     pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                     pathTransform.endScale, pathTransform.endScale, 0, "", 0, new List<float> { 0, 0, 0 });

        PathTransformChanger.makeTransformStatic( pathTransform);
     
        return keyFrameDisappear;
        
    }

    public List<KeyFrame> genAppearFrame(string matname, string goname, float startTime,ref PathTransform pathTransform)
    {       
        List<KeyFrame> re = new List<KeyFrame>();

      

        KeyFrame keyFrameAppear = new KeyFrame(0, 2, matname, goname, startTime,
                       pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                       pathTransform.endScale, pathTransform.endScale, 0, "", 0, new List<float> { 0, 0, 0 });
        PathTransformChanger.makeTransformStatic( pathTransform);
        re.Add(keyFrameAppear);
        //FrameFactoryFollow frameFactoryFollow = new FrameFactoryFollow(goname, startTime, 0, recorder, pathTransform);
        //List<KeyFrame> followFrames = frameFactoryFollow.getInitFollowFrame(pathTransform.startPosition, pathTransform.endPosition);
        //re.AddRange(followFrames);
        return re ;
    }

    public  KeyFrame genAudioFrame(string audioFileName, float startTime,float duration,ref PathTransform pathTransform)
    {
        KeyFrame keyFrame = new KeyFrame(3, 2, audioFileName, "", startTime,
                               pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation, pathTransform.endRotation,
                               pathTransform.startScale, pathTransform.endScale, duration, "", 0, new List<float> { 0, 0, 0 });
        PathTransformChanger.makeTransformStatic( pathTransform);
        return keyFrame;
    }

    public  KeyFrame genBkgFrame(string bkgName, float startTime, float duration)
    {
        PathTransform pathTransform = new PathTransform();
        //pathTransform.init();

        KeyFrame keyFrame = new KeyFrame(4, 2, bkgName, "", startTime,
                                pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation, pathTransform.endRotation,
                                 pathTransform.startScale, pathTransform.endScale,duration, "", 0, new List<float> { 0, 0, 0 });
        PathTransformChanger.makeTransformStatic( pathTransform);
        return keyFrame;
    }

    public KeyFrame genSubFrame( float startTime, float duration,ref PathTransform pathTransform,string text)
    {
        KeyFrame keyFrame = new KeyFrame(5, 2, "", "", startTime,
                               pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation,  pathTransform.endRotation,
                               pathTransform.startScale, pathTransform.endScale, duration, text, 0, new List<float> { 0, 0, 0 });
        PathTransformChanger.makeTransformStatic( pathTransform);
        return keyFrame;
    }


    


    public  List<KeyFrame> genChildKeyframe(string subgoname, float startTime, float duration,List<float>startPos,List<float>endPos)
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
        PathTransform pathTransformSub = DataUtil.Clone<PathTransform>(recorder.bkgPosRecorder.transformDic[subgoname]);
        PathTransform pathTransformObj = recorder.bkgPosRecorder.transformDic[childGoName];

        PathTransformChanger.movePositionTogether( pathTransformSub, ref pathTransformObj);
        PathTransformChanger.faceSameDirection( pathTransformSub, ref pathTransformObj, subname, childname);

        KeyFrame keyFrame = new KeyFrame(2, 2, childname, childGoName, startTime,
           startPos, endPos, pathTransformObj.startRotation,
           pathTransformObj.endRotation, pathTransformObj.startScale, pathTransformObj.endScale,
           duration, "", 1, new List<float> { 0, 0, 0 });
        re.Add(keyFrame);
        PathTransformChanger. makeTransformStatic( pathTransformObj);
        //recorder.bkgPosRecorder.updateTransform(childGoName, pathTransformObj);
        posUpdater.updateAPosInfo(childGoName, pathTransformObj, recorder.currBkgName, PosUpdater.UpdateType.update);
        
        return re;

    }


    public KeyFrame turnToCenterFrame( PathTransform pathTransform, string subgoname,BasicEvent basicEvent)
    {
        //改变朝向 移动后，朝向中心位置
        List<int> pointList = this.recorder.bkgPosRecorder.getSameTypePointList(basicEvent.backgroundName,basicEvent.pointFlag);
        int midIndex = (pointList.Count) / 2;
        List<float> midPos = recorder.bkgPosRecorder.getPosByIndex(basicEvent.backgroundName, midIndex);

        //PathTransform newPathTransform = DataUtil.Clone<PathTransform>(pathTransform);
        PathTransformChanger.faceTo( pathTransform, basicEvent.subjectName, midPos);

        KeyFrame keyFrameTurn = genStaticActFrame(basicEvent.subjectName, subgoname, "", basicEvent.startTime + basicEvent.duration * 0.99f, basicEvent.duration * 0.01f,pathTransform,"");
        return keyFrameTurn;
    }

    public List<float> getForwardPoint(string goname, string matName, PathTransform pathTransform,string bkgName)
    {
        float speed = DataMap.matSettingList.getMatDefaultSpeed(matName);
        int speedInt = (int)speed;

        List<float> re = new List<float>();
        int curIndex = recorder.bkgPosRecorder.getIndexByPos(bkgName, pathTransform.endPosition);

        string curFlag = recorder.bkgPosRecorder.getPointFlag(bkgName, curIndex);
        List<int> curPointList = recorder.bkgPosRecorder.getSameTypePointList(bkgName, curFlag);
        if (PathTransformChanger.isFaceRight(pathTransform, matName))
        {
            re = recorder.bkgPosRecorder.getForwardNeighborPoint(bkgName, curIndex, curIndex + 1, speedInt);

        }
        else
        {
            re = recorder.bkgPosRecorder.getForwardNeighborPoint(bkgName, curIndex, curIndex - 1, speedInt);

        }

        //int index = recorder.bkgPosRecorder.getIndexByPos(goname, basicEvent.backgroundName, re);
        if (re.Count == 0)
        {
            //用最慢速尝试
            if (PathTransformChanger.isFaceRight(pathTransform, matName))
            {
                re = recorder.bkgPosRecorder.getForwardNeighborPoint(bkgName, curIndex, curIndex + 1, 1);

            }
            else
            {
                re = recorder.bkgPosRecorder.getForwardNeighborPoint(bkgName, curIndex, curIndex - 1, 1);

            }
            if (re.Count == 0)
            {
                re = DataUtil.Clone<List<float>>(pathTransform.endPosition);
            }


        }

        return re;
    }


}
