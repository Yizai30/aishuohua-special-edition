using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneRolesAndObjs : MonoBehaviour
{
    public Button SwitchRolePage, SwitchBkgPage, SwitchObjectPage;
    public GameObject RolePageItems, BkgPageItems, ObjPageItems;
    public Button left, right;
    public int currentPage = 0;// 0 -> role 1 -> bkg 2 -> obj
    private const int ROLE = 0, BKG = 1, OBJ = 2;
    private const int ROLE_NUM_PER_PAGE = 4, BKG_NUM_PER_PAGE = 3, OBJ_NUM_PER_PAGE = 4;
    public Image pageBackground;
    private List<int> loadedPreview = new List<int>();
    private List<PreviewItem> roleList = new List<PreviewItem>();
    private List<PreviewItem> bkgList = new List<PreviewItem>();
    private List<PreviewItem> objList = new List<PreviewItem>();
    private List<GameObject> roles = new List<GameObject>(), bkgs = new List<GameObject>(), objs = new List<GameObject>();
    int roleNum;
    int roleStartIndex = 0;
    int bkgNum;
    int bkgStartIndex = 0;
    int objNum;
    int objStartIndex = 0;
    public SceneSelectMaterials selected;

    void Start()
    {
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
        for (int i = 0; i < ROLE_NUM_PER_PAGE; i++)
            roles.Add(GameObject.Find("SelectedRole" + (i + 1).ToString()));
        for (int i = 0; i < BKG_NUM_PER_PAGE; i++)
            bkgs.Add(GameObject.Find("SelectedBkg" + (i + 1).ToString()));
        for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
            objs.Add(GameObject.Find("SelectedObj" + (i + 1).ToString()));
    }

    public void refresh()
    {
        loadedPreview.Clear();
        switchPreview();
    }


     void switchPreview()
    {
        RolePageItems.SetActive(false); BkgPageItems.SetActive(false); ObjPageItems.SetActive(false);
        switch (currentPage)
        {
            case ROLE:
                pageBackground.sprite = selected.rolePage;
                RolePageItems.SetActive(true);
                break;
            case BKG:
                pageBackground.sprite = selected.bkgPage;
                BkgPageItems.SetActive(true);
                break;
            case OBJ:
                pageBackground.sprite = selected.objPage;
                ObjPageItems.SetActive(true);
                break;
        }
        if (!loadedPreview.Contains(currentPage))
        {
            loadedPreview.Add(currentPage);
            preparePreview();
        }
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
        switch (currentPage)
        {
            case ROLE:
                List<PreviewItem> selectedRoleList = new List<PreviewItem>();
                foreach (int i in selected.selectRole) {
                    selectedRoleList.Add(selected.roleList[i]);
                }
                roleList = selectedRoleList;
                roleNum = selectedRoleList.Count;
                break;
            case BKG:
                List<PreviewItem> selectedBkgList = new List<PreviewItem>();
                foreach (int i in selected.selectBkg)
                {
                    selectedBkgList.Add(selected.bkgList[i]);
                }
                bkgList = selectedBkgList;
                bkgNum = selectedBkgList.Count;
                break;
            case OBJ:
                List<PreviewItem> selectedObjList = new List<PreviewItem>();
                foreach (int i in selected.selectObj)
                {
                    selectedObjList.Add(selected.objList[i]);
                }
                objList = selectedObjList;
                objNum = selectedObjList.Count;
                break;
        }
        showPreview();
    }

    void showPreview()
    {
        switch (currentPage)
        {
            case ROLE:
                for (int i = 0; i < ROLE_NUM_PER_PAGE; i++)
                    if (i + roleStartIndex < roleNum)
                        roles[i].SetActive(true);
                    else
                        roles[i].SetActive(false);
                for (int i = 0; i < ROLE_NUM_PER_PAGE; i++)
                {
                    if (i + roleStartIndex < roleNum)
                    {
                        string picUrl = roleList[i + roleStartIndex].Url;
                        picUrl = picUrl.Substring(10);
                        picUrl = picUrl.Remove(picUrl.Length - 4);
                        GameObject.Find("SelectedRole" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                        GameObject.Find("SelectedRole" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = roleList[i + roleStartIndex].KeyName;
                        var rect = GameObject.Find("SelectedRole" + (i + 1).ToString() + "/rect");
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                    }
                }
                break;
            case BKG:
                for (int i = 0; i < BKG_NUM_PER_PAGE; i++)
                    if (i + bkgStartIndex < bkgNum)
                        bkgs[i].SetActive(true);
                    else
                        bkgs[i].SetActive(false);
                for (int i = 0; i < BKG_NUM_PER_PAGE; i++)
                {
                    if (i + bkgStartIndex < bkgNum)
                    {
                        string picUrl = bkgList[i + bkgStartIndex].Url;
                        picUrl = picUrl.Substring(10);
                        picUrl = picUrl.Remove(picUrl.Length - 4);
                        GameObject.Find("SelectedBkg" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                        GameObject.Find("SelectedBkg" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = bkgList[i + bkgStartIndex].KeyName;
                        var rect = GameObject.Find("SelectedBkg" + (i + 1).ToString() + "/rect");
                        var text = GameObject.Find("SelectedBkg" + (i + 1).ToString() + "/Text");
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                        text.GetComponent<Text>().color = Color.black;
                    }
                }
                break;
            case OBJ:
                for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
                    if (i + objStartIndex < objNum)
                        objs[i].SetActive(true);
                    else
                        objs[i].SetActive(false);
                for (int i = 0; i < OBJ_NUM_PER_PAGE; i++)
                {
                    if (i + objStartIndex < objNum)
                    {
                        string picUrl = objList[i + objStartIndex].Url;
                        picUrl = picUrl.Substring(10);
                        picUrl = picUrl.Remove(picUrl.Length - 4);
                        Debug.Log("SelectedObj" + (i + 1).ToString() + "/pic");
                        Debug.Log(GameObject.Find("SelectedObj" + (i + 1).ToString() + "/pic"));
                        Debug.Log(GameObject.Find("SelectedObj" + (i + 1).ToString() + "/pic").GetComponent<Image>());
                        GameObject.Find("SelectedObj" + (i + 1).ToString() + "/pic").GetComponent<Image>().sprite = Resources.Load(picUrl, typeof(Sprite)) as Sprite;
                        GameObject.Find("SelectedObj" + (i + 1).ToString() + "/Text").GetComponent<Text>().text = objList[i + objStartIndex].KeyName;
                        var rect = GameObject.Find("SelectedObj" + (i + 1).ToString() + "/rect");
                        rect.GetComponent<CanvasGroup>().alpha = 1;
                    }
                }
                break;
        }
    }
}

