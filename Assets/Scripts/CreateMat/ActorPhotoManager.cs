using Assets.Scripts.TellStory.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Interfaces;
using System;

public class ActorPhotoManager : MonoBehaviour
{

    #region UI
    public Image inputImage;

    public Button photoBtn;
    public Button selectBtn;
    #endregion


    public string rawName { private set;  get; }

    public string userName { get;private set; }

    public Sprite rawSprite { set; get; }
    public Sprite cutSprite { set; get; }
    public Texture2D rawImageTex { get; private set; }  
    public Texture2D cutImageTex { get; private set; }

    public SceneCreateActor sceneCreate;
    public ServerPoster serverPoster;
    public UsrDicGenerator usrDicGenerator;
    //public SceneCreateActor sceneCreateActor;

    public GameObject testGo;

    public CamManager camManager;
    public CamUtil camUtil;

    public SaveUserMat saveUserMat;

    public bool createFinished=false;

    private void Awake()
    {
        serverPoster = GetComponent<ServerPoster>();
        usrDicGenerator = GetComponent<UsrDicGenerator>();
        sceneCreate = GetComponent<SceneCreateActor>();

        camManager = GetComponent<CamManager>();
        camUtil = GetComponent<CamUtil>();
        saveUserMat = GetComponent<SaveUserMat>();
    }

    private void Start()
    {
        userName = Server.getMatCreator();
        
       
       
        photoBtn.onClick.AddListener(onClickTakePhoto);
        selectBtn.onClick.AddListener(onClickSelectImg);
        //returnMainBtn.onClick.AddListener(() => { SceneManager.LoadScene("SceneMainMenu"); });
      
        rawImageTex = new Texture2D((int)inputImage.rectTransform.rect.width, (int)inputImage.rectTransform.rect.height,TextureFormat.RGB24,false);
        
        //testGo = new GameObject();
    }

    public void onClickTakePhoto()
    {
        if (Application.isEditor)
        {
            string path = "";
#if UNITY_EDITOR
            path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png,jpg");
#endif
            if (path.Length != 0)
            {
                var fileContent = File.ReadAllBytes(path);

                rawImageTex.LoadImage(fileContent);
                rawImageTex.Apply();
                rawSprite = ImgUtil.createSprite(rawImageTex);
            }
            inputImage.sprite = rawSprite;

        }
        else
        {           
            camUtil.TakePicture(512,inputImage);
          
        }


    }

    public void onClickSelectImg()
    {
        if (Application.isEditor)
        {
            string path = "";
#if UNITY_EDITOR
            path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png,jpg");
#endif
            if (path.Length != 0)
            {
                var fileContent = File.ReadAllBytes(path);

                rawImageTex.LoadImage(fileContent);
                rawImageTex.Apply();

                // 检查文件扩展名
                if (Path.GetExtension(path).ToLower() == ".png")
                {
                    // 如果是 PNG，转换为 JPG
                    fileContent = rawImageTex.EncodeToJPG();
                    rawImageTex.LoadImage(fileContent);
                    rawImageTex.Apply();
                }


                rawSprite = ImgUtil.createSprite(rawImageTex);
            }
            inputImage.sprite = rawSprite;

        }
        else
        {
            ImgUtil.PickImage(512, inputImage);
        }
    }



    public void onClickCreate()
    {
        if (inputImage.sprite == null)
        {
            createFinished = true;
            return;
        }

        rawSprite = inputImage.sprite;
        rawImageTex = rawSprite.texture;

        SceneCreateActor sceneCreateActor = GetComponent<SceneCreateActor>();
        if (sceneCreateActor.matMode == SceneCreateActor.MATMODE.Background)
        {
            this.cutImageTex = rawImageTex;
            this.cutSprite = rawSprite;
            this.inputImage.sprite = null;
            createFinished = true;
        }
        else
        {
            StartCoroutine(cutImg(cutCallBack));
        }

            
    }

    void cutCallBack()
    {
        Debug.Log("分割图像完毕");
        this.inputImage.sprite = null;       
        createAnim();
       
    }

    public void onClickCheck(string inputName)
    {
        saveMat(inputName);
        Debug.Log("已保存素材:" + this.rawName);
    }

    public void onClickClean()
    {
        Destroy(testGo);
        inputImage.sprite = null;
        rawSprite = null;
        cutSprite = null;
        rawImageTex = new Texture2D((int)inputImage.rectTransform.rect.width, (int)inputImage.rectTransform.rect.height);
        cutImageTex = null;
    }

    IEnumerator cutImg(Action callback)
    {
        Texture2D inputTex = rawImageTex;

        Directory.CreateDirectory(Application.persistentDataPath + "/cutTmpImg/" + userName + "/input");
        //if(Directory.Exists(Application.persistentDataPath+""))
        //string localTmpUrl = "/cutTmpImg/";
        string localTmpUrl = "/cutTmpImg/" + userName + "/input/";
        ImgUtil.savePngNoBound("inputImg.jpg", inputTex, localTmpUrl);

        Texture2D outputTex = new Texture2D(inputTex.width, inputTex.height,TextureFormat.RGB24,false);
        yield return StartCoroutine(serverPoster.UploadCutImg(userName, localTmpUrl, "inputImg.jpg", outputTex));

        //ImgUtil cutUtil = new ImgUtil();
        //Texture2D outputTex = cutUtil.grabCut(inputTex);
        Sprite outSprite = Sprite.Create(outputTex, new Rect(0.0f, 0.0f, outputTex.width, outputTex.height), new Vector2(0.5f, 0.5f), 100.0f); ;

        this.cutImageTex = outputTex;
        this.cutSprite = outSprite;

        callback.Invoke();

        yield return new WaitForSeconds(2);
        createFinished = true;
    }



    void saveMat(string rawName)
    {
        //this.rawName = inputField.text;

        if(rawName==null || rawName == "")
        {
            Debug.Log("请输入素材名称");
            AndroidUtils.ShowToast("请输入素材名称");
            return;
        }
        if (cutImageTex == null)
        {
            Debug.Log("未设置素材");
            AndroidUtils.ShowToast("未设置素材");
            return;
        }

        //联网检查
        SceneCreateActor sceneCreateActor = GetComponent<SceneCreateActor>();
        SceneCreateActor.MATMODE matMode = sceneCreateActor.matMode;
        if (matMode == SceneCreateActor.MATMODE.Actor)
        {
            //saveActor(rawName, cutImageTex,rawImageTex);
            saveUserMat.saveActor(userName, rawName, cutImageTex, rawImageTex);
        }
        else if (matMode == SceneCreateActor.MATMODE.Prop)
        {
            //saveProp(rawName, cutImageTex,rawImageTex);
            saveUserMat.saveProp(userName,rawName, cutImageTex, rawImageTex);
        }
        else
        {
            saveUserMat.saveBkg(userName, rawName, cutImageTex);
            //saveBkg(rawName, cutImageTex);
        }

        DataMap.initData();
        usrDicGenerator.initUserDic();
        //DataMap.mergePrivateInfo();

    }

    /*
    void saveActor(string name,Texture2D texture2D,Texture2D texture2DRaw)
    {
        string matName = userName + "_" + name;
        //本地存放url
        string url = "/private/actor/" + matName;
        PrivateActor privateActor = new PrivateActor(matName, name, userName, url, "human");
        ImgUtil. savePng(matName+".png", texture2D, url);
        ImgUtil. savePngNoBound(matName + "_raw.png", texture2DRaw, url);
        
        if (DataMap.privateActorList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("已覆盖同名素材");
            DataMap.privateActorList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Actor", "Name",matName));
        }
        DataMap.privateActorList.privateActorList.Add(privateActor);
        //这里要先添加联网检查
        
        StartCoroutine(serverPoster.AddElemToDataBase("Huiben_Private_Material", "Actor", privateActor));
        // StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "actor", userName, matName));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + "_raw.png", "actor", userName, matName));
    }

    void saveBkg(string name,Texture2D texture2D)
    {
        string matName = userName + "_" + name;
        //本地存放url
        string url = "/private/background/" + matName;
        PrivateBackground privateBackground = new PrivateBackground(matName, name, userName, url);
        ImgUtil.savePngNoBound(matName + ".png", texture2D, url);

        if (DataMap.privateBackgroundList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("已覆盖同名素材");
            DataMap.privateBackgroundList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Background", "Name", matName));
        }        
        DataMap.privateBackgroundList.privateBackgroundList.Add(privateBackground);
        //这里要先添加联网检查
        StartCoroutine(serverPoster.AddElemToDataBase<PrivateBackground>("Huiben_Private_Material", "Background", privateBackground));
        //StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "background", userName, matName));


    }

    void saveProp(string name, Texture2D texture2D, Texture2D texture2DRaw)
    {
        string matName = userName + "_" + name;
        //本地存放url
        string url = "/private/prop/" + matName;
        PrivateProp privateProp = new PrivateProp(matName, name, userName, url);
        ImgUtil. savePng(matName + ".png", texture2D, url);
        ImgUtil.savePng(matName + "_raw.png", texture2DRaw, url);

        if (DataMap.privatePropList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("已覆盖同名素材");
            DataMap.privatePropList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Prop", "Name", matName));
        }
        DataMap.privatePropList.privatePropList.Add(privateProp);
        //这里要先添加联网检查
        StartCoroutine(serverPoster.AddElemToDataBase<PrivateProp>("Huiben_Private_Material", "Prop", privateProp));
        //StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "prop", userName, matName));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + "_raw.png", "prop", userName, matName));
    }
    */


    void createAnim()
    {
        if (cutSprite == null)
        {
            Debug.Log("没有设置动画原始图片");
            return;
        }
        testGo = new GameObject("TestGo");
        //testGo.layer = 6;
        testGo.transform.SetParent(inputImage.transform);
        testGo.transform.localScale = new Vector3(180, 180, 180);
        

        SpriteRenderer renderer = testGo.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "screen_show_layer";
        renderer.sprite = cutSprite;


        SceneCreateActor sceneCreateActor = GetComponent<SceneCreateActor>();
        if (sceneCreateActor.matMode == SceneCreateActor.MATMODE.Actor)
        {
            Animation animation = testGo.AddComponent<Animation>();

            AnimationClip animationClip = AnimFactory.genStretchAnimClip(testGo.transform.localScale.y, "y");

            animation.AddClip(animationClip, "testAnim");

            animation.Play("testAnim");
            AndroidUtils.ShowToast("创建动画完毕");
        }

       

    }

    


 




}
