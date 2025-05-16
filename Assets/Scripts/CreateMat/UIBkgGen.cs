using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.WSA;
using System;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using static UnityEngine.Random;
using System.Security.Cryptography;


public class UIBkgGen : MonoBehaviour
{
    public Button buttonGen, buttonCancel;

    public Button LandButton, waterButton, inhouseButton, xButton;

    private enum PhotoClass
    {
        nature_land,
        nature_water,
        human_inhouse,
        xi_class,
        useless
    }

    private Image selectedImageClass = null;

    public Button BackUpperButton, ReGenButton, SaveButton;

    public SceneCreateActor createManager;

    public InputField bkgName, bkgDisc;

    public string bkgNameStr;

    public GameObject reGen;

    PhotoClass cato = PhotoClass.useless;

    private string retrieve_one = null;

    public Image toShow;

    public delegate void StringCallback(List<string> ok);

    public SaveUserMat saveUserMat;

    public Text ReGenHint;

    public Dropdown dropdown;

    private string chooseItem;
    private int num_under_cato ;

    public GameObject textbkgname2;
    void Start()
    {
        
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
        
        // reGen.gameObject.SetActive(false);

        print("cato initial value is -> " + cato);
        //第一步，选择图片类别
        waterButton.onClick.AddListener((() =>
        {
            if (selectedImageClass != null)
            {
                selectedImageClass.color = Color.white;
            }

            selectedImageClass = waterButton.image;
            selectedImageClass.color = Color.red;
            cato = PhotoClass.nature_water;
            print("cato value is -> " + cato);
            
            //edit and change dropdown list here =>
            // 清空当前的选项列表
            dropdown.ClearOptions();
            // 创建一个新的选项列表
            List<string> options = new List<string>() { "新建","桥边", "湖泊", "池塘", "溪边","河岸" };
            // 将新的选项列表添加到Dropdown组件中
            dropdown.AddOptions(options);
            bkgName.gameObject.SetActive(true);
           
            textbkgname2.SetActive(true);
            
        }));

        LandButton.onClick.AddListener((() =>
        {
            if (selectedImageClass != null)
            {
                selectedImageClass.color = Color.white;
            }

            selectedImageClass = LandButton.image;
            selectedImageClass.color = Color.red;
            cato = PhotoClass.nature_land;
            print("cato value is -> " + cato);
            
            //edit and change dropdown list here =>
            // 清空当前的选项列表
            dropdown.ClearOptions();
            // 创建一个新的选项列表
            List<string> options = new List<string>() {"新建", "蜂巢", "森林", "森林小屋", "花园", "草地", "洞穴", "乡村小路", "公园","山坡" };
            // 将新的选项列表添加到Dropdown组件中
            dropdown.AddOptions(options);
            bkgName.gameObject.SetActive(true);
          
            textbkgname2.SetActive(true);
            
        }));


        inhouseButton.onClick.AddListener((() =>
        {
            if (selectedImageClass != null)
            {
                selectedImageClass.color = Color.white;
            }

            selectedImageClass = inhouseButton.image;
            selectedImageClass.color = Color.red;
            cato = PhotoClass.human_inhouse;
            print("cato value is -> " + cato);
            
            //edit and change dropdown list here =>
            // 清空当前的选项列表
            dropdown.ClearOptions();
            // 创建一个新的选项列表
            List<string> options = new List<string>() {"新建", "游乐园", "生日舞会", "医院", "屋里", "图书馆", "警察局", "浴室", "教室","滑冰场" };
            // 将新的选项列表添加到Dropdown组件中
            dropdown.AddOptions(options);
            bkgName.gameObject.SetActive(true);
            textbkgname2.SetActive(true);
            
            
        }));
        
        xButton.onClick.AddListener((() =>
        {
            if (selectedImageClass != null)
            {
                selectedImageClass.color = Color.white;
            }

            selectedImageClass = xButton.image;
            selectedImageClass.color = Color.red;
            cato = PhotoClass.xi_class;
            print("cato value is -> " + cato);
            
            //edit and change dropdown list here =>
            // 清空当前的选项列表
            dropdown.ClearOptions();
            // 创建一个新的选项列表
            List<string> options = new List<string>() {"新建", "壶口瀑布", "阿尔山", "小岗村", "喀什", "梁家河","延边","伊春","海南","六盘山","唐古拉山镇","北师大", "香港", "澳门", "自贸区" };
            // 将新的选项列表添加到Dropdown组件中
            dropdown.AddOptions(options);
            bkgName.gameObject.SetActive(true);
            textbkgname2.SetActive(true);
            
            
        }));
        //确定图片类别的cato值，根据这个值去对全部的素材资源进行分类。也就是传参给onClickAIGen()函数

        buttonGen.onClick.AddListener(() =>
        {
            if (cato == PhotoClass.useless)
            {
                AndroidUtils.ShowToast("请选择第一步的图片分类");
                return;
            }
            reGen.gameObject.SetActive(true);
            ReGenHint.gameObject.SetActive(true);
            print("cato value for Gen AI is -> " + cato);
            onClickAIGen(cato);
            print(" Gen AI completed  -> onClickAIGen(cato);");
        });
        
        //buttonGen目前的逻辑是联网获取新生成的图片
        //但是目前是存在本地persistent资源那里的
        //现在需要更改逻辑，在BkgReGenPannel这个界面进行展现，并且由用户决定[返回，重新生成，保留资源]三个功能
        //也就是原来通过buttonGen一步保存在本地的步骤，改为上述步骤，由用户决定是否保留，还是继续生成


        buttonCancel.onClick.AddListener(() =>
        {
            createManager.editMode = SceneCreateActor.EDITMODE.Photo;
            this.gameObject.SetActive(false);
        });


        BackUpperButton.onClick.AddListener(() => { reGen.gameObject.SetActive(false); });

        ReGenButton.onClick.AddListener(() =>
        {
            ReGenHint.gameObject.SetActive(true);
            print("cato value for Gen AI is -> " + cato);
            onClickAIGen(cato);
        });

        SaveButton.onClick.AddListener(() =>
        {
            StartCoroutine(saveAIGenBkg());
        });
    }

    void DropdownValueChanged(Dropdown change)
    {
        chooseItem = change.options[change.value].text;
        if (chooseItem == "新建")
        {
            // bkgName.ActivateInputField();
            bkgName.gameObject.SetActive(true);
            textbkgname2.SetActive(true);
            // 显示一个空白的文本框
            // 这里需要你提供具体的实现代码
            print("选择的项是: " + chooseItem);
            num_under_cato = dropdown.value;
            retrieve_one = null;
        }
        else
        {
            // bkgName.DeactivateInputField();
            bkgName.gameObject.SetActive(false);
            textbkgname2.SetActive(false);
            print("选择的项是: " + chooseItem);
            num_under_cato = dropdown.value;
            bkgName.text = chooseItem;
        }
    }
    
    IEnumerator saveAIGenBkg()
    {
        print("开始保存图片");
        Texture2D texture = toShow.mainTexture as Texture2D;
        if (texture == null)
        {
            print("Main texture is not a Texture2D");
        }
        byte[] imageBytes = texture.EncodeToPNG();
        // print("step 1");
        Texture2D rawImageTex = new Texture2D((int)toShow.rectTransform.rect.width, (int)toShow.rectTransform.rect.height);
        // print("step 2");
        rawImageTex.LoadImage(imageBytes);
        // print("step 3");
        rawImageTex.Apply();
        // print("step 4");
        if (bkgName != null)
        {
            saveUserMat.saveBkg(Server.getMatCreator(), bkgName.text, rawImageTex);
        }
        else
        {
            saveUserMat.saveBkg(Server.getMatCreator(), retrieve_one, rawImageTex);
        }
        // print("step 5");
        reGen.gameObject.SetActive(false); 
        // print("6");
        //这个控制的是整个显示图片的那个框的出现和消失；
        //最终效果：保存最终获取的图片，存到资源库，存到数据库，一系列的通信问题
        print(bkgNameStr+" 已经保存到我的素材数据库");
        
        yield return null;
        // 加载图片
        // 图片源来自要插入到数据库的那张图片
        // 
        
        StartCoroutine(call_sam_handler("hamn", imageBytes));
    }
    void onClickAIGen(PhotoClass cato)
    {
        Dictionary<int, string> Land_Pairs = new Dictionary<int, string>();
        Dictionary<int, string> Water_Pairs = new Dictionary<int, string>();
        Dictionary<int, string> Human_Pairs = new Dictionary<int, string>();
        Dictionary<int, string> Xi_Sights = new Dictionary<int, string>();
        //分了三类，感觉还是要上模糊匹配，
        //水域风光
        Water_Pairs.Add(1,"bridge");
        Water_Pairs.Add(4,"creek");
        Water_Pairs.Add(2,"lake");
        Water_Pairs.Add(3,"pond");
        Water_Pairs.Add(5,"riverBank");
        
        //陆地风光
        //List<string> options = new List<string>() { "蜂巢", "森林", "森林小屋", "花园", "草地", "洞穴", "乡村小路", "公园","山坡" };
        Land_Pairs.Add(7,"countryRoad");
        Land_Pairs.Add(2,"forest");
        Land_Pairs.Add(3,"forestHouse");
        Land_Pairs.Add(1,"beeHome");
        Land_Pairs.Add(4, "garden");
        Land_Pairs.Add(5,"grassland");
        Land_Pairs.Add(9,"mountain");
        Land_Pairs.Add(8,"park");
        Land_Pairs.Add(6,"cave");
        
        //室内风光
        // List<string> options = new List<string>() { "游乐园", "生日舞会", "医院", "屋里", "图书馆", "警察局", "浴室", "教室","滑冰场" };
        Human_Pairs.Add(1,"amusementPark");
        Human_Pairs.Add(7,"bathroom");
        Human_Pairs.Add(2,"birthdayGround");
        Human_Pairs.Add(8,"classroom");
        Human_Pairs.Add( 3,"hospital");
        Human_Pairs.Add(4,"inHouse");
        Human_Pairs.Add(5,"library");
        Human_Pairs.Add( 6,"policeOffice");
        Human_Pairs.Add(9,"rink");

        //List<string> options = new List<string>() { "壶口瀑布", "阿尔山", "小岗村", "喀什", "梁家河","延边","伊春",
        //"海南","六盘山","唐古拉山镇","北师大", "香港", "澳门", "自贸区" };
        Xi_Sights.Add(1,"hukoupubu");
        Xi_Sights.Add(2,"aershan");
        Xi_Sights.Add(3,"xiaogangcun");
        Xi_Sights.Add(4,"kashi");
        Xi_Sights.Add(5,"liangjiahe");
        Xi_Sights.Add(6,"yanbian");
        Xi_Sights.Add(7,"yichun");
        Xi_Sights.Add(8,"hainan");
        Xi_Sights.Add(9,"liupanshan");
        Xi_Sights.Add(10,"tanggula");
        Xi_Sights.Add(11,"beishida");
        Xi_Sights.Add(12,"xianggang");
        Xi_Sights.Add(13,"aomen");
        Xi_Sights.Add(14,"zimaoqu");













        //显示在reGen的里面，可以通过buttonGen，或者ReGenButton来调用生成图片；
        if (cato == PhotoClass.useless)
        {
            AndroidUtils.ShowToast("请首先选择生成景物的类别");
            return;
        }

        

        if (num_under_cato == 0 || bkgName.text != null)
            //分两种情况选取背景，第一种是系统内置的；第二种是完全根据用户输入的，在这里进行选择
        {
            bkgNameStr = bkgName.text;
        }
        
        if (this.cato == PhotoClass.nature_land)
        {
            // 创建一个Random对象
            // int random = Range(0, Land_Pairs.Count);
            // 使用随机索引从列表中获取一个随机值
            if(num_under_cato !=0) 
                retrieve_one = Land_Pairs[num_under_cato];
        }

        else if (cato == PhotoClass.nature_water)
        {
            // int random = Range(0, Water_Pairs.Count);
            // 使用随机索引从列表中获取一个随机值
            if(num_under_cato != 0)
                retrieve_one = Water_Pairs[num_under_cato];
        }

        else if (this.cato == PhotoClass.human_inhouse)
        {
            // int random = Range(0, Human_Pairs.Count);
            // 使用随机索引从列表中获取一个随机值
            if(num_under_cato != 0)
                retrieve_one = Human_Pairs[num_under_cato];
        }
        else if (this.cato == PhotoClass.xi_class)
        {
            if (num_under_cato != 0)
                retrieve_one = Xi_Sights[num_under_cato];
        }
        
        bkgNameStr = bkgName.text;
        string bkgDiscStr = bkgDisc.text;
        
        if (bkgDiscStr == "")
        {
            AndroidUtils.ShowToast("no can do");
            return;
        }
        print("bkgNameStr is ->" + bkgNameStr);
        print("bkgDiscStr is -> " + bkgDiscStr);
        if (retrieve_one != null || bkgDiscStr != null)
        {
            //genAIBkg完成对于图片的获取与展示，在reGen这个gamobject true的状态下。
            genAIBkg(retrieve_one, bkgNameStr, bkgDiscStr);
        }
        else
        {
            print("genAIBkg无法调用，因为两个值都为空");
        }
    }

    private Texture2D duplicateTexture(Texture2D source)
    {
        print("需要进行图片资源的可读性转换");
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
    void OnGetStringResult(List<string> ok)
    {
        string complete_prompt = ok[0];
        string retrive_one = ok[1];
        Debug.Log("Result from coroutine: " + ok);
        // 在这里处理结果字符串,也就是后续一系列的步骤吧
        //将本地资源转换为Stable Diffusion接收的base64格式
        print("gen bkg by AI...step 2 ... 连接Stable Diffusion");
        // Hajimette
        //获取本地背景资源
        if (retrieve_one == null)
        {
            var url = "http://139.224.214.59:7860/sdapi/v1/txt2img";
            print("定义服务地址与端口,不读取本地资源");
            //定义请求的POST数据体
            print("定义请求的POST数据体 : Description -> " + bkgDisc + " payload -> " + bkgName);
            var requestData_new = new
            {
                prompt = complete_prompt,
                negative_prompt = "human, animal",
                width = 768,
                height = 512,
                steps = 20,
                // alwayson_scripts = new
                // {
                //     controlnet = new
                //     {
                //         args = new[]
                //         {
                //             new
                //             {
                //                 enabled = true,
                //                 model = "control_v11p_sd15_canny [d14c016b]",
                //                 module = "canny",
                //                 weight = 1.3,
                //                 save_detected_map = false,
                //                 input_image = promptimage_temp_code,
                //             }
                //         }
                //     }
                // }
                //不匹配本地图片，不使用controlnet；
            };

            string jsonData_new = JsonConvert.SerializeObject(requestData_new);
            print("发送给服务器的json文本 ：-> " + jsonData_new);
            print("定义服务地址与端口");
            StartCoroutine(SendRequest(url, jsonData_new));
            print("SendRequest OK in no controlnet");
        }
        else
        {
            // 手动构造英文元素列表，用于检查AIGenBkg子文件夹
            List<string> englishElements = new List<string>
            {
                "hukoupubu",
                "aershan",
                "xiaogangcun",
                "kashi",
                "liangjiahe",
                "yanbian",
                "yichun",
                "hainan",
                "liupanshan",
                "tanggula",
                "beishida",
                "xianggang",
                "aomen",
                "zimaoqu"
            };
            // 假设 retrieve_one 是要检查的字符串

            // 检查 retrieve_one 是否在列表中
            Texture2D texture_bla = null;
            // if (englishElements.Contains(retrieve_one))
            //     texture_bla = Resources.Load<Texture2D>("Environment/2D/AIGenBkg/" + retrive_one);
            //
            texture_bla = Resources.Load<Texture2D>("Environment/2D/" + retrive_one);
            print("Success to load texture: " + retrive_one);
            
            if (texture_bla == null)
            {
                print("Failed to load texture: " + retrive_one);
                return;
            }
            
            Texture2D texture = duplicateTexture(texture_bla);
            if (texture == null)
            {
                Debug.LogError("Failed to load texture: " + retrive_one);
                return;
            }
            print("获取本地资源背景成功" + retrive_one);
            byte[] bytes = texture.EncodeToPNG();
            string promptimage_temp_code = Convert.ToBase64String(bytes);
            print("将本地资源转换为了Stable Diffusion接收的base64格式");
            //定义服务地址与端口
            var url = "http://139.224.214.59:7860/sdapi/v1/txt2img";
            Console.WriteLine("请输入背景描述：");
            // Console.WriteLine(bkgDisc);
            Console.WriteLine("继续生成");
            print("定义服务地址与端口");
            //定义请求的POST数据体
            print("定义请求的POST数据体 : Description -> " + bkgDisc + " payload -> " + bkgName);
            var requestData = new
                    {
                        prompt = complete_prompt,
                        negative_prompt = "human, animal",
                        width = 768,
                        height = 512,
                        steps = 20,
                        alwayson_scripts = new
                        {
                            controlnet = new
                            {
                                args = new[]
                                {
                                    new
                                    {
                                        enabled = true,
                                        model = "control_v11p_sd15_canny [d14c016b]",
                                        module = "canny",
                                        weight = 1.3,
                                        save_detected_map = false,
                                        input_image = promptimage_temp_code,
                                    }
                                }
                            }
                        }
                    };
                    string jsonData = JsonConvert.SerializeObject(requestData);
                    print("发送给服务器的json文本 ：-> " + jsonData);
                    print("定义服务地址与端口");
                    StartCoroutine(SendRequest(url, jsonData));
                    print("SendRequest OK");
        }
    }
    IEnumerator GetUrlContent(string url, string retrieve_one, StringCallback callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 6000;
            www.SetRequestHeader("Content-Type", "text/html;charset=UTF-8");
            
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                print(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                JToken token = JToken.Parse(jsonResponse);
                string dstValue = token["trans_result"][0]["dst"].ToString();
                print("final answer -> " + dstValue);
                print(dstValue);
                callback(new List<string> {dstValue, retrieve_one}); // 将结果传递给回调函数
            }
        }
    }
    void genAIBkg(string retrive_one, string bkgName, string bkgDisc)
    {
        if (retrive_one == null && bkgName == null)
        {
            AndroidUtils.ShowToast("第二步选择背景时，请选择一种方式，来自基础背景，或者新建");
            return;
        }

        if ( bkgName == null)
        {
            bkgName = retrieve_one;

        }
        print("Generate bkg by AI...step 1... translate into English;");
        string q = bkgName + ", " + bkgDisc;
        // 原文
        // string q = "苹果和香蕉打了一架";
        // 源语言
        string from = "zh";
        // 目标语言
        string to = "en";
        // 改成您的APP ID
        string appId = "20231102001867737";
        // Random rd = new Random();
        // string salt = rd.Next(100000).ToString();
        string salt = Range(0, 100000).ToString();
        // 改成您的密钥
        string secretKey = "ImzS6kiVE3ixQLt9RmzA";
        string sign = EncryptString(appId + q + salt + secretKey);
        string translateURL = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
        translateURL += "q=" + UnityWebRequest.EscapeURL(q);
        translateURL += "&from=" + from;
        translateURL += "&to=" + to;
        translateURL += "&appid=" + appId;
        translateURL += "&salt=" + salt;
        translateURL += "&sign=" + sign;
        StartCoroutine(GetUrlContent(translateURL,retrive_one,OnGetStringResult));
    }

    public IEnumerator SendRequest(string url, string jsonData)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.timeout = 120;
        print("构建好了www");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            // Debug.LogError("Failed to send request: " + www.error);
            print("Failed to send request: " + www.error);
        }
        else
        {
            // 处理服务器返回的数据
            print("Received data: " + www.downloadHandler.text);
            JObject r = JObject.Parse(www.downloadHandler.text);
            JArray images = (JArray)r["images"];
            print("拿到了最后的返回数据");
            foreach (JToken imageToken in images)
            {
                string base64Image = imageToken.ToString().Split(',')[0];
                print(" 处理base64Image的JToken   " + base64Image);
                byte[] imageBytes = Convert.FromBase64String(base64Image.Replace(" ", "+"));

                // 检验，或者构建保存路径
                string folderPath = Path.Combine(Application.persistentDataPath, "images");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string savePath = Path.Combine(folderPath, bkgNameStr + ".png");

                // 保存图片
                File.WriteAllBytes(savePath, imageBytes);
                print("生成完毕，保存路径：" + savePath + ".png");
                
                yield return StartCoroutine(LoadImage(savePath));

                //这里插入数据库
                // Texture2D rawImageTex = new Texture2D((int)toShow.rectTransform.rect.width, (int)toShow.rectTransform.rect.height);
                // rawImageTex.LoadImage(imageBytes);
                // rawImageTex.Apply();
                // saveUserMat.saveBkg(Server.getMatCreator(), bkgNameStr, rawImageTex);
            }
        }
    }

    IEnumerator LoadImage(string filePath)
    {
        ReGenHint.gameObject.SetActive(false);
        // 等待文件写入完成
        yield return new WaitForEndOfFrame();

        // 加载图片
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        // 创建Sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        // 将Sprite设置到Image组件上
        toShow.sprite = sprite;

        
    }

    public class UserData
    {
        public byte[] filedata;
    }
    IEnumerator call_sam_handler(string creatorId, byte[] fileData)
    {
        creatorId = Server.getMatCreator() + "_" + bkgNameStr;
        string uploadUrl = $"http://139.224.214.59:7862/processing-agent?creatorId={creatorId}&image_name={bkgNameStr}";
        // Read the file data
        // Create a form to send the data
        print("bkgnamrstr is ->" + bkgNameStr);
        var user = new UserData();
        user.filedata = fileData;
        WWWForm form = new WWWForm();
        form.AddBinaryData("input_image",fileData);
        using (UnityWebRequest req = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error While Sending: " + req.error);
            }
            else
            {
                Debug.Log("Upload complete! Response: " + req.downloadHandler.text);
            }
        }
        // string json = JsonUtility.ToJson(user);
        // var req = new UnityWebRequest(uploadUrl,"POST");
        // byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        // req.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        // req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        // req.SetRequestHeader("Content-Type", "multipart/form-data");

        // Create a UnityWebRequest
       
    }
    
    public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }


    



}

[System.Serializable]
public class ControlNetRequestData
{
    public string prompt { get; set; }
    public string negative_prompt{ get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int steps { get; set; }
    public AlwaysonScripts alwayson_scripts { get; set; }
}

[System.Serializable]
public class AlwaysonScripts
{
    public ControlNetArgs[] controlnet { get; set; }
}

[System.Serializable]
public class ControlNetArgs
{
    public bool enabled { get; set; }
    public string model { get; set; }
    public string module { get; set; }
    public float weight { get; set; }
    public bool save_detected_map { get; set; }
    public string input_image { get; set; }
}