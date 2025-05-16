using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlanner : Planner
{
    string audioFileName = "";
    public AudioPlanner(BasicEvent basicEvent,Recorder recorder,string audioFileName) : base(basicEvent,recorder)
    {
        this.audioFileName = audioFileName;
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
        KeyFrame keyFrame = frameFactory.genAudioFrame(this.audioFileName, basicEvent.startTime, basicEvent.duration,ref pathTransform);
        keyFrames.Add(keyFrame);
        this.keyFrames= keyFrames;
    }



    public override void planTransform()
    {
        return;
    }
}
