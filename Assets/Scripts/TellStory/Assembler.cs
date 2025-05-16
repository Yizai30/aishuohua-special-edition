using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AnimGenerator;
using System;
using System.Linq;
//using UnityEngine.Recorder.Examples;

public class Assembler : MonoBehaviour
{
    //private string storyName;
    public string targetName { set; get; }
    public Controller controller;
    public Interpreter interpreter;
    public float allDuration = 0;
    //public MovieRecorderExa movieRecorder;

    private void Start()
    {
        targetName = "";
       // movieRecorder.enabled = false;
    }

    //生成显示某一句话最后时刻位置信息的json,用于撤销
    public void assembleLastStateJson(Recorder recorder)
    {
        
        FrameFactory frameFactory = new FrameFactory(recorder);
        Dictionary<string, PathTransform> goTransDic = recorder.bkgPosRecorder.transformDic;
        List<KeyFrame> re = new List<KeyFrame>();
        //背景
        KeyFrame bkgFrame = frameFactory.genBkgFrame(recorder.currBkgName, 0, 0);
        re.Add(bkgFrame);
        //角色
        foreach(string goname in goTransDic.Keys)
        {
            PathTransform goTransform = goTransDic[goname];
            string matname = DataUtil.getMatnameByGoname(goname);
            List<KeyFrame> initFrame = frameFactory.genAppearFrame(matname, goname, 0,ref goTransform);
            re.AddRange(initFrame);
        }

        Debug.Log("正在生成回退动画文件");
        string storyName = controller.storyName;
        string directoryPath = $"{Application.persistentDataPath}/";
        this.targetName = storyName + "_" + recorder.fileNum + "Return";
        string targetFilePath = directoryPath + storyName + "_" + recorder.fileNum + "Return.json";
        KeyFrameList keyFrameList = new KeyFrameList();
        keyFrameList.keyFrames = re;
        JsonOperator.Obj2Json<KeyFrameList>(keyFrameList, targetFilePath);

    }

    //获取同背景名的文件列表
    public void assembleReturn(string curBkgName)
    {
        string directoryPath = $"{Application.persistentDataPath}/";
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
        string storyName = controller.storyName;

        FileInfo[] fileInfos = directoryInfo.GetFiles(storyName + "*"+curBkgName+".json");

        //根据文件名排序
        //文件排序
        List<FileInfo> sortedFiles = sortFiles(fileInfos);

        KeyFrameList keyFrameList = new KeyFrameList();
        float currTimeStamp = 0;
        //int index = 0;
        int fileNum = sortedFiles.Count;
        for (int i = 0; i < fileNum; i++)
        {
            string tempFileName = sortedFiles[i].FullName;
            KeyFrameList tempKeyframeList = JsonOperator.parseObj<KeyFrameList>(tempFileName);
            float listDuration = tempKeyframeList.keyFrames[0].duration;//录音frame包含一个list的时长
            float gap = 0;
            if (i != 0) gap = 0.001f;
            for (int ki = 0; ki < tempKeyframeList.keyFrames.Count; ki++)
            {

                KeyFrame tempKeyframe = tempKeyframeList.keyFrames[ki];
                tempKeyframe.timestamp = tempKeyframe.timestamp + currTimeStamp + gap;

                keyFrameList.keyFrames.Add(tempKeyframe);

                if (ki == tempKeyframeList.keyFrames.Count - 1)
                {
                    currTimeStamp += listDuration;
                }

            }

        }
        allDuration = currTimeStamp;
        Debug.Log("已合并背景为"+ curBkgName + "的" + fileInfos.Length + "个文件，动画时长：" + allDuration);
        Debug.Log("正在生成动画文件");
        this.targetName = storyName+"_"+curBkgName + "Return";
        string targetFilePath = directoryPath + storyName+"_"+curBkgName + "Return.json";
        JsonOperator.Obj2Json<KeyFrameList>(keyFrameList, targetFilePath);
    }

    

   

    public string assembleAllKeyframeBuffer()
    {

        string directoryPath = $"{Application.persistentDataPath}/";

        string storyName = controller.storyName;
        if (storyName == null || storyName == "")
        {
            Debug.Log("故事名称为空");
            return null;
        }


        
        List<List<KeyFrame>> frameList = new List<List<KeyFrame>>();
        for(int i = 0; i < controller.keyFrameListBufferList.Count; i++)
        {
            frameList.Add(controller.keyFrameListBufferList[i].mergeFrameList);

        }

        List<KeyFrame> keyFrames = assemble(frameList);
        KeyFrameList mergedKeyframe = new KeyFrameList();
        mergedKeyframe.keyFrames = keyFrames;
        Debug.Log("正在生成动画文件");
        this.targetName = storyName + "All";
        string targetFilePath = directoryPath + storyName + "All.json";
        JsonOperator.Obj2Json<KeyFrameList>(mergedKeyframe, targetFilePath);
        //
        return targetFilePath;

        
    }

    //合成当前keyframeBuffer的若干个keyframeList内容
    public List<KeyFrame> assemble(List<List<KeyFrame>> framesList)
    {
      
        // List<List<KeyFrame>> framesList = RecorderList.getAllFrameList();
        //List<List<KeyFrame>> framesList = controller.keyFrameListBuffer.keyFrameListList;
        KeyFrameList keyFrameList = new KeyFrameList();
        float currTimeStamp = 0;
        //int index = 0;
        if (framesList == null)
        {
            framesList = new List<List<KeyFrame>>();
        }
        int frameListNum = framesList.Count;
        for (int i = 0; i < frameListNum; i++)
        {
            
            List<KeyFrame> tempKeyframeList = framesList[i];
            float listDuration = 0;//所有录音frame的时长之和
            foreach (KeyFrame frame in tempKeyframeList)
            {
                if (frame.action == 3)
                {
                    listDuration += frame.duration;
                }
            }
            float gap = 0;
            if (i != 0) gap = 0.001f;
            for (int ki = 0; ki < tempKeyframeList.Count; ki++)
            {

                KeyFrame tempKeyframe = tempKeyframeList[ki];
                tempKeyframe.timestamp = tempKeyframe.timestamp + currTimeStamp + gap;

                keyFrameList.keyFrames.Add(tempKeyframe);

                if (ki == tempKeyframeList.Count - 1)
                {
                    currTimeStamp += listDuration;
                }

            }

        }
        allDuration = currTimeStamp;
        Debug.Log("已合并" + framesList.Count + "个文件，动画时长：" + allDuration);

        KeyFrameList mergedKeyframe = mergeAction(keyFrameList);
        return mergedKeyframe.keyFrames;

        
        
    }


    List<FileInfo> sortFiles(FileInfo[] rawFiles)
    {
        List<FileInfo> fileInfos = new List<FileInfo>();
        Dictionary<int, FileInfo> tempDic = new Dictionary<int, FileInfo>();
        foreach(FileInfo fileInfo in rawFiles)
        {

            int sharpIndex = fileInfo.Name.IndexOf('#');
            string fileNum = fileInfo.Name.Substring(controller.storyName.Length, sharpIndex-controller.storyName.Length);
            int fileNunInt = Convert.ToInt32(fileNum);
            tempDic.Add(fileNunInt, fileInfo);
        }

        //字典排序
        tempDic = tempDic.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);
        foreach(int fileNum in tempDic.Keys)
        {
            fileInfos.Add(tempDic[fileNum]);
        }
        return fileInfos;
    }

    //合并相同的动画
    KeyFrameList mergeAction(KeyFrameList keyFrameList)
    {
        
        KeyFrameList re = new KeyFrameList();
        KeyFrameList stallKeyframeList = keyFrameList;
       
        while (stallKeyframeList.keyFrames.Count != 0)
        {
            KeyFrame curKeyframe = stallKeyframeList.keyFrames[0];
            stallKeyframeList.keyFrames.RemoveAt(0);
            
            if (curKeyframe.action == 2)
            {
                List<KeyFrame> mergedFrames = getMergedActionFrames(curKeyframe, stallKeyframeList);
                
                float actionEndTime = curKeyframe.timestamp+curKeyframe.duration;
                for(int i = 0; i < mergedFrames.Count; i++)
                {
                    stallKeyframeList.keyFrames.Remove(mergedFrames[i]);
                    if (actionEndTime < mergedFrames[i].timestamp + mergedFrames[i].duration)
                    {
                        actionEndTime = mergedFrames[i].timestamp + mergedFrames[i].duration;
                    }
                }
                curKeyframe.duration = actionEndTime - curKeyframe.timestamp;
                
            }
            re.keyFrames.Add(curKeyframe);

        }
        return re;
        
       
    }

    //找出可以merge的动作帧
    List<KeyFrame> getMergedActionFrames(KeyFrame actionFrame,KeyFrameList keyFrameList)
    {
        List<KeyFrame> re = new List<KeyFrame>();
        //在i之后查找符合以下条件的帧：1、timestamp大于i 2、goname一致 3、action内容一致
        //且遇到goname的disappear，或者goname的transform变化，或者goname切换动画，终止。       
        for(int i = 0; i < keyFrameList.keyFrames.Count; i++)
        {
            KeyFrame curKeyframe = keyFrameList.keyFrames[i];

            if(curKeyframe.action==1 && curKeyframe.goname==actionFrame.goname||
                curKeyframe.action==2 && curKeyframe.goname==actionFrame.goname && curKeyframe.content != actionFrame.content||
                 curKeyframe.action == 2 && curKeyframe.goname == actionFrame.goname && !isTransformSame(curKeyframe,actionFrame))
            {
                break;
            }
            else
            {
                if(curKeyframe.timestamp>=actionFrame.timestamp+actionFrame.duration &&
                    curKeyframe.goname==actionFrame.goname &&
                    curKeyframe.action==2 &&
                    curKeyframe.content == actionFrame.content)
                {
                    re.Add(curKeyframe);
                }
            }
        }
        return re;
    }

    //比较两个kf的transform是否一样
    bool isTransformSame(KeyFrame keyFrame1,KeyFrame keyFrame2)
    {

        if(DataUtil.compareList<float>(keyFrame1.startpos,keyFrame2.startpos)&&
            DataUtil.compareList<float>(keyFrame1.endpos, keyFrame2.endpos) &&
            DataUtil.compareList<float>(keyFrame1.startrotation, keyFrame2.startrotation) &&
            DataUtil.compareList<float>(keyFrame1.endrotation, keyFrame2.endrotation) &&
            DataUtil.compareList<float>(keyFrame1.startscale, keyFrame2.startscale) &&
            DataUtil.compareList<float>(keyFrame1.endscale, keyFrame2.endscale))       
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}
