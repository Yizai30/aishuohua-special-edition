using System.Collections;
using System.Collections.Generic;
using AnimGenerator;
using UnityEngine;

public abstract class Planner
{
    public BasicEvent basicEvent { set; get; }
    //public PathTransform pathTransform { set; get; }
    //当前规划生成的规划路径结果
    public Dictionary<string,PathTransform> transformDic { set; get; }

    public List<KeyFrame> keyFrames { set; get; }

    public Recorder recorder { set; get; }

    protected FrameFactory frameFactory { set; get; }

    protected AtomMoveFactory atomMoveFactory { set; get; }

    protected PosUpdater posUpdater { set; get; }

    protected ObjectStateChanger objectStateChanger { set; get; }
    public Planner(BasicEvent basicEvent,Recorder recorder)
    {
        this.basicEvent = basicEvent;
        this.recorder = recorder;
        //pathTransform = new PathTransform();
        transformDic = new Dictionary<string, PathTransform>();
        keyFrames = new List<KeyFrame>();
        frameFactory = new FrameFactory(recorder);
        atomMoveFactory = new AtomMoveFactory(recorder, frameFactory);

        posUpdater = new PosUpdater(recorder);
        objectStateChanger = new ObjectStateChanger(frameFactory,posUpdater,recorder);
    }

    

    public abstract void planning();

    public abstract void planTransform();
    public abstract void planKeyframe();



   


}
