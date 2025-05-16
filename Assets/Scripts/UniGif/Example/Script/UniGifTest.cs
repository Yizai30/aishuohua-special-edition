using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// gif显示测试
/// </summary>
public class UniGifTest : MonoBehaviour
{
    [SerializeField]
    private FilterMode m_filterMode = FilterMode.Point;
    // Textures wrap mode
    [SerializeField]
    private TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
    // Debug log flag
    [SerializeField]
    private bool m_outputDebugLog;

    public static UniGifTest Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        
    }
    //这里不调用则不预解码gif
    public void initGifData()
    {
        AddGifMatSlot();
        DecodeAllGif();
    }

    private void AddGifMatSlot()
    {
        if (DataMap.matGifCollection.matGifLists.Count != 0) return;
        foreach(var privateMat in DataMap.privateActorList.privateActorList)
        {
            string matName = privateMat.Name;
            MatGifList matGifList = new MatGifList(matName);
            DataMap.matGifCollection.AddMatGifList(matGifList);
        }
       
    }

    //private IEnumerator ViewGifCoroutine()
    //{
    //    Debug.Log("start load gif");
    //    yield return StartCoroutine(gifImage.SetGifFromUrlCoroutine(gifUrlInput.text, gifUrlInput.text, false));
    //    m_mutex = false;
    //}

    private void DecodeAllGif()
    {
        if(DataMap.matGifCollection.matGifLists==null ||
            DataMap.matGifCollection.matGifLists.Count == 0)
        {
            print("没有gif素材记录");
        }
        foreach(MatGifList matGifList in DataMap.matGifCollection.matGifLists)
        {
            DecodeAPrivateMatGifData(matGifList.MatName);
        }
    }

    //获取一个素材的所有gif的文件数据，存放到datamap
    private void DecodeAPrivateMatGifData(string matName)
    {
        
        MatGifList thisMatGifList=DataMap.matGifCollection.FindMatGifListByName(matName);
        if (thisMatGifList == null)
        {
            return;
        }
        foreach(string actionName in DataMap.gifMapAsset.GetActionNameList())
        {
            PlayableGif playableGif = new PlayableGif();
            StartCoroutine(SetGifFromUrlCoroutine(Server.getMatCreator(), matName, actionName, playableGif, () => {
                thisMatGifList.MatPlayableGifList.Add(playableGif);
                print("已添加gif素材:" + playableGif.gifMatName);
                }));
            
        }

       
    }





    //读取远程gif文件，形成一个playble类
    public IEnumerator SetGifFromUrlCoroutine(string username, string matName, string actionName, PlayableGif playableGif,Action callback)
    {
        
        string url = Server.HOSTANIM+"animate/getExistedGif/" +
            username + "/" + matName + "/" + actionName;

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("URL is nothing.");
            yield break;
        }

        string path;
        if (url.StartsWith("http"))
        {
            // from WEB
            path = url;
        }
        else
        {
            // from StreamingAssets
            path = Path.Combine("file:///" + Application.streamingAssetsPath, url);
        }

        // Load file
        UnityWebRequest request = UnityWebRequest.Get(path);
        request.SendWebRequest();
        while (!request.isDone)
        {
            //print("request...");
            yield return null;
        }
        //这里要改成验证素材是否正确生成
        int timeout = 100;
        int cur = 0;
        if (!string.IsNullOrEmpty(request.error) || request.downloadHandler.data.Length==0)
        {
            // 请求失败，等待2秒后重新发起请求
            //Debug.LogError("Request error: " + request.error);
            Debug.Log("gif素材正在生成..");
            yield return new WaitForSeconds(2f);
            cur += 2;
            if (cur > timeout)
            {
                Debug.LogError("gif素材生成超时");
                yield break;
            }
            yield return SetGifFromUrlCoroutine(username, matName, actionName, playableGif, callback);
            yield break;
        }



        print("已获取gif文件: "+matName+"_"+actionName);
        yield return new WaitForSeconds(2);
        // Get GIF textures
        yield return StartCoroutine(UniGif.GetTextureListCoroutine(request.downloadHandler.data, (gifTexList, loopCount, width, height) =>
        {
            request.Dispose();
            if (gifTexList != null)
            {
                //m_gifTextureList = gifTexList;
                //this.loopCount = loopCount;
                //this.width = width;
                //this.height = height;

                string gifName = matName + "_" + actionName;
                playableGif.SetPlayableGif(gifName, loopCount, width, height, gifTexList);
                print("已解码gif文件: " + matName + " " + actionName);
                
                callback.Invoke();

            }
            else
            {
                Debug.LogError("Gif texture get error.");
                
            }
        },
        m_filterMode, m_wrapMode, m_outputDebugLog));

    }
}