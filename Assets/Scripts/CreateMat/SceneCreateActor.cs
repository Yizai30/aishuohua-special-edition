using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CreateMat;
using FreeDraw;

public class SceneCreateActor : MonoBehaviour
{
    public Button buttonBack, buttonCreateMat, buttonSave, buttonCancelSave, buttonMyMat, buttonGenAI;
    public GameObject AIDialog, photoHintDialog;

    public RecordButton RecordMatNameButton;
    public Sprite RecordButtonIdle, RecordButtonRecording;
    public Toggle togglePhotoWay, toggleDrawWay;
    public Toggle toggleActor, toggleBkg, toggleProp;
    public InputField inputMatNameFiled;

    public CanvasGroup canvasPhoto, canvasDraw, canvasSave;


    public Action createAction, cleanAction;
    public Action<string> checkAction;

    //manager
    ActorPhotoManager actorPhotoManager;
    ActorDrawManager actorDrawManager;

    public ActionPreviewManager actionPreviewManager;

    //public string userName { get; private set; }
    //param
    public enum MATMODE
    {
        Actor,Background,Prop
    }
    public enum EDITMODE
    {
        Photo,Draw,AI
    }

    public MATMODE matMode = MATMODE.Actor;
    public EDITMODE editMode = EDITMODE.Photo;
    private void Awake()
    {
        actorPhotoManager = GetComponent<ActorPhotoManager>();
        actorDrawManager = GetComponent<ActorDrawManager>();
        //userName = Server.getMatCreator();
        //actorPhotoManager.initBtnAction = ActorPhotoManagerInit;
    }

    private void Start()
    {
        AIDialog.SetActive(false);

        createAction = actorPhotoManager.onClickCreate;
        checkAction = actorPhotoManager.onClickCheck;
        cleanAction = actorPhotoManager.onClickClean;

        toggleDrawWay.onValueChanged.AddListener(turnDrawWay);
        togglePhotoWay.onValueChanged.AddListener(turnPhotoWay);

        buttonBack.onClick.AddListener(() => { SceneManager.LoadScene("SceneUserMatIndex"); });

        buttonCreateMat.onClick.AddListener(clickCreateBtn);
        buttonSave.onClick.AddListener(clickSaveBtn);
        buttonCancelSave.onClick.AddListener(onClickCancelSave);
        buttonMyMat.onClick.AddListener(() => { SceneManager.LoadScene("SceneMyMat"); });

        toggleActor.onValueChanged.AddListener(turnActorMode);
        toggleBkg.onValueChanged.AddListener(turnBkgMode);
        toggleProp.onValueChanged.AddListener(turnPropMode);

        buttonGenAI.onClick.AddListener(() =>
        {
            AIDialog.SetActive(true);
            editMode = EDITMODE.AI;

        });


        //设置默认状态
        //创建角色
        if (SceneMsgToMatCreate.MatType == 1)
        {
            toggleActor.isOn = true;
            matMode = MATMODE.Actor;
            buttonGenAI.gameObject.SetActive(false);
            photoHintDialog.gameObject.SetActive(true);
        }
        //创建物品
        else if (SceneMsgToMatCreate.MatType == 2)
        {
            matMode = MATMODE.Prop;
            toggleProp.isOn = true;
            buttonGenAI.gameObject.SetActive(false);
            photoHintDialog.gameObject.SetActive(false);
        }
        //创建背景
        else
        {
            matMode = MATMODE.Background;
            toggleBkg.isOn = true;
            buttonGenAI.gameObject.SetActive(true);
            photoHintDialog.gameObject.SetActive(false);
        }

        togglePhotoWay.isOn = true;
        editMode = EDITMODE.Photo;
        
        actorDrawManager.drawSpriteGo.SetActive(false);

        RecordMatNameButton.OnPressStateChanged = pressed =>
        {
            RecordMatNameButton.GetComponent<Image>().sprite = pressed ? RecordButtonRecording : RecordButtonIdle;
        };
        RecordMatNameButton.OnRecordFinish = str =>
        {
            inputMatNameFiled.text = str;
        };
    }

    public void CreateActor()
    {
        StartCoroutine(createMat());
    }




    void turnPhotoWay(bool isOn)
    {
        if (isOn)
        {
            editMode = EDITMODE.Photo;
            

            //指定action
            createAction = actorPhotoManager.onClickCreate;
            checkAction = actorPhotoManager.onClickCheck;
            cleanAction = actorPhotoManager.onClickClean;

            //改变canvas
            canvasPhoto.alpha = 1;
            canvasPhoto.interactable = true;
            canvasPhoto.blocksRaycasts = true;

            canvasDraw.alpha = 0;
            canvasDraw.blocksRaycasts = false;
            canvasDraw.interactable = false;

            actorDrawManager.drawSpriteGo.SetActive(false);

            if (matMode == MATMODE.Actor)
            {
                photoHintDialog.SetActive(true);
            }
        }

       
    }

    void turnDrawWay(bool isOn)
    {
        if (isOn)
        {
            editMode = EDITMODE.Draw;           
            //指定action
            createAction = actorDrawManager.onClickCreate;
            checkAction = actorDrawManager.onClickCheck;
            cleanAction = actorDrawManager.onClickClean;

            canvasPhoto.alpha = 0;
            canvasPhoto.interactable = false;
            canvasPhoto.blocksRaycasts = false;

            canvasDraw.alpha = 1;
            canvasDraw.blocksRaycasts = true;
            canvasDraw.interactable = true;
            
            actorDrawManager.drawSpriteGo.SetActive(true);
        }
       
    }

    void turnActorMode(bool isOn)
    {
        if (isOn)
        {
            matMode = MATMODE.Actor;
            buttonGenAI.gameObject.SetActive(false);
            cleanAction.Invoke();

            if (editMode == EDITMODE.Photo)
            {
                photoHintDialog.SetActive(true);
            }
        }
    }

    void turnBkgMode(bool isOn)
    {
        if (isOn)
        {
            matMode = MATMODE.Background;
            buttonGenAI.gameObject.SetActive(true);
            cleanAction.Invoke();
        }
    }

    void turnPropMode(bool isOn)
    {
        if (isOn)
        {
            matMode = MATMODE.Prop;
            buttonGenAI.gameObject.SetActive(false);
            cleanAction.Invoke();
        }
    }




    void clickCreateBtn()
    {
        StartCoroutine(createMat());
    }

    IEnumerator createMat()
    {
        buttonCreateMat.interactable = false;
        AndroidUtils.ShowToast("正在创建素材");
        createAction.Invoke();
        
        if (editMode == EDITMODE.Photo)
        {
            while (!actorPhotoManager.createFinished) yield return null;
            if (actorPhotoManager.testGo != null || matMode==MATMODE.Background)
            {
                //actorPhotoManager.testGo.SetActive(false);
                if (actorPhotoManager.testGo != null)
                {
                    Destroy(actorPhotoManager.testGo);
                }
                
                savePanelAppear();
            }
            
            
           

            actorPhotoManager.createFinished = false;
            
        }
        else
        {
            Drawable drawable =actorDrawManager.drawSpriteGo.GetComponent<Drawable>();
            
            if (drawable.painted)
            {
                yield return new WaitForSeconds(2);
                //actorDrawManager.onClickClean();
                actorDrawManager.drawSpriteGo.SetActive(false);
                Destroy(actorDrawManager.testGo);
                savePanelAppear();
                
                drawable.painted = false;
            }
            else
            {
                actorDrawManager.drawSpriteGo.SetActive(true);
               
            }

            



        }

        buttonCreateMat.interactable = true;
        /*
        if (actorDrawManager.testGo != null)
        {
            actorDrawManager.testGo.SetActive(false);
        }
        if (actorPhotoManager.testGo != null)
        {
            actorPhotoManager.testGo.SetActive(false);
        }
        */

    }

    void clickSaveBtn()
    {
        StartCoroutine(saveMat());
    }


    IEnumerator saveMat()
    {
        string inputMatName = inputMatNameFiled.text;
        bool isLegalFirst = true;
        while (inputMatName == "" || ! TextUtil.IsHanziChar(inputMatName))
        {
            if (isLegalFirst)
            {
                AndroidUtils.ShowToast("请输入合法的素材名称");
                isLegalFirst = false;
            }
            
            yield return null;
        }

        bool isDeplicateFirst = true;
        List<string> allActName= DataMap.privateActorList.GetAllRawName();
        List<string> allObjName = DataMap.privatePropList.GetAllRawName();
        List<string> allBkgName = DataMap.privateBackgroundList.GetAllRawName();

        while (matMode==MATMODE.Actor && allActName.Contains(inputMatName)||
            matMode==MATMODE.Prop && allObjName.Contains(inputMatName)||
            matMode==MATMODE.Background&& allBkgName.Contains(inputMatName))
        {
            if (isDeplicateFirst)
            {
                AndroidUtils.ShowToast("名称重复，请重新输入");
                isDeplicateFirst = false;
            }

            yield return null;
        }


        checkAction.Invoke(inputMatName);
        buttonSave.interactable = false;
        AndroidUtils.ShowToast("正在保存素材...");
        yield return new WaitForSeconds(2);
        
        savePanelDisappear();
        buttonSave.interactable = true;
        inputMatNameFiled.text = "";

        actorDrawManager.drawSpriteGo.SetActive(true);
        //actionPreviewManager.InitPreviewActionJsonFiles(Server.deviceUniqueIdentifier + "_" + inputMatName);
        //actionPreviewManager.ShowActionPreviewPanel();
        

    }

    void savePanelAppear()
    {
        //actorPhotoManager.panelEditMod = ActorPhotoManager.PanelEditMod;
        canvasSave.alpha = 1;
        canvasSave.interactable = true;
        canvasSave.blocksRaycasts = true;

       
    }

    void savePanelDisappear()
    {
        
        canvasSave.alpha = 0;
        canvasSave.interactable = false;
        canvasSave.blocksRaycasts = false;

        
    }

    
    void onClickCancelSave()
    {
        savePanelDisappear();
        cleanAction.Invoke();
    }


}
