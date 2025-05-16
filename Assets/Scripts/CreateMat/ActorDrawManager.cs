using FreeDraw;
using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ActorDrawManager : MonoBehaviour
{

    //public InputField inputField;
    //public Button createBtn, checkBtn;
    public Image inputImage;

    public Sprite CanvasSprite;
   
    public GameObject drawSpriteGo;

    public string rawName;
    private string userName;

    public SceneCreateActor sceneCreate;
    public ServerPoster serverPoster;
    public UsrDicGenerator usrDicGenerator;
    //public SceneCreateActor sceneCreateActor;

    public GameObject testGo;

    private void Awake()
    {
        //sceneCreate = GetComponent<SceneCreateActor>();
        serverPoster = GetComponent<ServerPoster>();
        usrDicGenerator = GetComponent<UsrDicGenerator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        userName = Server.getMatCreator();
        
    }

   
    public void onClickCreate()
    {

        Drawable drawable = drawSpriteGo.GetComponent<Drawable>();
        if (!drawable.painted) return;
        drawSpriteGo.SetActive(false);
        //drawSpriteGo.SetActive(true);
        //Drawable drawable = drawSpriteGo.GetComponent<Drawable>();
        //drawable.ResetCanvas();
        SceneCreateActor sceneCreateActor = GetComponent<SceneCreateActor>();
        if (sceneCreateActor.matMode == SceneCreateActor.MATMODE.Actor)
        {
            createAnim();
        }

        AndroidUtils.ShowToast("�ز��Ѵ���");
        Debug.Log("�����������");


    }

    public void onClickCheck(string name)
    {
        saveMat(name);
        Debug.Log("�ѱ����ز�:" + this.rawName);
    }

    public void onClickClean()
    {
        Destroy(testGo);
        drawSpriteGo.SetActive(true);
        Drawable drawable = drawSpriteGo.GetComponent<Drawable>();
        drawable.ResetCanvas();
        //CanvasSprite = null;
        
    }

    void createAnim()
    {
        if (CanvasSprite == null)
        {
            Debug.Log("û�����ö���ԭʼͼƬ");
            return;
        }
        testGo = new GameObject("TestGo");
        //testGo.layer = 6;
        testGo.transform.SetParent(inputImage.transform);
        testGo.transform.localScale = new Vector3(100, 100, 100);


        SpriteRenderer renderer = testGo.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "screen_show_layer";
        renderer.sprite = CanvasSprite;

        Animation animation = testGo.AddComponent<Animation>();

        AnimationClip animationClip = AnimFactory.genStretchAnimClip(testGo.transform.localScale.y, "y");

        animation.AddClip(animationClip, "testAnim");

        animation.Play("testAnim");

     



    }

    void saveMat(string rawName)
    {
        //this.rawName = inputField.text;

        if (rawName == null || rawName == "")
        {
            Debug.Log("�������ز�����");
            AndroidUtils.ShowToast("�������ز�����");
            return;
        }
        if (CanvasSprite == null)
        {
            Debug.Log("�������ز�����");
            AndroidUtils.ShowToast("�������ز�����");
            return;
        }

        //�������
        SceneCreateActor sceneCreateActor = GetComponent<SceneCreateActor>();
        SceneCreateActor.MATMODE matMode = sceneCreateActor.matMode;
        if (matMode == SceneCreateActor.MATMODE.Actor)
        {
            saveActor(rawName,CanvasSprite);
        }else if (matMode == SceneCreateActor.MATMODE.Prop)
        {
            saveProp(rawName,CanvasSprite);
        }
        else
        {
            saveBkg(rawName,CanvasSprite);
        }
        DataMap.initData();
        usrDicGenerator.initUserDic();
        Drawable drawable = drawSpriteGo.GetComponent<Drawable>();
        drawable.ResetCanvas();
        drawSpriteGo.SetActive(true);
    }

    void saveActor(string name,Sprite sprite)
    {
        string matName = userName + "_" + name;
        //���ش��url
        string url = "/private/actor/" + matName;
        PrivateActor privateActor = new PrivateActor(matName, name, userName, url, "human");
        ImgUtil. savePng(matName+".png", sprite.texture, url);
        if (DataMap.privateActorList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("�Ѹ���ͬ���ز�");
            DataMap.privateActorList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Actor", "Name", matName));
        }
        DataMap.privateActorList.privateActorList.Add(privateActor);
        //����Ҫ������������
        StartCoroutine(serverPoster.AddElemToDataBase<PrivateActor>("Huiben_Private_Material", "Actor", privateActor));
        // StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png","actor",userName,matName));
    }

    void saveBkg(string name, Sprite sprite)
    {
        string matName = userName + "_" + name;
        //���ش��url
        string url = "/private/background/" + matName;
        PrivateBackground privateBackground = new PrivateBackground(matName, name, userName, url);

        ImgUtil. savePngNoBound(matName + ".png", sprite.texture, url);
        if (DataMap.privateBackgroundList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("�Ѹ���ͬ���ز�");
            DataMap.privateBackgroundList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Background", "Name", matName));
        }
        DataMap.privateBackgroundList.privateBackgroundList.Add(privateBackground);
        //����Ҫ������������
        StartCoroutine(serverPoster.AddElemToDataBase<PrivateBackground>("Huiben_Private_Material", "Background", privateBackground));
        // StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "background", userName, matName));


    }

    void saveProp(string name, Sprite sprite)
    {
        string matName = userName + "_" + name;
        //���ش��url
        string url = "/private/prop/" + matName;
        PrivateProp privateProp = new PrivateProp(matName, name, userName, url);
        ImgUtil. savePng(matName + ".png", sprite.texture, url);
        if (DataMap.privatePropList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("�Ѹ���ͬ���ز�");
            DataMap.privatePropList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Prop", "Name", matName));
        }
        DataMap.privatePropList.privatePropList.Add(privateProp);
        //����Ҫ������������
        StartCoroutine(serverPoster.AddElemToDataBase<PrivateProp>("Huiben_Private_Material", "Prop", privateProp));
        // StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "prop", userName, matName));
    }



}
