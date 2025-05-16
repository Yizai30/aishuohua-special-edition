using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitPlanner : Planner
{
    private string goname;
    private string actionRawName;
    public InitPlanner(BasicEvent basicEvent,Recorder recorder,string actionRawName):base (basicEvent,recorder)
    {
        this.actionRawName = actionRawName;
    }

    public override void planning()
    {
       
        planTransform();
        planKeyframe();
    }



    public override void planKeyframe()
    {
        if (goname == null) return;
        List<KeyFrame> keyFrames = new List<KeyFrame>();


        AttrRecord attrRecord = null;
        attrRecord = recorder.getAttrDicEle(goname);
        
        PathTransform pathTransform = this.transformDic[this.goname];
        List<float> color = new List<float>();
        if (attrRecord != null)
        {
            pathTransform.startScale = DataUtil.Clone<List<float>>( attrRecord.scale);
            pathTransform.endScale= DataUtil.Clone<List<float>>(attrRecord.scale);
            color = DataUtil.Clone<List<float>>(attrRecord.color);
        }

        List<KeyFrame> keyFrame = frameFactory.genAppearFrame(basicEvent.subjectName, goname, basicEvent.startTime,ref pathTransform);

        keyFrames.AddRange(keyFrame);

        if (color.Count != 0)
        {
            KeyFrame ckeyFrame = frameFactory.genChangeColorFrame(basicEvent.subjectName, goname, basicEvent.startTime + 0.1f,ref pathTransform, color);
           
            keyFrames.Add(ckeyFrame);
        }

        posUpdater.updateAPosInfo(goname, pathTransform, basicEvent.backgroundName, PosUpdater.UpdateType.appear);

        this.keyFrames= keyFrames;
    }

   
    public override void planTransform()
    {
        //得到subobject的初始位置
        //MatInitSettingElement el = DataMap.matInitSettingList.getElementByObjAndBkg(basicEvent.subjectName, basicEvent.backgroundName);

        if (!this.recorder.bkgPosRecorder.bkgPosUseDic.ContainsKey(basicEvent.backgroundName))
        {         
            throw new System.Exception("bkg_setting.json中没有对应的背景信息"+ basicEvent.backgroundName);
        }
       
        List<float> initPos = getInitPos();       

        if (initPos.Count == 0) return;
        //生成名字
        this.goname = this.recorder.genGoName(basicEvent.subjectName);
       // setupMatZ(ref initPos, basicEvent.subjectName);

        PathTransform pathTransform = new PathTransform();
        pathTransform.startPosition = initPos;
        pathTransform.endPosition = initPos;

        pathTransform.startRotation = DataUtil.Clone<List<float>>( DataMap.matSettingList.getMatSettingMap(basicEvent.subjectName).initRotation);
        pathTransform.endRotation = DataUtil.Clone<List<float>>(pathTransform.startRotation);


        pathTransform.startScale = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(basicEvent.subjectName).initScale);
        pathTransform.endScale = DataUtil.Clone<List<float>>(pathTransform.startScale);


        //避免看向无任何对象的方向

        int initPosIndex = this.recorder.bkgPosRecorder.getIndexByPos(this.basicEvent.backgroundName, initPos);
        if (!this.recorder.bkgPosRecorder.isEmpty(this.basicEvent.backgroundName, 0, initPosIndex))
        {
            List<float> tempPos = this.recorder.bkgPosRecorder.getPosByIndex(this.basicEvent.backgroundName, 0);
            PathTransformChanger.faceTo( pathTransform, basicEvent.subjectName, tempPos);
        }
       
        
        this.transformDic.Add(goname, pathTransform);
    }

    List<float> getInitPos()
    {
        List<float> re = new List<float>();
        
        BkgSettingElement bkgSettingElement = DataMap.bkgSettingList.getElementByBkgName(this.basicEvent.backgroundName);
        List<string> classList = DataMap.matSettingList.getMatSettingMap(basicEvent.subjectName).classList;
        List<int> initPosIndexList = new List<int>();

        //处理环境物,出现时，出现在自己的位置
        if (classList.Contains("env_prop"))
        {
            EnvPointSettingElement envPointSettingElement= DataMap.envPointSettingList.getElementByEnvPropMatName(basicEvent.subjectName);
            List<string> pointFlagList = envPointSettingElement.pointNameList;
            string targetPointFlag = "";

            foreach(PointMark pointMark in bkgSettingElement.pointMarks)
            {
                if (pointFlagList.Contains(pointMark.pointType))
                {
                    targetPointFlag = pointMark.pointType;
                }
            }
            
            initPosIndexList = bkgSettingElement.getPointListByPointType(targetPointFlag);
            re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
            return re;
        }

        //有包含入场词
        if (DataMap.comeFlag.Contains(actionRawName))
        {
            //根据pointFlag
            if (this.basicEvent.pointFlag != "")
            {
                //如果是每个地图都有的pointFlag，取入口点
                if (basicEvent.pointFlag == "defaultPoint" || basicEvent.pointFlag.Contains("groundPoint"))
                {
                    basicEvent.pointFlag = "ground_enterPoint";
                }
                else if (basicEvent.pointFlag.Contains("skyPoint"))
                {
                    basicEvent.pointFlag = "sky_enterPoint";
                }
                else if (basicEvent.pointFlag.Contains("waterPoint"))
                {
                    basicEvent.pointFlag = "water_enterPoint";
                }

                initPosIndexList = bkgSettingElement.getPointListByPointType(basicEvent.pointFlag);


                if (initPosIndexList.Count != 0)
                {
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
                }
                // interval = 0;
            }
            //根据入场词动作
            if (re.Count == 0)
            {
                //在入口点初始化
                if (!classList.Contains("prop") && (DataMap.skyComeFlag.Contains(actionRawName) || DataMap.groundComeFlag.Contains(actionRawName)))
                {
                    if (DataMap.groundComeFlag.Contains(actionRawName))
                    {
                        //"来到"不确定，继续用种类判断

                        if (classList.Contains("sky"))
                        {
                            initPosIndexList = bkgSettingElement.getPointListByPointType("sky_enterPoint");
                          
                        }
                        else if (classList.Contains("water"))
                        {
                            initPosIndexList = bkgSettingElement.getPointListByPointType("water_enterPoint");
                            if (initPosIndexList.Count == 0)
                            {
                                initPosIndexList = bkgSettingElement.getPointListByPointType("ground_enterPoint");
                            }

                        }
                        else
                        {
                            initPosIndexList = bkgSettingElement.getPointListByPointType("ground_enterPoint");
                        }

                        //initPosIndexList = bkgSettingElement.getPointListByPointType("ground_enterPoint");
                        //re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, goname, 1, initPosIndexList.Count / 2);

                    }
                    else if (DataMap.skyComeFlag.Contains(actionRawName))
                    {
                        initPosIndexList = bkgSettingElement.getPointListByPointType("sky_enterPoint");
                        //re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, goname, 1, initPosIndexList.Count / 2);
                    }                

                }

                if (initPosIndexList.Count > 0)
                {
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 1, initPosIndexList.Count / 2);
                }
                else
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType("defaultPoint");
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
                }
            }
        }
        else
        {
            //如果没有入场词，不在起点

            //先根据pointFlag
            if (this.basicEvent.pointFlag != "")
            {
                if (this.basicEvent.pointFlag== "defaultPoint" && classList.Contains("prop"))
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType("propPoint");
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
                    //re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, goname, 2, initPosIndexList.Count / 2);
                }
                else
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType(basicEvent.pointFlag);
                    //当前场景中没有对应的pointFlag
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
                }
                
            }
            else
            {
                

                //没有pointFlag，根据物体种类
                if (classList.Contains("sky"))
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType("skyPoint");
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);

                }
                else if (classList.Contains("prop"))
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType("propPoint");
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2,initPosIndexList.Count/2);
                    //re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, goname, 2, initPosIndexList.Count / 2);
                }
                else if (classList.Contains("water"))
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType("waterPoint");
                    re = this.recorder.bkgPosRecorder.getEmptyRightFirst(basicEvent.backgroundName, initPosIndexList, 2);

                }
                else
                {
                    initPosIndexList = bkgSettingElement.getPointListByPointType("defaultPoint");
                    re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
                }
            }
            
            
        }
      
        //
        if (re.Count == 0)
        {
            initPosIndexList = bkgSettingElement.getPointListByPointType("defaultPoint");
            re = this.recorder.bkgPosRecorder.getEmptyPosMidFirst(basicEvent.backgroundName, initPosIndexList, 2, initPosIndexList.Count / 2);
        }
        return re;
    }


    

}
