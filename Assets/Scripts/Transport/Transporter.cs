  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFtp;
using System.IO;
using MongoDB.Bson;
using System.Linq;
using System;
using MongoClass;
using MongoDB.Driver;

public class Transporter : MonoBehaviour
{

    //MongoDbHelper mgDbHelper;
    //static string DbUrl = "mongodb://root:123456@47.100.88.221:27017/AnimationSource?authSource=admin";
    //public static string DbUrl = "mongodb://root:123456@47.100.88.221:27017/AnimationSource?authSource=admin";
    public static string DbUrl = "mongodb://Anim:990124@221.12.170.99:27017/AnimationSource?authSource=admin";
    MongoDbHelper mgDbHelper = new MongoDbHelper(DbUrl);

    public string actorLocalUrl = "", actorFtpUrl = "", animLocalUrl = "", animFtpUrl = "",
        actorType = "", animType = "", actorPackageName = "", animPackageName = "";

    public List<string> actorNameList = new List<string>();
    public List<string> animNameList = new List<string>();
   

    // Start is called before the first frame update
    void Start()
    {
        //ftpManager = new FtpManager();
        
       
        //actorNameList = new List<string>();
        //animNameList = new List<string>();

        //上传角色文件
        //string packageName = "cuteCow";
        
        //UploadActor(localUrl, ftpUrl,"animal");

        //上传动画文件
        //UploadAnimation(@"D:\projects\unity\AnimationPlatform\Assets\Resources\Animation\cuteAnimals\TypeA\", @"/Resources/Animation/cuteAnimals/","1");

        //link
       //LinkActorAndAnimation(actorNameList, animNameList);
          
    }

   
    // Update is called once per frame
    void Update() 
    {
        
    }

    //上传压缩文件 zipName[texture/prefab/material/model]
    //数据库中的actor名对应一个prefab名。加一个字段为zipName保存该prefab所在的压缩包名。
    //待上传的文件名，待上传的文件夹路径（"\"不能漏掉；脚本会自动压缩上传）、目标文件夹路径、种类
    //UploadActor("cuteCow","D:\cuteCow\", "/Resources/Characters/Animals/,"animal")
    //本地结构：Resources/Characters/[className]/[zipName]
    public void UploadActor()
    {
        genActorFtpUrl();
        if(actorLocalUrl=="" || actorFtpUrl=="" || actorType == "" || actorPackageName=="")
        {
            Debug.Log("上传角色的信息不全，上传失败");
            return;
        }

        DirectoryInfo locoalInfo = new DirectoryInfo(actorLocalUrl);
        string zipName = locoalInfo.Name;

        //1、查看待上传文件在ftp中是否已经存在
        if (FtpManager.FileExists(actorFtpUrl + zipName + ".zip"))
        {
             Debug.Log("目标文件已经存在，上传失败");
            return;
        }

        //2、找到localurl中的prefab名,查看mongodb中是否有该prefab
        DirectoryInfo prefabDir = new DirectoryInfo(actorLocalUrl + @"\Prefab");

        FileInfo[] fileInfos = prefabDir.GetFiles();
        if (fileInfos.Length == 0)
        {
           
            Debug.Log("prefab文件夹中没有找到文件");
            return;
        }
        BsonDocument document = new BsonDocument();
        List<string> prefabNameList = new List<string>();
        foreach (var fi in fileInfos)
        {
            //记录所有后缀名为.prefab的文件
            //注意，这里要求数据库中一个document的name和素材prefab的name一致
            //后续插入信息到mongodb也是用prefab的名字，prefab名是查找actor的唯一方式
            if (fi.Name.EndsWith("prefab")){
                string fname = fi.Name.Substring(0, fi.Name.IndexOf("."));
                //在数据库中查找
                prefabNameList.Add(fname);
                document.Clear();
                document.Add("Name", fname);
                var actorList = mgDbHelper.FindByCondition<Actor_info>("Actor", document).ToList();
                if (actorList.Count() != 0)
                {
                    Debug.Log("预制体名在数据库中已经存在，上传失败");
                    return;
                }
            }
            
        }
        //3 压缩package
        
        string parentName = Directory.GetParent(actorLocalUrl).FullName;
        string zipFileFullName = parentName + @"\"+zipName + ".zip";
        ZipUtil.DirToZip(actorLocalUrl, zipFileFullName);
        
        //4 上传文件夹到ftp服务器
        try
        {
            FtpManager.UploadFile(zipFileFullName, actorFtpUrl + zipName+".zip");
            //FtpManager.UploadDirectory(zipFileFullName, ftpUrl);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        //delete the zip file after uploading
        File.Delete(zipFileFullName);

        //5 添加资源信息到mongodb
        try
         {
            foreach (string preName in prefabNameList)
            {
                string preUrl = actorFtpUrl + zipName+ "/Prefab/" + preName;
                string zipUrl = actorFtpUrl + zipName + ".zip";
                
                var doc = new BsonDocument
            {
                {"Url",preUrl },
                {"ZipUrl", zipUrl},//下载的时候根据这个下zip文件
                {"Type",actorType },
                {"Animation",new BsonArray{ } },
                {"Clothes",new BsonArray{ new BsonArray { } } },
                {"Name", preName}

            };
                mgDbHelper.Insert("Actor", doc);
                Debug.Log("已上传角色" + preName);
            }
            //设置全局变量
            Debug.Log("上传成功");
            actorNameList = prefabNameList;
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
               
     
    }

    //批量上传动画，要放在一个文件夹内,url都是文件夹
    //UploadAnimation(@"D:\projects\unity\AnimationPlatform\Assets\Resources\Animation\cuteAnimals\TypeA\", "/Resources/Animation/cuteAnimals/");
    public void UploadAnimation()
    {

        //生成ftpurl
        genAnimFtpUrl();

        if (animLocalUrl=="" || animFtpUrl=="" || animType == "" ||animPackageName=="")
        {
            Debug.Log("上传动画的信息不全，上传失败");
            return;
        }

        

        DirectoryInfo localInfo = new DirectoryInfo(animLocalUrl);
        string zipName = localInfo.Name;
        //查看url文件夹在ftp中是否已经存在
        if (FtpManager.FileExists(animFtpUrl + zipName + ".zip"))
        {
            Debug.Log("目标文件"+ animFtpUrl + zipName+".zip"+"已经存在，上传失败");
            return;
        }

        //遍历文件夹中的动画名，看mongodb中是否已经存在

        FileInfo[] fileInfos = localInfo.GetFiles();
        if (fileInfos.Length == 0)
        {

            Debug.Log("本地文件夹中没有文件");
            return;
        }
        BsonDocument document = new BsonDocument();
        List<string> animationList = new List<string>();
        foreach (var fi in fileInfos)
        {
            if(fi.Name.EndsWith("fbx") || fi.Name.EndsWith("FBX") || fi.Name.EndsWith("anim"))
            {
                string fname = fi.Name.Substring(0, fi.Name.IndexOf("."));
                //在数据库中查找
                animationList.Add(fi.Name);//这里要带后缀名，数据库存后缀名
                document.Clear();
                document.Add("Name", fname);
                var actorList = mgDbHelper.FindByCondition<Animation_info>("Animation", document).ToList();
                if (actorList.Count() != 0)
                {
                    Debug.Log("动画名"+fname+"已经存在，上传失败");
                    return;
                }
            }

        }
        if (animationList.Count == 0)
        {
            Debug.Log("本地文件夹中没有以fbx、FBX、anim格式的文件");
            return;
        }

        // 压缩package
        string parentName = Directory.GetParent(Directory.GetParent(animLocalUrl).FullName).FullName;
        string zipFileFullName = parentName + @"\"+zipName + ".zip";
        ZipUtil.DirToZip(animLocalUrl, zipFileFullName);

        //上传文件夹到ftp服务器
        try
        {
            FtpManager.UploadFile(zipFileFullName, animFtpUrl + zipName + ".zip");
            //FtpManager.UploadDirectory(zipFileFullName, ftpUrl);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        //delete the zip file after uploading
        File.Delete(zipFileFullName);
        
        //遍历每个动画文件，添加到mongodb
        try
        {
            foreach (string animName in animationList)
            {
            
                
                string animUrl = animFtpUrl + zipName + "/" + animName;//url带后缀
                string zipUrl = animFtpUrl + zipName + ".zip";
                string dataAnimName = animName.Substring(0, animName.IndexOf("."));//名字不带后缀

                //BsonObjectId objectId;
                var doc = new BsonDocument
            {
                {"Name",dataAnimName },
                {"Type", animType},//下载的时候根据这个下zip文件
                {"ActorList",new BsonArray{ } },                               
                {"Url", animUrl},
                {"ZipUrl",zipUrl }

            };
                mgDbHelper.Insert("Animation", doc);
                Debug.Log("已上传动画"+animName);
            }
            //设置全局变量
            foreach(string name in animationList)
            {
                animNameList.Add(name.Substring(0, name.IndexOf(".")));
            }
            Debug.Log("上传成功");

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void LinkActorAndAnimation()
    {
        //根据名字查找mongodb，在mongodb中将角色和动画相互关联。 
        //修改actor的animation

        //得到objId
        List<ObjectId> animIDs = new List<ObjectId>();
        List<ObjectId> actorIDs = new List<ObjectId>();
        foreach (var animName in animNameList)
        {
            //var con = new BsonDocument { { "Name", animName } };
            BsonDocument document = new BsonDocument();
            document.Add("Name", animName);
            var animInfo = mgDbHelper.FindByCondition<Animation_info>("Animation", document).ToList();
            animIDs.Add(animInfo[0]._id);
        }

        foreach (var actorName in actorNameList)
        {
            BsonDocument document = new BsonDocument();
            document.Add("Name", actorName);
            var actorInfo = mgDbHelper.FindByCondition<Actor_info>("Actor", document).ToList()[0];
            actorIDs.Add(actorInfo._id);
        }
        //ObjectId[] animIDArr = animIDs.ToArray();
        //ObjectId[] actorIDArr = actorIDs.ToArray();

        //update actor's animation list
        foreach (var actorName in actorNameList)
        {
            //获取当前actor已连接的动画
            BsonDocument document = new BsonDocument();
            document.Add("Name", actorName);
            var actorInfo = mgDbHelper.FindByCondition<Actor_info>("Actor", document).ToList()[0];
            List<ObjectId> animIdExist = actorInfo.Animation.ToList();
            //加上新增连接动画
            foreach(var id in animIdExist)
            {
                if (!animIDs.Contains(id))
                {
                    animIDs.Add(id);
                }
            }
            //animIDs.AddRange(animIdExist);
            ObjectId[] animIDArr = animIDs.ToArray();


            var filter = Builders<BsonDocument>.Filter.Eq("Name", actorName);
            var update = Builders<BsonDocument>.Update.Set("Animation", animIDArr);
            mgDbHelper.Update("Actor", filter, update, false);
            
        }

        //update animation's actor list
        foreach (var animName in animNameList)
        {
            //获取当前anim已连接的actor
            BsonDocument document = new BsonDocument();
            document.Add("Name", animName);
            var animInfo = mgDbHelper.FindByCondition<Animation_info>("Animation", document).ToList()[0];
            List<ObjectId> actorIdExist = animInfo.ActorList.ToList();
            //加上新增连接actor
            foreach(var id in actorIdExist)
            {
                if (!actorIDs.Contains(id))
                {
                    actorIDs.Add(id);
                }
            }
            //actorIDs.AddRange(actorIdExist);
            ObjectId[] actorIDArr = actorIDs.ToArray();

            var filter = Builders<BsonDocument>.Filter.Eq("Name", animName);
            var update = Builders<BsonDocument>.Update.Set("ActorList", actorIDArr);
            mgDbHelper.Update("Animation", filter, update, false);

        }

        Debug.Log("完成连接");
    }


    //存放url规则 /Resources/Characters/[className]/[packageName]/
    //在上传本地文件时，将目标文件放在package文件夹下
    
    public void genActorFtpUrl()
    {
        string className = actorType;
        FileInfo file = new FileInfo(actorLocalUrl);
        
        this.actorFtpUrl = "/Resources/Character/" + className + "/" + this.actorPackageName + "/";
        Debug.Log("生成远程url " + this.actorFtpUrl);

    }

    // /Resources/Animation/[packageName]/
    public void genAnimFtpUrl()
    {        
        FileInfo file = new FileInfo(animLocalUrl);
        
        this.animFtpUrl = "/Resources/Animation/" + this.animPackageName + "/";
        Debug.Log("生成远程url " + this.animFtpUrl);
    }
    // 没有得到必要的输入信息则不可用
    public bool canUploadActor()
    {
        return (actorLocalUrl != ""  && actorType != "" && actorPackageName!="");
        
    }

    public bool canUploadAnime()
    {
        return ( animLocalUrl != "" && animType != "" && animPackageName!="");
       
    }

 


    //return file names in a directory
    public List<FileInfo> getFileInDirectory(string dirPath)
    {
        List<FileInfo> fileList = new List<FileInfo>();
        string path = Application.persistentDataPath + "/" + dirPath;
        if (!Directory.Exists(path))
        {
            Debug.Log("文件夹不存在");
            return fileList;
        }
        var directoryInfo = new DirectoryInfo(path);
        var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        foreach (var t in fileInfos)
        {
           fileList.Add(t);
           Debug.Log("Name:" + t.Name);
            
        }
        return fileList;

    }

}
