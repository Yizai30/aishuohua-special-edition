using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JiebaNet.Segmenter;
using JiebaNet.Segmenter.PosSeg;
using System.Linq;
using System.Text;
using System;
using Assets.Scripts.TellStory.Util;
using TellStory.Util;

public class AsrMain : MonoBehaviour
{
    string APP_ID = "29697206";
    string API_KEY = "U2zA8A72fSTBUsLlAle48yhg";
    string SECRET_KEY = "GKkWhd90w5uV3Ly1O4hbdNuI1muTVmST";
    string auPath = "";
    public static string testRecogPath = "";
    public static string testResultPath = "";
    public static string usrDicPath = "";
    private static PosSegmenter posSeg;
    private static JiebaSegmenter jiebaSegmenter;
    public static bool isJiebaLoaded = false;
    public static bool hasJiebaLoaded = false;
    public string resultPath { set; get; }

    //public Controller controller;
    //public MicroPhoneManager microPhoneManager;
    private void Awake()
    {
        auPath = Application.persistentDataPath + "/audio/";
        testRecogPath = Application.persistentDataPath + "/audio_text.txt";
        testResultPath = Application.persistentDataPath + "/basic_event.json";
        resultPath = Application.persistentDataPath + "/result.txt";
        
        JsonUtils.ClearFileContent(testResultPath);
    }

    private void Start()
    {
        //测试
        //Debug.Log(auPath);
        //string line = "小兔子最喜欢的东西是蘑菇";
        //Debug.Log(Time.realtimeSinceStartupAsDouble);
        //cutLine(line);
        //Debug.Log(Time.realtimeSinceStartupAsDouble);

    }

    public static string[] resourceFiles =
    {
        "jiebaConfig/char_state_tab.json",
        "jiebaConfig/cn_synonym.txt",
        //"jiebaConfig/dict.txt",
        //"jiebaConfig/idf.txt",
        "jiebaConfig/pos_prob_emit.json",
        "jiebaConfig/pos_prob_start.json",
        "jiebaConfig/pos_prob_trans.json",
        "jiebaConfig/prob_emit.json",
        "jiebaConfig/prob_trans.json",
        "jiebaConfig/stopwords.txt",
        "jiebaConfig/usr_dic.txt"
    };

    public static Dictionary<string, WWW> requests = new Dictionary<string, WWW>();
    public static Dictionary<string, byte[]> requestsResults = new Dictionary<string, byte[]>();

    public static bool allRequestsDone()
    {
        bool isDone = true;
        foreach (string r in resourceFiles)
        {
            isDone &= requests[r].isDone;
        }
        if (isDone)
        {
            foreach (string r in resourceFiles)
            {
                requestsResults[r] = requests[r].bytes;
            }
        }
        return isDone;
    }


    public static void regenProcessor()
    {
        jiebaSegmenter = new JiebaSegmenter();
        Debug.Log(usrDicPath);
        jiebaSegmenter.LoadUserDict(usrDicPath);
        posSeg = new PosSegmenter(jiebaSegmenter);
    }

    public static void InitJieba()
    {

        foreach (string r in resourceFiles)
        {
            ResourceUtils.CopyToPersistentDataPath(r, requestsResults[r]);
        }
        ConfigManager.ConfigFileBaseDir = ResourceUtils.persistentDataPath + "/jiebaConfig";
        jiebaSegmenter = new JiebaSegmenter();
        Debug.Log(usrDicPath);
        jiebaSegmenter.LoadUserDict(usrDicPath);
        posSeg = new PosSegmenter(jiebaSegmenter);
        isJiebaLoaded = true;
        Debug.Log("Jieba loaded.");
    }

    public string getVoiceText(string auFileName)
    {
#if UNITY_EDITOR
        string re = "";
        var client = new Baidu.Aip.Speech.Asr(APP_ID, API_KEY, SECRET_KEY);
        var data = File.ReadAllBytes(auFileName);
        var options = new Dictionary<string, object>
        {
            {"dev_pid",1537 }
        };
        client.Timeout = 120000;
        var resultJson = client.Recognize(data, "wav", 16000, options);
        foreach(var el in resultJson)
        {
            if (el.Key.Equals("result"))
            {
                re = el.Value.First.ToString();
            }
        }
        AndroidUtils.recordResult = re;
#endif
        Debug.Log("已获取语音结果:" + AndroidUtils.recordResult) ;
        return AndroidUtils.recordResult;
    }

    // 从手写的语音识别结果文件中读取文本
    public string readTestRecogText()
    {
        string re = "";
        
        // 检查文件是否存在，如果不存在则创建
        if (!File.Exists(testRecogPath))
        {
            try
            {
                // 创建文件
                using (File.Create(testRecogPath)) { }
                Console.WriteLine("File created at: " + testRecogPath);
            }
            catch (Exception ex)
            {
                // 处理创建文件时可能发生的异常
                Console.WriteLine("An error occurred while creating the testRecog file: " + ex.Message);
                return re; // 如果创建失败，返回空字符串
            }
        }
        
        try
        {
            // 读取文件内容
            re = File.ReadAllText(testRecogPath);
        }
        catch (Exception ex)
        {
            // 可以在这里处理异常，例如记录日志等
            Console.WriteLine("An error occurred while reading the testRecog file: " + ex.Message);
        }
        return re;
    }

    public string cutLine(string line)
    {
        string re = "";
        //Debug.Log(Time.realtimeSinceStartupAsDouble);
        if (string.IsNullOrEmpty(line))
        {
            Debug.Log("获取分词结果失败，语音识别结果为空");
            return "";
        }
        var tokens = posSeg.Cut(line);
        re = string.Join("",tokens.Select(token => string.Format("{0}{1}", token.Word, token.Flag)));
        //Debug.Log(Time.realtimeSinceStartupAsDouble);
        Debug.Log("已获取分词结果"+re);
        return re;

    }

    public void writeLine(string line,string filePath)
    {
        try
        {
            StreamWriter sw = new StreamWriter(filePath,true,Encoding.UTF8);

            sw.WriteLine(line);
            sw.Close();
            Debug.Log("已将结果写入到 " + filePath);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    public string[] splitText(string rawLine)
    {

#if UNITY_EDITOR
        string[] re = rawLine.Split('，');
#else
        string[] re = rawLine.Split(new char[] {'。'}, StringSplitOptions.RemoveEmptyEntries);
#endif

        if (re.Length == 0) re = new string[] { rawLine };
        return re;
    }
}
