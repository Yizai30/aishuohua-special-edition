using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneReport : MonoBehaviour
{
	// Start is called before the first frame update
	public Button ButtonReturn;
	public Button ButtonCancel;
	public Button ButtonUpload;
	public Button AddScreenShot1;
	public Button AddScreenShot2;
	public Button AddScreenShot3;
	public SpriteRenderer Template;
	private String path1 = "";
	private String path2 = "";
	private String path3 = "";
	public InputField field;
	void Start()
    {
		Screen.SetResolution(1280, 800, true, 60);
		ButtonReturn.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("SceneMainMenu");
		});
		ButtonCancel.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("SceneMainMenu");
		});
		ButtonUpload.onClick.AddListener(() =>
		{
			StartCoroutine(SendToServer());
		});
		AddScreenShot1.onClick.AddListener(() =>
		{
			PickImage(AddScreenShot1, (path)=>
			{
				path1 = path;
			}, ()=>
			{
				path1 = "";
			}
			);
		});
		AddScreenShot2.onClick.AddListener(() =>
		{
			PickImage(AddScreenShot2, (path) =>
			{
				path2 = path;
			}, () =>
			{
				path2 = "";
			}
);
		});
		AddScreenShot3.onClick.AddListener(() =>
		{
			PickImage(AddScreenShot3, (path)=>
			{
				path3 = path;
			}, ()=>
			{
				path3 = "";
			}
			);
		});
	}

	public IEnumerator SendToServer()
	{
		WWWForm form = new WWWForm();
		List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
		form.AddField("reportText", field.text);
		if (path1.Length > 0)
        {
			form.AddBinaryData("pic1", System.IO.File.ReadAllBytes(path1), path1);
		}
		if (path2.Length > 0)
		{
			form.AddBinaryData("pic2", System.IO.File.ReadAllBytes(path2), path2);
		}
		if (path3.Length > 0)
		{
			form.AddBinaryData("pic3", System.IO.File.ReadAllBytes(path3), path3);
		}
		UnityWebRequest www = UnityWebRequest.Post(Server.HOST+"api/report/", form);
		www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
		yield return www.SendWebRequest();
		if (www.responseCode != 200)
		{
			Debug.Log(www.error);
			AndroidUtils.ShowToast("提交失败");
		}
		else
		{
			AndroidUtils.ShowToast("已成功提交");
			Debug.Log("Report Success! ");
		}
		SceneManager.LoadScene("SceneMainMenu");
	}

	void PickImage(Button button, Action<string> callbackOnLoaded, Action callbackOnRemoved)
    {
		string filePath = "";
		NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
		{
			Debug.Log("Image path: " + path);
			if (path != null)
			{
				// Create Texture from selected
				Texture2D tex = NativeGallery.LoadImageAtPath(path);
				filePath = path;
				// tex.GetRawTextureData();
				if (tex == null)
				{
					Debug.Log("Couldn't load texture from " + path);
					return;
				}
				var originalSprite = button.GetComponent<Image>().sprite;
				button.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
				var closeButton = button.GetComponentsInChildren<Button>(true)[1];
				Debug.Log("closeButton:" + closeButton.name);
				closeButton.gameObject.SetActive(true);
				closeButton.onClick.AddListener(() =>
				{
					button.GetComponent<Image>().sprite = Template.sprite;
					closeButton.gameObject.SetActive(false);
					button.onClick.AddListener(() =>
					{
						PickImage(button, callbackOnLoaded, callbackOnRemoved);
					});
					callbackOnRemoved();
				});
				button.onClick.RemoveAllListeners();
			}
		});
		callbackOnLoaded(filePath);
		Debug.Log("Permission result: " + permission);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
