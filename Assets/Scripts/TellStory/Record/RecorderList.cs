using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimGenerator;

public class RecorderList
{
    public static List<Recorder> recorderList { set; get; }

    public static int stateNum { set; get; }

    public static void initRecorderList()
    {
        recorderList = new List<Recorder>();
        
        stateNum = 0;
    }

    public static List<List<KeyFrame>> getAllFrameList(int startStateNum,int endStateNum)
    {
        List<List<KeyFrame>> re = new List<List<KeyFrame>>();
        for(int i = startStateNum; i <= endStateNum; i++)
        {
            Recorder curRecord = getRecordByNum(i);
            re.Add(curRecord.getKeyframeList());
        }
        return re;
    }
    public static Recorder getRecordByNum(int num)
    {
        foreach(Recorder recorder in recorderList)
        {
            if (recorder.fileNum.Equals(num))
            {
                return recorder;
            }
        }
        throw new System.Exception("没有找到对应的recorder" + num);
    }

    public static Recorder getCurrentRecord()
    {
        foreach (Recorder recorder in recorderList)
        {
            if (recorder.fileNum.Equals(stateNum))
            {
                return recorder;
            }
        }
        throw new System.Exception("没有找到对应的recorder" + stateNum);
    }

    public static void clearCurRecord()
    {
        foreach (Recorder recorder in recorderList)
        {
            if (recorder.fileNum.Equals(stateNum))
            {
                recorder.clearRecord();
            }
        }
        throw new System.Exception("没有找到对应的recorder" + stateNum);
    }

    public static void deleteRecordAt(int index)
    {
        if(index>=0 && index < recorderList.Count)
        {
            recorderList.RemoveAt(index);
        }
    }
    public static void deleteCurRecord()
    {
        int removeIndex = -1;
        for(int i = 0; i < recorderList.Count; i++)
        {
            if (recorderList[i].fileNum.Equals(stateNum))
            {
                removeIndex = i;
            }
        }
        if (removeIndex != -1)
        {
            recorderList.RemoveAt(removeIndex);
        }
        else
        {
            throw new System.Exception("没有找到对应的recorder" + stateNum);
        }
        
    }

    public static void clearAll()
    {
        stateNum = 0;
        if (recorderList != null)
        {
            recorderList.Clear();
        }
        
    }

    public static void genNewRecorder()
    {
        Recorder recorder = new Recorder();
        if (stateNum != 0)
        {
            recorder = DataUtil.Clone<Recorder>(recorderList[stateNum - 1]);
        }
        recorder.fileNum = stateNum;
        recorderList.Add(recorder);
        
    }
}
