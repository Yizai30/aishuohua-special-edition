
using AnimGenerator;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

using System.Linq;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Profiling;
using TellStory.Util;


public class KeyframeListBuffer
{
    public List<List<KeyFrame>> keyFrameListList { set; get; }
    public List<KeyFrame> mergeFrameList { set; get; }
    public int startIndex { set; get; }
    public int endIndex { set; get; }

    public KeyframeListBuffer(List<List<KeyFrame>> keyFrameListList, int startIndex, int endIndex)
    {
        this.keyFrameListList = keyFrameListList;
        this.startIndex = startIndex;
        this.endIndex = endIndex;
    }
}

public class Controller : MonoBehaviour
{

    Queue<KeyFrame> frameQueue;
    //string currLine = "";

    string textPath = "/result.txt";

    public Interpreter interpreter;
    public Text fullScreenSubtitle;
    public Text subtitle;
    public Image backgroundPanel;
    public Image selectedBackgrounds;
    public Image selectedRoles;
    public Assembler assembler;
    public PannelController pannelController;
    public Camera mainCam, sideCam; public RenderTexture background;
    public SceneSelectMaterials ssm;

    public MicroPhoneSingleView microPhoneSingleView;
    public AndroidUtils autils;
    public AllExtractor allExtractor;
    public ServerPoster serverPoster;
    public AudioManager audioManager;
    Coroutine eachFrameCorotine;

    //public int fileNum { set; get; }
    public string storyName { set; get; }
    public static bool Loaded = false;

    public FramePlanner framePlanner;
    public UsrDicGenerator userdicGenerator;
    public bool readyToGen;
   
    public KeyframeListBuffer keyFrameListBuffer { set; get; }

    public List<KeyframeListBuffer> keyFrameListBufferList { set; get; }
    public Text StoryName;


    // Start is called before the first frame update
    void Awake()
    {
        frameQueue = new Queue<KeyFrame>();
        keyFrameListBufferList = new List<KeyframeListBuffer>();
        //StartCoroutine(serverPoster.FetchJsonData(fetchJsonDataCallback));
        
        
    }


    private void Start()
    {
        

        readyToGen = false;

        sideCam.enabled = false;
        sideCam.targetDisplay = 1;
        mainCam.enabled = true;
        mainCam.targetDisplay = 0;


        RecorderList.initRecorderList();
        //清除result.txt
        clearText(Application.persistentDataPath + textPath);
        //生成storyjson
        clearTempStoryFIle();

        this.storyName = genStoryName();
        microPhoneSingleView.storyName = this.storyName;
        try
        {
           eachFrameCorotine=  StartCoroutine(InEachFrame());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        Screen.SetResolution(1280, 800, true, 60);
        Loaded = true;

        
        DataMap.changeAllPos(backgroundPanel.rectTransform.rect.height, backgroundPanel.rectTransform.rect.width, 0, 0);
    }


  


    //取出frame
    KeyFrameList popFrames()
    {
        KeyFrameList keyFrameList = new KeyFrameList();
        while (frameQueue.Count > 0)
        {
            keyFrameList.keyFrames.Add(frameQueue.Dequeue());

        }
        return keyFrameList;
    }

    //放入frame
    public void pushFrames(List<KeyFrame> keyFrames)
    {
        foreach (KeyFrame frame in keyFrames)
        {
            frameQueue.Enqueue(frame);
        }

    }

    //撤销
    public void redo()
    {
        try
        {

            if (RecorderList.stateNum <= 0) return;
            //RecorderList.stateNum--;
            RecorderList.stateNum = keyFrameListBuffer.startIndex;
            
            for(int i = keyFrameListBuffer.endIndex; i >=keyFrameListBuffer.startIndex; i--)
            {
                RecorderList.deleteRecordAt(i);
            }

            //重新生成text
            //string[] lineList = readFileAllLine();
            List<string> lineList = readFileAllLine().ToList();
            for (int i = keyFrameListBuffer.endIndex; i >= keyFrameListBuffer.startIndex; i--)
            {
                lineList.RemoveAt(i);
            }
            writeFileAllLine(lineList);

            keyFrameListBufferList.RemoveAt(keyFrameListBufferList.Count - 1);

            string curJsonFile = Application.persistentDataPath + "/" + this.storyName + keyFrameListBuffer.startIndex + "-" + keyFrameListBuffer.endIndex+".json";
            Debug.Log("准备删除" + curJsonFile);
            if (File.Exists(curJsonFile))
            {
                File.Delete(curJsonFile);
            }

            if (RecorderList.stateNum > 0)
            {
                RecorderList.stateNum--;
                assembler.assembleLastStateJson(RecorderList.getCurrentRecord());
                //清除屏幕
                interpreter.clearScene();
                interpreter.setJsonFileName(assembler.targetName);//

                RecorderList.stateNum++;
            }
            else
            {
                interpreter.clearScene();
                interpreter.setJsonFileName("");
                pannelController.changeBkg("defaultBkg");
                //this.storyName = genStoryName();
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }


    }
    public static bool isAssemble = false;


    //生成视频
    public void genVideo()
    {
        //StopCoroutine(eachFrameCorotine);
        //readyToGen = true;
        AudioConfiguration aConfig = AudioSettings.GetConfiguration();
        aConfig.dspBufferSize = 1024;
        AudioSettings.Reset(aConfig);

        Controller.isAssemble = true;
        Debug.Log("合并所有json文件...");
        var resultJsonPath = assembler.assembleAllKeyframeBuffer();

        Debug.Log("准备生成视频...");
        this.resetState();
        clearText(Application.persistentDataPath + textPath);
        string targetName = this.storyName + "All";
        interpreter.setJsonFileName(targetName);

        Debug.Log("上传数据至服务器...");
        var resultTxtPath = Application.persistentDataPath + textPath;
        Server.refreshTimeStamp();
        StartCoroutine(Server.sendLogResultTextToServer(resultTxtPath));
        StartCoroutine(Server.sendLogJsonFileToServer(resultJsonPath));

        Debug.Log("开始录屏");
        StartCoroutine(autoStopRecordMovie(assembler.allDuration));
        
    }

    public void EnterFullScreenMode()
    {
        ssm.setVisible(ssm.subtitleCanvas, true);
        ssm.subtitleCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        interpreter.subtitle = fullScreenSubtitle;
        fullScreenSubtitle.text = "";
        mainCam.targetTexture = background;
        sideCam.enabled = true;
        sideCam.targetDisplay = 0;
        mainCam.targetDisplay = 1;
    }

    public void ExitFullScreenMode()
    {
        ssm.setVisible(ssm.subtitleCanvas, false);
        ssm.subtitleCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        interpreter.subtitle = subtitle;
        mainCam.targetTexture = null;
        sideCam.enabled = false;
        sideCam.targetDisplay = 1;
        mainCam.targetDisplay = 0;

        
    }

    IEnumerator autoStopRecordMovie(float time)
    {
        EnterFullScreenMode();
        autils.StartRecording();

        //AndroidUtils.PlayWavFile(Application.streamingAssetsPath + "/music/ld.wav");
        audioManager.Playbgm();
        yield return new WaitForSeconds(time + 1);
        audioManager.StopPlaybgm();
        autils.StopRecording();
        ExitFullScreenMode();

        //resetAll();
        //AndroidUtils.ShowToast("已保存至本地相册。");
        ReRecord.needUpload = true;
        isAssemble = false;
        SceneCreateStory.ShowConfirmUploadPanel = true;
        ExitFullScreenMode();

        //eachFrameCorotine = StartCoroutine(InEachFrame());
    }


    IEnumerator InEachFrame()
    {

        while (true)
        {
            //如果当前有生成规约文件，取出，通知interpreter显示动画
            KeyFrameList keyFrameList = popFrames();
            if (keyFrameList.keyFrames.Count > 0)
            {
                string jsonName = "/" + storyName +keyFrameListBuffer.startIndex+"-"+keyFrameListBuffer.endIndex ;
                string filePath = $"{Application.persistentDataPath}/{jsonName}.json";
                JsonOperator.Obj2Json<KeyFrameList>(keyFrameList, filePath);

                Debug.Log($"正在读取json: {jsonName}");
                yield return new WaitForSeconds(1);
                interpreter.setJsonFileName(jsonName);
                //StartCoroutine(interpreter.interpreteFrames());
                //RecorderList.stateNum++;
            }
            else
            {
                yield return null;
            }

            //读取一行内容
            string[] lineList = readFileAllLine();
            //string line = readFile();
            if (lineList.Length>RecorderList.stateNum)
            {
                               
                float eventDuration = microPhoneSingleView.getAudioLen(RecorderList.stateNum,10f);
                //分句
                //string[] lineList = readFileAllLine();
                List<string> curLineList = new List<string>();
                for(int i = RecorderList.stateNum ; i < lineList.Length; i++)
                {
                    curLineList.Add(lineList[i]);
                }

                int startIndex = RecorderList.stateNum;
                
                foreach(string singleLine in curLineList)
                {
                    RecorderList.genNewRecorder();
                    allExtractor.setStartVal(singleLine, eventDuration, storyName, RecorderList.stateNum);

                    Debug.Log("提取事件...");
                    Debug.Log($"当前的句子是：{singleLine}");
                    allExtractor.extractEvent();

                    // yield return new WaitUntil(() => allExtractor.extractFinished == true);
                    //Debug.Log("提取事件完毕");

                    EventsWrapper eventsWrapper = new EventsWrapper(singleLine, allExtractor.eventList);
                    allExtractor.allEventWrapperList.Add(eventsWrapper);

                    Debug.Log("生成帧中...");

                    allExtractor.genKeyframes();

                    Debug.Log("生成帧完毕");

                    //pushFrames(allExtractor.keyFrameList);

                    RecorderList.stateNum++;
                    //keyFrameListBuffer.Add(allExtractor.keyFrameList);

                }
                
                // 将 allExtractor 的 allEventWrapperList 写入到 json 文件中
                JsonUtils.WriteToJsonFile(allExtractor.allEventWrapperList, AsrMain.testResultPath);
                Debug.Log("basicEventList 已生成。");
                
                int endIndex = RecorderList.stateNum - 1;
                keyFrameListBuffer = new KeyframeListBuffer(RecorderList.getAllFrameList(startIndex, endIndex), startIndex, endIndex);
                //基于下文的规划调整
                
                //framePlanner.keyframeListBuffer = keyFrameListBuffer;
                //framePlanner.FixCurByNextFrame();               
                //合成当前这几句话的动画规约
                List<KeyFrame> showkeyFrames = assembler.assemble(keyFrameListBuffer.keyFrameListList);
                keyFrameListBuffer.mergeFrameList = showkeyFrames;

                //用于总动画生成
                keyFrameListBufferList.Add(keyFrameListBuffer);
                
                pushFrames(showkeyFrames);
                //readyToGen = true;

            }
            else
            {
                yield return null;
            }
        }

    }


    public void writeFileAllLine(List<string>lineList)
    {
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + textPath,false, Encoding.UTF8);
       
       
        try
        {
            foreach(string line in lineList)
            {
                sw.WriteLine(line);
            }
           


        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            sw.Close();
        }

    }

    public string[] readFileAllLine()
    {
        StreamReader sr = new StreamReader(Application.persistentDataPath + textPath, Encoding.UTF8);
        string[] re = new string[] { };
        try
        {

            string[] lines = File.ReadAllLines(Application.persistentDataPath + textPath).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToArray();
            if (lines.Count() != 0) re = lines;
         

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            sr.Close();
        }

        return re;

    }


    


    void clearText(string filePath)
    {
        //var filePath = Application.dataPath + "Scripts/zoo/result.txt";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            File.Create(filePath).Dispose();
        }
        else
        {
            File.Create(filePath).Dispose();
        }
    }

    void clearTempStoryFIle()
    {
        string streamDir = Application.persistentDataPath;
        DirectoryInfo directoryInfo = new DirectoryInfo(streamDir);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*.json");
        foreach (FileInfo fileInfo in fileInfos)
        {
            File.Delete(fileInfo.FullName);
        }

    }

    void clearTempAuFIle()
    {
        string streamDir = Application.persistentDataPath + "/audio/";
        DirectoryInfo directoryInfo = new DirectoryInfo(streamDir);
        FileInfo[] fileInfos = directoryInfo.GetFiles("*.wav");
        foreach (FileInfo fileInfo in fileInfos)
        {
            File.Delete(fileInfo.FullName);
        }

    }

    private string genStoryName()
    {
        string re = "";
        DateTime dateTime = DateTime.Now;
        string strNowTime = string.Format("{0:D}{1:D}{2:D}{3:D}{4:D}{5:D}", dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        re = strNowTime + "_story";
        return re;
    }

    public void resetState()
    {

        //Recorder.initData();
        RecorderList.clearAll();
        keyFrameListBufferList.Clear();
        interpreter.setJsonFileName("");
        interpreter.clearScene();
    }

    public void resetAll()
    {


        resetState();
        //删除json文件
        clearTempStoryFIle();
        //删除录音文件   
        //clearTempAuFIle();
        //重置result.txt
        clearText(Application.persistentDataPath + textPath);
        pannelController.changeBkg("defaultBkg");
        

    }

    


}













