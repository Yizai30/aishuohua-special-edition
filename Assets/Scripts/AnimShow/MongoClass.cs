using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace MongoClass
{
    /// <summary>
    /// Actor类
    /// </summary>
    public class Actor_info
    {
        /// <summary>
        /// 系统自带_id
        /// </summary>
        public ObjectId _id { get; set; }
        public string Url { get; set; }
        public string ZipUrl { get; set; }
        public string Type { get; set; }
        public ObjectId[] Animation { get; set; }
        public string[][] Clothes { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Animation类
    /// </summary>
    public class Animation_info
    {
        /// <summary>
        /// 系统自带_id
        /// </summary>
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public ObjectId[] ActorList { get; set; }
        //public double Length { get; set; }
        public string Url { get; set; }
        public string ZipUrl { get; set; }
    }

}

namespace Interfaces
{
    [Serializable]
    public class PublicActor
    {
        public string Url;
        public string ZipUrl;
        public string Type;
        public List<string> Animation;
        public string Name;
        public string Style;
        public string ClassId;
        public static List<PublicActor> FetchByName(string name)
        {
            WWW reader = new WWW($"{Server.HOST}api/v2/public/actor/" + name);
            Debug.Log("GET " + $"{Server.HOST}api/v2/public/actor/" + name);
            float startTime = Time.realtimeSinceStartup;
            while (!reader.isDone && Time.realtimeSinceStartup - startTime < 2) { }
            if (!reader.isDone)
            {
                AndroidUtils.ShowToast("网络错误，请退出重试！");
                return new List<PublicActor>();
            } else
            {
                //var results = JsonUtility.FromJson<PublicActorList>(reader.text);
                string str = reader.text.Substring(11, reader.text.Length - 11);
                str = str.Substring(0, str.Length - 1);
                var results = JsonOperator.parseObjFormStr<List<PublicActor>>(str);
                //Debug.Log(reader.text);
                return results;
            }
        }

     
    }
    [Serializable]
    public class PublicActorList
    {
        //public PublicActor[] publicActors;
        public List<PublicActor> publicActors;
        public PublicActorList()
        {
            publicActors = new List<PublicActor>();
        }

        public List<PublicActor> GetElemInStyle(string styleNum)
        {
            List<PublicActor> re = new List<PublicActor>();
            foreach(PublicActor publicActor in publicActors)
            {
                if (publicActor.Style.Equals(styleNum))
                {
                    re.Add(publicActor);
                }
            }
            return re;
        }

    }

    [Serializable]
    public class PublicBackground
    {
        public string Name;
        public string Style;
        public string Url;



    }

    [Serializable]
    public class PublicBackgroundList
    {
        public List<PublicBackground> publicBackgrounds;
        public PublicBackgroundList()
        {
            publicBackgrounds = new List<PublicBackground>();
        }

        public List<PublicBackground> GetElemInStyle(string styleNum)
        {
            List<PublicBackground> re = new List<PublicBackground>();
            foreach (PublicBackground publicBkg in publicBackgrounds)
            {
                if (publicBkg.Style!= null && publicBkg.Style.Equals(styleNum))
                {
                    re.Add(publicBkg);
                }
            }
            return re;
        }
    }

    [Serializable]
    public class PublicAnimation
    {
        public string Name;
        public string Type;
        public List<string> ActorList;
        public string Url;
        public string ZipUrl;
        public static List<PublicAnimation> FetchById(string objId)
        {
            WWW reader = new WWW($"{Server.HOST}api/v2/public/animation/" + objId);
            //Debug.Log("GET " + $"{Server.HOST}api/data/animation/" + objId);
            while (!reader.isDone) { }

            string str = reader.text.Substring(11, reader.text.Length - 11);
            str = str.Substring(0, str.Length - 1);
            var results = JsonOperator.parseObjFormStr<List<PublicAnimation>>(str);
            //var results = JsonUtility.FromJson<PublicAnimationList>(reader.text);
            //Debug.Log(reader.text);
            return results ;
        }
    }
    [Serializable]
    public class PublicAnimationList
    {
        public List<PublicAnimation> publicAnimations;

        public PublicAnimationList()
        {
            publicAnimations = new List<PublicAnimation>();
        }
    }
    
    [Serializable]
    public class Style
    {
        public string StyleId;
        public string StyleName;
    }

    public class StyleList
    {
        public List<Style> styles;

        public StyleList()
        {
            styles = new List<Style>();
        }
    }


    [Serializable]
    public class PrivateActor
    {
        public string Name;
        public string RawName;
        public string Creator;
        public string Url;
        public string Type;

        public PrivateActor(string name, string rawName, string creator, string url, string type)
        {
            Name = name;
            RawName = rawName;
            Creator = creator;
            Url = url;
            Type = type;
        }
    }

    public class PrivateActorList
    {
        public PrivateActorList()
        {
            this.privateActorList = new List<PrivateActor>();
        }
       public List<PrivateActor> privateActorList { set; get; }

        public PrivateActor FindPrivateActorByMatName(string matName)
        {
            foreach(PrivateActor privateActor in privateActorList)
            {
                if (privateActor.Name == matName)
                {
                    return privateActor;
                }
            }
            return null;
        }

        public List<string> GetAllRawName()
        {
            List<string> re = new List<string>();
            foreach (PrivateActor privateActor in privateActorList)
            {
                re.Add(privateActor.RawName);
            }
            return re;
        }

        public bool ContainMat(string matName)
        {
            foreach(PrivateActor privateActor in privateActorList)
            {
                if (privateActor.Name.Equals(matName))
                {
                    return true;
                }
            }
            return false;
        }

        public int DelMat(string matName)
        {
            int delIndex = 0;
            PrivateActor temp= null;
            foreach (PrivateActor privateActor in privateActorList)
            {
                if (privateActor.Name.Equals(matName))
                {
                    temp = privateActor;
                }
                delIndex++;
                
            }
            if (temp != null)
            {
                privateActorList.Remove(temp);
                return delIndex;
            }
            else
            {
                return -1;
            }
            
        }

    }

    [Serializable]
    public class PrivateProp
    {
        public string Name;
        public string RawName;
        public string Creator;
        public string Url;

        public PrivateProp(string name, string rawName, string creator, string url)
        {
            Name = name;
            RawName = rawName;
            Creator = creator;
            Url = url;
        }



    }

    [Serializable]
    public class PrivatePropList
    {
        public PrivatePropList()
        {
            this.privatePropList = new List<PrivateProp>();
        }

        public List<PrivateProp> privatePropList { set; get; }

        public PrivateProp FindPrivatePropByMatName(string matName)
        {
            foreach (PrivateProp privateProp in privatePropList)
            {
                if (privateProp.Name == matName)
                {
                    return privateProp;
                }
            }
            return null;
        }

        public List<string> GetAllRawName()
        {
            List<string> re = new List<string>();
            foreach (PrivateProp privateProp in privatePropList)
            {
                re.Add(privateProp.RawName);
            }
            return re;
        }

        public bool ContainMat(string matName)
        {
            foreach (PrivateProp privateProp in privatePropList)
            {
                if (privateProp.Name.Equals(matName))
                {
                    return true;
                }
            }
            return false;
        }

        public int DelMat(string matName)
        {
            int delIndex = 0;
            PrivateProp temp = null;
            foreach (PrivateProp privateProp in privatePropList)
            {
                if (privateProp.Name.Equals(matName))
                {
                    temp = privateProp;
                }
                delIndex++;

            }
            if (temp != null)
            {
                privatePropList.Remove(temp);
                return delIndex;
            }
            else
            {
                return -1;
            }

        }
    }

    [Serializable]
    public class PrivateBackground
    {
        public string Name;
        public string RawName;
        public string Creator;
        public string Url;

        public PrivateBackground(string name, string rawName, string creator, string url)
        {
            Name = name;
            RawName = rawName;
            Creator = creator;
            Url = url;
        }
    }

    [Serializable]
    public class PrivateBackgroundList
    {
        public PrivateBackgroundList()
        {
            this.privateBackgroundList = new List<PrivateBackground>();
        }

        public List<PrivateBackground> privateBackgroundList { set; get; }


        public PrivateBackground FindPrivateBkgByMatName(string matName)
        {
            foreach (PrivateBackground privateBackground in privateBackgroundList)
            {
                if (privateBackground.Name == matName)
                {
                    return privateBackground;
                }
            }
            return null;
        }

        public List<string> GetAllRawName()
        {
            List<string> re = new List<string>();
            foreach (PrivateBackground privateBackground in privateBackgroundList)
            {
                re.Add(privateBackground.RawName);
            }
            return re;
        }

        public List<string> GetAllName()
        {
            List<string> re = new List<string>();
            foreach (PrivateBackground privateBackground in privateBackgroundList)
            {
                re.Add(privateBackground.Name);
            }
            return re;
        }

        public bool ContainMat(string matName)
        {
            foreach (PrivateBackground privateBackground in privateBackgroundList)
            {
                if (privateBackground.Name.Equals(matName))
                {
                    return true;
                }
            }
            return false;
        }

        public int DelMat(string matName)
        {
            int delIndex = 0;
            PrivateBackground temp = null;
            foreach (PrivateBackground privateBackground in privateBackgroundList)
            {
                if (privateBackground.Name.Equals(matName))
                {
                    temp = privateBackground;
                }
                delIndex++;

            }
            if (temp != null)
            {
                privateBackgroundList.Remove(temp);
                return delIndex;
            }
            else
            {
                return -1;
            }

        }

    }


}
