using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CamManager : MonoBehaviour
{

    public string deviceName;
    WebCamTexture tex;


    // Start is called before the first frame update
    void Start()
    {
       
        // StartCoroutine(StartCamera());
        //photoBtn.gameObject.SetActive(true);
        //photoBtn.onClick.AddListener(AfterPhoto);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator StartCamera()
    {
        PhotoInfo.texture2D = null;
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            deviceName = devices[0].name;
            tex = new WebCamTexture(deviceName, Screen.width, Screen.height - 100, 60);
            tex.Play();
        }
    }

    public void AfterPhoto()
    {
        getTexture();
        tex.Stop();
        //photoBtn.gameObject.SetActive(false);
        //SceneManager.LoadScene("SceneCreateActor");
    }

    public void getTexture()
    {
        //yield return new WaitForEndOfFrame();
        Texture2D t = new Texture2D(Screen.width, Screen.height - 100);
        t.ReadPixels(new Rect(0, 100, Screen.width, Screen.height - 100), 0, 0, false);
        t.Apply();
        PhotoInfo.texture2D = t;

    }

}

public class PhotoInfo
{
   public static  Texture2D texture2D;
}





