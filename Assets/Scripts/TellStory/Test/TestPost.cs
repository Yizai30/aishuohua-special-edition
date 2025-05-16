using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;

public class TestPost : MonoBehaviour
{
    public ServerPoster serverPoster;
    List<PrivateActor> re = new List<PrivateActor>();
    // Start is called before the first frame update
    void Start()
    {
        serverPoster = GetComponent<ServerPoster>();

        //StartCoroutine(serverPoster.PostJsonData("userMatInfoList"));
        //StartCoroutine(serverPoster.UploadFile(Application.streamingAssetsPath + "/" + "test.zip", "test.zip"));
        PrivateActor privateActor = new PrivateActor("testName", "testRawName", "testUser", "testUrl", "human");
        //StartCoroutine(serverPoster.AddElemToDataBase<PrivateActor>("Huiben_Private_Material","Actor",privateActor));
        
        //StartCoroutine(serverPoster.FindElemFromDataBase<PrivateActor>("Huiben_Private_Material", "Actor", "","",re));
        StartCoroutine(serverPoster.DeleteElemFromDataBase("Huiben_Private_Material", "Actor", "Name", "_ะฆมห"));
    }

    // Update is called once per frame
    void Update()
    {
        //if (re != null)
        //{
        //    print(re.Count);
        //}
    }
}
