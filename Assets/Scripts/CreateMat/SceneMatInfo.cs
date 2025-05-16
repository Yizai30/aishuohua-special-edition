using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneMatInfo : MonoBehaviour
{

    Canvas matInfoCanvas;

    public Button cancleBtn, renameBtn, deleteBtn;
    public RecordButton RecordMatNameButton;
    public Sprite RecordButtonRecording, RecordButtonIdle;

    public string selectedRawName="", matType="";

    public InputField reNameField;

    UserMatManager userMatManager;

    UsrDicGenerator dicGenerator;

    public Action closeCallback;


    
    // Start is called before the first frame update
    void Start()
    {
        matInfoCanvas = GetComponent<Canvas>();
        userMatManager = GetComponent<UserMatManager>();
        dicGenerator = GetComponent<UsrDicGenerator>();
        
        cancleBtn.onClick.AddListener(() => { this.gameObject.SetActive(false); });
        renameBtn.onClick.AddListener(onclickRename);
        deleteBtn.onClick.AddListener(onclickDelete);
        RecordMatNameButton.OnPressStateChanged = pressed =>
        {
            RecordMatNameButton.GetComponent<Image>().sprite = pressed ? RecordButtonRecording : RecordButtonIdle;
        };
        RecordMatNameButton.OnRecordFinish = str =>
        {
            reNameField.text = str;
        };
    }

    public void initInfo(string selectRawName,string type)
    {
        this.selectedRawName = selectRawName;
        this.matType = type;

        reNameField.text = selectedRawName;
    }

    void onclickRename()
    {
        print("修改素材");
        string newName = reNameField.text;
        if (newName.Equals(selectedRawName))
        {
            AndroidUtils.ShowToast("名称未修改");
            return;
        }
        if (DataMap.privateActorList.ContainMat(Server.getMatCreator() + "_"+newName)||
            DataMap.privatePropList.ContainMat(Server.getMatCreator() + "_" + newName)||
            DataMap.privateBackgroundList.ContainMat(Server.getMatCreator() + "_" + newName))

        {
            AndroidUtils.ShowToast("名称已经存在");
            return;
        }
        if (!TextUtil.IsHanziChar(newName))
        {
            AndroidUtils.ShowToast("请输入汉字");
            return;
        }
        if (matType == "Actor")
        {
            userMatManager.renameActorMat(Server.getMatCreator() + "_"+selectedRawName, newName);
            AndroidUtils.ShowToast("名称修改成功");
            closeCanvas();
        }
        else if (matType == "Prop")
        {
            userMatManager.renamePropMat(Server.getMatCreator() + "_"+selectedRawName, newName);
            AndroidUtils.ShowToast("名称修改成功");
            closeCanvas();
        }
        else if (matType == "Bkg")
        {
            userMatManager.renameBkgMat(Server.getMatCreator() + "_" + selectedRawName, newName);
            AndroidUtils.ShowToast("名称修改成功");
            closeCanvas();
        }
        else
        {
            return;
        }

    }


    void onclickDelete()
    {
        print("删除素材");
        if (selectedRawName == "") return;
        if (matType == "Actor")
        {
            userMatManager.deleteActorMat( Server.getMatCreator()+"_"+selectedRawName);
            AndroidUtils.ShowToast("素材删除成功");
            closeCanvas();
        }
        else if (matType == "Prop")
        {
            userMatManager.deletePropMat(Server.getMatCreator() + "_"+ selectedRawName);
            AndroidUtils.ShowToast("素材删除成功");
            closeCanvas();
        }
        else if (matType == "Bkg")
        {
            userMatManager.deleteBkgMat(Server.getMatCreator() + "_" + selectedRawName);
            AndroidUtils.ShowToast("素材删除成功");
            closeCanvas();
        }
        else
        {
            return;
        }

    }

    void closeCanvas()
    {
        matInfoCanvas.gameObject.SetActive(false);
        closeCallback.Invoke();

        DataMap.initData();
        
        dicGenerator.initUserDic();
    }
}
