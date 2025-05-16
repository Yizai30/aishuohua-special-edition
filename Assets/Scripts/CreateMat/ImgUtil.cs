using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImgUtil : MonoBehaviour
{
  

    public static Sprite createSprite(Texture2D texture)
    {
        Sprite sprite = Sprite.Create(texture, new UnityEngine.Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0), 100.0f);
        return sprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
        Texture2D Tex2D;
        byte[] FileData;
        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    //确定边界
    public static Texture2D getBound(Texture2D texture2D)
    {
        Color[] colors = new Color[texture2D.width * texture2D.height];
        Color[] txtColors = texture2D.GetPixels();
        for (int i = 0; i < txtColors.Length; i++)
        {
            if (txtColors[i].r == 1 && txtColors[i].g == 1 && txtColors[i].b == 1)
            {
                txtColors[i] = new Color(0, 0, 0, 0f);
            }
        }
        //改成透明后的纹理
        Texture2D result = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, false);
        result.SetPixels(txtColors);
        result.Apply();

        // byte[] byt=result.EncodeToPNG();
        // string path = @"E:\Unity Project\proj1\Assets\Sprites";
        // string savePath = path + "/" + Time.time + ".png";
        // File.WriteAllBytes(savePath, byt);

        var left = 0;
        var top = 0;
        var right = result.width;
        var botton = result.height;

        // 左侧
        for (var i = 0; i < result.width; i++)
        {
            var find = false;
            for (var j = 0; j < result.height; j++)
            {
                var color = result.GetPixel(i, j);
                if (Mathf.Abs(color.a) > 1e-6)
                {
                    find = true;
                    break;
                }
            }
            if (find)
            {
                left = i;
                break;
            }
        }

        // 右侧
        for (var i = result.width - 1; i >= 0; i--)
        {
            var find = false;
            for (var j = 0; j < result.height; j++)
            {
                var color = result.GetPixel(i, j);
                if (Mathf.Abs(color.a) > 1e-6)
                {
                    find = true;
                    break;
                }
            }
            if (find)
            {
                right = i;
                break;
            }
        }

        // 上侧
        for (var j = 0; j < result.height; j++)
        {
            var find = false;
            for (var i = 0; i < result.width; i++)
            {
                var color = result.GetPixel(i, j);
                if (Mathf.Abs(color.a) > 1e-6)
                {
                    find = true;
                    break;
                }
            }
            if (find)
            {
                top = j;
                break;
            }
        }

        //下侧
        for (var j = result.height - 1; j >= 0; j--)
        {
            var find = false;
            for (var i = 0; i < result.width; i++)
            {
                var color = result.GetPixel(i, j);
                if (Mathf.Abs(color.a) > 1e-6)
                {
                    find = true;
                    break;
                }
            }
            if (find)
            {
                botton = j;
                break;
            }
        }

        // 创建新纹理
        var width = right - left + 1;
        var height = botton - top + 1;

        var newresult = new Texture2D(width, height, TextureFormat.RGBA32, false);


        //use var texture = new Texture2D(ancho, altoLista, TextureFormat.Alpha8, true);

        
        //newresult.alphaIsTransparency = true;

        // 复制有效颜色区块
        var colours = texture2D.GetPixels(left, top, width, height);        //内部不透明
        //var colours = result.GetPixels(left, top, width, height);         //内部透明
        newresult.SetPixels(0, 0, width, height, colours);
        newresult.Apply();

        return newresult;
    }


    public static void addBkgColor(Texture2D texture )
    {
        var left = 0;
        var top = 0;
        var right = texture.width;
        var bottom = texture.height;

        for(int i = 0; i < bottom; i++)
        {
            for(int j = 0; j < right; j++)
            {
                if (texture.GetPixel(i, j).a == 0)
                {
                    texture.SetPixel(i, j, Color.white);
                }
            }
        }
    }

    public static void savePng(string matName, Texture2D texture, string url)
    {
        Texture2D boundedText = ImgUtil.getBound(texture);
        byte[] bytes = boundedText.EncodeToPNG();

        var dirPath = Application.persistentDataPath + "/" + url;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "/" + matName, bytes);
    }

    public static void savePngNoBound(string matName, Texture2D texture, string url)
    {

        byte[] bytes = texture.EncodeToPNG();

        var dirPath = Application.persistentDataPath + "/" + url;
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "/" + matName, bytes);
    }

    public static void PickImage(int maxSize,Image image)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                

                // Create Texture from selected image
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize);
                Texture2D textureCopy = ImgUtil.duplicateTexture(texture);

                if (Path.GetExtension(path).ToLower() == ".png")
                {
                    // 如果是 PNG，转换为 JPG
                    var fileContent = textureCopy.EncodeToJPG();
                    textureCopy.LoadImage(fileContent);
                    textureCopy.Apply();
                }

                Sprite rawSprite = ImgUtil.createSprite(textureCopy);
                image.sprite = rawSprite;

                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

              
               
            }
        });

        Debug.Log("Permission result: " + permission);
    }


    public static Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableText = new Texture2D(source.width, source.height, TextureFormat.RGB24, false);
        readableText.ReadPixels(new UnityEngine.Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }


}
