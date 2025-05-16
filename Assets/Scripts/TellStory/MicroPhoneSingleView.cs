using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MicroPhoneSingleView : MonoBehaviour
{
    #region ［public Way］
    public MicroPhoneManager microPhoneManager;
    public static bool audioIsSaved = false;
    #endregion

    public AsrMain asrMain;
    public string storyName;
    public static Dictionary<int, string> audioSavePath = new Dictionary<int, string>();
    private void Awake()
    {
        //startBtn.onClick.AddListener(StartRecord);
        //endBtn.onClick.AddListener(StopRecord);
    }

    private void Start()
    {
#if UNITY_EDITOR
        microPhoneManager.audioSavePath = Application.persistentDataPath + "/audio/";
        if (!Directory.Exists(microPhoneManager.audioSavePath))
        {
           Directory.CreateDirectory(microPhoneManager.audioSavePath);
        }
        clearTempAuFIle();
#endif
    }

    private void Update()
    {
        if (audioIsSaved)
        {
            try
            {
                Debug.Log("Load audio from dict2: " + RecorderList.stateNum.ToString());
                string line;
                if (TestRunner.TestingMode)
                {
                    line = TestRunner.CurrentAudioText;
                }
                else
                {
#if UNITY_EDITOR
                    // line = asrMain.getVoiceText(microPhoneManager.audioSavePath + storyName + RecorderList.stateNum + ".wav");
                    line = asrMain.readTestRecogText();
                    Debug.Log("已读取到了语音识别结果文本（测试用）");
#else
                    line = asrMain.getVoiceText(audioSavePath[RecorderList.stateNum]);
#endif
                }
                //以句号为单位划分句子
                string[] lineList = asrMain.splitText(line);

                foreach(string l in lineList)
                {
                    //分词
                    string cut = asrMain.cutLine(l);
                    if (TestRunner.TestingMode)
                    {
                        TestRunner.CurrentCuttedText += cut + "\n";
                    }
                    //写入文件
                    asrMain.writeLine(cut, asrMain.resultPath);
                }
               
                
               
                audioIsSaved = false;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.Log(e.Message);
            }
        }
    }

    //开始录音，写入文件
    public void StartRecord()
    {
#if UNITY_EDITOR
        microPhoneManager.setOutPutDir(Application.streamingAssetsPath + "/audio/");
        microPhoneManager.setOutPutFilename(this.storyName + RecorderList.stateNum + ".wav");
        microPhoneManager.StartRecordAudio();
#else
        AndroidUtils.startRecognize();
        audioIsSaved = false;
#endif
    }

    public void StopRecord()
    {
#if UNITY_EDITOR
        microPhoneManager.StopRecordAudio();
        audioIsSaved = true;
#else
        string returnedValue = AndroidUtils.stopRecognize();
        Debug.Log("returnedValue: " + returnedValue);
        var values = returnedValue.Split('|');
        if (values[0].Length > 0 && values[1].Length > 0)
        {
            AndroidUtils.recordResult = values[0];
            Debug.Log("Save audio to dict: " + RecorderList.stateNum.ToString() + " -> " + values[1]);
            if (!values[1].Contains(";"))
            {
                audioSavePath[RecorderList.stateNum] = values[1];
            } else
            {
                var paths = values[1].Split(';');
                for (int i = 0; i < paths.Length; i++)
                {
                    audioSavePath[RecorderList.stateNum + i] = paths[i];
                }
            }
            audioIsSaved = true;
        }
        else
        {
            AndroidUtils.ShowToast("网络错误，请重试");
            SceneCreateStory.CurrentState = SceneCreateStory.State.IDLE;
        }
        if (values.Length == 3)
        {
            var af = values[2].Split(';');
            foreach (var a in af)
            {
                Server.audioFiles.Add(a);
            }
        }
#endif
    }

    void clearTempAuFIle()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(microPhoneManager.audioSavePath);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*.wav");
        foreach (FileInfo fileInfo in fileInfos)
        {
            File.Delete(fileInfo.FullName);
        }

    }

    //获取某一个音频文件的时长
    public float getAudioLen(int fileNum, float defaultLen)
    {
        if (TestRunner.TestingMode)
        {
            return TestRunner.CurrentAudioLength;
        }
#if UNITY_EDITOR
        string tempPath = microPhoneManager.audioSavePath + storyName + fileNum + ".wav";
#else
        string tempPath = audioSavePath[fileNum];
#endif
        if (!File.Exists(tempPath))
        {
            Debug.LogWarning("录音文件不存在，使用默认时长：" + tempPath);
            return defaultLen;
        }
        WaveFileReader wf = new WaveFileReader(tempPath);
        float len = (float)(wf.TotalTime.TotalMilliseconds * 0.001);
        wf.Close();
        return len;
    }
}
