using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

[System.Serializable]
public class PreviewItem {
    public string Name;
    public string KeyName;
    public string Type;
    public int ID;//增加ID项
    public string Url;
}
[System.Serializable]
public class PreviewResults
{
    public PreviewItem[] Results;
}

public class SelectBkgRole: MonoBehaviour
{

    public MicroPhoneSingleView microPhoneSingleView;
    public Image recordButtonProgressBar;
    public DateTime recordStartTime;

    List<PreviewItem> bkgList = new List<PreviewItem>();
    int sceneNum;
    int bkgStartIndex = 0;
    int bkgShowStartIndex = 0;

    List<PreviewItem> roleList = new List<PreviewItem>();
    int roleNum;
    int roleStartIndex=0;
    int roleShowStartIndex = 0;

    List<PreviewItem> objList = new List<PreviewItem>();
    int objNum;
    int objStartIndex = 0;

    List<int> selectBkg = new List<int>();
    List<int> selectRole = new List<int>();
    List<int> selectObj = new List<int>();
    public List<Button> bkgSelectRects;
    public List<Button> roleSelectRects;

    // Start is called before the first frame update

    int selectBkgNum = 0;
    List<GameObject> bkgShowList=new List<GameObject>();
    int selectRoleNum = 0;
    List<GameObject> roleShowList=new List<GameObject>();

    string recordingUrl;
    string toRecordUrl;
    string redoUrl;
    string redoGrayUrl;
    public Button returnButton;
    public bool isEndingRecord = false, isRecording = false;
    public Button RecordBtn, RedoBtn, FinishBtn;
    public Button BackSelectBtn;
    static bool couldUseRedo = false;
    bool selectObject = false;
    public Button SwitchRolePage, SwitchObjectPage;
    public Sprite rolePage, objPage;
    public Image pageBackground;


    public static Action testingFunction;
    public static void setTestingFunction(Action action)
    {
        testingFunction = action;
    }


    void Start()
    {
        selectObject = false;
        StartCoroutine(PrepareBackgrounds());
        StartCoroutine(PrepareRoles());
        StartCoroutine(PrepareObjs());
        recordingUrl = "RecordPage/recording";
        toRecordUrl = "RecordPage/record";
        redoUrl = "RecordPage/redo";
        redoGrayUrl = "RecordPage/redo_gray";
        
        RedoBtn.enabled = true;
        RecordBtn.enabled = true;
        FinishBtn.enabled = true;
        RedoBtn.interactable = true;
        RecordBtn.interactable = true;
        FinishBtn.interactable = true;

        returnButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SceneMainMenu");
        });
        recordButtonProgressBar.gameObject.SetActive(false);
        recordButtonProgressBar.gameObject.AddComponent<Button>().onClick.AddListener(Record);
        SwitchRolePage.onClick.AddListener(() => { selectObject = false; ShowRole(); RoleBtn(); pageBackground.sprite = rolePage; });
        SwitchObjectPage.onClick.AddListener(() => { selectObject = true; ShowObj(); RoleBtn(); pageBackground.sprite = objPage; });

        if (TestRunner.TestingMode)
        {
            testingFunction();
        }
    }

    void Update()
    {
        if (recordButtonProgressBar.IsActive())
        {
            if (DateTime.Now - recordStartTime < TimeSpan.FromSeconds(15))
            {
                recordButtonProgressBar.fillAmount = (float)((DateTime.Now - recordStartTime).TotalMilliseconds / TimeSpan.FromSeconds(15).TotalMilliseconds);
            }
            else
            {
                // Stop record
                AndroidUtils.ShowToast("录音时长已达上限，请稍后再继续");
                Record();
            }
        }
        RedoBtn.enabled = (!AnimGenerator.Interpreter.isPlayingAudio && !isRecording && couldUseRedo);
        RecordBtn.enabled = !AnimGenerator.Interpreter.isPlayingAudio;
        FinishBtn.enabled = !AnimGenerator.Interpreter.isPlayingAudio && !isRecording;
        RedoBtn.interactable = (!AnimGenerator.Interpreter.isPlayingAudio && !isRecording && couldUseRedo);
        RecordBtn.interactable = !AnimGenerator.Interpreter.isPlayingAudio;
        FinishBtn.interactable = !AnimGenerator.Interpreter.isPlayingAudio && !isRecording;
        BackSelectBtn.enabled = (!AnimGenerator.Interpreter.isPlayingAudio && !isRecording);
        BackSelectBtn.interactable = (!AnimGenerator.Interpreter.isPlayingAudio && !isRecording);
    }

    IEnumerator PrepareRoles()
    {
        List<PreviewItem> characterList = new List<PreviewItem>();
        List<PreviewItem> animalList = new List<PreviewItem>();

        yield return StartCoroutine(FetchPreviewData("Character", characterList));
        yield return StartCoroutine(FetchPreviewData("Animal", animalList));
        characterList.Sort((a, b) =>
        {
            return a.ID <= b.ID ? -1 : 1;
        });
        animalList.Sort((a, b) =>
        {
            return a.ID <= b.ID ? -1 : 1;
        });

        roleList = characterList.Concat(animalList).ToList();
        roleNum = roleList.Count;
        Debug.Log(roleNum);
        ShowRole();
        for (int i = 0; i < 7; i++)
        {
            roleSelectRects.Add(GameObject.Find("Role" + (i + 1).ToString() + "/rect").GetComponent<Button>());
            roleSelectRects[i].onClick.AddListener(ChangeChoosedRole);
        }
    }

    IEnumerator PrepareObjs()
    {
        yield return StartCoroutine(FetchPreviewData("Object", objList));
        objList.Sort((a, b) =>
        {
            if (a.ID == b.ID)
            {
                return 0;
            }
            return a.ID < b.ID ? -1 : 1;
        });
        objNum = objList.Count;
    }

    IEnumerator PrepareBackgrounds()
    {
        yield return StartCoroutine(FetchPreviewData("Background", bkgList));
        bkgList.Sort((a, b) =>
        {
            return a.ID <= b.ID ? -1 : 1;
        });
        sceneNum = bkgList.Count;
        //selectBkg.Add(0);
        ShowBackground();
        for (int i = 0; i < 3; i++)
        {
            bkgSelectRects.Add(GameObject.Find("Pic" + (i + 1).ToString() + "/rect").GetComponent<Button>());
            bkgSelectRects[i].onClick.AddListener(ChangeChoosedBkg);
        }
    }

    IEnumerator FetchPreviewData(string type, List<PreviewItem> results)
    {
        UnityWebRequest www = UnityWebRequest.Get(Server.HOST+"api/data/preview/" + type);
        www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
        yield return www.SendWebRequest();
        Debug.Log(Server.HOST+"api/data/preview/" + type);
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(JsonUtility.FromJson<PreviewResults>(www.downloadHandler.text));
            results.AddRange(JsonUtility.FromJson<PreviewResults>(www.downloadHandler.text).Results);
            Debug.Log(results.Count);
        }
    }

    private void ChangeChoosedBkg()
    {
        GameObject clickedBtn = EventSystem.current.currentSelectedGameObject;
        string name=clickedBtn.GetComponent<Transform>().parent.gameObject.name;
        int number = int.Parse(name.Substring(name.Length - 1, 1))-1+bkgStartIndex;
        if(selectBkg.Contains(number))
        {
            selectBkg.Remove(number);
        }
        else
        {
            selectBkg.Add(number);
        }
        ShowBackground();
    }
    private void ChangeChoosedRole()
    {
        if (selectObject)
        {
            GameObject clickedBtn = EventSystem.current.currentSelectedGameObject;
            string name = clickedBtn.GetComponent<Transform>().parent.gameObject.name;
            Debug.Log(name);
            int number = int.Parse(name.Substring(name.Length - 1, 1)) - 1 + objStartIndex;
            if (selectObj.Contains(number))
            {
                selectObj.Remove(number);
            }
            else
            {
                selectObj.Add(number);
            }
            ShowObj();
        } else
        {
            GameObject clickedBtn = EventSystem.current.currentSelectedGameObject;
            string name = clickedBtn.GetComponent<Transform>().parent.gameObject.name;
            Debug.Log(name);
            int number = int.Parse(name.Substring(name.Length - 1, 1)) - 1 + roleStartIndex;
            if (selectRole.Contains(number))
            {
                selectRole.Remove(number);
            }
            else
            {
                selectRole.Add(number);
            }
            ShowRole();
        }
    }
    void BkgBtn()
    {
        if (bkgStartIndex == 0)
        {
            GameObject.Find("first/Left").GetComponent<Button>().enabled = false;
            GameObject.Find("first/Left").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("first/Left").GetComponent<Button>().enabled = true;
            GameObject.Find("first/Left").GetComponent<Button>().interactable = true;
        }
        if (bkgStartIndex + 3 == sceneNum)
        {
            GameObject.Find("first/Right").GetComponent<Button>().enabled = false;
            GameObject.Find("first/Right").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("first/Right").GetComponent<Button>().enabled = true;
            GameObject.Find("first/Right").GetComponent<Button>().interactable = true;
        }
    }
    public void LastBkg()
    {
        bkgStartIndex -= 1;
        BkgBtn();
        ShowBackground();
    }
    public void NextBkg()
    {
        bkgStartIndex += 1;
        BkgBtn();
        ShowBackground();
    }
    void ShowBackground()
    {
        for (int i = 0; i < 3; i++)
        {
            string picUrl = bkgList[i + bkgStartIndex].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            Debug.Log(picUrl);
            Texture2D texture = Resources.Load<Texture2D>(picUrl);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            GameObject.Find("Pic" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = sprite;
            GameObject.Find("Pic" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = bkgList[i + bkgStartIndex].KeyName;
            var rect = GameObject.Find("Pic" + (i + 1).ToString() + "/rect");
            var text = GameObject.Find("Pic" + (i + 1).ToString() + "/Text");
            /*Debug.Log(i);
            Debug.Log(bkgStartIndex);
            Debug.Log("length" + selectBkg.Count);*/
            if (selectBkg.Contains(i+bkgStartIndex))
            {
                rect.GetComponent<CanvasGroup>().alpha = 1;
                text.GetComponent<Text>().color = Color.white;
                //rect.GetComponent<Button>().enabled = true;
            }
            else
            {
                rect.GetComponent<CanvasGroup>().alpha = 0;
                text.GetComponent<Text>().color = new Color32(202, 150, 117, 255);
                //rect.GetComponent<Button>().enabled = false;
            }
        }

    }
    void RoleBtn()
    {
        if (selectObject)
        {
            if (objStartIndex == 0)
            {
                GameObject.Find("second/Left").GetComponent<Button>().enabled = false;
                GameObject.Find("second/Left").GetComponent<Button>().interactable = false;
            }
            else
            {
                GameObject.Find("second/Left").GetComponent<Button>().enabled = true;
                GameObject.Find("second/Left").GetComponent<Button>().interactable = true;
            }
            if (objStartIndex + 7 == objNum)
            {
                GameObject.Find("second/Right").GetComponent<Button>().enabled = false;
                GameObject.Find("second/Right").GetComponent<Button>().interactable = false;
            }
            else
            {
                GameObject.Find("second/Right").GetComponent<Button>().enabled = true;
                GameObject.Find("second/Right").GetComponent<Button>().interactable = true;
            }
        }
        else
        {
            if (roleStartIndex == 0)
            {
                GameObject.Find("second/Left").GetComponent<Button>().enabled = false;
                GameObject.Find("second/Left").GetComponent<Button>().interactable = false;
            }
            else
            {
                GameObject.Find("second/Left").GetComponent<Button>().enabled = true;
                GameObject.Find("second/Left").GetComponent<Button>().interactable = true;
            }
            if (roleStartIndex + 7 == roleNum)
            {
                GameObject.Find("second/Right").GetComponent<Button>().enabled = false;
                GameObject.Find("second/Right").GetComponent<Button>().interactable = false;
            }
            else
            {
                GameObject.Find("second/Right").GetComponent<Button>().enabled = true;
                GameObject.Find("second/Right").GetComponent<Button>().interactable = true;
            }
        }
    }
    public void LastRole()
    {
        if (selectObject)
        {
            objStartIndex -= 1;
            RoleBtn();
            ShowObj();
        } else
        {
            roleStartIndex -= 1;
            RoleBtn();
            ShowRole();
        }
    }
    public void NextRole()
    {
        if (selectObject)
        {
            objStartIndex += 1;
            RoleBtn();
            ShowObj();
        }
        else
        {
            roleStartIndex += 1;
            RoleBtn();
            ShowRole();
        }
    }

    void ShowRole()
    {
        for (int i = 0; i < 7; i++)
        {
            string picUrl = roleList[i + roleStartIndex].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            //Debug.Log(picUrl);
            GameObject.Find("Role"+(i+1).ToString()+"/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
            GameObject.Find("Role" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = roleList[i + roleStartIndex].KeyName;
            var rect = GameObject.Find("Role" + (i + 1).ToString() + "/rect");
            var text = GameObject.Find("Role" + (i + 1).ToString() + "/Text");
            if (selectRole.Contains(i + roleStartIndex))
            {
                rect.GetComponent<CanvasGroup>().alpha = 1;
                text.GetComponent<Text>().color = Color.white;
                //rect.GetComponent<Button>().enabled = true;
            }
            else
            {
                rect.GetComponent<CanvasGroup>().alpha = 0;
                text.GetComponent<Text>().color = new Color32(202,150,117,255);
                //rect.GetComponent<Button>().enabled = false;
            }
        }
    }

    void ShowObj()
    {
        for (int i = 0; i < 7; i++)
        {
            string picUrl = objList[i + objStartIndex].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            GameObject.Find("Role" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
            GameObject.Find("Role" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = objList[i + objStartIndex].KeyName;
            var rect = GameObject.Find("Role" + (i + 1).ToString() + "/rect");
            var text = GameObject.Find("Role" + (i + 1).ToString() + "/Text");
            if (selectObj.Contains(i + objStartIndex))
            {
                rect.GetComponent<CanvasGroup>().alpha = 1;
                text.GetComponent<Text>().color = Color.white;
            }
            else
            {
                rect.GetComponent<CanvasGroup>().alpha = 0;
                text.GetComponent<Text>().color = new Color32(202, 150, 117, 255);
            }
        }
    }

    void RecordPageBkgBtn()
    {
        if (bkgShowStartIndex == 0)
        {
            GameObject.Find("bkg/Left").GetComponent<Button>().enabled = false;
            GameObject.Find("bkg/Left").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("bkg/Left").GetComponent<Button>().enabled = true;
            GameObject.Find("bkg/Left").GetComponent<Button>().interactable = true;
        }
        if (bkgShowStartIndex + 1 == selectBkgNum)
        {
            GameObject.Find("bkg/Right").GetComponent<Button>().enabled = false;
            GameObject.Find("bkg/Right").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("bkg/Right").GetComponent<Button>().enabled = true;
            GameObject.Find("bkg/Right").GetComponent<Button>().interactable = true;
        }
    }
    public void RecordPageLastBkg()
    {
        bkgShowStartIndex -= 1;
        RecordPageBkgBtn();
        ShowRecordPageBackground();
    }
    public void RecordPageNextBkg()
    {
        bkgShowStartIndex += 1;
        RecordPageBkgBtn();
        ShowRecordPageBackground();
    }
    void RecordPageRoleBtn()
    {
        if (roleShowStartIndex == 0)
        {
            GameObject.Find("role/Left").GetComponent<Button>().enabled = false;
            GameObject.Find("role/Left").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("role/Left").GetComponent<Button>().enabled = true;
            GameObject.Find("role/Left").GetComponent<Button>().interactable = true;
        }
        if (roleShowStartIndex + 2 >= selectRoleNum)
        {
            GameObject.Find("role/Right").GetComponent<Button>().enabled = false;
            GameObject.Find("role/Right").GetComponent<Button>().interactable = false;
        }
        else
        {
            GameObject.Find("role/Right").GetComponent<Button>().enabled = true;
            GameObject.Find("role/Right").GetComponent<Button>().interactable = true;
        }
        //下一页只剩一个可显示
        if(roleShowStartIndex+1== selectRoleNum)
        {
            GameObject.Find("role/Image2").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("role/Text2").GetComponent<CanvasGroup>().alpha = 0;
        }
        else
        {
            GameObject.Find("role/Image2").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("role/Text2").GetComponent<CanvasGroup>().alpha = 1;
        }
    }

    public void RecordPageLastRole()
    {
        roleShowStartIndex -= 2;
        RecordPageRoleBtn();
        ShowRecordPageRole();
    }
    public void RecordPageNextRole()
    {
        roleShowStartIndex += 2;
        RecordPageRoleBtn();
        ShowRecordPageRole();
    }
    void ShowRecordPageBackground()
    {
        if (selectBkgNum == 0)
            return;
        string picUrl = bkgList[selectBkg[bkgShowStartIndex]].Url;
        picUrl = picUrl.Substring(10);
        picUrl = picUrl.Remove(picUrl.Length - 4);
        //Debug.Log(picUrl);
        Texture2D texture = Resources.Load<Texture2D>(picUrl);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        GameObject.Find("bkg/Image").GetComponent<CircleImage>().sprite = sprite;
        GameObject.Find("bkg/Text").GetComponent<Text>().text = bkgList[selectBkg[bkgShowStartIndex]].KeyName;
    }
    void ShowRecordPageRole()
    {
        if(selectRoleNum!=0)
        {
            string picUrl = roleList[selectRole[roleShowStartIndex]].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            //Debug.Log(picUrl);
            GameObject.Find("role/Image1").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
            GameObject.Find("role/Text1").GetComponent<Text>().text = roleList[selectRole[roleShowStartIndex]].KeyName;
        }
        
        if(roleShowStartIndex +2<=selectRoleNum)
        {
            string picUrl = roleList[selectRole[roleShowStartIndex+1]].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            //Debug.Log(picUrl);
            GameObject.Find("role/Image2").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
            GameObject.Find("role/Text2").GetComponent<Text>().text = roleList[selectRole[roleShowStartIndex+1]].KeyName;
        }
    }

    void ShowRecordPage()
    {
        //Background
        selectBkgNum = selectBkg.Count;
        var bkgimg = GameObject.Find("bkg/Image");
        var bkgtext = GameObject.Find("bkg/Text");
        if(selectBkgNum==0)
        {
            bkgimg.GetComponent<CanvasGroup>().alpha = 0;
            bkgimg.GetComponent<CanvasGroup>().interactable = false;
            bkgimg.GetComponent<CanvasGroup>().blocksRaycasts = false ;
            bkgtext.GetComponent<CanvasGroup>().alpha = 0;
            bkgtext.GetComponent<CanvasGroup>().interactable = false;
            bkgtext.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            bkgimg.GetComponent<CanvasGroup>().alpha = 1;
            bkgimg.GetComponent<CanvasGroup>().interactable = true;
            bkgimg.GetComponent<CanvasGroup>().blocksRaycasts = true;
            bkgtext.GetComponent<CanvasGroup>().alpha = 1;
            bkgtext.GetComponent<CanvasGroup>().interactable = true;
            bkgtext.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        if (selectBkgNum<=1)
        {
            GameObject.Find("bkg/Left").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("bkg/Left").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("bkg/Left").GetComponent<CanvasGroup>().blocksRaycasts = false;
            GameObject.Find("bkg/Right").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("bkg/Right").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("bkg/Right").GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            GameObject.Find("bkg/Left").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("bkg/Left").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("bkg/Left").GetComponent<CanvasGroup>().blocksRaycasts = true;
            GameObject.Find("bkg/Right").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("bkg/Right").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("bkg/Right").GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        ShowRecordPageBackground();
        //Role
        selectRoleNum = selectRole.Count;
        Debug.Log(selectRoleNum);
        var roleimg1 = GameObject.Find("role/Image1");
        var roleimg2 = GameObject.Find("role/Image2");
        var roletext1 = GameObject.Find("role/Text1");
        var roletext2 = GameObject.Find("role/Text2");
        if (selectRoleNum <= 2)
        {
            GameObject.Find("role/Left").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("role/Left").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("role/Left").GetComponent<CanvasGroup>().blocksRaycasts = false;
            GameObject.Find("role/Right").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.Find("role/Right").GetComponent<CanvasGroup>().interactable = false;
            GameObject.Find("role/Right").GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            GameObject.Find("role/Left").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("role/Left").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("role/Left").GetComponent<CanvasGroup>().blocksRaycasts = true;
            GameObject.Find("role/Right").GetComponent<CanvasGroup>().alpha = 1;
            GameObject.Find("role/Right").GetComponent<CanvasGroup>().interactable = true;
            GameObject.Find("role/Right").GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        if(selectRoleNum==0)
        {
            roleimg1.GetComponent<CanvasGroup>().alpha = 0;
            roleimg1.GetComponent<CanvasGroup>().interactable = false;
            roleimg1.GetComponent<CanvasGroup>().blocksRaycasts = false;
            roletext1.GetComponent<CanvasGroup>().alpha = 0;
            roletext1.GetComponent<CanvasGroup>().interactable = false;
            roletext1.GetComponent<CanvasGroup>().blocksRaycasts = false;
            roleimg2.GetComponent<CanvasGroup>().alpha = 0;
            roleimg2.GetComponent<CanvasGroup>().interactable = false;
            roleimg2.GetComponent<CanvasGroup>().blocksRaycasts = false;
            roletext2.GetComponent<CanvasGroup>().alpha = 0;
            roletext2.GetComponent<CanvasGroup>().interactable = false;
            roletext2.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else if(selectRoleNum==1)
        {
            roleimg1.GetComponent<CanvasGroup>().alpha = 1;
            roleimg1.GetComponent<CanvasGroup>().interactable = true;
            roleimg1.GetComponent<CanvasGroup>().blocksRaycasts = true;
            roletext1.GetComponent<CanvasGroup>().alpha = 1;
            roletext1.GetComponent<CanvasGroup>().interactable = true;
            roletext1.GetComponent<CanvasGroup>().blocksRaycasts = true;
            roleimg2.GetComponent<CanvasGroup>().alpha = 0;
            roleimg2.GetComponent<CanvasGroup>().interactable = false;  
            roleimg2.GetComponent<CanvasGroup>().blocksRaycasts = false;
            roletext2.GetComponent<CanvasGroup>().alpha = 0;
            roletext2.GetComponent<CanvasGroup>().interactable = false;
            roletext2.GetComponent <CanvasGroup>().blocksRaycasts = false;
            roleimg1.GetComponent<Transform>().localPosition = new Vector3(0,9,0);
            roletext1.GetComponent<Transform>().localPosition = new Vector3(0,-66,0);
            
        }
        else
        {
            roleimg2.GetComponent<CanvasGroup>().alpha = 1;
            roleimg2.GetComponent<CanvasGroup>().interactable = true;
            roleimg2.GetComponent<CanvasGroup>().blocksRaycasts = true;
            roletext2.GetComponent<CanvasGroup>().alpha = 1;
            roletext2.GetComponent<CanvasGroup>().interactable = true;
            roletext2.GetComponent<CanvasGroup>().blocksRaycasts = true;
            roleimg1.GetComponent<Transform>().localPosition = new Vector3(0, 93, 0);
            roletext1.GetComponent<Transform>().localPosition = new Vector3(0, 18, 0);
            roleimg2.GetComponent<Transform>().localPosition = new Vector3(0, -84, 0);
            roletext2.GetComponent<Transform>().localPosition = new Vector3(0, -159, 0);
        }
        ShowRecordPageRole();
    }
    /*void ShowRecordPage()
    {
        //bkgShowList.Clear();
        //Background
        selectBkgNum = selectBkg.Count;
        var bkg1 = GameObject.Find("Bkg1");
        bkgShowList.Add(bkg1);
        var bkgTransform=bkg1.GetComponent<Transform>();
        //UnityEditorInternal.ComponentUtility.CopyComponent(bkg1.GetComponent<Transform>());
        for (int i=1;i<selectBkgNum;i++)
        {
            var bkgCopy = GameObject.Instantiate(bkg1);
            bkgCopy.name = "Bkg" + (i + 1).ToString();
            bkgShowList.Add(bkgCopy);
            bkgCopy.transform.SetParent(bkg1.transform.parent);
            bkgCopy.transform.position=bkgTransform.position;
            bkgCopy.transform.localScale = bkgTransform.localScale;
            //UnityEditorInternal.ComponentUtility.PasteComponentValues(bkgCopy.GetComponent<Transform>());
        }
        Debug.Log(bkgShowList.Count);
        for (int i = 0; i < selectBkgNum; i++)
        {
            string picUrl = bkgList[selectBkg[i]].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            Texture2D texture = Resources.Load<Texture2D>(picUrl);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            GameObject.Find("Bkg" + (i+1).ToString() + "/Image").GetComponent<CircleImage>().sprite = sprite;
            GameObject.Find("Bkg" + (i+1).ToString() + "/Text").GetComponent<Text>().text = bkgList[selectBkg[i]].KeyName;

        }
        //Role
        selectRoleNum = selectRole.Count;
        var role1 = GameObject.Find("RolePic1");
        roleShowList.Add(role1);
        var roleTransform=role1.GetComponent<Transform>();
        //UnityEditorInternal.ComponentUtility.CopyComponent(role1.GetComponent<Transform>());
        for (int i = 1; i < selectRoleNum; i++)
        {
            var roleCopy = GameObject.Instantiate(role1);
            roleCopy.name = "RolePic" + (i + 1).ToString();
            roleShowList.Add(roleCopy);
            roleCopy.transform.SetParent(role1.transform.parent);
            roleCopy.transform.position=roleTransform.position;
            roleCopy.transform.localScale= roleTransform.localScale;
            //UnityEditorInternal.ComponentUtility.PasteComponentValues(roleCopy.GetComponent<Transform>());
        }
        Debug.Log(roleShowList.Count);
        for (int i = 0; i < selectRoleNum; i++)
        {
            string picUrl = roleList[selectRole[i]].Url;
            picUrl = picUrl.Substring(10);
            picUrl = picUrl.Remove(picUrl.Length - 4);
            GameObject.Find("RolePic" + (i + 1).ToString() + "/Image").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
            GameObject.Find("RolePic" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = roleList[selectRole[i]].KeyName;

        }
    }*/
    public void NextPage()
    {
        Debug.Log("ShowRecordPage");
        if (!AsrMain.isJiebaLoaded)
        {
            AndroidUtils.ShowToast("系统数据加载中，请稍后再试");
            return;
        }
        AnimGenerator.Interpreter.isPlayingAudio = false;
        Debug.Log("Next");
        GameObject.Find("1-02").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("1-02").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("1-02").GetComponent<GraphicRaycaster>().enabled = false;
        GameObject.Find("1-03").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("1-03").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("1-03").GetComponent<GraphicRaycaster>().enabled = true;
        selectBkg.Sort();
        selectRole.Sort();
        
        ShowRecordPage();
    }
    public void BackSelect()
    {
        Debug.Log("Back");
        Debug.Log("isRecording:" + isRecording + ", isPlayingAudio:" + AnimGenerator.Interpreter.isPlayingAudio);
        /*for(int i=1;i<selectBkgNum;i++)
        {
            GameObject.Destroy(bkgShowList[i]);
        }
        for(int i=1;i<selectRoleNum;i++)
        {
            GameObject.Destroy(roleShowList[i]);
        }
        bkgShowList.Clear();
        roleShowList.Clear();*/
        bkgShowStartIndex = 0;
        roleShowStartIndex = 0;
        GameObject.Find("bkg/Left").GetComponent<Button>().enabled = false;
        GameObject.Find("bkg/Right").GetComponent<Button>().enabled = true;
        GameObject.Find("role/Left").GetComponent<Button>().enabled = false;
        GameObject.Find("role/Right").GetComponent<Button>().enabled = true;
        GameObject.Find("bkg/Left").GetComponent<Button>().interactable = false;
        GameObject.Find("bkg/Right").GetComponent<Button>().interactable = true;
        GameObject.Find("role/Left").GetComponent<Button>().interactable = false;
        GameObject.Find("role/Right").GetComponent<Button>().interactable = true;
        GameObject.Find("1-02").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("1-02").GetComponent<CanvasGroup>().interactable = true;
        GameObject.Find("1-02").GetComponent<GraphicRaycaster>().enabled = true;
        GameObject.Find("1-03").GetComponent<CanvasGroup>().alpha = 0;
        GameObject.Find("1-03").GetComponent<CanvasGroup>().interactable = false;
        GameObject.Find("1-03").GetComponent<GraphicRaycaster>().enabled = false;
    }

    public void Record()
    {
        var recordBtn = RecordBtn;
        var btntext = GameObject.Find("RecordText").GetComponent<Text>();
        if(btntext.text.Equals("录音"))
        {
            recordBtn.GetComponent<Image>().sprite = Resources.Load(recordingUrl, typeof(Sprite)) as Sprite;
            btntext.text = "正在录音";
            Debug.Log("开始录音");
            //TODO：开始录音
            microPhoneSingleView.StartRecord();
            recordStartTime = DateTime.Now;
            recordButtonProgressBar.gameObject.SetActive(true);
            isRecording = true;
        }
        else
        {
            if (isEndingRecord)
            {
                return;
            }
            isEndingRecord = true;
            Debug.Log("结束录音");
            AndroidUtils.ShowToast("录音结束，正在处理……");
            recordButtonProgressBar.gameObject.SetActive(false);
            StartCoroutine(WaitForRecordFinished());
            couldUseRedo = true;
        }
    }

    IEnumerator WaitForRecordFinished()
    {
        var recordBtn = RecordBtn;
        var btntext = GameObject.Find("RecordText").GetComponent<Text>();
        yield return new WaitForSeconds(1);
        microPhoneSingleView.StopRecord();
        recordBtn.GetComponent<Image>().sprite = Resources.Load(toRecordUrl, typeof(Sprite)) as Sprite;
        btntext.text = "录音";
        RedoBtn.GetComponent<Image>().sprite = Resources.Load(redoUrl, typeof(Sprite)) as Sprite;
        isEndingRecord = false;
        isRecording = false;
    }

    //如果当前录音列表已经清空，则RedoBtn和FinishBtn应设置为不可响应，否则，应为true
    //初次进入页面时已设置默认无法响应
    ////RedoBtn.GetComponent<Button>().enabled=false;
    ////FinishBtn.GetComponent<Button>().enabled = false;

    public void Redo()
    {
        Debug.Log("撤销");
        RedoBtn.interactable = false;
        RedoBtn.GetComponent<Image>().sprite = Resources.Load(redoGrayUrl, typeof(Sprite)) as Sprite;
        couldUseRedo = false;
        //TODO：撤销上一句
    }

    public void Finish()
    {
        Debug.Log("完成");
        //TODO：生成总动画并跳转下一页
    }


}
