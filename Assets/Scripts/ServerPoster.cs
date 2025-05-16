using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Interfaces;
using System.Text.RegularExpressions;

public class ServerPoster : MonoBehaviour
{
    //public Text readerHint;
    public static int DEFAULT_TIMEOUT = 10;
    public static int MAX_RETRY_COUNT = 5;

    private int annoStateCode=1;
    private void Start()
    {
        //readerHint.text = "正在处理";
    }
    //2024-7-5新增 existedBkgSettingList;
    //用于在主界面拉取bkgSettingList_gn.json文件
    BkgSettingList existedBkgSettingList;

    #region json相关
    //从服务器获取json设置文件内容
    public IEnumerator FetchJsonData(Action DataCallback)
    {
        float checkTime = Time.time;
        bool firstCon = true;
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {

            if (firstCon)
            {
                AndroidUtils.ShowToast("网络未连接！请连接网络！");
                firstCon = false;
            }

            yield return null;
        }

        string creator = Server.getMatCreator();

        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "convMatMapListChild", "", "", DataMap.convMatMapListChild.map_list));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "actSettingList", "", "", DataMap.actSettingList.actSettingList));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "matSettingList", "", "", DataMap.matSettingList.matSettingList));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "convMatMapList", "", "", DataMap.convMatMapList.map_list));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "convMatMapList", "", "", DataMap.convMatMapListRaw.map_list));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "attrList", "", "", DataMap.attrList.attr_List));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "bkgSettingList", "", "", DataMap.bkgSettingList.bkgSettingList));
        StartCoroutine(getPointFile());
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "pointSettingList", "", "", DataMap.pointSettingList.pointSettingList));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "correctList", "", "", DataMap.correctList.correctList));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "relatedPosList", "", "", DataMap.relatedPosList.related_pos_list));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "diyActList", "", "", DataMap.diyActList.diy_act_list));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "envPointSettingList", "", "", DataMap.envPointSettingList.envPointSettingList));
        StartCoroutine(FindElemFromDataBase("Huiben_Setting", "actSplitList", "", "", DataMap.actSplitList.act_split_list));

      
        StartCoroutine(FindElemFromDataBase("Huiben_Public_Material", "Actor", "", "", DataMap.publicActorList.publicActors));
        StartCoroutine(FindElemFromDataBase("Huiben_Public_Material", "Background", "", "", DataMap.publicBackgroundList.publicBackgrounds));

        StartCoroutine(FindElemFromDataBase("Huiben_Public_Material", "Style", "", "", DataMap.styleList.styles));



        bool firstShow = true;
        while (DataMap.convMatMapListRaw.map_list.Count == 0 ||
            DataMap.convMatMapList.map_list.Count == 0 ||
            DataMap.convMatMapListChild.map_list.Count == 0 ||
            DataMap.actSettingList.actSettingList.Count == 0 ||
            DataMap.matSettingList.matSettingList.Count == 0 ||

            //DataMap.privateActorList.privateActorList.Count==0 ||
            //DataMap.privateBackgroundList.privateBackgroundList.Count==0||
            //DataMap.privatePropList.privatePropList.Count==0||
            DataMap.publicActorList.publicActors.Count == 0 ||
            DataMap.publicBackgroundList.publicBackgrounds.Count == 0)
        {
            float currTime = Time.realtimeSinceStartup;
            if (currTime - checkTime > 5 && firstShow)
            {
                firstShow = false;
                AndroidUtils.ShowToast("网络条件不良，请稍候...");
            }
            yield return null;

        }
        DataCallback.Invoke();
        print("Datamap初始化完成");

    }

    public IEnumerator FetchPrivaeMatJsonData(Action DataCallback)
    {
        float checkTime = Time.time;
        bool firstCon = true;
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {

            if (firstCon)
            {
                AndroidUtils.ShowToast("网络未连接！请连接网络！");
                firstCon = false;
            }

            yield return null;
        }

        string creator = Server.getMatCreator();

     
        yield return StartCoroutine(FindElemFromDataBase("Huiben_Private_Material", "Actor", "Creator", creator, DataMap.privateActorList.privateActorList));
        StartCoroutine(FindElemFromDataBase("Huiben_Private_Material", "Background", "Creator", creator, DataMap.privateBackgroundList.privateBackgroundList));
        StartCoroutine(FindElemFromDataBase("Huiben_Private_Material", "Prop", "Creator", creator, DataMap.privatePropList.privatePropList));

        print("private actor 初始化完成");
        DataCallback.Invoke();
        

    }



    //上传json文件到服务器
    public IEnumerator PostJsonData(string jsonfileName)
    {
        int retryCount = 0;

        string url = Application.streamingAssetsPath + "/data/JsonFiles/" + jsonfileName + ".json";
        string serverUrl = Server.HOST+"api/v2/jsons/" + jsonfileName + ".json";
        string str = File.ReadAllText(url, Encoding.UTF8);
       

        byte[] postData = Encoding.UTF8.GetBytes(str); // 把字符串转换为bype数组
        UnityWebRequest www = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        while (true)
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("上传完毕");
                Debug.Log(www.downloadHandler.text);
                break;
            }

            //请求失败 重试
            if (retryCount < MAX_RETRY_COUNT)
            {
                retryCount++;
                www.Abort();

                //创建新请求
                www = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
                www.timeout = DEFAULT_TIMEOUT;
                www.uploadHandler = new UploadHandlerRaw(postData);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Accept", "application/json");

                //等待一段时间后发送
                yield return new WaitForSeconds(1f);
            }
            else
            {
                //次数最大
                AndroidUtils.ShowToast("网络条件不佳！");
                break;
            }

        }
    }
    #endregion

    #region 素材文件

    public IEnumerator UploadMatFile(string serverDir, string localDir, string fileName)
    {

        string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverFileUrl = Server.HOST + "api/v2/" + serverDir + "/" + fileName;

        byte[] postData = File.ReadAllBytes(localUrl + "/" + fileName);
        UnityWebRequest www = new UnityWebRequest(serverFileUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Upload complete!");
            Debug.Log(www.downloadHandler.text);
        }
    }


    public IEnumerator UploadPrivateMatFile(string localDir, string fileName, string matType, string userName, string matName)
    {
        string localUrl = Application.persistentDataPath + "/" + localDir;
        byte[] postData = File.ReadAllBytes(localUrl + "/" + fileName);


        string serverFileUrl = Server.HOST+"api/v2/PostPrivateMat/" +
            matType + "/" + userName + "/" + matName + "/" + fileName;


        UnityWebRequest www = new UnityWebRequest(serverFileUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("上传私人素材完毕!");
            Debug.Log(www.downloadHandler.text);
        }
    }


    public IEnumerator UploadCutImg(string userName, string localDir, string inputImgName, Texture2D resultTexture2D)
    {

        string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverFileUrl = Server.HOST + "api/v2/uploadCut/" + userName + "/" + inputImgName;

        byte[] postData = File.ReadAllBytes(localUrl + "/" + inputImgName);
        UnityWebRequest www = new UnityWebRequest(serverFileUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = 50;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Upload complete!");
            Debug.Log(www.downloadHandler.text);
            yield return StartCoroutine(getCutImg(resultTexture2D, userName));
        }
    }

    public IEnumerator getCutImg(Texture2D texture2D, string userName)
    {

        //string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverUrl = Server.HOST + "api/v2/getCutImg/" + userName;
        UnityWebRequest www = UnityWebRequest.Get(serverUrl);
        www.timeout = 50;//长一些
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("已获取分割结果");
            byte[] results = www.downloadHandler.data;
            texture2D.LoadImage(results);
            texture2D.Apply();
        }
    }

    



    //将远程用户素材下载到本地
    public IEnumerator DownloadPrivateImg(string typeName,string userName,string matName,string fileName)
    {

        string localUrl = "/private/" + typeName + "/" + matName;
        string serverFileUrl = Server.HOST + "api/v2/getUserMat/" +
            typeName + "/" + userName + "/" + matName + "/" + fileName;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(serverFileUrl);
        www.timeout = DEFAULT_TIMEOUT;

        //UnityWebRequest www = UnityWebRequest.Get(serverFileUrl);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
            ImgUtil.savePngNoBound(fileName, texture2D, localUrl);

        }
    }


    //将远程用户素材下载到本地
    public IEnumerator DownLoadUserImg(string userMatName, string typeName)
    {

        string localUrl = "/private/" + typeName + "/" + userMatName;
        string serverFileUrl = Server.HOST + "api/v2/getUserMat/" + userMatName;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(serverFileUrl);
        www.timeout = DEFAULT_TIMEOUT;

        //UnityWebRequest www = UnityWebRequest.Get(serverFileUrl);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture2D = ((DownloadHandlerTexture)www.downloadHandler).texture;

            ImgUtil.savePngNoBound(userMatName + ".png", texture2D, localUrl);

        }
    }



    #endregion

    #region animate drawing

    //上传待动画化的素材,一步生成
    public IEnumerator UploadAnimateImg(string userName, string matName, string fileName, Texture2D inputTexture)
    {

        //string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverFileUrl = Server.HOSTANIM + "animate/genGif/" +
            userName + "/" + matName + "/" + fileName;

        // byte[] postData = File.ReadAllBytes(localUrl + "/" + fileName);
        byte[] postData = inputTexture.EncodeToPNG();
        UnityWebRequest www = new UnityWebRequest(serverFileUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            // 后台任务已经启动
            Debug.Log("Upload complete!");
            Debug.Log(www.downloadHandler.text);
            //yield return StartCoroutine(getCutImg(resultTexture2D, userName));
        }
    }


    public IEnumerator UploadAnimateImgAndGenAnim(string userName, string matName, string fileName,string actionName, Texture2D inputTexture)
    {
        yield return StartCoroutine(UploadAnimateImgAndGenAnno(userName, matName, fileName, inputTexture));
        yield return new WaitForSeconds(5);
        
        // 每 2 秒检查 annoStateCode 的值
        while (annoStateCode != 0)
        {
            yield return StartCoroutine(getAnnoStateCode(userName, matName, actionName));
            print("AnnoStateCode：" + annoStateCode);
            if (annoStateCode == -1)
            {
                // 如果 annoStateCode 为 -1，显示错误提示并退出协程
                AndroidUtils.ShowToast("无法识别,请选择其他素材！");
                yield break; // 退出协程
            }

            // 等待 2 秒后再次检查
            yield return new WaitForSeconds(2);
        }

        AndroidUtils.ShowToast("已生成标注文件，开始生成动作文件...");
        yield return StartCoroutine(getAnimateGif(userName, matName, actionName));
        //print("已启动生成任务:" + matName + " " + actionName);

        PlayableGif playableGif = new PlayableGif();

        GameObject gifManager = GameObject.Find("GifManager");
        UniGifTest uniGifTest = gifManager.GetComponent<UniGifTest>();

        
        StartCoroutine(uniGifTest.SetGifFromUrlCoroutine(Server.getMatCreator(), matName, actionName, playableGif, () =>
        {
            MatGifList matGifList = DataMap.matGifCollection.FindMatGifListByName(matName);
            if (matGifList == null)
            {
                matGifList = new MatGifList(matName);
            }
            matGifList.AddPlayable(playableGif);
            DataMap.matGifCollection.AddMatGifList(matGifList);
            DataMap.mergeOnePrivateGifMap(matName,actionName);
            //DeleteElemFromDataBase("Huiben_Private_Material", "ActorGif", "MatName", matName);
            //AddElemToDataBase("Huiben_Private_Material", "ActorGif", matGifList);
            print("已添加gif素材:" + playableGif.gifMatName+"动作: "+actionName);
            AndroidUtils.ShowToast("已添加gif素材:" + playableGif.gifMatName + "动作: " + actionName+" 动作生成完毕！");
        }));
    }

     IEnumerator UploadAnimateImgAndGenAnno(string userName, string matName, string fileName, Texture2D inputTexture)
    {

        //string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverFileUrl = Server.HOSTANIM+"animate/img2anno/" +
            userName + "/" + matName + "/" + fileName;

        // byte[] postData = File.ReadAllBytes(localUrl + "/" + fileName);
        byte[] postData = inputTexture.EncodeToPNG();
        UnityWebRequest www = new UnityWebRequest(serverFileUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            // 后台任务已经启动
            Debug.Log("Upload complete!");
            Debug.Log(www.downloadHandler.text);
            yield return new WaitForSeconds(2);
            //yield return StartCoroutine(getCutImg(resultTexture2D, userName));
        }
    }

    //这个任务启动后，间隔时间请求gif，获取到则停止
     IEnumerator getAnimateGif(string userName, string matName, string actionName)
    {

        //string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverUrl =Server.HOSTANIM+"animate/anno2anim/" +
            userName + "/" + matName + "/" + actionName;
        print("test.."+serverUrl);
        UnityWebRequest www = UnityWebRequest.Get(serverUrl);
        www.timeout = 10;//长一些
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("已开始后台任务");
            //byte[] results = www.downloadHandler.data;
            //texture2D.LoadImage(results);
            //texture2D.Apply();
        }
    }


    //获取标注状态
    IEnumerator getAnnoStateCode(string userName, string matName, string actionName)
    {

        //string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverUrl = Server.HOSTANIM + "animate/getAnnoStatus/" +
            userName + "/" + matName + "/" + actionName;
        //print("test:" + serverUrl);
        UnityWebRequest www = UnityWebRequest.Get(serverUrl);
        www.timeout = 10;//长一些
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            print(www.downloadHandler.text);
            string res = www.downloadHandler.text;
            annoStateCode =int.Parse( res.ToString());
            
        }
    }




    #endregion

    #region 数据库
    public IEnumerator AddElemToDataBase<T>(string databaseName, string collectionName, T element)
    {
        string jsonStr = JsonOperator.Obj2jsonStr<T>(element);
        string wrappedJsonStr = wrapInsert(jsonStr);
        string serverUrl = Server.HOST+"api/v2/custom_insert/" + databaseName + "/" + collectionName;

        //UnityWebRequest www = UnityWebRequest.Post(serverUrl, wrappedJsonStr);       

        byte[] postData = Encoding.UTF8.GetBytes(wrappedJsonStr); // 把字符串转换为bype数组
        UnityWebRequest www = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            //AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("数据库更新完毕");
            Debug.Log(www.downloadHandler.text);
        }
    }

    public IEnumerator DeleteElemFromDataBase(string databaseName, string collectionName, string fieldName, string valName)
    {
        //string jsonStr = JsonOperator.Obj2jsonStr<T>(element);
        string wrappedJsonStr = wrapDelete(fieldName, valName);
        string serverUrl = Server.HOST + "api/v2/custom_delete/" + databaseName + "/" + collectionName;

        //UnityWebRequest www = UnityWebRequest.Post(serverUrl, wrappedJsonStr);       

        byte[] postData = Encoding.UTF8.GetBytes(wrappedJsonStr); // 把字符串转换为bype数组
        UnityWebRequest www = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = DEFAULT_TIMEOUT;
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            //AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {
            print("数据删除完毕:" + databaseName + " " + collectionName);
            //Debug.Log(www.downloadHandler.text);
            string rawText = www.downloadHandler.text;
            print(rawText);


        }
    }
   
    
    IEnumerator getPointFile()
    {
        existedBkgSettingList = new BkgSettingList();
        yield return StartCoroutine(FindElemFromDataBase("Huiben_Setting", "bkgSettingList", "", "", existedBkgSettingList.bkgSettingList));
        //新增下面'一行'，生命周期内拉取最新json文件到该文件路径
        string fileName = Application.streamingAssetsPath + "/data/JsonFiles/bkgSettingList_gn.json";
        JsonOperator.Obj2Json<List<BkgSettingElement>>(existedBkgSettingList.bkgSettingList,fileName);
        print("已获取最新的点位信息到本地："+fileName);
        // setPanel(curBkgName);
        // showBkgPoint(curBkgName);
    }

    public IEnumerator FindElemFromDataBase<T>(string databaseName, string collectionName, string fieldName, string valName, List<T> re)
    {
        int retryCount = 0;
        //string jsonStr = JsonOperator.Obj2jsonStr<T>(element);
        string wrappedJsonStr = wrapFind(fieldName, valName);
        string serverUrl = Server.HOST + "api/v2/custom_find/" + databaseName + "/" + collectionName;

        //UnityWebRequest www = UnityWebRequest.Post(serverUrl, wrappedJsonStr);       

        byte[] postData = Encoding.UTF8.GetBytes(wrappedJsonStr); // 把字符串转换为bype数组
        Debug.Log($"请求数据：{wrappedJsonStr}");
        UnityWebRequest www = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
        www.timeout = 50;
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");

        while (true)
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("数据库查询完毕:" + databaseName + " " + collectionName);
                Debug.Log($"响应数据：{www.downloadHandler.text}");
                string rawText = www.downloadHandler.text;

                string wrappedText = wrapResponseJson(rawText);
                List<T> list = JsonOperator.parseObjFormStr<List<T>>(wrappedText);
                foreach (T el in list)
                {
                    re.Add(el);
                }
                break;
            }

            if (retryCount < MAX_RETRY_COUNT)
            {
                retryCount++;
                //print("超时重连" + retryCount);
                www.Abort();

                www = new UnityWebRequest(serverUrl, UnityWebRequest.kHttpVerbPOST);
                www.timeout = 50;
                www.uploadHandler = new UploadHandlerRaw(postData);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Accept", "application/json");
                yield return new WaitForSeconds(1f);
            }
            else
            {
                AndroidUtils.ShowToast("网络连接异常！");
                break;
            }

        }



    }
    #endregion


    #region nlp
    //获取amr结果
    public IEnumerator getAMR(string cutLine, List<AMRGraph> amrList, Action callback)
    {

        //string localUrl = Application.persistentDataPath + "/" + localDir;
        string serverUrl = Server.HOST + "nlp/amr/" + cutLine;
        UnityWebRequest www = UnityWebRequest.Get(serverUrl);
        www.timeout = DEFAULT_TIMEOUT;
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            //AndroidUtils.ShowToast("网络连接异常！");
            Debug.Log(www.error);
        }
        else
        {

            Debug.Log("获取amr结果");
            //Debug.Log(www.downloadHandler.text);
            string rawText = www.downloadHandler.text;

            //string wrappedText = wrapResponseJson(rawText);
            AMRGraph amr = JsonOperator.parseObjFormStr<AMRGraph>(rawText);
            amrList.Add(amr);

            callback.Invoke();

        }
    }
    #endregion


    #region 包装函数
    string wrapInsert(string jsonStr)
    {
        StringBuilder stringBuilder = new StringBuilder("{\"document\":");
        stringBuilder.Append(jsonStr);
        stringBuilder.Append("}");
        return stringBuilder.ToString();
    }

    string wrapDelete(string fieldName, string valName)
    {
        StringBuilder stringBuilder = new StringBuilder("{\"filter\":");
        if (fieldName == "")
        {
            stringBuilder.Append("{}");
        }
        else
        {
            stringBuilder.Append("{\"" + fieldName + "\":\"" + valName + "\"}");

        }
        //"projection": {" id": false} }
        stringBuilder.Append("}");
        return stringBuilder.ToString();
    }
    string wrapFind(string fieldName, string valName)
    {
        StringBuilder stringBuilder = new StringBuilder("{\"query\":");
        if (fieldName == "")
        {
            stringBuilder.Append("{}");
        }
        else
        {
            stringBuilder.Append("{\"" + fieldName + "\":\"" + valName + "\"}");

        }
        //"projection": {" id": false} }
        stringBuilder.Append(",\"projection\": {\"_id\": false} }");
        return stringBuilder.ToString();
    }

    //用外层对象包装，返回对象
    string wrapResponseJson(string highLevelName, string rawJsonStr)
    {
        StringBuilder stringBuilder = new StringBuilder(rawJsonStr);


        StringBuilder stringBuilderNew = new StringBuilder();

        StringBuilder stringBuilderRe = new StringBuilder();
        for (int i = 1; i < stringBuilder.Length - 1; i++)
        {
            if (stringBuilder[i] == '\'')
            {
                stringBuilderNew.Append("\"");
            }
            else
            {
                stringBuilderNew.Append(stringBuilder[i]);
            }
        }

        stringBuilderRe.Append("{\"" + highLevelName + "\":");
        stringBuilderRe.Append(stringBuilderNew);
        stringBuilderRe.Append("}");



        return stringBuilderRe.ToString();
    }
    //直接包装，返回列表
    string wrapResponseJson(string rawJsonStr)
    {
        StringBuilder stringBuilder = new StringBuilder(rawJsonStr);
        StringBuilder stringBuilderNew = new StringBuilder();
        for (int i = 1; i < stringBuilder.Length - 1; i++)
        {
            if (stringBuilder[i] == '\'')
            {
                stringBuilderNew.Append("\"");
            }
            else
            {
                stringBuilderNew.Append(stringBuilder[i]);
            }
        }
        //去掉objectId
        string str = stringBuilderNew.ToString();

        string regexStr = @"""Animation"": \[.*?\]";
        string re = Regex.Replace(str, regexStr, "Animation: []");   // 通过 Regex 类中的 Replace 方法，来进行匹配替换

        return re;
    }


    #endregion




}
