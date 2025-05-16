using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttrChangePlanner : Planner
{
    string defaultSubGoname;
    AttrRecord attrRecord = new AttrRecord();
    public AttrChangePlanner(BasicEvent basicEvent,Recorder recorder) : base(basicEvent,recorder)
    {
        
    }
    public override void planning()
    {
        if (!this.recorder.goMemDic.ContainsKey(basicEvent.subjectName))
        {
            return;
           
        }
       
        this.defaultSubGoname = this.recorder.getTopGo(basicEvent.subjectName);
        //this.defaultObjGoname = basicEvent.objectName + "1";
        planTransform();
        planKeyframe();
    }

    public override void planKeyframe()
    {
        List<KeyFrame> keyFrames = new List<KeyFrame>();
        
        if (!this.recorder.goMemDic.ContainsKey(basicEvent.subjectName))
        {
            throw new System.Exception("AttrChangePlanner没有找到角色" + basicEvent.subjectName);
        }
       
        for(int attrI = 0; attrI < basicEvent.attrList.Count; attrI++)
        {
            AttrElement attrElement = DataMap.attrList.getElementByAttrName(basicEvent.attrList[attrI]);

            if (attrElement == null)
            {
                //可能是角色              
                if (DataMap.ContainObjNameMapKey(new List<string> { basicEvent.attrList[attrI] }))
                {
                    //对角色变化的处理
                    //string prefabName = DataMap.convMatMapList.getConvMapByMapName("ObjectMap").getValByKeyList(new List<string> { basicEvent.attrList[attrI] });
                    string prefabName = DataMap.GetObjNameMapValue(new List<string> { basicEvent.attrList[attrI] });
                    planKeyframeRecover(prefabName, defaultSubGoname);
                    continue;
                }
                else continue;


            }

            //PathTransform pathTransform = this.transformDic[basicEvent.subjectName + "1"];
            if (attrElement.attrType.Equals("scale"))
            {
                planKeyframeScale(attrElement, defaultSubGoname);
            }
            else if (attrElement.attrType.Equals("color"))
            {
                planKeyframeColor(attrElement, defaultSubGoname);
            }else if (attrElement.attrType.Equals("recover"))
            {
                planKeyframeRecover(basicEvent.subjectName, defaultSubGoname);
            }

            //更新attrDic
            //AttrRecord attrRecord=new AttrRecord()
            if(this.attrRecord.color.Count!=0 || this.attrRecord.scale.Count != 0)
            {
                if (recorder.containAttrDicEle(defaultSubGoname))
                {
                    //AttrRecord existAttr = recorder.attrRecordDic[basicEvent.subjectName];
                    AttrRecord existAttr = recorder.getAttrDicEle(defaultSubGoname);
                    if (attrRecord.color.Count != 0) existAttr.color = DataUtil.Clone<List<float>>(attrRecord.color);
                    if (attrRecord.scale.Count != 0) existAttr.scale = DataUtil.Clone<List<float>>(attrRecord.scale);
                    //recorder.attrRecordDic[basicEvent.subjectName]
                }
                else
                {
                    AttrRecord newAttr = new AttrRecord();
                    if (attrRecord.color.Count != 0) newAttr.color = DataUtil.Clone<List<float>>(attrRecord.color);
                    if (attrRecord.scale.Count != 0) newAttr.scale = DataUtil.Clone<List<float>>(attrRecord.scale);
                    recorder.addAttrDicEle(defaultSubGoname, attrRecord);
                    //recorder.attrRecordDic.Add(basicEvent.subjectName, attrRecord);
                }

                
                if (recorder.getAttrDicEle(defaultSubGoname).scale.Count == 0)
                {
                    PathTransform transform = this.transformDic[defaultSubGoname];
                    AttrRecord attrRecord = recorder.getAttrDicEle(defaultSubGoname);
                    attrRecord.scale= DataUtil.Clone<List<float>>(transform.endScale);
                    //recorder.addAttrDicEle
                    //recorder.attrRecordDic[basicEvent.subjectName].scale =DataUtil.Clone<List<float>>( transform.endScale);
                }
            }
           
        }
        
        
    }

    public void planKeyframeScale(AttrElement attrElement,string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>( this.transformDic[subgoname]);
        pathTransform.startScale = DataUtil.listMul(pathTransform.endScale, attrElement.attrValue);
        pathTransform.endScale = DataUtil.listMul(pathTransform.endScale, attrElement.attrValue);
        KeyFrame keyFrame = frameFactory.genChangeScaleFrame(basicEvent.subjectName, subgoname, basicEvent.startTime,ref pathTransform);
        keyFrames.Add(keyFrame);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);

        attrRecord.scale =DataUtil.Clone<List<float>>( pathTransform.endScale);
    }

    public void planKeyframeColor(AttrElement attrElement,string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(this.transformDic[subgoname]);
        List<float> color = attrElement.attrValue;

        KeyFrame keyFrame = frameFactory.genChangeColorFrame(basicEvent.subjectName, subgoname, basicEvent.startTime,ref pathTransform, color);
        keyFrames.Add(keyFrame);
        /*
        KeyFrame keyFrame = new KeyFrame(7, 2, basicEvent.subjectName, subgoname, basicEvent.startTime,
                      pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                      pathTransform.startScale, pathTransform.endScale, 0, "", 0, color);
        keyFrames.Add(keyFrame);
        */

        attrRecord.color = DataUtil.Clone<List<float>>(color);
    }

    public void planKeyframeRecover(string regenPrefabName,string subgoname)
    {
        PathTransform pathTransform = DataUtil.Clone<PathTransform>(this.transformDic[subgoname]);
        float scaleSize = Mathf.Abs(DataMap.matSettingList.getMatSettingMap(basicEvent.subjectName).initScale[0]);
       

        List<float>tempEndScale =DataUtil.Clone<List<float>>( pathTransform.endScale);
        List<float>tempAbsScale= DataUtil.Clone<List<float>>(pathTransform.endScale);
        for(int i=0;i<tempEndScale.Count;i++)
        {
           tempAbsScale[i]=  Mathf.Abs(tempEndScale[i]);
        }

        pathTransform.startScale = DataUtil.listMul( DataUtil.listDiv(tempEndScale, tempAbsScale), scaleSize);

        pathTransform.endScale = DataUtil.Clone<List<float>>(pathTransform.startScale);

        pathTransform.startRotation = DataUtil.Clone<List<float>>( DataMap.matSettingList.getMatSettingMap(basicEvent.subjectName).initRotation);
        pathTransform.endRotation = DataUtil.Clone<List<float>>(pathTransform.startRotation);


        //重新生成。
        KeyFrame keyFrameDisappear = frameFactory.genDisappearFrame(basicEvent.subjectName, subgoname, basicEvent.startTime, pathTransform);

        List<KeyFrame> keyFrameAppear = frameFactory.genAppearFrame(basicEvent.subjectName, subgoname, basicEvent.startTime+0.01f,ref pathTransform);
        /*
        KeyFrame keyFrameDisappear = new KeyFrame(1, 2, basicEvent.subjectName, subgoname, basicEvent.startTime,
                      pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                      pathTransform.endScale, pathTransform.endScale, 0, "", 0, new List<float> { 0, 0, 0 });

        KeyFrame keyFrameAppear = new KeyFrame(0, 2, regenPrefabName, subgoname, basicEvent.startTime+0.05f,
                      pathTransform.endPosition, pathTransform.endPosition, pathTransform.endRotation, pathTransform.endRotation,
                      pathTransform.endScale, pathTransform.endScale, 0, "", 0, new List<float> { 0, 0, 0 });
        */
        keyFrames.Add(keyFrameDisappear);
        keyFrames.AddRange(keyFrameAppear);
        //this.recorder.bkgPosRecorder.updateTransform(subgoname, pathTransform);
        posUpdater.updateAPosInfo(subgoname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.update);
        recorder.removeAttrDicEle(subgoname);
        
    }

   
   

    public override void planTransform()
    {
       
       if (this.recorder.bkgPosRecorder.transformDic.ContainsKey(defaultSubGoname))
       {
            this.transformDic.Add(defaultSubGoname, this.recorder.bkgPosRecorder.getTransformByGoname(defaultSubGoname));
       }
        
        
        
    }

   
}
