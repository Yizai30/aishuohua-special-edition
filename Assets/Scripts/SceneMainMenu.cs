using Assets.Scripts.TellStory.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Interfaces;

public class SceneMainMenu : MonoBehaviour
{
    public static Action testingFunction;
    public static void setTestingFunction(Action action)
    {
        testingFunction = action;
    }
    public ServerPoster serverPoster;
    public UsrDicGenerator userdicGenerator;
    public UniGifTest gifTest;

    public Button ButtonCreateStory;
    public Button ButtonMyStories;
    public Button ButtonUserMat;
    public Button ButtonShare;
    //public Button ButtonReport;
    public VideoPlayer vp;
    public Button DebugButton;
    public RawImage videoOutput;
    public RawImage fullscreen;
    public Button SwitchToPreVideo, FullScreenSwitchToPreVideo, SwitchToNxtVideo, FullScreenSwitchToNxtVideo;
    public string[] playList;
    public Button exitButton1, exitButton2;
    public int playingIndex = 0;
    public Button BigPlayButton;
    public Button ButtonDisableSendLogToSever;
    public Text DeviceID;
    public Canvas MainCanvas, SideCanvas;
    public void SwitchToVideo(int newIndex) {
        playingIndex = newIndex;
        vp.url = playList[playingIndex];
        vp.Prepare();
        vp.Pause();
    }
    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
        Server.deviceUniqueIdentifier = android_id;
        
#endif
        serverPoster = GetComponent<ServerPoster>();
        userdicGenerator = GetComponent<UsrDicGenerator>();

        GameObject gifManager = GameObject.Find("GifManager");
        gifTest = gifManager.GetComponent<UniGifTest>();
       
    }

    void fetchJsonDataCallback()
    {
        DataMap.initData();

        DownloadUserMat();

        ButtonCreateStory.interactable = true;
        ButtonMyStories.interactable = true;
        ButtonUserMat.interactable = true;
        userdicGenerator.initUserDic();
    }

    void fetchPrivateMatDataCallback()
    {
       // gifTest.initGifData();
    }

    void DownloadUserMat()
    {
        if(DataMap.privateActorList!=null && DataMap.privateActorList.privateActorList.Count != 0)
        {
            foreach (PrivateActor privateActor in DataMap.privateActorList.privateActorList)
            {
                // StartCoroutine(serverPoster.DownLoadUserImg(privateActor.Name, "actor"));
                StartCoroutine(serverPoster.DownloadPrivateImg("actor", Server.getMatCreator(),privateActor.Name,privateActor.Name+".png"));
                StartCoroutine(serverPoster.DownloadPrivateImg("actor", Server.getMatCreator(), privateActor.Name, privateActor.Name + "_raw.png"));
            }
        }

        if (DataMap.privatePropList != null && DataMap.privatePropList.privatePropList.Count != 0)
        {
            foreach (PrivateProp privateProp in DataMap.privatePropList.privatePropList)
            {
                // StartCoroutine(serverPoster.DownLoadUserImg(privateProp.Name, "prop"));
                StartCoroutine(serverPoster.DownloadPrivateImg("prop", Server.getMatCreator(), privateProp.Name, privateProp.Name + ".png"));
                StartCoroutine(serverPoster.DownloadPrivateImg("prop", Server.getMatCreator(), privateProp.Name, privateProp.Name + "_raw.png"));
            }
        }

        if (DataMap.privateBackgroundList != null && DataMap.privateBackgroundList.privateBackgroundList.Count != 0)
        {
            foreach ( PrivateBackground privateBackground in DataMap.privateBackgroundList.privateBackgroundList)
            {
                // StartCoroutine(serverPoster.DownLoadUserImg(privateBackground.Name, "background"));
                StartCoroutine(serverPoster.DownloadPrivateImg("background", Server.getMatCreator(), privateBackground.Name, privateBackground.Name + ".png"));
            }
        }

    }

    

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
        DeviceID.text = android_id;
        Server.deviceUniqueIdentifier = android_id;
#endif
        ButtonDisableSendLogToSever.onClick.AddListener(() =>
        {
            Debug.Log("onClick: ButtonDisableSendLogToSever");
            Server.sendLogToServer = false;
            AndroidUtils.ShowToast("已停止向服务器发送日志");
        });

        if (DataMap.publicActorList.publicActors.Count == 0)
        {
            AndroidUtils.ShowToast("请等待初始化...");
            ButtonCreateStory.interactable = false;
            ButtonMyStories.interactable = false;
            ButtonUserMat.interactable = false;
            StartCoroutine(serverPoster.FetchJsonData(fetchJsonDataCallback));
            StartCoroutine(serverPoster.FetchPrivaeMatJsonData(fetchPrivateMatDataCallback));
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Disable sleep during the application.

        sideCam.enabled = false;
        sideCam.targetDisplay = 1;
        mainCam.enabled = true;
        mainCam.targetDisplay = 0;
       

        try
        {  // I don't know what would be caught, but we better keep this to prevent crashes.
            ButtonCreateStory.onClick.AddListener(TurnTellStory);
            ButtonMyStories.onClick.AddListener(TurnAnimList);
            ButtonUserMat.onClick.AddListener(TurnUserMat);
            ButtonShare.onClick.AddListener(TurnShare);
           
            /*DebugButton.onClick.AddListener(() =>
            {

                if (AndroidUtils.useNativeAudioPlayer)
                {
                    AndroidUtils.useNativeAudioPlayer = false;
                    AndroidUtils.ShowToast("切换至Unity音频播放器");
                    Debug.Log("Switch to Unity Audio Player");
                }
                else
                {
                    AndroidUtils.useNativeAudioPlayer = true;
                    AndroidUtils.ShowToast("切换至SoundPool");
                    Debug.Log("Switch to SoundPool Audio Player");
                }

            });
            exitButton1.onClick.AddListener(() => {
                AndroidUtils.BackToMainMenu();
                Application.Quit();
            });
            exitButton2.onClick.AddListener(() => {
                AndroidUtils.RestoreVoiceService();
                Application.Quit();
            });*/
            Screen.SetResolution(1280, 800, true, 60);
            vp.isLooping = false;
            playList = new string[] { Application.streamingAssetsPath + "/video/usage_.mp4", Application.streamingAssetsPath + "/video/pattern_.mp4" };
            SwitchToVideo(0);
            SwitchToPreVideo.onClick.AddListener(() => {
                Debug.Log("onClick: SwitchToPreVideo");
                SwitchToVideo(playingIndex > 0 ? playingIndex - 1 : playList.Length - 1);
            });
            FullScreenSwitchToPreVideo.onClick.AddListener(() => {
                Debug.Log("onClick: FullScreenSwitchToPreVideo");
                SwitchToVideo(playingIndex > 0 ? playingIndex - 1 : playList.Length - 1);
            });
            SwitchToNxtVideo.onClick.AddListener(() => {
                Debug.Log("onClick: SwitchToNxtVideo");
                SwitchToVideo(playingIndex < playList.Length - 1 ? playingIndex + 1 : 0);
            });
            FullScreenSwitchToNxtVideo.onClick.AddListener(() => {
                Debug.Log("onClick: FullScreenSwitchToNxtVideo");
                SwitchToVideo(playingIndex < playList.Length - 1 ? playingIndex + 1 : 0);
            });
            var button1 = videoOutput.gameObject.AddComponent<Button>();
            button1.onClick.AddListener(DoubleClickSwithVideoPlayerMode);
            var button2 = fullscreen.gameObject.AddComponent<Button>();
            button2.onClick.AddListener(DoubleClickSwithVideoPlayerMode);
            if (!AsrMain.isJiebaLoaded && !AsrMain.hasJiebaLoaded)
            {
                AsrMain.hasJiebaLoaded = true;
                Application.logMessageReceivedThreaded += Server.sendLogMessageToServer;
                Server.deviceName = SystemInfo.deviceName;
                // Server.deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
                AsrMain.usrDicPath = Application.persistentDataPath + "/jiebaConfig/usr_dic.txt";
                ResourceUtils.persistentDataPath = Application.persistentDataPath;
                foreach (string r in AsrMain.resourceFiles)
                {
                    AsrMain.requests[r] = new WWW(ResourceUtils.getReaderPath(r));
                }
                while (!AsrMain.allRequestsDone()) { }
                new Thread(AsrMain.InitJieba).Start();
                
                AndroidUtils.RecognizeInit();
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += (string perm) => {
                    AndroidUtils.ShowToast("权限申请失败，部分功能可能无法使用，请手动开启。");
                };
                callbacks.PermissionDeniedAndDontAskAgain += (string perm) => {
                    AndroidUtils.ShowToast("权限申请失败，部分功能可能无法使用，请手动开启。");
                };
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
            BigPlayButton.onClick.AddListener(() =>
            {
                Debug.Log("onClick: BigPlayButton");
                vp.Play();
            });
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
        }

        if (TestRunner.TestingMode)
        {
            testingFunction();
        }
    }

    public Camera mainCam, sideCam; public RenderTexture background;
    float lastTime = 0; bool fullScreenMode = false;
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
        if (!fullScreenMode)
        {
            mainCam.targetTexture = background;
            sideCam.enabled = true;
            sideCam.targetDisplay = 0;
            mainCam.targetDisplay = 1;
            fullScreenMode = true;
            MainCanvas.GetComponent<GraphicRaycaster>().enabled = false;
            SideCanvas.GetComponent<GraphicRaycaster>().enabled = true;
        }
        else
        {
            mainCam.targetTexture = null;
            sideCam.enabled = false;
            sideCam.targetDisplay = 1;
            mainCam.targetDisplay = 0;
            fullScreenMode = false;
            MainCanvas.GetComponent<GraphicRaycaster>().enabled = true;
            SideCanvas.GetComponent<GraphicRaycaster>().enabled = false;
        }
    }

     void TurnTellStory()
    {
        Debug.Log("onClick: TurnTellStory");
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            var callbacks = new PermissionCallbacks();
            
            callbacks.PermissionGranted += (string perm) =>
            {
                SceneManager.LoadScene("SceneTellStory");
            };
            callbacks.PermissionDenied += (string perm) => {
                AndroidUtils.ShowToast("权限申请失败，部分功能可能无法使用，请手动开启。");
            };
            callbacks.PermissionDeniedAndDontAskAgain += (string perm) => {
                AndroidUtils.ShowToast("权限申请失败，部分功能可能无法使用，请手动开启。");
            };
            Permission.RequestUserPermission(Permission.Microphone);
        } else
        {
            SceneManager.LoadScene("SceneTellStory");
        }
    }

    void TurnUserMat()
    {
        Debug.Log("onClick: TurnUserMat");
        SceneManager.LoadScene("SceneUserMatIndex");
    }

    void TurnAnimList()
    {
        Debug.Log("onClick: TurnAnimList");
        SceneManager.LoadScene("SceneMyStories");
    }

    void TurnShare()
    {
        Debug.Log("onClick: TurnShare");
        SceneManager.LoadScene("SceneShare");
    }

    void SaveVideo()
    {
        string oriPath = System.IO.Path.Combine(Application.streamingAssetsPath, "preview.mp4.bytes");
        WWW reader = new WWW(oriPath);
        while (!reader.isDone) { }

        var realPath = Application.persistentDataPath + "/preview.mp4";
        System.IO.File.WriteAllBytes(realPath, reader.bytes);

        byte[] mediaBytes = File.ReadAllBytes(realPath);
        NativeGallery.SaveVideoToGallery(mediaBytes, "TellStory", "preview.mp4", (bool success, string path) => {
            Debug.Log("NativeGallery.SaveImageToGallery: " + success);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!vp.isPlaying)
        {
            //BigPlayButtonImage.enabled = true;
            BigPlayButton.enabled = true;
            BigPlayButton.GetComponent<Image>().enabled = true;
        } else
        {
            //BigPlayButtonImage.enabled = false;
            BigPlayButton.enabled = false;
            BigPlayButton.GetComponent<Image>().enabled = false;
        }
    }
}
