using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class SceneCreateStory : MonoBehaviour
{
    public enum State
    {
        IDLE, RECORDING, PROCESSING, PLAYING, READYGEN
    };
    static public State CurrentState = State.IDLE;
    public Image recordButtonProgressBar;
    public DateTime recordStartTime;
    public Button RecordBtn, RedoBtn, FinishBtn, RolesAndObjsButton;
    public Button ReturnSelectMaterialBtn;
    //public InputField inputFiledCreater, inputFieldStoryName;
    public InputField inputFieldStoryName;
    public Text inputCreatorText;

    public Sprite RecordButtonIdle, RecordButtonRecording, RecordButtonProcessing;
    public MicroPhoneSingleView microPhoneSingleView;
    public Controller Ctl;
    static bool ableToRedo = false;
    public GameObject ConfirmUploadPanel;
    public static bool ShowConfirmUploadPanel = false;
    public Button ButtonCancelUpload, ButtonConfrimUpload;
    public List<string> activeGOs;
    float TotalRecordTime = 180.0f;

    public RecordButton RecordStoryNameButton;

    class RecordHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public SceneCreateStory obj;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CurrentState == State.IDLE)
            {
                Debug.Log("开始录音");
                obj.microPhoneSingleView.StartRecord();
                obj.recordStartTime = DateTime.Now;
                obj.gameObject.SetActive(true);
                CurrentState = State.RECORDING;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (CurrentState == State.RECORDING)
            {
                Debug.Log("结束录音");
                AndroidUtils.ShowToast("录音结束，正在处理……");
                obj.recordButtonProgressBar.gameObject.SetActive(false);
                StartCoroutine(obj.WaitForRecordFinished());
                CurrentState = State.PROCESSING;
            }
        }
    }

    void Start()
    {
        RedoBtn.onClick.AddListener(Redo);
        FinishBtn.onClick.AddListener(Finish);
        ReturnSelectMaterialBtn.onClick.AddListener(ReturnSelectMaterials);
        recordButtonProgressBar.gameObject.SetActive(false);
        RecordBtn.gameObject.AddComponent<RecordHandler>().obj = this;
        recordButtonProgressBar.gameObject.AddComponent<RecordHandler>().obj = this;
        

        ConfirmUploadPanel.SetActive(false);
        ButtonConfrimUpload.interactable = false;
        ButtonCancelUpload.onClick.AddListener(() =>
        {
            Debug.Log("onClick: ButtonCancelUpload");
            if (RecordStoryNameButton.IsRecording)
            {
                AndroidUtils.ShowToast("请先停止录音");
                return;
            }
            ConfirmUploadPanel.SetActive(false);
            Ctl.resetAll();
            CurrentState = State.IDLE;
        });
        ButtonConfrimUpload.onClick.AddListener(() =>
        {
            Debug.Log("onClick: ButtonConfrimUpload");
            if (RecordStoryNameButton.IsRecording)
            {
                AndroidUtils.ShowToast("请先停止录音");
                return;
            }
            Ctl.resetAll();
            CurrentState = State.IDLE;
            Server.name = Ctl.StoryName.text;
            Server.child_id = inputCreatorText.text;
            SceneManager.LoadScene("SceneStoryDetails");
        });

        if (SceneMsgToStoryCreate.uiType == 1)
        {
            ReturnSelectMaterials();
        }
        else
        {
            ReturnTellStory();
        }
        SceneMsgToStoryCreate.uiType = 1;

        RecordStoryNameButton.OnPressStateChanged = pressed =>
        {
            RecordStoryNameButton.GetComponent<Image>().sprite = pressed ? RecordButtonRecording : RecordButtonIdle;
        };
        RecordStoryNameButton.OnRecordFinish = str =>
        {
            inputFieldStoryName.text = str;
        };
    }

    void Update()
    {
        if(inputFieldStoryName.text != "" &&
          //inputFiledCreater.text != "")
          inputCreatorText.text!="")
        {
            ButtonConfrimUpload.interactable = true;
        }

        if (ShowConfirmUploadPanel)
        {
            ConfirmUploadPanel.SetActive(true);
            foreach (var goname in Ctl.interpreter.gos.Keys)
            {
                if (Ctl.interpreter.gos[goname].activeSelf)
                {
                    Ctl.interpreter.gos[goname].SetActive(false);
                }
            }
            ShowConfirmUploadPanel = false;
        }

        if (recordButtonProgressBar.IsActive())
        {
            if (DateTime.Now - recordStartTime < TimeSpan.FromSeconds(TotalRecordTime))
            {
                recordButtonProgressBar.fillAmount = (float)((DateTime.Now - recordStartTime).TotalMilliseconds / TimeSpan.FromSeconds(TotalRecordTime).TotalMilliseconds);
            }
            else
            {
                AndroidUtils.ShowToast("录音时长已达上限，请稍后再继续");
                if (CurrentState == State.RECORDING)
                {
                    AndroidUtils.ShowToast("录音结束，正在处理……");
                    recordButtonProgressBar.gameObject.SetActive(false);
                    StartCoroutine(WaitForRecordFinished());
                    CurrentState = State.PROCESSING;
                }
            }
        }


        switch (CurrentState)
        {
            case State.IDLE:
                RecordBtn.GetComponent<Image>().sprite = RecordButtonIdle;
                RecordBtn.enabled = true;
                RecordBtn.interactable = true;
                FinishBtn.enabled = true;
                FinishBtn.interactable = true;
                RedoBtn.enabled = ableToRedo;
                RedoBtn.interactable = ableToRedo;
                ReturnSelectMaterialBtn.enabled = true;
                ReturnSelectMaterialBtn.interactable = true;
                RolesAndObjsButton.enabled = true;
                RolesAndObjsButton.interactable = true;
                break;
            case State.RECORDING:
                RecordBtn.GetComponent<Image>().sprite = RecordButtonRecording;
                RecordBtn.enabled = true;
                RecordBtn.interactable = true;
                FinishBtn.enabled = false;
                FinishBtn.interactable = false;
                RedoBtn.enabled = false;
                RedoBtn.interactable = false;
                ReturnSelectMaterialBtn.enabled = false;
                ReturnSelectMaterialBtn.interactable = false;
                RolesAndObjsButton.enabled = true;
                RolesAndObjsButton.interactable = true;
                break;
            case State.PROCESSING:
                RecordBtn.GetComponent<Image>().sprite = RecordButtonProcessing;
                RecordBtn.enabled = false;
                RecordBtn.interactable = false;
                FinishBtn.enabled = false;
                FinishBtn.interactable = false;
                RedoBtn.enabled = false;
                RedoBtn.interactable = false;
                ReturnSelectMaterialBtn.enabled = false;
                ReturnSelectMaterialBtn.interactable = false;
                RolesAndObjsButton.enabled = false;
                RolesAndObjsButton.interactable = false;
                break;
            case State.PLAYING:
                RecordBtn.GetComponent<Image>().sprite = RecordButtonIdle;
                RecordBtn.enabled = false;
                RecordBtn.interactable = false;
                FinishBtn.enabled = false;
                FinishBtn.interactable = false;
                RedoBtn.enabled = false;
                RedoBtn.interactable = false;
                ReturnSelectMaterialBtn.enabled = false;
                ReturnSelectMaterialBtn.interactable = false;
                RolesAndObjsButton.enabled = false;
                RolesAndObjsButton.interactable = false;
                break;
            case State.READYGEN:
                RecordBtn.GetComponent<Image>().sprite = RecordButtonProcessing;
                RecordBtn.enabled = false;
                RecordBtn.interactable = false;
                FinishBtn.enabled = true;
                FinishBtn.interactable = true;
                RedoBtn.enabled = false;
                RedoBtn.interactable = false;
                ReturnSelectMaterialBtn.enabled = false;
                ReturnSelectMaterialBtn.interactable = false;
                RolesAndObjsButton.enabled = false;
                RolesAndObjsButton.interactable = false;
                break;
        }
        if (CurrentState == State.PROCESSING && AnimGenerator.Interpreter.isPlayingAudio)
        {
            CurrentState = State.PLAYING;
        }
        if (CurrentState == State.PLAYING && !AnimGenerator.Interpreter.isPlayingAudio)
        {
            CurrentState = State.IDLE;
            ableToRedo = true;
        }
        if (CurrentState == State.PROCESSING && Ctl.readyToGen)
        {
            CurrentState = State.READYGEN;
        }
    }

    public GameObject selectMaterials, createStory;
    void ReturnSelectMaterials()
    {
        Debug.Log("Return to Select Materials");
        selectMaterials.GetComponent<CanvasGroup>().alpha = 1;
        selectMaterials.GetComponent<CanvasGroup>().interactable = true;
        selectMaterials.GetComponent<CanvasGroup>().blocksRaycasts = true;
        selectMaterials.GetComponent<GraphicRaycaster>().enabled = true;
        createStory.GetComponent<CanvasGroup>().alpha = 0;
        createStory.GetComponent<CanvasGroup>().interactable = false;
        createStory.GetComponent<GraphicRaycaster>().enabled = false;
        createStory.GetComponent<CanvasGroup>().blocksRaycasts = false;
        Ctl.resetAll();
    }

    void ReturnTellStory()
    {
        
        selectMaterials.GetComponent<CanvasGroup>().alpha = 0;
        selectMaterials.GetComponent<CanvasGroup>().interactable = false;
        selectMaterials.GetComponent<CanvasGroup>().blocksRaycasts = false;
        selectMaterials.GetComponent<GraphicRaycaster>().enabled = false;
        
        createStory.GetComponent<CanvasGroup>().alpha = 1;
        createStory.GetComponent<CanvasGroup>().interactable = true;
        createStory.GetComponent<GraphicRaycaster>().enabled = true;
        createStory.GetComponent<CanvasGroup>().blocksRaycasts = true;
        Ctl.resetAll();
    }

    IEnumerator WaitForRecordFinished()
    {
        yield return new WaitForSeconds(2);
        microPhoneSingleView.StopRecord();
    }
    

    public void Redo()
    {
        Debug.Log("撤销");
        ableToRedo = false;
        Ctl.redo();
    }

    public void Finish()
    {
        
        Debug.Log("生成");
        Ctl.genVideo();
    }


}
