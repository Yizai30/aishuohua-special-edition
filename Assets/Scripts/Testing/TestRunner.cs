using AnimGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestRunner : MonoBehaviour
{
    public static bool TestingMode = false;
    public static float CurrentAudioLength = 0;
    public static string CurrentAudioText = "";
    public static string CurrentCuttedText = "";
#if UNITY_EDITOR
    RecorderController m_RecorderController;
    public bool m_RecordAudio = false;
    internal MovieRecorderSettings m_Settings = null;
    public Button myStory;

    void StartRecord(string path)
    {
        if (File.Exists(path + ".mp4")) {
            File.Delete(path + ".mp4");
        }
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        m_RecorderController = new RecorderController(controllerSettings);
        m_Settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        m_Settings.name = "TestRecorder";
        m_Settings.Enabled = true;
        m_Settings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        m_Settings.VideoBitRateMode = VideoBitrateMode.High;
        m_Settings.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = 1280,
            OutputHeight = 800
        };
        m_Settings.VideoBitRateMode = VideoBitrateMode.Low;
        m_Settings.AudioInputSettings.PreserveAudio = m_RecordAudio;
        m_Settings.OutputFile = path;
        controllerSettings.AddRecorderSettings(m_Settings);
        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 30.0f;

        RecorderOptions.VerboseMode = false;
        m_RecorderController.PrepareRecording();
        m_RecorderController.StartRecording();
    }

    void StopRecord()
    {
        m_RecorderController.StopRecording();
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        TestingMode = true;
        SceneMainMenu.setTestingFunction(()=> {
            if (testStarted)
            {
                return;
            }
            testStarted = true;
            ClickButtonByName("ButtonTellStory");
        });
        SceneSelectMaterials.setTestingFunction(() =>
        {
            while (!AsrMain.isJiebaLoaded) Thread.Sleep(100);
            EstimateTotalTimeConsumption();
            StartCoroutine(TestMain());

        });
        SceneManager.LoadScene("SceneMainMenu");
    }



    void ClickButtonByName(string buttonGameObjectName)
    {
        Debug.Log("Press button " + buttonGameObjectName + " in scene " + SceneManager.GetActiveScene().name);
        try
        {
            var btn = GameObject.Find(buttonGameObjectName).GetComponent<Button>();
            btn.onClick.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to press button " + buttonGameObjectName + " in scene " + SceneManager.GetActiveScene().name);
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
        }
    }

    public IEnumerator TestMain()
    {
        var dir = new DirectoryInfo(Path.Combine(Application.dataPath, "..", "TestCases"));
        var testCases = dir.GetFiles();
        foreach (var testCase in testCases)
        {
            yield return TestFile(testCase.FullName);
        }
        testFinished = true;
        Debug.Log($"TestRunner: All tests has been finished.");
    }

    public IEnumerator TestFile(string path)
    {
        CurrentCuttedText = "";
        ClickButtonByName("ButtonStartCreation");
        var lines = File.ReadAllText(path).Split('\n');
        Debug.Log($"TestRunner: start playing test case: {path}");
        var savedPath = new FileInfo(Path.Combine(Application.dataPath, "..", "TestResults", Path.GetFileNameWithoutExtension(path))).FullName;
        Debug.Log($"TestRunner: save record video to: {savedPath}");
        //GameObject.Find("Controller").GetComponent<Controller>().EnterFullScreenMode();
        StartRecord(savedPath);
        foreach (var line in lines)
        {
            if (line.Length > 0)
            {
                float duration = EstimateSentenceTimeConsumption(line);
                yield return TestSentence(duration, line);
            }
        }
        Debug.Log($"TestRunner: finish test case: {path}");
        StopRecord();
        File.WriteAllText(new FileInfo(Path.Combine(Application.dataPath, "..", "TestResults", Path.GetFileNameWithoutExtension(path))).FullName + "_cutted.txt", CurrentCuttedText);
        //GameObject.Find("Controller").GetComponent<Controller>().ExitFullScreenMode();
        ClickButtonByName("BackSelectBtn");
    }
    public IEnumerator TestSentence(float time, string text)
    {
        Debug.Log($"TestRunner: play text: {text} in {time} seconds.");
        CurrentAudioLength = time;
        CurrentAudioText = text.Replace("\n", "").Replace("\r", "");
        MicroPhoneSingleView.audioIsSaved = true;
        yield return new WaitForSeconds(time + 2);
    }

    void EstimateTotalTimeConsumption()
    {
        float total = 0;
        var dir = new DirectoryInfo(Path.Combine(Application.dataPath, "..", "TestCases"));
        var testCases = dir.GetFiles();
        foreach (var testCase in testCases)
        {
            total += EstimateFileTimeConsumption(testCase.FullName);
        }
        Debug.Log($"TestRunner: Estimate Total Time Consumption: {total:0.00} seconds.");
    }

    float EstimateFileTimeConsumption(string path)
    {
        float total = 0;
        var lines = File.ReadAllText(path).Split('\n');
        foreach (var line in lines)
        {
            if (line.Length > 0)
            {
                total += EstimateSentenceTimeConsumption(line);
            }
        }
        return total;
    }

    float EstimateSentenceTimeConsumption(string line)
    {
        return Math.Min(Math.Max(line.Length * 0.6f + 2, 5), 10);
    }

    bool testStarted = false;
    bool testFinished = false;

    void Update()
    {
        if (testStarted && testFinished)
        {
            testStarted = false; testFinished = false;
            Debug.DebugBreak();
        }
    }
#endif
}
