using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using Animation = UnityEngine.Animation;
using UnityEditor;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Interfaces;

namespace AnimGenerator
{
    [Serializable]
    public class KeyFrameList
    {
        public KeyFrameList()
        {
            this.keyFrames = new List<KeyFrame>();
        }

        public KeyFrameList(List<KeyFrame> keyFrames)
        {
            this.keyFrames = keyFrames;
        }

        public List<KeyFrame> keyFrames;

        //获取keyframeList的背景
        public string getBkgName()
        {
            string bkgName = "";
            foreach(KeyFrame keyFrame in keyFrames)
            {
                if (keyFrame.action == 4)
                {
                    bkgName = keyFrame.name;
                }
            }
            return bkgName;
        }

        public float getDuration()
        {
            float maxDuration = 0;
            foreach (KeyFrame keyFrame in keyFrames)
            {
                float current = keyFrame.timestamp + keyFrame.duration;
                if (current > maxDuration)
                {
                    maxDuration = current;
                }
            }
            return maxDuration;
        }

    }


    public enum EInterpreterMode
    {
        Display,
        Edit,
        Wait
    }

    public class Interpreter : MonoBehaviour
    {

       // public GameObject contentPanel;

        KeyFrameList keyFrameList;

        /// <summary>
        /// 加载的json文件名
        /// </summary>
        //private const string JSON_FILE_NAME = "dbTest";
        //private const string JSON_FILE_NAME = "duckHunting0";
        //private  string JSON_FILE_NAME = "2dTest";
        //private const string JSON_FILE_NAME = "realDuck";
        //private const string JSON_FILE_NAME = "zoo";
        //private const string JSON_FILE_NAME = "ExperimentHunter";
        //private const string JSON_FILE_NAME = "Experiment_two_scenes";
        private string JSON_FILE_NAME = "";

        // 2D 下的场景边界
        private const float MIN_X_2D = -16;
        private const float MIN_Y_2D = -9;
        private const float MAX_X_2D = 16;
        private const float MAX_Y_2D = 9;

        // 对话气泡的高度
        private const float DIALOG_HEIGHT = .3f;

        // 各类形象的尺寸
        private const float FLYING_HEIGHT = .4f;
        private const float FLYING_WIDTH = .2f;

        // 切换展示模式和配置模式
        private float starttime = 0;
        private EInterpreterMode interpreterMode = EInterpreterMode.Wait;

        private DirectoryInfo streamingDir;
        private bool suitable=false;

        /// <summary>
        /// 用以索引场景中的所有 GameObject goname go
        /// </summary>
        public Dictionary<string, GameObject> gos = new Dictionary<string, GameObject>();

        /// <summary>
        /// 用以索引场景中 GameObject 的对话框
        /// </summary>
        Dictionary<string, GameObject> dialogs = new Dictionary<string, GameObject>();

        ResManager resManager;
        public AudioManager audioManager;
       // WeatherManager weather;

        public LoadResource loadResource;
        public PannelController pannelController;

        //[Header("预制的一系列角色")]
        
        //public GameObject stuff;  // 静态道具
        //public GameObject human;  // 人
        //public GameObject flying; // 飞行动物
        //public GameObject creeping; // 四脚爬行动物
        //public GameObject suitHuman;//有多套服装的
        //public GameObject dialog;
       
        [Header("直接索引的场景中的物体或资产")]
        public GameObject audioContainer; 
        public Text subtitle;

        public Image panel;
        public Button buttonLLM;

        // Start is called before the first frame update
        void Start()
        {
            keyFrameList = new KeyFrameList();
            streamingDir = new DirectoryInfo(Application.persistentDataPath);
            resManager = this.gameObject.AddComponent<ResManager>();
            resManager.panel = this.panel;
            //resManager.TestGetActorInfoByName();
            StartCoroutine(interpreteFrames());
        } 

     

        public void StartDisplay(string jsonFileName)
        {
            // 解析 json 文件并排序
            
            keyFrameList = JsonOperator.parseObj<KeyFrameList>(jsonFileName);
            //preHandle(keyFrameList);
            if (keyFrameList == null)
            {
                Debug.LogError("Unexpected json string.");
            }
            keyFrameList.keyFrames.Sort((k1, k2) =>
            {
                if (k1.timestamp < k2.timestamp) return -1;
                return 1;
            });

            interpreterMode = EInterpreterMode.Display;
            starttime = Time.time;
        }

        //public void WaitNewInput()
        //{
        //    interpreterMode = EInterpreterMode.Wait;
        //}

        /// <summary>
        /// 记录当前解析到第几条了
        /// </summary>
        //private int i = 0;

        /// <summary>
        /// 当前解析到第几个文件了
        /// </summary>
        private int fileIndex = 0;


        public void setJsonFileName(string fileName)
        {
            this.JSON_FILE_NAME = fileName;
        }


        public IEnumerator interpreteFrames()
        {
           
            
            int i = 0;
            string fileName = JSON_FILE_NAME;
            while (true)
            {
                if (interpreterMode == EInterpreterMode.Display)
                {

                    while (i < keyFrameList.keyFrames.Count && Time.time - starttime > keyFrameList.keyFrames[i].timestamp)
                    {

                        KeyFrame frame = keyFrameList.keyFrames[i];
                        //Debug.Log("name: " + frame.name + " goname: " + frame.goname + "action: " + frame.action + " content: " + frame.content);
                        
                        switch (frame.action)
                        {
                            case 0: // 出现
                               
                                Appear(frame);
                                
                                break;
                            case 1: // 消失
                                DisAppear(frame);
                                break;
                            case 2: // 移动和动画                               
                                Move(frame);                               
                                break;
                            case 3: // 播放音乐
                                Audio(frame);
                                break;
                            case 4: // 更换背景图片
                                Background(frame);
                                break;
                            case 5:
                                ShowSubtitle(frame);
                                break;
                            case 6:
                                ShowDialog(frame);
                                break;
                            case 7:
                                changeColor(frame);
                                break;

                            default:
                                Debug.LogError($"Unexpected action type {frame.action} at timestamp {frame.timestamp}");
                                break;
                        }
                        
                        ++i;
                    }
                    
                    if (i >= keyFrameList.keyFrames.Count)
                    {
                        //Debug.Log("动画展示完毕");
                        // buttonLLM.gameObject.SetActive(true);
                        i = 0;
                        interpreterMode = EInterpreterMode.Wait;
                    }
                }
                else if (interpreterMode == EInterpreterMode.Wait)
                {
                    string newFile = JSON_FILE_NAME;
                    while (newFile.Equals(fileName) || newFile.Equals(""))
                    {

                        fileName = newFile;
                        yield return null;
                        
                        newFile = JSON_FILE_NAME;
                    }
                    //Debug.Log("准备播放文件:" + newFile);
                    // buttonLLM.gameObject.SetActive(false);
                    fileName = newFile;
                    string filePath = $"{Application.persistentDataPath}/{newFile}.json";
                    StartDisplay(filePath);                  
                    
                }
                yield return null;
            }
           
        }

        void Update()
        {
           
        }

        private void Appear(KeyFrame frame)
        {
            // 重复名称报错
            if (gos.ContainsKey(frame.goname))
            {
                Debug.LogError($"Duplicate gameobject name {frame.goname}.");
                return;
            }

            // 实例化物体，如果指定好了，将这个指定好的go给到tobeinstantiate
            //GameObject tobeinstantiate = creeping;
            //suitable = false;
            //switch (frame.type)
            //{
            //    case 0:
            //        tobeinstantiate = stuff;
            //        break;
            //    case 1:
            //        tobeinstantiate = human;
            //        break;
            //    case 2:
            //        tobeinstantiate = creeping;
            //        break;
            //    case 3:
            //        tobeinstantiate = flying;
            //        break;
            //    case 4:
            //        {
            //            tobeinstantiate = suitHuman;
            //            suitable = true;
            //            break;
            //        }
                    
            //    default:
            //        Debug.LogError($"Unexpected gameobject type {frame.type} when instantiating at timestamp {frame.timestamp}");
            //        break;
            //}
           
            try
            {
                //Profiler.BeginSample("InitCharacter");
                //GameObject go = resManager.InitCharacter(frame.name, frame.goname, frame.StartPosition, frame.StartRotation, suitable);
                
                GameObject go = resManager.InitCharacter(frame.name, frame.goname, frame.getVec3(frame.startpos),frame.getQuaternion(frame.startrotation),frame.getVec3(frame.startscale) ,suitable);
                
                //go.transform.localScale = frame.getVec3(frame.startscale);

               
                gos[frame.goname] = go;
            }catch(Exception e)
            {
                Debug.Log(e.Message);
            }
           

         

        }       
     
    
        private void UpdateDialogPos(Vector3 pos, RectTransform trans)
        {
            Vector2 anchor = new Vector2((pos.x - MIN_X_2D) / (MAX_X_2D - MIN_X_2D), (pos.y - MIN_Y_2D) / (MAX_Y_2D - MIN_Y_2D));
            trans.anchorMin = anchor + new Vector2(-FLYING_WIDTH / 2, FLYING_HEIGHT / 2);
            trans.anchorMax = anchor + new Vector2(FLYING_WIDTH / 2, FLYING_HEIGHT / 2 + DIALOG_HEIGHT);
        }

        private void DisAppear(KeyFrame frame)
        {
            if (!gos.ContainsKey(frame.goname))
            {
                Debug.Log($"Gameobject {frame.goname} dose not exsit when removing at timestamp {frame.timestamp}.");
                return;
            }
            Destroy(gos[frame.goname]);
           // Destroy(dialogs[frame.name]);
            
            gos.Remove(frame.goname);
            //dialogs.Remove(frame.name);

            resManager.removeGoRes(frame.goname);

        }

        private void changeColor(KeyFrame frame)
        {
            if (!gos.ContainsKey(frame.goname))
            {
                Debug.LogError($"Gameobject {frame.goname} dose not exsit when removing at timestamp {frame.timestamp}.");
                return;
            }
            try
            {
                resManager.changeColor(frame.goname, frame.color);
            }catch(Exception e)
            {
                Debug.Log(e.Message);
            }
            

        }

        /*
        private void ChangeClothes(KeyFrame frame)
        {
            if (!gos.ContainsKey(frame.goname))
            {
                Debug.LogError($"Gameobject {frame.goname} does not exist when animating at timestamp {frame.timestamp}.");
                return;
            }
            resManager.ChangeClothes(frame.goname, frame.clothes);
        }
        */

        private void Move(KeyFrame frame)
        {
            if (!gos.ContainsKey(frame.goname))
            {
                Debug.LogError($"Gameobject {frame.goname} does not exist when animating at timestamp {frame.timestamp}.");
                return;
            }
            //播放动画
            try
            {
                              
                resManager.ChangeAnim(frame.goname, frame.content, frame.loop, frame.duration,
                    frame.getVec3(frame.startpos), frame.getVec3(frame.endpos), frame.getVec3(frame.startscale), 
                    frame.getVec3(frame.endscale), frame.getQuaternion(frame.startrotation), frame.getQuaternion(frame.endrotation));
  
                       
            }
            catch(Exception e)
            {
                Debug.Log(e);
                Debug.Log(e.Message);
            }
            
            //移动
            //StartCoroutine(Animate(frame));
        }

        AudioClip audioClip = null;
        string audioClipPath = "";
        public void setAudioClip(AudioClip clip, string fullPath)
        {
            audioClip = clip;
            audioClipPath = fullPath;
        }
        public static bool isPlayingAudio = false;

        private void Audio(KeyFrame frame)
        {
            //AssetDatabase.Refresh();
            StartCoroutine(LoadAudioAndPlay(frame));
        }

        IEnumerator LoadAudioAndPlay(KeyFrame frame)
        {
            if (TestRunner.TestingMode) yield break;
            if (!AndroidUtils.useNativeAudioPlayer)
            {
                yield return new WaitUntil(() => audioClip == null);
#if UNITY_EDITOR
                yield return loadResource.LoadAudioFile(Application.persistentDataPath + "/audio/" + frame.name + ".wav");
#else
            int number = int.Parse(frame.name.Split(new string[] { "_story" }, StringSplitOptions.None)[1]);
            Debug.Log("Load audio from dict1: " + number);
            yield return loadResource.LoadAudioFile(MicroPhoneSingleView.audioSavePath[number]);
#endif
                yield return DestroyAfterPlay(frame);
            }
            else
            {
                int number = int.Parse(frame.name.Split(new string[] { "_story" }, StringSplitOptions.None)[1]);
                //Debug.Log("Load audio from dict1: " + number);
                yield return new WaitUntil(()=> {
                    return !isPlayingAudio;
                });
                isPlayingAudio = true;
                try
                {
                    Debug.Log("开始播放录制音频：" + MicroPhoneSingleView.audioSavePath[number]);
                    //AndroidUtils.PlayWavFile(MicroPhoneSingleView.audioSavePath[number]);
                    audioManager.Playre(MicroPhoneSingleView.audioSavePath[number]);
                } catch
                {

                }
                yield return new WaitForSeconds(frame.loop == 0 ? frame.duration : audioClip.length * frame.loop);
                
                isPlayingAudio = false;
            }
        }

        IEnumerator DestroyAfterPlay(KeyFrame frame)
        {
            // 先实例化一个声源，循环播放音频
            //GameObject go = Instantiate(audioContainer, Vector3.zero, Quaternion.identity);
            AudioSource source = audioContainer.GetComponent<AudioSource>();
            source.Stop();
            source.clip = audioClip;
            //Debug.Log("时长：" + audioClip.length+ ", isReadyToPlay:" + audioClip.loadState + "");
            if (frame.loop == 1)
            {
                source.loop = true;
            }
            if (frame.loop == 0)
            {
                source.loop = false;
            }
            Debug.Log("开始播放：" + audioClipPath);
            source.Play();
            isPlayingAudio = true;
            // 如果是循环模式是 -1，直接结束
            if (frame.loop == -1)
                yield break;

            // 否则等待对应时间后销毁声源
            yield return new WaitForSeconds(frame.loop == 0 ? frame.duration : audioClip.length * frame.loop);
            Debug.Log("播放完成：" + audioClipPath);
            audioClip = null;
            audioClipPath = "";
            isPlayingAudio = false;
            //Destroy(source);
            //Destroy(go);
        }

        private void Background(KeyFrame frame)
        {
            //Texture2D newtex = Resources.Load<Texture2D>($"Environment/2D/{frame.name}");
            //if(newtex == null)
            //{
            //    Debug.LogError($"Failed to load texture {frame.name} at timestamp {frame.timestamp}.");
            //    return;
            //}
            //bkgmat.SetTexture("_MainTex", newtex);
            PrivateBackground privateBackground = DataMap.privateBackgroundList.FindPrivateBkgByMatName(frame.name);
            if (privateBackground != null)
            {
                pannelController.changeBkgUserMat(privateBackground);
            }
            else
            {
                pannelController.changeBkg(frame.name);
            }         
        }

       

        Coroutine subCor;
        private void ShowSubtitle(KeyFrame frame)
        {
            subtitle.text = frame.content;
            if (subCor != null)
            {
                StopCoroutine(subCor);
            }
            subCor=StartCoroutine(WaitToEndSubtitle(frame.duration));
        }

        private IEnumerator WaitToEndSubtitle(float time)
        {
            yield return new WaitForSeconds(time);
            subtitle.text = string.Empty;
        }

        private void ShowDialog(KeyFrame frame)
        {
            if (!dialogs.ContainsKey(frame.name))
            {
                Debug.LogError($"Failed to find dialog attached to gameobject {frame.name} at timestamp {frame.timestamp}");
                return;
            }
            StartCoroutine(DisableAfterDialog(frame));
        }

        private IEnumerator DisableAfterDialog(KeyFrame frame)
        {
            GameObject dia = dialogs[frame.name];
            dia.SetActive(true);
            dia.GetComponentInChildren<Text>().text = frame.content;
            yield return new WaitForSeconds(frame.duration);
            dia.SetActive(false);
        }

        //清除所有存储数据
        public void clearScene()
        {
            if (gos.Count != 0)
            {
                foreach (string goname in gos.Keys)
                {
                    Destroy(gos[goname]);
                    resManager.removeGoRes(goname);
                }
            }

            gos.Clear();
        }



       
    }
}