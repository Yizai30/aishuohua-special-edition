using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamUtil : MonoBehaviour
{

    public void OnClickTakePhoto()
    {
        //TakePicture(512);
    }
    public void TakePicture(int maxSize, Image image)
    {
        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create a Texture2D from the captured image
                Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
                
                Texture2D textureCopy = ImgUtil.duplicateTexture(texture);
                Sprite rawSprite = ImgUtil.createSprite(textureCopy);
                image.sprite = rawSprite;
               
                //Debug.Log(texture);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

               
            }
        }, maxSize);

        Debug.Log("Permission result: " + permission);
    }

  
}
