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

public class SceneUserMat : MonoBehaviour
{
    public Button SwitchRolePage, SwitchBkgPage, SwitchObjectPage;
    public GameObject RolePageItems, BkgPageItems, ObjPageItems;
    //public Button showSelectedItems, closeSelectedItems;
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
    
    public List<PrivateActor> roleList = new List<PrivateActor>();
    public List<PrivateBackground> bkgList = new List<PrivateBackground>();
    public List<PrivateProp> objList = new List<PrivateProp>();
    int roleNum;
    int roleStartIndex = 0;
    int bkgNum;
    int bkgStartIndex = 0;
    int objNum;
    int objStartIndex = 0;
    public List<int> selectRole = new List<int>();
    public List<int> selectBkg = new List<int>();
    public List<int> selectObj = new List<int>();

    public List<string> activeGOs;

    public Canvas userMatInfoCanvas;
    public SceneMatInfo sceneMatInfo;

    public GameObject ActionPage;

    //2024-7-5新增 existedBkgSettingList;
    BkgSettingList existedBkgSettingList;
    private ServerPoster serverPoster = new ServerPoster();
    //新增下面'一行'，生命周期内拉取最新json文件到该文件路径
    
    void Start()
    {
        StartCoroutine(getPointFile());
        userMatInfoCanvas.gameObject.SetActive(false);
        sceneMatInfo.closeCallback = () => { preparePreview(); showPreview(); };

        SwitchRolePage.onClick.AddListener(() => {
            currentPage = 0;
            switchPreview();
        });
        SwitchBkgPage.onClick.AddListener(() => {
            currentPage = 1;
            switchPreview();
        });
        SwitchObjectPage.onClick.AddListener(() => {
            currentPage = 2;
            switchPreview();
        });
        left.onClick.AddListener(leftPage);
        right.onClick.AddListener(rightPage);
        returnMainMenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SceneUserMatIndex");
        });
        startCreation.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SceneTellStory");
            SceneMsgToStoryCreate.uiType = 2;
        });
        preparePreview();
        switchPreview();
       
        
        if (TestRunner.TestingMode)
        {
            testingFunction();
        }
    }
    IEnumerator getPointFile()
    {
        existedBkgSettingList = new BkgSettingList();
        yield return StartCoroutine(serverPoster.FindElemFromDataBase("Huiben_Setting", "bkgSettingList", "", "", existedBkgSettingList.bkgSettingList));
        DataMap.bkgSettingList.bkgSettingList = existedBkgSettingList.bkgSettingList;
        //新增下面'一行'，生命周期内拉取最新json文件到该文件路径
        
        string fileName = Application.streamingAssetsPath + "/data/JsonFiles/bkgSettingList_gn.json";
        JsonOperator.Obj2Json<List<BkgSettingElement>>(DataMap.bkgSettingList.bkgSettingList,fileName);
        print("已获取最新的点位信息到本地："+fileName);
        // setPanel(curBkgName);
        // showBkgPoint(curBkgName);
    }

    public static void setTestingFunction(Action action)
    {
        testingFunction = action;
    }

    public void setVisible(GameObject go, bool visible)
    {
        if (go.GetComponent<CanvasGroup>() != null )
        {
            go.GetComponent<CanvasGroup>().alpha = visible ? 1 : 0;
            go.GetComponent<CanvasGroup>().interactable = visible;
            go.GetComponent<CanvasGroup>().blocksRaycasts = visible;
        }
        go.GetComponent<GraphicRaycaster>().enabled = visible;
    }

    void switchPreview()
    {
       
        RolePageItems.SetActive(false); BkgPageItems.SetActive(false); ObjPageItems.SetActive(false);
        switch (currentPage)
        {
            case ROLE:
                pageBackground.sprite = rolePage;
                RolePageItems.SetActive(true);
                for (int i = 0; i < ROLE_NUM_PER_PAGE; i++)
                {
                    roleSelectRects.Add(GameObject.Find("Role" + (i + 1).ToString() + "/rect").GetComponent<Button>());
                    roleSelectRects[i].onClick.AddListener(chooseItem);
                }

                break;
            case BKG:
                pageBackground.sprite = bkgPage;
                BkgPageItems.SetActive(true);
                for (int i = 0; i < BKG_NUM_PER_PAGE; i++)
                {
                    bkgSelectRects.Add(GameObject.Find("Bkg" + (i + 1).ToString() + "/rect").GetComponent<Button>());
                    bkgSelectRects[i].onClick.AddListener(chooseItem);
                }
                break;
            case OBJ:
                pageBackground.sprite = objPage;
                ObjPageItems.SetActive(true);
                for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
                {
                    objSelectRects.Add(GameObject.Find("Obj" + (i + 1).ToString() + "/rect").GetComponent<Button>());
                    objSelectRects[i].onClick.AddListener(chooseItem);
                }
                break;
        }
        //preparePreview();
        showPreview();
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

    

    void preparePreview()
    {

        roleList =new List<PrivateActor>( DataMap.privateActorList.privateActorList);

        roleList.Reverse();
        roleNum = roleList.Count;
        //showPreview();
       


        bkgList =new List<PrivateBackground>( DataMap.privateBackgroundList.privateBackgroundList);
        bkgList.Reverse();
        bkgNum = bkgList.Count;
        //showPreview();
      

        objList =new List<PrivateProp>( DataMap.privatePropList.privatePropList);
        objList.Reverse();
        objNum = objList.Count;
        //showPreview();
         
        updateLeftRightButton();
    }

    void showPreview()
    {
        clearShow();
        switch (currentPage)
        {
            case ROLE:
                for (int i = 0; i < ROLE_NUM_PER_PAGE && i+roleStartIndex<roleNum; i++)
                {

                    //string picUrl = roleList[i + roleStartIndex].Url;
                    PrivateActor privateActor = roleList[i + roleStartIndex];
                    string picUrl = Application.persistentDataPath + "/" + privateActor.Url + "/" + privateActor.Name + ".png";
                    
                    Texture2D texture = ImgUtil.LoadTexture(picUrl);
                    Sprite sprite = null;
                    if (texture != null)
                    {
                        sprite = ImgUtil.createSprite(texture);
                    }
                    

                    GameObject.Find("Role" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = sprite;
                    GameObject.Find("Role" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = privateActor.RawName;
                    var rect = GameObject.Find("Role" + (i + 1).ToString() + "/rect");
                    if (selectRole.Contains(i + roleStartIndex))
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                    else
                        rect.GetComponent<CanvasGroup>().alpha = 0;
                }
                break;
            case BKG:
                for (int i = 0; i < BKG_NUM_PER_PAGE && i+bkgStartIndex<bkgNum; i++)
                {
                    PrivateBackground privateBackground = bkgList[i + bkgStartIndex];
                    string picUrl = Application.persistentDataPath + "/" + privateBackground.Url + "/" + privateBackground.Name + ".png";

                    Texture2D texture = ImgUtil.LoadTexture(picUrl);
                    Sprite sprite = ImgUtil.createSprite(texture);
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = sprite;
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = bkgList[i + bkgStartIndex].RawName;
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
                for (int i = 0; i < OBJ_NUM_PER_PAGE && i+objStartIndex<objNum; i++)
                {
                    PrivateProp privateProp = objList[i + objStartIndex];
                    string picUrl = Application.persistentDataPath + "/" + privateProp.Url + "/" + privateProp.Name + ".png";

                    Texture2D texture = ImgUtil.LoadTexture(picUrl);
                    Sprite sprite = ImgUtil.createSprite(texture);
                    GameObject.Find("Obj" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = sprite; 
                    GameObject.Find("Obj" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = objList[i + objStartIndex].RawName;
                    var rect = GameObject.Find("Obj" + (i + 1).ToString() + "/rect");
                    if (selectObj.Contains(i + objStartIndex))
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                    else
                        rect.GetComponent<CanvasGroup>().alpha = 0;
                }
                break;
        }
       
    }

    void clearShow()
    {
        switch (currentPage)
        {
            case ROLE:
                for (int i = 0; i < ROLE_NUM_PER_PAGE ; i++)
                {

                    GameObject.Find("Role" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = null;
                    GameObject.Find("Role" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = "";
                    
                }
                break;
            case BKG:
                for (int i = 0; i < BKG_NUM_PER_PAGE ; i++)
                {
                
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = null;
                    GameObject.Find("Bkg" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = "";
                   
                }
                break;
            case OBJ:
                for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
                {
                    
                    GameObject.Find("Obj" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = null;
                    GameObject.Find("Obj" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = "";
                    
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
                //if (selectRole.Contains(number))
                //    selectRole.Remove(number);
                //else
                 //   selectRole.Add(number);                

                print("select " + roleList[number].RawName);
                sceneMatInfo.initInfo(roleList[number].RawName, "Actor");
                userMatInfoCanvas.gameObject.SetActive(true);
                ActionPage.gameObject.SetActive(true);
                //showPreview();

                break;
            case BKG:
                number += bkgStartIndex;
                //if (selectBkg.Contains(number))
                //    selectBkg.Remove(number);
                //else
                //    selectBkg.Add(number);
                print("select " + bkgList[number].RawName);
                sceneMatInfo.initInfo(bkgList[number].RawName, "Bkg");
                userMatInfoCanvas.gameObject.SetActive(true);
                ActionPage.gameObject.SetActive(false);

                //showPreview();
                break;
            case OBJ:
                number += objStartIndex;
                //if (selectObj.Contains(number))
                //    selectObj.Remove(number);
                //else
                //    selectObj.Add(number);
                //showPreview();

                print("select " + objList[number].RawName);
                sceneMatInfo.initInfo(objList[number].RawName, "Prop");
                userMatInfoCanvas.gameObject.SetActive(true);
                ActionPage.gameObject.SetActive(false);

                break;
        }
    }
}
