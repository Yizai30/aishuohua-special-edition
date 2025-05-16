using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//由MatGifList组成的列表
public class MatGifCollection
{
    public List<MatGifList> matGifLists;
    public MatGifCollection()
    {
        matGifLists = new List<MatGifList>();
    }

    public MatGifCollection(List<MatGifList> matGifLists)
    {
        this.matGifLists = matGifLists;
    }

    public MatGifList FindMatGifListByName(string matName)
    {
        foreach (var matGifList in matGifLists)
        {
            if (matGifList.MatName == matName)
            {
                return matGifList;
            }
        }
        return null;
    }

    

    public void AddMatGifList(MatGifList matGifList)
    {
        matGifLists.Add(matGifList);
    }

    //添加一个gifplayable对象
    public void AddPlayable(string matName,PlayableGif playableGif)
    {
        //用户名_素材名
        MatGifList matGifList = FindMatGifListByName(matName);
        if (matGifList == null)
        {
            MatGifList matGifListNew = new MatGifList(matName);
            matGifListNew.MatPlayableGifList.Add(playableGif);
             
            DataMap.matGifCollection.AddMatGifList(matGifListNew);

        }
        else
        {
            if( matGifList.FindPlayableGifByGifName(playableGif.gifMatName)==null)
            {
                matGifList.AddPlayable(playableGif);
            }
        }

    }
    //
    public PlayableGif FindPlayableGif(string matName,string gifName)
    {
        MatGifList matGifList = FindMatGifListByName(matName);
        if (matGifList != null)
        {
            PlayableGif playableGif = matGifList.FindPlayableGifByGifName(gifName);
            return playableGif;
        }
        return null;
    }

    public PlayableGif FindPlayableGif(string gifName)
    {
        foreach(MatGifList matGifList in matGifLists)
        {
            foreach(PlayableGif playableGif in matGifList.MatPlayableGifList)
            {
                if(playableGif.gifMatName == gifName)
                {
                    return playableGif;
                }
            }
        }
        return null;
    }

}
//某个mat的所有可播放对象
[Serializable]
public class MatGifList
{
    public string MatName;
    public List<PlayableGif> MatPlayableGifList;

    public MatGifList(string matName)
    {
        MatName = matName;
        MatPlayableGifList= new List<PlayableGif>();
    }

    public PlayableGif FindPlayableGifByGifName(string gifName)
    {
        foreach(PlayableGif playableGif in MatPlayableGifList)
        {
            if(playableGif.gifMatName == gifName)
            {
                return playableGif;
            }
        }
        return null;
    }

    public void AddPlayable(PlayableGif playableGif)
    {
        if (this.FindPlayableGifByGifName(playableGif.gifMatName) == null)
        {
            this.MatPlayableGifList.Add(playableGif);
        }
    }

}

[Serializable]
public class PlayableGif
{
    public PlayableGif(string gifMatName, int loopCount, int width, int height, List<UniGif.GifTexture> gifTextureList)
    {
        this.gifMatName = gifMatName;
        this.loopCount = loopCount;
        this.width = width;
        this.height = height;
        this.gifTextureList = gifTextureList;
    }
    public PlayableGif() { 
    }

    public void SetPlayableGif(string gifMatName, int loopCount, int width, int height, List<UniGif.GifTexture> gifTextureList)
    {
        this.gifMatName = gifMatName;
        this.loopCount = loopCount;
        this.width = width;
        this.height = height;
        this.gifTextureList = gifTextureList;
    }

    public string gifMatName
    {
        get;
        private set;
    }

    /// <summary>
    /// Animation loop count (0 is infinite)
    /// </summary>
    public int loopCount
    {
        get;
        private set;
    }

    /// <summary>
    /// Texture width (px)
    /// </summary>
    public int width
    {
        get;
        private set;
    }

    /// <summary>
    /// Texture height (px)
    /// </summary>
    public int height
    {
        get;
        private set;
    }

    public List<UniGif.GifTexture> gifTextureList
    {
        get;
        private set;
    }


}
