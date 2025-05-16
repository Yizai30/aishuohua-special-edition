using AnimGenerator;
using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class SceneSelectMaterials : MonoBehaviour
{
    public Dropdown dropdown;
    public Button SwitchRolePage, SwitchBkgPage, SwitchObjectPage;
    public GameObject RolePageItems, BkgPageItems, ObjPageItems;
    public GameObject selectMaterials, createStory, rolesAndObjs, subtitleCanvas;
    public Button showSelectedItems, closeSelectedItems;
    public Button left, right;
    public Button returnMainMenu, startCreation;
    public Sprite rolePage, bkgPage, objPage;
    public int currentPage = 0;// 0 -> role 1 -> bkg 2 -> obj
    private const int ROLE = 0, BKG = 1, OBJ = 2;
    private const int ROLE_NUM_PER_PAGE = 4, BKG_NUM_PER_PAGE = 3, OBJ_NUM_PER_PAGE = 4;
    public Image pageBackground;
    private List<int> loadedPreview = new List<int>();
    public static Action testingFunction;
    private List<Button> roleSelectRects = new List<Button>(), bkgSelectRects = new List<Button>(), objSelectRects = new List<Button>();
    public List<PreviewItem> roleList = new List<PreviewItem>();
    public List<PreviewItem> bkgList = new List<PreviewItem>();
    public List<PreviewItem> objList = new List<PreviewItem>();
    int roleNum;
    int roleStartIndex = 0;
    int bkgNum;
    int bkgStartIndex = 0;
    int objNum;
    int objStartIndex = 0;
    public List<int> selectRole = new List<int>();
    public List<int> selectBkg = new List<int>();
    public List<int> selectObj = new List<int>();
    public SceneRolesAndObjs roleAndObjs;
    public Interpreter intp;
    public List<string> activeGOs;

    public string curStyleNum = "0";

    void Start()
    {
        SwitchRolePage.onClick.AddListener(() => {
            currentPage = 0;
            switchPreview(curStyleNum);
        });
        SwitchBkgPage.onClick.AddListener(() => {
            currentPage = 1;
            switchPreview(curStyleNum);
        });
        SwitchObjectPage.onClick.AddListener(() => {
            currentPage = 2;
            switchPreview(curStyleNum);
        });
        left.onClick.AddListener(leftPage);
        right.onClick.AddListener(rightPage);
        returnMainMenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SceneMainMenu");
        });
        startCreation.onClick.AddListener(() =>
        {
            if (!AsrMain.isJiebaLoaded)
            {
                AndroidUtils.ShowToast("ϵͳ���ݼ����У����Ժ�����");
                return;
            }
            AnimGenerator.Interpreter.isPlayingAudio = false;
            setVisible(selectMaterials, false);
            setVisible(createStory, true);
        });
        switchPreview(curStyleNum);
        setVisible(selectMaterials, true);
        setVisible(createStory, false);
        setVisible(rolesAndObjs, false);
        setVisible(subtitleCanvas, false);
        showSelectedItems.onClick.AddListener(() =>
        {
            roleAndObjs.refresh();
            setVisible(selectMaterials, false);
            setVisible(createStory, false);
            setVisible(rolesAndObjs, true);
            setVisible(subtitleCanvas, false);
            activeGOs = new List<string>();
            foreach (var goname in intp.gos.Keys)
            {
                if (intp.gos[goname].activeSelf)
                {
                    activeGOs.Add(goname);
                    intp.gos[goname].SetActive(false);
                }
            }
        });
        closeSelectedItems.onClick.AddListener(() =>
        {
            Debug.Log("closeSelectedItems on click");
            setVisible(selectMaterials, false);
            setVisible(rolesAndObjs, false);
            setVisible(createStory, true);
            setVisible(subtitleCanvas, false);
            foreach (var goname in activeGOs)
            {
                intp.gos[goname].SetActive(true);
            }
        });
        //下拉框
        List<string> styleNameList = new List<string>();
        foreach(Style style in DataMap.styleList.styles)
        {
            styleNameList.Add(style.StyleName);
        }
        
        UpdateDropdownView(styleNameList);
        dropdown.onValueChanged.AddListener(DropdownChange);
        if (TestRunner.TestingMode)
        {
            testingFunction();
        }
    }

    private void UpdateDropdownView(List<string> showNames)
    {
        //清空下下拉框数据
        dropdown.options.Clear();
        Dropdown.OptionData tempData;
        for (int i = 0; i < showNames.Count; i++)
        {
            tempData = new Dropdown.OptionData();
            tempData.text = showNames[i];
            dropdown.options.Add(tempData);
        }
        //把第一条数据显示为默认
        dropdown.captionText.text = showNames[0];
    }

    private void DropdownChange(int index)
    {
        print("dropdown changed" + index);
        curStyleNum = index.ToString();
        switchPreview(curStyleNum);
    }

    public static void setTestingFunction(Action action)
    {
        testingFunction = action;
    }

    public void setVisible(GameObject go, bool visible)
    {
        if (go.GetComponent<CanvasGroup>() != null)
        {
            go.GetComponent<CanvasGroup>().alpha = visible ? 1 : 0;
            go.GetComponent<CanvasGroup>().interactable = visible;
            go.GetComponent<CanvasGroup>().blocksRaycasts = visible;
        }
        go.GetComponent<GraphicRaycaster>().enabled = visible;
    }


   
    void switchPreview(string styleNum)
    {

        //if (!loadedPreview.Contains(currentPage))
        //{
        //    loadedPreview.Add(currentPage);
        //    StartCoroutine(preparePreview());
        //}
        RolePageItems.SetActive(false);BkgPageItems.SetActive(false); ObjPageItems.SetActive(false);
        //delClick();
        
        switch (currentPage)
        {
            case ROLE:
                pageBackground.sprite = rolePage;
                RolePageItems.SetActive(true);
                break;
            case BKG:
                pageBackground.sprite = bkgPage;
                BkgPageItems.SetActive(true);
                break;
            case OBJ:
                pageBackground.sprite = objPage;
                ObjPageItems.SetActive(true);
                break;
        }
        StartCoroutine(preparePreview(styleNum));
        updateLeftRightButton();
    }

    void updateLeftRightButton()
    {
        int startIndex = 0, limit = 0, num = 0;
        switch (currentPage)
        {
            case ROLE:
                startIndex = roleStartIndex; limit = ROLE_NUM_PER_PAGE; num = roleNum;
                break;

            case BKG:
                startIndex = bkgStartIndex; limit = BKG_NUM_PER_PAGE; num = bkgNum;
                break;
            case OBJ:
                startIndex = objStartIndex; limit = OBJ_NUM_PER_PAGE; num = objNum;
                break;
        }
        if (startIndex == 0)
        {
            left.enabled = false;
            left.interactable = false;
        }
        else
        {
            left.enabled = true;
            left.interactable = true;
        }
        if (startIndex + limit == num || num <= limit)
        {
            right.enabled = false;
            right.interactable = false;
        }
        else
        {
            right.enabled = true;
            right.interactable = true;
        }
    }

    void leftPage()
    {
        switch (currentPage)
        {
            case ROLE:
                roleStartIndex -= 1;
                break;
            case BKG:
                bkgStartIndex -= 1;
                break;
            case OBJ:
                objStartIndex -= 1;
                break;
        }
        updateLeftRightButton();
        showPreview();
    }

    void rightPage()
    {
        switch (currentPage)
        {
            case ROLE:
                roleStartIndex += 1;
                break;
            case BKG:
                bkgStartIndex += 1;
                break;
            case OBJ:
                objStartIndex += 1;
                break;
        }
        updateLeftRightButton();
        showPreview();
    }

    IEnumerator fetchPreview(string type, List<PreviewItem> results)
    {
        UnityWebRequest www = UnityWebRequest.Get(Server.HOST+"api/v2/data/preview/" + type);
        www.timeout = Server.SIMPLE_REQUEST_TIME_OUT;
        yield return www.SendWebRequest();
        
        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
        {
            Debug.Log(JsonUtility.FromJson<PreviewResults>(www.downloadHandler.text));
            results.AddRange(JsonUtility.FromJson<PreviewResults>(www.downloadHandler.text).Results);
            Debug.Log(results.Count);
        }
    }

    //从所有preview中筛选属于某个风格的素材
    List<PreviewItem> selectPreviewOfStyle(string styleNum,string previewType,List<PreviewItem> inputPreview)
    {
        List<PreviewItem> re = new List<PreviewItem>();
        if (previewType.Equals("role")||previewType.Equals("obj"))
        {
            List<PublicActor> publicActorStyle = DataMap.publicActorList.GetElemInStyle(styleNum);
            List<string> matNameList = new List<string>();
            foreach(PublicActor publicActor in publicActorStyle)
            {
                matNameList.Add(publicActor.Name);
            }
            foreach(PreviewItem previewItem in inputPreview)
            {
                if (matNameList.Contains(previewItem.Name))
                {
                    re.Add(previewItem);
                }
            }
        }
        else if (previewType.Equals("bkg"))
        {
            List<PublicBackground> publicBkgStyle = DataMap.publicBackgroundList.GetElemInStyle(styleNum);
            List<string> matNameList = new List<string>();
            foreach (PublicBackground publicBkg in publicBkgStyle)
            {
                matNameList.Add(publicBkg.Name);
            }
            foreach (PreviewItem previewItem in inputPreview)
            {
                if (matNameList.Contains(previewItem.Name))
                {
                    re.Add(previewItem);
                }
            }
        }
       

        return re;
    }

    IEnumerator preparePreview(string styleNum)
    {
        switch (currentPage)
        {
            case ROLE:

                            
                List<PreviewItem> characterList = new List<PreviewItem>();
                List<PreviewItem> animalList = new List<PreviewItem>();
                yield return StartCoroutine(fetchPreview("Character", characterList));
                yield return StartCoroutine(fetchPreview("Animal", animalList));

                characterList.RemoveAll(data => data.ID.Equals(-1));
                animalList.RemoveAll(data => data.ID.Equals(-1));

                characterList.Sort((a, b) =>
                {
                    return a.ID <= b.ID ? -1 : 1;
                });
                animalList.Sort((a, b) =>
                {
                    return a.ID <= b.ID ? -1 : 1;
                });
                roleList = characterList.Concat(animalList).ToList();
                roleList = selectPreviewOfStyle(styleNum, "role", roleList);
                roleNum = roleList.Count;
                cleanShow();
                showPreview();
                for (int i = 0; i < ROLE_NUM_PER_PAGE; i++)
                {
                    roleSelectRects.Add(GameObject.Find("Role" + (i + 1).ToString() + "/rect").GetComponent<Button>());                   
                    roleSelectRects[i].onClick.RemoveAllListeners();
                    roleSelectRects[i].onClick.AddListener(chooseItem);
                }
                break;
            case BKG:


                bkgList.Clear();
                yield return StartCoroutine(fetchPreview("Background", bkgList));
                bkgList.RemoveAll(data =>  data.ID.Equals(-1));
                bkgList.Sort((a, b) =>
                {
                    return a.ID <= b.ID ? -1 : 1;
                });
                bkgList = selectPreviewOfStyle(styleNum, "bkg", bkgList);
                bkgNum = bkgList.Count;
                cleanShow();
                showPreview();
                for (int i = 0; i < BKG_NUM_PER_PAGE; i++)
                {
                    bkgSelectRects.Add(GameObject.Find("Bkg" + (i + 1).ToString() + "/rect").GetComponent<Button>());
                    bkgSelectRects[i].onClick.RemoveAllListeners();
                    bkgSelectRects[i].onClick.AddListener(chooseItem);
                }
                break;
            case OBJ:

                objList.Clear();
                yield return StartCoroutine(fetchPreview("Object", objList));
                objList = selectPreviewOfStyle(styleNum, "obj", objList);
                objList.RemoveAll(data => data.ID.Equals(-1));
                objList.Sort((a, b) =>
                {
                    if (a.ID == b.ID)
                        return 0;
                    return a.ID < b.ID ? -1 : 1;
                });
                objNum = objList.Count;
                cleanShow();
                showPreview();
                for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
                {
                    objSelectRects.Add(GameObject.Find("Obj" + (i + 1).ToString() + "/rect").GetComponent<Button>());
                    objSelectRects[i].onClick.RemoveAllListeners();
                    objSelectRects[i].onClick.AddListener(chooseItem);
                }
                break;
        }
        updateLeftRightButton();
    }

    void cleanShow()
    {
        switch (currentPage)
        {
            case ROLE:
                for (int i = 0; i < ROLE_NUM_PER_PAGE; i++)
                {
                    string picUrl = "/UI_aishuohua/SelectMaterials/PropRolePre.png";
                    GameObject.Find("Role" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                    GameObject.Find("Role" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = "";

                }
                break;

            case BKG:
                for (int i = 0; i < BKG_NUM_PER_PAGE; i++)
                {
                    string picUrl = "/UI_aishuohua/SelectMaterials/BkgePre.png";
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = "";

                }
                break;

            case OBJ:
                for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
                {
                    string picUrl = "/UI_aishuohua/SelectMaterials/PropRolePre.png";
                    GameObject.Find("Obj" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                    GameObject.Find("Obj" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = "";

                }
                break;


        }
       
                          
       
                       
       
               

        
    }

    void showPreview()
    {
        
        switch (currentPage)
        {
            case ROLE:
                for (int i = 0; i < ROLE_NUM_PER_PAGE && i<roleList.Count; i++)
                {
                    string picUrl = roleList[i + roleStartIndex].Url;
                    picUrl = picUrl.Substring(10);
                    picUrl = picUrl.Remove(picUrl.Length - 4);
                    GameObject.Find("Role" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                    GameObject.Find("Role" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = roleList[i + roleStartIndex].KeyName;
                    var rect = GameObject.Find("Role" + (i + 1).ToString() + "/rect");
                    if (selectRole.Contains(i + roleStartIndex))
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                    else
                        rect.GetComponent<CanvasGroup>().alpha = 0;
                }
                break;
            case BKG:
                for (int i = 0; i < BKG_NUM_PER_PAGE && i<bkgList.Count; i++)
                {
                    string picUrl = bkgList[i + bkgStartIndex].Url;
                    picUrl = picUrl.Substring(10);
                    picUrl = picUrl.Remove(picUrl.Length - 4);
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite= Resources.Load(picUrl, typeof(Sprite)) as Sprite;

                    GameObject.Find("Bkg" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = bkgList[i + bkgStartIndex].KeyName;
                    var rect = GameObject.Find("Bkg" + (i + 1).ToString() + "/rect");
                    var text = GameObject.Find("Bkg" + (i + 1).ToString() + "/Text");
                    if (selectBkg.Contains(i + bkgStartIndex))
                    {
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                        text.GetComponent<Text>().color = Color.black;
                    }
                    else
                    {
                        rect.GetComponent<CanvasGroup>().alpha = 0;
                        text.GetComponent<Text>().color = Color.white;
                    }
                }
                break;
            case OBJ:
                for (int i = 0; i < OBJ_NUM_PER_PAGE && i<objList.Count; i++)
                {
                    string picUrl = objList[i + objStartIndex].Url;
                    picUrl = picUrl.Substring(10);
                    picUrl = picUrl.Remove(picUrl.Length - 4);
                    GameObject.Find("Obj" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                    GameObject.Find("Obj" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = objList[i + objStartIndex].KeyName;
                    var rect = GameObject.Find("Obj" + (i + 1).ToString() + "/rect");
                    if (selectObj.Contains(i + objStartIndex))
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                    else
                        rect.GetComponent<CanvasGroup>().alpha = 0;
                }
                break;
        }

    }

    private void chooseItem()
    {
        GameObject clickedBtn = EventSystem.current.currentSelectedGameObject;
        string name = clickedBtn.GetComponent<Transform>().parent.gameObject.name;
        int number = int.Parse(name.Substring(name.Length - 1, 1)) - 1;
        switch (currentPage)
        {
            case ROLE:
                number += roleStartIndex;
                if (selectRole.Contains(number))
                    selectRole.Remove(number);
                else
                    selectRole.Add(number);
                showPreview();
                break;
            case BKG:
                number += bkgStartIndex;
                if (selectBkg.Contains(number))
                    selectBkg.Remove(number);
                else
                    selectBkg.Add(number);
                showPreview();
                break;
            case OBJ:
                number += objStartIndex;
                if (selectObj.Contains(number))
                    selectObj.Remove(number);
                else
                    selectObj.Add(number);
                showPreview();
                break;
        }
    }
}