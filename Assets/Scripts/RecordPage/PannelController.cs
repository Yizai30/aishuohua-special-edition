using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Interfaces;

public class PannelController : MonoBehaviour
{

    Image bkgImage;
    string bkgName = "";
    private void Start()
    {
        bkgImage = GetComponent<Image>();
        changeBkg("defaultBkg"); 
    }
    public void changeBkg(string bkgName)
    {
        try
        {
            Sprite bkgSprite= Resources.Load<Sprite>("Environment/2D/" + bkgName);
            this.bkgName = bkgName;
            if (bkgImage.sprite.name.Equals(bkgSprite.name)) return;

            bkgImage.sprite = bkgSprite;
            bkgImage.color = new Color(1, 1, 1, 0);
            StartCoroutine(changeBkgGradient());
        }catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }

    public void changeBkgUserMat(PrivateBackground privateBackground)
    {
        try
        {
            if (privateBackground.Name.Equals(this.bkgName)) return;
            this.bkgName = privateBackground.Name;
            Texture2D texture = ImgUtil.LoadTexture(Application.persistentDataPath + "/" + privateBackground.Url + "/" + privateBackground.Name + ".png");
            Sprite bkgSprite = ImgUtil.createSprite(texture);
            
            bkgImage.sprite = bkgSprite;
            bkgImage.color = new Color(1, 1, 1, 0);
            StartCoroutine(changeBkgGradient());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    IEnumerator changeBkgGradient()
    {

        while (bkgImage.color.a != 1)
        {
            var a= bkgImage.color.a;
            bkgImage.color = new Color(1, 1, 1,a+0.005f);
            yield return null;
        }
    }
}
