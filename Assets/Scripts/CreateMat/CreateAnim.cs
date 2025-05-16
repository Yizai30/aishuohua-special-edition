using Interfaces;
using SharpCompress.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateAnim : MonoBehaviour
{

    public Button genAnimButton;
    public InputField inputField;
    public Dropdown gifDropdown;

    public ServerPoster serverPoster;

    private GifMapAsset gifMapAsset;

    private int curSelectedIndex;

    private void Awake()
    {
        serverPoster = GetComponent<ServerPoster>();


        genAnimButton.onClick.AddListener(genAnim);
        gifDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    // Start is called before the first frame update
    void Start()
    {
        gifMapAsset = DataMap.gifMapAsset;
        initGifDropDown();
    }

    private void initGifDropDown()
    {
        foreach(string rawActName in gifMapAsset.GetActionRawNameList())
        {
            gifDropdown.options.Add(new Dropdown.OptionData(rawActName));
           
        }
    }

    // 当选项发生更改时调用的方法
    private void OnDropdownValueChanged(int selectedIndex)
    {
        curSelectedIndex = selectedIndex;
    }

    void genAnimCallback()
    {

    }

    void genAnim()
    {
        string matName =Server.getMatCreator()+"_"+ inputField.text;

        if(gifMapAsset!=null && gifMapAsset.GifMapList.Count > curSelectedIndex)
        {
            string actName = gifMapAsset.GifMapList[curSelectedIndex].actName;
            StartCoroutine(StartUploadCoroutine(matName, actName, genAnimCallback));
        }
        else
        {
            Debug.LogWarning("gifasset获取错误");
        }
       
    }

    //根据matName获取

    public IEnumerator StartUploadCoroutine(string matName, string gifActName,Action callback)
    {
        PrivateActor privateActor=DataMap.privateActorList.FindPrivateActorByMatName(matName);
        
        if (privateActor == null) {
            print("未找到 "+matName);
            AndroidUtils.ShowToast("未找到" + matName);
            yield break;
        }
        string userName = privateActor.Creator;
        string fileName = matName + "_raw.png";
        string localurl = Application.persistentDataPath + "/" + privateActor.Url+"/"+fileName;
        Texture2D texture2D = new Texture2D(200, 200, TextureFormat.RGB24, false);
        byte[] fileData = System.IO.File.ReadAllBytes(localurl);
        texture2D.LoadImage(fileData);
        //yield return StartCoroutine(LoadTextureFromPersistentUrl(localurl,  texture2D));

        print("已获取本地图片："+ localurl);
        AndroidUtils.ShowToast("已获取本地图片：" + localurl);
        yield return StartCoroutine(serverPoster.UploadAnimateImgAndGenAnim(userName, matName, fileName,gifActName, texture2D));
        //添加
        
        //print("已启动动画生成后台任务");
        //AndroidUtils.ShowToast("已启动动画生成后台任务，请等待动作生成。。。");
        callback.Invoke();
    }


    IEnumerator LoadTextureFromPersistentUrl(string url, Texture2D texture)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {

            //texture = DownloadHandlerTexture.GetContent(www);

            byte[] results = www.downloadHandler.data;
            texture.LoadImage(results);
            texture.Apply();
            // 在这里使用加载的纹理
            // 例如，将纹理应用于材质或渲染器
        }
        else
        {
            Debug.LogError("Failed to load texture: " + www.error);
        }
    }
}
