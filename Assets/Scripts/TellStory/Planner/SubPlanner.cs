using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPlanner : Planner
{
    string subtitleText = "";
    public SubPlanner(BasicEvent basicEvent,Recorder recorder,string subtitleText) : base(basicEvent,recorder)
    {
        this.subtitleText = subtitleText;
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
        KeyFrame keyFrame = frameFactory.genSubFrame(basicEvent.startTime, basicEvent.duration,ref pathTransform, subtitleText);
        /*
        KeyFrame keyFrame = new KeyFrame(5, 2, basicEvent.subjectName, basicEvent.subjectName + "1", basicEvent.startTime,
                               pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation, pathTransform.endRotation,
                               pathTransform.startScale, pathTransform.endScale, basicEvent.duration, subtitleText, 0, new List<float> { 0, 0, 0 });
        */
        keyFrames.Add(keyFrame);
        this.keyFrames= keyFrames;
    }



    public override void planTransform()
    {
        return;
    }
}
