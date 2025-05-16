using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class UIController : MonoBehaviour
{
    Transporter transporter = new Transporter();

    public Button actUpBtn, actFileBtn, animUpBtn, animFileBtn, linkBtn, linkAutoBtn;
    public Text actHint, animHint, linkHint;
    public InputField actFileInput, actTypeInput, animFileInput, animTypeInput,
        actLinkInput, animLinkInput, actPackageInput, animPackageInput;
    // Start is called before the first frame update
    void Start()
    {
        //test
        /*
        transporter.actorNameList.Add("cuteRabbit");
        transporter.animNameList.Add("cuteAnimal_TypeA_Attack");
        transporter.animNameList.Add("cuteAnimal_TypeA_Damage");
        transporter.animNameList.Add("cuteAnimal_TypeA_Die");
        transporter.animNameList.Add("cuteAnimal_TypeA_Eat");
        transporter.animNameList.Add("cuteAnimal_TypeA_Idle");
        transporter.animNameList.Add("cuteAnimal_TypeA_Jump");
        transporter.animNameList.Add("cuteAnimal_TypeA_Rest");
        transporter.animNameList.Add("cuteAnimal_TypeA_Run");
        transporter.animNameList.Add("cuteAnimal_TypeA_Walk");
        */


        actUpBtn.interactable = false;
        animUpBtn.interactable = false;
        linkBtn.interactable = false;
        
        actFileBtn.onClick.AddListener(getActorDir);
        animFileBtn.onClick.AddListener(getAnimDir);

        actUpBtn.onClick.AddListener(uploadActor);
        animUpBtn.onClick.AddListener(uploadAnim);
        linkBtn.onClick.AddListener(link);
        linkAutoBtn.onClick.AddListener(showLinkObj);
    }

    void uploadActor()
    {
        try
        {
            transporter.UploadActor();
        } 
          catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    void uploadAnim()
    {
        try
        {
            transporter.UploadAnimation();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
    }

    void link()
    {
        try
        {
            transporter.actorNameList = getLinkedObj(actLinkInput);
            transporter.animNameList = getLinkedObj(animLinkInput);

            transporter.LinkActorAndAnimation();

        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
    }

    void getActorDir()
    {
        string path = "";
#if UNITY_EDITOR
        //some code here that uses something from the UnityEditor namespace
        path = EditorUtility.OpenFolderPanel("选择角色文件", "", "");
#endif
        transporter.actorLocalUrl = path;
        actFileInput.text = path;
        Debug.Log(path);
    }

    void getAnimDir()
    {
        string path = "";
#if UNITY_EDITOR
        //some code here that uses something from the UnityEditor namespace
        path = EditorUtility.OpenFolderPanel("选择动画文件", "", "");
#endif
        transporter.animLocalUrl = path;
        animFileInput.text = path;
        Debug.Log(path);
    }

    void showLinkObj()
    {
        setInputByList(animLinkInput, transporter.animNameList);
        setInputByList(actLinkInput, transporter.actorNameList);
    }

    void setInputByList(InputField field,List<string> ls)
    {
        if (ls.Count == 0)
        {
            field.text = "";
            return;
        }
        string str = "";
        foreach(var e in ls)
        {
            str = str + " "+e;
        }
        field.text = str;
    }

    //从field中读取actor列表
    List<string> getLinkedObj(InputField field)
    {
        string[] ls = field.text.Split(' ');
        List<string> lss = new List<string>();
        foreach(string e in ls)
        {
            if(e!=" " && e != "")
            {
                lss.Add(e);
            }
        }
        return lss;
    }

    bool canLink()
    {
        return (actLinkInput.text != "" && animLinkInput.text != "");
    }

    // Update is called once per frame
    void Update()
    {

              
        //读取inputfield的内容到transporter
        transporter.actorType = actTypeInput.text;
        transporter.actorLocalUrl = actFileInput.text;
        transporter.actorPackageName = actPackageInput.text;

        transporter.animType = animTypeInput.text;
        transporter.animLocalUrl = animFileInput.text;
        transporter.animPackageName = animPackageInput.text;

        //判断当前能否操作
        actUpBtn.interactable = transporter.canUploadActor();
        animUpBtn.interactable = transporter.canUploadAnime();
        linkBtn.interactable = canLink();


    }
}
