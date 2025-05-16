using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisPlanner : Planner
{

    public DisPlanner(BasicEvent basicEvent,Recorder recorder) : base(basicEvent,recorder)
    {
        this.basicEvent = basicEvent;
    }

    public override void planning()
    {
        planTransform();
        planKeyframe();
    }

    public override void planKeyframe()
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();


        PathTransform pathTransform = new PathTransform();
        //pathTransform.init();
        int num;
        if (recorder.fileNum <= 0)
        {
            num= recorder.goMemDic[basicEvent.subjectName];
        }
        else
        {
            num = RecorderList.recorderList[recorder.fileNum-1].goMemDic[basicEvent.subjectName];
        }
       
        for(int i = 1; i <= num; i++)
        {
            string goname = basicEvent.subjectName + i;

            KeyFrame keyframe = frameFactory.genDisappearFrame(basicEvent.subjectName, goname, basicEvent.startTime,pathTransform);
            /*
            KeyFrame keyFrame = new KeyFrame(1, 2, basicEvent.subjectName, goname, basicEvent.startTime,
                           pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation, pathTransform.endRotation,
                           pathTransform.startScale, pathTransform.endScale, 0, "", 0, new List<float> { 0, 0, 0 });
            */
            keyFrames.Add(keyframe);
            //recorder.removeTumbleGoname(goname);
            recorder.removeGo(basicEvent.subjectName);
            
            
        }
       
        this.keyFrames= keyFrames;
    }


    public override void planTransform()
    {
        return;
    }
}
