using Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveUserMat : MonoBehaviour
{
    public ServerPoster serverPoster;

    public UsrDicGenerator usrDicGenerator;

    public void saveBkg(string userName,string name, Texture2D texture2D)
    {
        string matName = userName + "_" + name;
        print("matname -> 1"+matName);
        //本地存放url
        string url = "/private/background/" + matName;
        print("2");
        PrivateBackground privateBackground = new PrivateBackground(matName, name, userName, url);
        print("3");
        ImgUtil.savePngNoBound(matName + ".png", texture2D, url);
        print("4");
        if (DataMap.privateBackgroundList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("已覆盖同名素材");
            print("5");
            DataMap.privateBackgroundList.DelMat(matName);
            print("6");
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Background", "Name", matName));
            print("7");
        }
        DataMap.privateBackgroundList.privateBackgroundList.Add(privateBackground);
        print("8");
        //这里要先添加联网检查
        StartCoroutine(serverPoster.AddElemToDataBase<PrivateBackground>("Huiben_Private_Material", "Background", privateBackground));
        //StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "background", userName, matName));
        print("9");
        DataMap.initData();
        print("10");
        usrDicGenerator.initUserDic();
        print("11");
    }


    public void saveProp(string userName, string name, Texture2D texture2D, Texture2D texture2DRaw)
    {
        string matName = userName + "_" + name;
        //本地存放url
        string url = "/private/prop/" + matName;
        PrivateProp privateProp = new PrivateProp(matName, name, userName, url);
        ImgUtil.savePng(matName + ".png", texture2D, url);
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

        DataMap.initData();
        usrDicGenerator.initUserDic();
    }


    public void saveActor(string userName,string name, Texture2D texture2D, Texture2D texture2DRaw)
    {
        string matName = userName + "_" + name;
        //本地存放url
        string url = "/private/actor/" + matName;
        PrivateActor privateActor = new PrivateActor(matName, name, userName, url, "human");
        ImgUtil.savePng(matName + ".png", texture2D, url);
        ImgUtil.savePngNoBound(matName + "_raw.png", texture2DRaw, url);

        if (DataMap.privateActorList.ContainMat(matName))
        {
            AndroidUtils.ShowToast("已覆盖同名素材");
            DataMap.privateActorList.DelMat(matName);
            StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Actor", "Name", matName));
        }
        DataMap.privateActorList.privateActorList.Add(privateActor);
        //这里要先添加联网检查

        StartCoroutine(serverPoster.AddElemToDataBase("Huiben_Private_Material", "Actor", privateActor));
        // StartCoroutine(serverPoster.UploadMatFile("mat/private",url, matName + ".png"));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + ".png", "actor", userName, matName));
        StartCoroutine(serverPoster.UploadPrivateMatFile(url, matName + "_raw.png", "actor", userName, matName));

        DataMap.initData();
        usrDicGenerator.initUserDic();
    }


}
