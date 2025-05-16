using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BkgPlanner : Planner
{

    public BkgPlanner(BasicEvent basicEvent,Recorder recorder) : base(basicEvent,recorder)
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
        KeyFrame keyFrame = frameFactory.genBkgFrame(basicEvent.backgroundName, basicEvent.startTime, basicEvent.duration);
        /*
        KeyFrame keyFrame = new KeyFrame(4, 2, this.basicEvent.backgroundName, "", basicEvent.startTime,
                               pathTransform.startPosition, pathTransform.endPosition, pathTransform.startRotation, pathTransform.endRotation, 
                                pathTransform.startScale, pathTransform.endScale,basicEvent.duration, "", 0, new List<float> { 0, 0, 0 });
        */
        keyFrames.Add(keyFrame);
               
        this.keyFrames= keyFrames;
    }

    public override void planTransform()
    {
        return;
    }

   
}
