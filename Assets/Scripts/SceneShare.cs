using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneShare : MonoBehaviour
{
    public Button backBtn;
    public Image QRCode;
    // Start is called before the first frame update
    void Start()
    {
        backBtn.onClick.AddListener(()=> { SceneManager.LoadScene("SceneMainMenu"); });
        StartCoroutine(LoadQRCode());
    }

    IEnumerator LoadQRCode()
    {
        WWWForm form = new WWWForm();
        form.AddField("message", Server.HOST+"api/subscribe#" + Server.deviceUniqueIdentifier);
        UnityWebRequest www = UnityWebRequest.Post($"{Server.HOST}api/v2/gen_qrcode", form);
        www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
        www.useHttpContinue = false;
        yield return www.SendWebRequest();
        if (www.responseCode != 200)
        {
            Debug.Log("Post failed: " + www.responseCode + " , reason: " + www.error);
        }
        else
        {
            var tex = new Texture2D(1, 1);
            tex.LoadImage(www.downloadHandler.data);
            QRCode.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
