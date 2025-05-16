using Assets.Scripts.TellStory.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class ReRecord : MonoBehaviour
{
    public VideoPlayer vPlayer;
    public VideoProgressBar fullScreenProgressBar;
    public AudioSource audioSource;
    //string videoSavedPath;
    public static bool lastSceneFromStoryList = false;
    public Button backButton;
    public Button deleteButton;
    public Button downloadButton;
    public Button shareButton;
    public Button deleteConfrim, deleteCancel;
    public RawImage videoOutput;
    public RawImage fullscreen;
    public bool isSwitchable = true;
    public static bool uploaded = false;
    public static bool needUpload = false;
    public Image overlaySubtitleBackground;
    public Text overlaySubtitle;
    public bool showingOverlay;
    public SortedList<int, string> subtitles = new SortedList<int, string>();

    public Button fullscreenPlay, fullscreenPause, fullscreenStop;
    public Button normalPlay, normalPause, normalStop;
    public Button ButtonCloseWait;
    public Button OneMoreButton;
    public bool isRecordingVoice = false;
    //public CanvasGroup DeleteConfrimDialog;
    //public CanvasGroup WaitDialog;


    public Image ProgressBarImage;
    public Text ProgressBarText;

    public GameObject canvasFullScreen, canvasConfirmDialog, canvasWaitDialog;

    public void setVisible(GameObject go, bool visible)
    {
        if (go.GetComponent<CanvasGroup>() != null)
        {
            go.GetComponent<CanvasGroup>().alpha = visible ? 1 : 0;
            go.GetComponent<CanvasGroup>().interactable = visible;
            go.GetComponent<CanvasGroup>().blocksRaycasts = visible;
        }
        go.GetComponent<GraphicRaycaster>().enabled = visible;
    }



    // Start is called before the first frame update
    void Start()
    {
        sideCam.enabled = false;
        sideCam.targetDisplay = 1;
        mainCam.enabled = true;
        mainCam.targetDisplay = 0;
        setVisible(canvasFullScreen, false);
        backButton.onClick.AddListener(BackToPreviousScene);
        deleteButton.onClick.AddListener(() => {
            Debug.Log("onClick: deleteButton");
            setVisible(canvasConfirmDialog, true);
        });
        deleteCancel.onClick.AddListener(() =>
        {
            Debug.Log("onClick: deleteCancel");
            setVisible(canvasConfirmDialog, false);
        });
        deleteConfrim.onClick.AddListener(() =>
        {
            Debug.Log("onClick:  deleteConfrim");
            setVisible(canvasConfirmDialog, false);
            StartCoroutine(DeleteAudio());
        });
        setVisible(canvasConfirmDialog, false);

        downloadButton.onClick.AddListener(() => {
            Debug.Log("DownLoad");
            StartCoroutine(DownLoadVideo());
        });
        shareButton.onClick.AddListener(() => {
            Debug.Log("Share");
            StartCoroutine(ShareVideo());
        });
        var button1 = videoOutput.gameObject.AddComponent<Button>();
        button1.onClick.AddListener(DoubleClickSwithVideoPlayerMode);
        var button2 = fullscreen.gameObject.AddComponent<Button>();
        button2.onClick.AddListener(DoubleClickSwithVideoPlayerMode);
        //mpmanager = gameObject.AddComponent<MicroPhoneManager>();
        if(needUpload)
        {
            setVisible(canvasWaitDialog, true);
            needUpload = false;
            StartCoroutine(Server.UpdateProgress());
            StartCoroutine(Server.FixRecordedVideoAndSaveToFile());
        } else
        {
            setVisible(canvasWaitDialog, false);
        }
        if (!lastSceneFromStoryList)
        {
            OneMoreButton.onClick.AddListener(() =>
            {
                Debug.Log("onClick: OneMoreButton");
                SceneManager.LoadScene("SceneTellStory");
            });
            OneMoreButton.gameObject.SetActive(true);
        } else
        {
            OneMoreButton.gameObject.SetActive(false);
        }

        overlaySubtitleBackground.enabled = false;
        overlaySubtitle.enabled = false;

        ButtonCloseWait.onClick.AddListener(() =>
        {
            AndroidUtils.ShowToast("您已取消上传");
            SceneManager.LoadScene("SceneMainMenu");
        });
    }

    public void BackToPreviousScene()
    {
        Debug.Log("onClick: BackToPreviousScene");
        if (isRecordingVoice)
            {
                AndroidUtils.stopRecordAudio();
                isRecordingVoice = false;
            }
            if (lastSceneFromStoryList)
            {
                lastSceneFromStoryList = false;
                SceneManager.LoadScene("SceneMyStories");
            }
            else
            {
                lastSceneFromStoryList = false;
                SceneManager.LoadScene("SceneMainMenu");
            }
    }


    // Update is called once per frame
    public DateTime playStartTime;
    void Update()
    {
        if (uploaded)
        {
            uploaded = false;
            vPlayer.Stop();
            vPlayer.source = VideoSource.Url;
            vPlayer.url = AndroidUtils.RecordFileName;
            Debug.Log("vPlayer.url:" + vPlayer.url);
            vPlayer.Prepare();
            vPlayer.Play();
            videoProgressBar.draggable = true;
            setVisible(canvasWaitDialog, false);
        }
        if (overlaySubtitle.enabled)
        {
            var pretext = "";
            foreach (var k in subtitles)
            {
                if (k.Key >= (DateTime.Now - playStartTime).TotalMilliseconds)
                {
                    pretext = k.Value;
                    break;
                }
            }
            overlaySubtitle.text = pretext;
        }
        if (!Server.StopUploadProgress)
        {
            ProgressBarImage.fillAmount = (float)Server.UploadProgress;
            ProgressBarText.text = Mathf.RoundToInt((float)(Server.UploadProgress * 100f)) + "%"; ;
        }
        
    }

    public Camera mainCam, sideCam; public RenderTexture background;
    float lastTime = 0;bool fullScreenMode = false;

    void DoubleClickSwithVideoPlayerMode()
    {
        Debug.Log("onClick: DoubleClickSwithVideoPlayerMode");
        if (Time.realtimeSinceStartup - lastTime < 0.5f)
        {
            SwitchVideoPlayerMode();
        }
        lastTime = Time.realtimeSinceStartup;
    }

    void SwitchVideoPlayerMode()
    {
        if (isSwitchable)
        {
            if (!fullScreenMode)
            {
                setVisible(canvasFullScreen, true);
                mainCam.targetTexture = background;
                sideCam.enabled = true;
                sideCam.targetDisplay = 0;
                mainCam.targetDisplay = 1;
                fullScreenMode = true;
            }
            else
            {
                setVisible(canvasFullScreen, false);
                mainCam.targetTexture = null;
                sideCam.enabled = false;
                sideCam.targetDisplay = 1;
                mainCam.targetDisplay = 0;
                fullScreenMode = false;
            }
        }
    }


    bool isRerecording = false;
    //重新配音
    public void RedoRecord()
    {
        //AndroidUtils.ShowToast("功能调试中，敬请期待");
        //return;
        var recordBtn = GameObject.Find("ReRecordBtn");
        if (!isRecordingVoice)
        {
            Debug.Log("开始录音");
            backButton.GetComponent<Button>().enabled = false;
            deleteButton.GetComponent<Button>().enabled = false;
            downloadButton.GetComponent<Button>().enabled = false;
            shareButton.GetComponent<Button>().enabled = false;
            backButton.GetComponent<Button>().interactable = false;
            deleteButton.GetComponent<Button>().interactable = false;
            downloadButton.GetComponent<Button>().interactable = false;
            shareButton.GetComponent<Button>().interactable = false;
            StartRecord();
        }
        else
        {
            AndroidUtils.stopRecordAudio();
            isRecordingVoice = false;
            RecoverToReRecord();
            vPlayer.loopPointReached -= StopRecord;
            vPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            vPlayer.Stop();
            vPlayer.Play();
        }
    }



    public void RecoverToReRecord()
    {
        videoButton1.enabled = true;
        videoButton2.enabled = true;
        videoButton3.enabled = true;
        videoButton1.interactable = true;
        videoButton2.interactable = true;
        videoButton3.interactable = true;
        videoProgressBar.draggable = true;
        //当前页面其余按钮恢复响应
        backButton.GetComponent<Button>().enabled = true;
        deleteButton.GetComponent<Button>().enabled = true;
        downloadButton.GetComponent<Button>().enabled = true;
        shareButton.GetComponent<Button>().enabled = true;
        backButton.GetComponent<Button>().interactable = true;
        deleteButton.GetComponent<Button>().interactable = true;
        downloadButton.GetComponent<Button>().interactable = true;
        shareButton.GetComponent<Button>().enabled = true;
    }





    public Button videoButton1;
    public Button videoButton2;
    public Button videoButton3;
    public VideoProgressBar videoProgressBar;
    public void StartRecord()
    {
        AndroidUtils.startRecordAudio();
        isRecordingVoice = true;
        vPlayer.isLooping = false;
        vPlayer.audioOutputMode = VideoAudioOutputMode.None;
        vPlayer.Stop();
        vPlayer.Play();
        vPlayer.loopPointReached += StopRecord;
        videoButton1.enabled = false;
        videoButton2.enabled = false;
        videoButton3.enabled = false;
        videoButton1.interactable = false;
        videoButton2.interactable = false;
        videoButton3.interactable = false;
        videoProgressBar.draggable = false;
    }

    public void StopRecord(VideoPlayer video)
    {
        RecoverToReRecord();
        video.Stop();
        StartCoroutine(StopRecordAndUpload());
    }

    public IEnumerator StopRecordAndUpload()
    {

        AndroidUtils.ShowToast("录音完成，处理中...");
        AndroidUtils.stopRecordAudio();
        isRecordingVoice = false;
        var results = AndroidUtils.recordResult.Split(new[] { "\",\"" }, StringSplitOptions.None);
        foreach (string result in results)
        {
            var timeAndText = result.Split(new[] { "\": \"" }, StringSplitOptions.None);
            if (timeAndText.Length == 2) {
                var time = timeAndText[0].Replace("\"", "").Replace("{", "");
                var text = timeAndText[1].Replace("\"", "").Replace("}", "");
                Debug.Log("Use time:" + time + ",text:" + text);
                subtitles.Add(int.Parse(time), text);
            } else
            {
                Debug.Log("Bad length for: " + result);
            }
        }
        audioSource.Stop();
        var audioFilePath = Application.persistentDataPath + "/audio/" + AndroidUtils.audioFileRecordingTime + ".wav";
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + audioFilePath, AudioType.WAV))
        {
            www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
            Debug.Log("Reading from: " + audioFilePath + ", existence: " + (File.Exists(audioFilePath) ? "yes" : "no"));
            if (!File.Exists(audioFilePath))
            {
                DirectoryInfo dir = Directory.GetParent(audioFilePath);
                Debug.Log("In " + dir.FullName);
                FileInfo[] files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    Debug.Log("Exist file: " + files[i].FullName);
                }
                dir = dir.Parent;
                Debug.Log("In " + dir.FullName);
                files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < files.Length; i++)
                {
                    Debug.Log("Exist file: " + files[i].FullName);
                }
            }
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
            }
        }

        Server.videoUrl = AndroidUtils.RecordFileName;
        Server.audioUrl = audioFilePath;
        new Thread(Server.CombineAudioVideo).Start();
        /*yield return Server.CombineAudioVideo(AndroidUtils.RecordFileName,
                    audioFilePath,
                    (videoUrl) =>
                    {
                        //vPlayer.url = videoUrl;
                        //vPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
                        //vPlayer.Stop();
                        //vPlayer.Play();
                    });*/
        EnterPreviewMode();
    }

    public void EnterPreviewMode()
    {
        vPlayer.Stop();
        vPlayer.isLooping = false;
        vPlayer.loopPointReached -= StopRecord;
        vPlayer.loopPointReached += ExitPreviewMode;
        vPlayer.audioOutputMode = VideoAudioOutputMode.None;
        fullScreenProgressBar.draggable = false;
        if (!fullScreenMode)
        {
            SwitchVideoPlayerMode();
        }
        isSwitchable = false;
        audioSource.Stop();
        audioSource.loop = false;
        vPlayer.Prepare();
        vPlayer.prepareCompleted += (VideoPlayer v) =>
        {
            vPlayer.Play();
            audioSource.Play();
            overlaySubtitle.enabled = true;
            overlaySubtitleBackground.enabled = true;
            playStartTime = DateTime.Now;
            fullscreenPlay.interactable = false;
            fullscreenPlay.enabled = false;
            fullscreenPause.interactable = false;
            fullscreenPause.enabled = false;
            fullscreenStop.interactable = false;
            fullscreenStop.enabled = false;
        };
    }


    public void ExitPreviewMode(VideoPlayer v)
    {
        Debug.Log("ExitPreviewMode"); 
        v.loopPointReached -= ExitPreviewMode;
        vPlayer.loopPointReached += StopRecord;
        isSwitchable = true;
        SwitchVideoPlayerMode();
        vPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        fullScreenProgressBar.draggable = true;
        vPlayer.Stop();
        overlaySubtitle.enabled = false;
        overlaySubtitleBackground.enabled = false;
        subtitles.Clear();
        fullscreenPlay.interactable = true;
        fullscreenPlay.enabled = true;
        fullscreenPause.interactable = true;
        fullscreenPause.enabled = true;
        fullscreenStop.interactable = true;
        fullscreenStop.enabled = true;
        AndroidUtils.ShowToast("重新配音结果已上传，稍后将会出现在故事列表中");
        SceneManager.LoadScene("SceneMyStories");
    }

    //清除当前配音




    public IEnumerator DeleteAudio()
    {
        if (lastSceneFromStoryList)
        {
            yield return Server.DeleteStories(() => {
                lastSceneFromStoryList = false;
                SceneManager.LoadScene("SceneMyStories");
            });
        }
        else
        {
            yield return Server.DeleteStories(() => {
                lastSceneFromStoryList = false;
                SceneManager.LoadScene("SceneTellStory");
            });
        }
    }


    //下载当前配音视频
    public IEnumerator DownLoadVideo()
    {
        byte[] mediaBytes;
        UnityWebRequest www = UnityWebRequest.Get(AndroidUtils.RecordFileName);
        yield return www.SendWebRequest();
        mediaBytes = www.downloadHandler.data;
        NativeGallery.SaveVideoToGallery(mediaBytes, "TellStory", "New Story.mp4", (bool success, string path) => {
            if (success)
            {
                Debug.Log("NativeGallery.SaveImageToGallery: " + success);
                AndroidUtils.ShowToast("已成功保存到相册");
                //if (!lastSceneFromStoryList) SceneManager.LoadScene("SceneMainMenu");
            } else
            {
                AndroidUtils.ShowToast("保存失败");
            }
        });
    }

    public IEnumerator ShareVideo()
    {
        string objId = AndroidUtils.RecordFileName;
        objId = objId.Substring(objId.IndexOf("uploadedFiles/"), objId.IndexOf(".mp4") - objId.IndexOf("uploadedFiles/")).Replace("uploadedFiles/", "");
        UnityWebRequest www = UnityWebRequest.Get(Server.HOST+"api/v2/share/"+ objId);
        www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
        yield return www.SendWebRequest();
        Debug.Log(www.downloadHandler.text);
        if (www.downloadHandler.text.Equals("\"success\"")) {
            AndroidUtils.ShowToast("短信发送成功");
        }
        else
        {
            AndroidUtils.ShowToast("分享失败，未知视频作者");
        }
        yield return null;
    }
}
