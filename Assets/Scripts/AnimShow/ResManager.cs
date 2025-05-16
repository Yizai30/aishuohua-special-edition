using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using Interfaces;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
//人物资源类管理类，包含所有的游戏对象，以及各个资源列表的索引值
[Serializable]
public class ActorRes
{
    //public string PackageName;//包名
    public string prefabname;//角色名
    public string goname;
    public string ActorType;//类型
    public GameObject prefab;//角色的预制体
    public GameObject go;//角色的游戏对象
    
    public GameObject mSkeleton;//人物骨骼对象rgv
    //public Character mCharacter;
    //public Performer mPerformer;

    //显示每个部位的索引,key为部位名称，value为某个部位的素材名称
    public Dictionary<string,string> clothesNameDic=new Dictionary<string,string>();

    //存储各个部位资源列表的字典，key为部位名，value为该部位对应的资源列表
    public Dictionary<string, List<GameObject>> clothesListDic = new Dictionary<string, List<GameObject>>();
   
    public int mAnimIdx = 0;//准备播放的动画索引
    public string mAnimName = "";//准备播放的动画名字
    public List<AnimationClip> mAnimList = new List<AnimationClip>();//动画资源数组

    

    //重置所有资源
    public void Reset()
    {
       foreach(var key in clothesNameDic.Keys)
        {
            clothesNameDic[key] = "";
        }
    }

   
}

 
public class ResManager : MonoBehaviour
{
    #region 常量

    private const string SkeletonName = "skeleton";

    #endregion

    #region 变量
    //缓存actRes词典
    private Dictionary<string, ActorRes> ActResMem = new Dictionary<string, ActorRes>();
    
    //Dictionary<int, string> partNameDic = new Dictionary<int, string>();//存储各个枚举变量名字字符串的字典
    private List<ActorRes> mActorResList = new List<ActorRes>(); //一个list保存一个角色的资源信息信息
    private ActorRes mActorRes = null;//当前的ActorRes
    private int mActorResIdx = 0;

    public Image panel;


    #endregion
    #region 函数

    public void removeGoRes(string goName)
    {
       foreach(ActorRes actorRes in mActorResList)
        {
            if (actorRes.goname == goName)
            {
                mActorResList.Remove(actorRes);
                break;
            }
        }
    }

    public void TestGetActorInfoByName()
    {
        InitDatabase();
        GetActorInfoByName("cuteTiger3D", "");
        //Debug.Log(mActorResList[0].prefabname); // Should be cuteTiger3D
        //Debug.Log("Loaded Animation number: " + mActorResList[0].mAnimList.Count); // Should be 6
    }

    /// <summary>
    /// 根据角色名，从数据库中获取该角色的信息，取出该角色的预制体
    /// </summary>
    /// <param name="name"></param>
    void GetActorInfoByName(string prefabname,string goname)
    {
       
        //如果在actorRes列表里，直接取结果
        if (ActResMem.ContainsKey(prefabname))
        {
            ActorRes newActRes = ActResMem[prefabname];
            newActRes.goname = goname;
            mActorResList.Add(newActRes);
            return;
        }
      
        PublicActor actorinfo = null;
        //保存该角色的资源类
        ActorRes res = new ActorRes();
        //获取collection
        
        
        var actorList = PublicActor.FetchByName(prefabname);
        
        // var ccList = collection.Find(document1).ToList();
        // 如果有重名对象，返回第一个
        if (actorList.Count == 0)
        {
            Debug.Log("数据库中没有此角色名称 "+prefabname);
            return;
        }
        actorinfo = actorList[0];
        //根据路径，导入gameobject
        //int findex = actorinfo.Url.IndexOf("Character");//此处有风险
        int findex = "Resources/".Length;
        Debug.Log("actorinfo.Url: " + actorinfo.Url);
        string trueUrl = actorinfo.Url.Substring(findex);

        res.prefab = Resources.Load<GameObject>(trueUrl);
        if (res.prefab == null)
        {
            throw new Exception("resmanager没有找到prefab"+ trueUrl);
        }
        var skPath = trueUrl.Substring(0, trueUrl.LastIndexOf("/")) + "/skeleton";
        res.mSkeleton = Resources.Load<GameObject>(skPath);
        //检测有无clothes文件，若有，读取到字典
        //var packageUrl = trueUrl.Substring(0, trueUrl.LastIndexOf('/'));
        var packageUrl = new DirectoryInfo(Application.dataPath + "/Resources/" + trueUrl).Parent.Parent.FullName;
        var dirList = new DirectoryInfo(packageUrl).GetDirectories();
        
        foreach(var dir in dirList)
        {
            
            if (dir.Name == "Clothes")
            {
                getActorClothes(dir.FullName,ref res);
            }
        }
        
        
        //查找动画资源并存储
        if (actorinfo.Type.Substring(0,2)!="db" &&actorinfo.Animation!=null&& actorinfo.Animation.Count != 0)
        {
            foreach (string animId in actorinfo.Animation)
            {
              
                var AnimList = Interfaces.PublicAnimation.FetchById(animId);
                

                //根据文件类型处理动画 .anim .fbx
                var url = AnimList[0].Url;//Resources/Animation
                if (url.EndsWith("anim"))
                {
                    var url0 = url.Substring(0, url.LastIndexOf('.')); //去后缀
                    int findex2 = "Resources/".Length;
                    url0 = url0.Substring(findex2);
                    AnimationClip clip = Resources.Load<AnimationClip>(url0);
                    res.mAnimList.Add(clip);
                }

                else if (url.EndsWith("fbx") || url.EndsWith("FBX"))
                {
                    var path = url.Substring(0, url.LastIndexOf(".")).Replace("Resources/", "");
                    var assetRepresentationsAtPath = Resources.LoadAll(path);
                    foreach (var assetRepresentation in assetRepresentationsAtPath)
                    {
                        var animationClip = assetRepresentation as AnimationClip;
                        if (animationClip != null)
                        {
                            res.mAnimList.Add(animationClip);
                        }
                    }
                }
            }
        }
        

        //res.mAnimList = actorinfo.Animation;
        res.prefabname = actorinfo.Name;
        res.ActorType = actorinfo.Type;
        res.goname = goname;//指定该游戏角色

        this.mActorResList.Add(res);
        if (!ActResMem.ContainsKey(prefabname))
        {
            ActResMem.Add(prefabname, res);
        }
        
    }

    /// <summary>
    /// 组装动画
    /// </summary>
    void AssembleAnimation(string name)
    {
        ActorRes actorRes = findgoResByName(name);
        GameObject inputgo = actorRes.go;
        if (actorRes.prefabname == "Hunter")
        {
            foreach(Transform child in actorRes.go.transform)
            {
                if (child.name == "clothes")
                {
                    inputgo = child.gameObject;
                }
            }
        }
              
        if (inputgo == null)
        {
            Debug.Log("组装动画前要找到gameobject");
            return;
        }
        Animation ani;
       
        if (inputgo.GetComponent<Animation>() == null)
        {
            ani = inputgo.AddComponent<Animation>();
        }
        else
        {
            ani = inputgo.GetComponent<Animation>();
        }

        //添加Animation组件，以及该角色的动画列表
        foreach (ActorRes res in this.mActorResList)
        {
            if (res.goname == name )
            {
                foreach (AnimationClip aniclip in res.mAnimList)
                {

                    if (ani.GetClip(aniclip.name) == null)
                    {
                        //aniclip.legacy = true;
                        ani.AddClip(aniclip, aniclip.name);
                    }
                  

                }
               
                break;
            }
        }
        if (ani.GetClipCount() == 0)
        {
            Debug.Log("为该游戏物体添加动画列表失败。"+name);
        }

        //findActorResByName(name).prefab = inputgo;
    }

    //根据不同类型，加载不同的performer类对象到预制体上(注意是预制体不是go)
    void AddPerformer(string name)
    {
        ActorRes actorRes = findgoResByName(name);
        if (actorRes.prefab.GetComponent<Performer>() != null)
        {
            Performer performer = actorRes.prefab.GetComponent<Performer>();
            if (performer.panel == null) performer.panel = this.panel;
          
            return;
        }
        try
        {
            Performer performer;
            if (actorRes.ActorType == "animal" || actorRes.ActorType=="else"||actorRes.ActorType=="user_prop")
            {
               performer= actorRes.prefab.AddComponent<AnimalPerformer>();
               performer.panel = panel;
                
            }
            else if (actorRes.ActorType == "user_actor")
            {
                performer = actorRes.prefab.AddComponent<UserMatPerformer>();
                performer.panel = panel;
            }
            else if (actorRes.ActorType == "user_actor_gif")
            {
                performer = actorRes.prefab.AddComponent<GifPerformer>();
                performer.panel = panel;
            }
            else if (actorRes.ActorType == "human")
            {
                
                if (actorRes.prefabname == "Hunter")
                {
                    performer = actorRes.prefab.AddComponent<AllChangePerformer>();
                    performer.panel = panel;
                }
                else
                {
                    performer = actorRes.prefab.AddComponent<PartChangePerformer>();
                    performer.panel = panel;
                }
                
                
            }
            else if (actorRes.ActorType.Substring(0, 2) == "db")
            {
                performer = actorRes.prefab.AddComponent<DbPerformer>();
                performer.panel = panel;

            }
            else
            { 
                Debug.Log("找不到这个类型");
                return;
            }
           
        }
        catch(Exception e)
        {
            Debug.Log(e.Message + "没有找到该名称对应的对象 "+ name);
        }
    }

    /// <summary>
    /// 根据预制体、出生点等信息，调用Character组件，
    /// 根据指定的信息和预制体生成游戏物体。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="inputgo"></param>
    /// <param name="startPosition"></param>
    /// <param name="startRotation"></param>
    /// <param name="suitable"></param>
    /// <returns></returns>
    public GameObject InitCharacter(string prefabname,string goname,Vector3 startPosition,Quaternion startRotation, Vector3 startScale, bool suitable)
    {
        if (prefabname == "" || goname=="")
        {
            Debug.Log("未指定角色名字！");
            return null;
        }
        try
        {

            //UserMatInfoElement userMatInfoElement = DataMap.userMatInfoList.FindUserMatInfoByMatName(prefabname);
            PrivateActor privateActor = DataMap.privateActorList.FindPrivateActorByMatName(prefabname);
            PrivateProp privateProp = DataMap.privatePropList.FindPrivatePropByMatName(prefabname);
            if (privateActor != null)
            {
                GameObject userGo;
                mActorRes = genPrivateActorMatRes(privateActor, goname,out userGo);
                AddPerformer(goname);//添加Performer类
                Performer performer = mActorRes.prefab.GetComponent<Performer>();
                performer.Generate(mActorRes, startPosition, startRotation,startScale);
                mActorRes.go = performer.getGenGo();
                Destroy(userGo);
  
            }else if (privateProp != null)
            {
                GameObject userGo;
                mActorRes = genPrivatePropMatRes(privateProp, goname, out userGo);
                AddPerformer(goname);//添加Performer类

                Performer performer = mActorRes.prefab.GetComponent<Performer>();
                performer.Generate(mActorRes, startPosition, startRotation, startScale);
                mActorRes.go = performer.getGenGo();
                Destroy(userGo);
            }
            else
            {
                //公共数据库中的素材
                GetActorInfoByName(prefabname, goname);//根据prefabname从数据库获取角色信息，生成一个go的资源

                mActorRes = findgoResByName(goname);
                AddPerformer(goname);//添加Performer类

                Performer performer = mActorRes.prefab.GetComponent<Performer>();
                performer.Generate(mActorRes, startPosition, startRotation, startScale);
                mActorRes.go = performer.getGenGo();
                //对非龙骨对象，要装配动画列表
                if (findgoResByName(goname).ActorType.Substring(0, 2) != "db")
                {
                    AssembleAnimation(goname);//加载该actor的动画到其ActorRes
                }
                mActorRes.go = performer.getGameObject();
            }
                   
           
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        

        //注意，之前mActorResList里的prefab是预制体
        //这里生成的go是场景中的游戏物体，不一样！
        //要更新之前列表中的go
        //ActorRes res = findgoResByName(goname);
        //res.go = mActorRes.go;

        return mActorRes.go;//返回实例化后的游戏物体Character.genGo。始终维护这个obj
         
    }
    //如果有gif素材 用gifperformer
    public ActorRes genPrivateActorMatRes(PrivateActor privateActor,string goName,out GameObject userGo)
    {
        if (DataMap.matGifCollection.FindMatGifListByName(privateActor.Name)!=null)
        {
            return genPrivateActorGifMatRes(privateActor, goName, out userGo);
        }

        string matName = privateActor.Name;
        ActorRes actorRes = new ActorRes();
        actorRes.prefabname = matName;
        userGo = new GameObject(goName+"test");
        userGo.transform.position = new Vector3(99, 99, 99);

        SpriteRenderer renderer = userGo.AddComponent<SpriteRenderer>();
        
        Texture2D texture = ImgUtil.LoadTexture(Application.persistentDataPath + "/" + privateActor.Url+"/"+privateActor.Name+".png");
        Sprite sprite = ImgUtil.createSprite(texture);
        if (sprite == null) return null;
        renderer.sprite = sprite;

        actorRes.prefab = userGo;
        //Destroy(userGo);
        actorRes.ActorType = "user_actor";
        actorRes.goname = goName;
        this.mActorResList.Add(actorRes);
        if (!ActResMem.ContainsKey(matName))
        {
            ActResMem.Add(matName, actorRes);
        }
        return actorRes;
    }

    public ActorRes genPrivateActorGifMatRes(PrivateActor privateActor, string goName, out GameObject userGo)
    {
        string matName = privateActor.Name;
        ActorRes actorRes = new ActorRes();
        actorRes.prefabname = matName;
        userGo = new GameObject(goName);
        //userGo.transform.position = new Vector3(99, 99, 99);
        userGo.AddComponent<RawImage>();
        RectTransform rectTransform = userGo.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(10, 10);
        UniGifImage gifImage = userGo.AddComponent<UniGifImage>();
        gifImage.setImgAspectCtrl(userGo.GetComponent<UniGifImageAspectController>());
        gifImage.setRawImage(userGo.GetComponent<RawImage>());

        actorRes.prefab = userGo;
        //Destroy(userGo);
        actorRes.ActorType = "user_actor_gif";
        actorRes.goname = goName;
        this.mActorResList.Add(actorRes);
        if (!ActResMem.ContainsKey(matName))
        {
            ActResMem.Add(matName, actorRes);
        }
        return actorRes;
    }


    public ActorRes genPrivatePropMatRes(PrivateProp privateProp, string goName, out GameObject userGo)
    {
        string matName = privateProp.Name;
        ActorRes actorRes = new ActorRes();
        actorRes.prefabname = matName;
        userGo = new GameObject(goName + "test");
        userGo.transform.position = new Vector3(99, 99, 99);

        SpriteRenderer renderer = userGo.AddComponent<SpriteRenderer>();

        Texture2D texture = ImgUtil.LoadTexture(Application.persistentDataPath + "/" + privateProp.Url + "/" + privateProp.Name + ".png");
        Sprite sprite = ImgUtil.createSprite(texture);
        if (sprite == null) return null;
        renderer.sprite = sprite;

        actorRes.prefab = userGo;
        //Destroy(userGo);
        actorRes.ActorType = "user_prop";
        actorRes.goname = goName;
        this.mActorResList.Add(actorRes);
        if (!ActResMem.ContainsKey(matName))
        {
            ActResMem.Add(matName, actorRes);
        }
        return actorRes;
    }

    public void changeColor(string goname,List<float> rgb)
    {
        Color color = getColorByList(rgb);
        ActorRes actorRes = findgoResByName(goname);
        GameObject go = actorRes.go;

        if (goname.Contains("db_"))
        {
            DragonBones.ColorTransform colorTransform = new DragonBones.ColorTransform();
            colorTransform.redMultiplier = color.r;
            colorTransform.greenMultiplier = color.g;
            colorTransform.blueMultiplier = color.b;
            //colorTransform.
            DragonBones.UnityArmatureComponent unityArmatureComponent = go.GetComponent<DragonBones.UnityArmatureComponent>();
            unityArmatureComponent.color = colorTransform;
            return;

        }

        List<GameObject> gos = getAllChildGo(go);
        //获取所有子游戏物体
        for(int i = 0; i < gos.Count; i++)
        {
            
            GameObject childGo = gos[i];
            MeshRenderer meshRenderer = null;
            SpriteRenderer spriteRenderer=null;
            SkinnedMeshRenderer skinnedMeshRenderer = null;
            meshRenderer = childGo.GetComponent<MeshRenderer>();
            spriteRenderer = childGo.GetComponent<SpriteRenderer>();
            skinnedMeshRenderer = childGo.GetComponent<SkinnedMeshRenderer>();
            if (spriteRenderer!= null)
            {
                foreach(Material material in spriteRenderer.materials)
                {
                    material.color = color;
                }
                
            }
            if (meshRenderer != null)
            {
                foreach (Material material in meshRenderer.materials)
                {
                    material.color = color;
                }

            }
            if (skinnedMeshRenderer != null)
            {
                foreach (Material material in skinnedMeshRenderer.materials)
                {
                    material.color = color;
                }

            }
        }
        
    }
    //广度优先获取所有子游戏物体
    List<GameObject> getAllChildGo(GameObject go)
    {

       
        List<GameObject> re = new List<GameObject>();
        re.Add(go);
        Queue<GameObject> queue = new Queue<GameObject>();
        for(int i = 0; i < go.transform.childCount; i++)
        {
            queue.Enqueue(go.transform.GetChild(i).gameObject);
            
        }
        while (queue.Count != 0)
        {
            GameObject goTemp = queue.Dequeue();
            re.Add(goTemp);
            for (int i = 0; i < goTemp.transform.childCount; i++)
            {
                queue.Enqueue(goTemp.transform.GetChild(i).gameObject);

            }
        }
        return re;
    }

    Color getColorByList(List<float>rgb)
    {
        Color color = new Color(0, 0, 0);
        if (rgb.Count == 3)
        {
            return new Color(rgb[0], rgb[1], rgb[2]);

        }
        else if (rgb.Count == 4)
        {
            return  new Color(rgb[0], rgb[1], rgb[2], rgb[3]);
        }
        else
        {
            throw new Exception("color参数错误");
        }
    }

    //获取所有衣服资源
 
    //换装，待完善
    public void ChangeClothes(string goname, Dictionary<string,string> partDic)
    {
        ActorRes actorRes = findgoResByName(goname);
        actorRes.clothesNameDic.Clear();
        //默认服装
        foreach(var it in actorRes.clothesListDic)
        {
            actorRes.clothesNameDic.Add(it.Key, it.Value[0].name);
        }

        //根据传入的dic更改
        foreach(var item in partDic)
        {
            if (actorRes.clothesNameDic.ContainsKey(item.Key) && item.Value!="")
            {
                actorRes.clothesNameDic[item.Key] = item.Value;
            }
        }

        if (actorRes.prefabname == "Hunter")
        {
            var hperformer = actorRes.go.GetComponent<AllChangePerformer>();
            hperformer.ChangeClothes(actorRes);
        }
        else
        {
            var performer = actorRes.go.GetComponent<Performer>();
            //performer.ChangeClothes(actorRes);
            //Destroy(actorRes.go);
            performer.Generate(actorRes, actorRes.go.transform.position, actorRes.go.transform.rotation, actorRes.go.transform.localScale);
        }
        
        

    }

    


    //根据游戏物体名和动画名字指定动画
    public void ChangeAnim(string goname,string animName,int loop,float duration,
        Vector3 startPos,Vector3 endPos,Vector3 startScale,Vector3 endScale,Quaternion startRot,Quaternion endRot)
    {
        ActorRes actorRes = findgoResByName(goname);
       
        actorRes.mAnimName = animName;
        int index = 0;
        
        foreach(AnimationClip clip in actorRes.mAnimList)
        {
            if (clip.name == animName)
            {
                actorRes.mAnimIdx = index;
            }
            index++;
        }
        Performer performer = actorRes.go.GetComponent<Performer>();
      
        performer.ChangeAnim(actorRes, loop,duration,startPos,endPos,startScale,endScale,startRot,endRot);
        
    }

    //换装待完善，未用到此函数
    /// <summary>
    /// 读取资源文件，创建所有的人物素材
    /// mActorResList中存放包内的所有角色资源文件
    /// 每个ActorRes中有每个部位的所有游戏物体，以及动画
    /// </summary>
    public void getActorClothes(string clothesPath,ref ActorRes res)
    {
       
        DirectoryInfo dir = new DirectoryInfo(clothesPath);
        var clothesDirList = dir.GetDirectories();
        // DirectoryInfo dir = new DirectoryInfo("Assets/Resources/" + path);
        if (clothesDirList.Length < 0)
        {
            Debug.Log("Clothes文件夹中没有文件夹");
            return;
        }
        foreach (var subdir in dir.GetDirectories())//得到包内各个角色的资源目录
        {
            //.../Resources/Cha...
            string rePath = subdir.FullName.Substring(subdir.FullName.IndexOf("Resources\\") + "Resources\\".Length);
            GameObject[] goList = Resources.LoadAll<GameObject>(rePath);
            res.clothesListDic.Add(subdir.Name, goList.ToList());
            //默认的衣服
            res.clothesNameDic.Add(subdir.Name, goList[0].name);

        }


    }


    public void InitDatabase()
    {
        
        //mgDbHelper = new MongoDbHelper(DbUrl);
        //database = mgDbHelper.GetDb(DbUrl);
    }
    /// <summary>
    /// 根据游戏物体名查找该物体对应的资源对象
    /// </summary>
    /// <param name="goname"></param>
    /// <returns></returns>
    public ActorRes findgoResByName(string goname)
    {
        //GameObject inputgo = null;
        foreach (ActorRes res in mActorResList)
        {
            if (res.goname == goname)
            {
                return res;
            }
        }
        Debug.Log("没有找到该游戏物体对应的资源");
        return null;
    }
    #endregion

    #region 内置函数

    // Use this for initialization
    void Start ()
    {      
        //数据库连接
        InitDatabase();

    }
	

    #endregion
}

