using System;
using System.Collections;
using System.Collections.Generic;
using UI.Dates;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneMyStories : MonoBehaviour
{
    // Start is called before the first frame update
    public Button ButtonReturn;
    public Canvas canvas;
    private RawImage[] rawImages;
    public int page;
    public Button ButtonLastPage;
    public Button ButtonNextPage;
    public DatePicker_InputField datePicker;
    public Button SearchButton;
    public Dropdown ClassNameFilter;
    List<string> ClassNames;
    public Button ButtonSelect, ButtonShareParent, ButtonShareTeacher, ButtonClear, ButtonSelectAll;
    public GameObject SharePanel;
    public Button OpenSharePanel, CloseSharePanel;

    void Start()
    {
        Screen.SetResolution(1280, 800, true, 60);
        ButtonReturn.onClick.AddListener(() =>
        {
            Debug.Log("ButtonReturn");
            SceneManager.LoadScene("SceneMainMenu");
        });
        ButtonLastPage.onClick.AddListener(() =>
        {
            Debug.Log("ButtonLastPage");
            UpdatePage(page - 1);
        });
        ButtonNextPage.onClick.AddListener(() =>
        {
            Debug.Log("ButtonNextPage");
            UpdatePage(page + 1);
        });
        SearchButton.onClick.AddListener(() =>
        {
            Debug.Log("SearchButton");
            try
            {
                string date;
                if (string.IsNullOrEmpty(datePicker.Ref_InputField.text))
                {
                    date = null;
                } else
                {
                    date = datePicker.SelectedDate.Date.ToDateString();
                }
                StartCoroutine(Server.GetStories(date, ClassNameFilter.value == 0 ? null : ClassNames[ClassNameFilter.value], () =>
                {
                    UpdatePage(0);
                    RefreshStoriesList();
                    AndroidUtils.ShowToast("共查询到" + Server.storiesList.stories.Length + "个视频");
                }));
            }
            catch (Exception e)
            {
                AndroidUtils.ShowToast("查询失败，请先选择日期");
                Debug.LogException(e);
            }
        });
        rawImages = canvas.GetComponentsInChildren<RawImage>();
        foreach(RawImage r in rawImages)
        {
            r.gameObject.AddComponent<Button>();
            r.gameObject.SetActive(false);
        }
        Array.Sort(rawImages, (RawImage rw1, RawImage rw2) => string.Compare(rw1.name, rw2.name));
        
        StartCoroutine(Server.GetClassNames((string[] classnames) => {
            ClassNames = new List<string> (classnames) ;
            ClassNames.Insert(0, "不限");
            ClassNameFilter.ClearOptions();
            ClassNameFilter.AddOptions(ClassNames);
        }));
        ButtonSelect.onClick.AddListener(OnClickSelectButton);
        ClearFilterAndRefresh();
        ButtonClear.onClick.AddListener(ClearFilterAndRefresh);
        ButtonSelectAll.onClick.AddListener(OnClickSelectAllButton);
        ButtonShareParent.onClick.AddListener(() => {
            StartCoroutine(Server.ShareTo("parent", OnClickSelectButton));
        });
        ButtonShareTeacher.onClick.AddListener(() =>
        {
            StartCoroutine(Server.ShareTo("teacher", OnClickSelectButton));
        });
        OpenSharePanel.onClick.AddListener(() =>
        {
            SharePanel.SetActive(true);
        });
        CloseSharePanel.onClick.AddListener(() =>
        {
            SharePanel.SetActive(false);
        });
    }


    void ClearFilterAndRefresh()
    {
        Debug.Log("onClick: ClearFilterAndRefresh");
        StartCoroutine(Server.GetStories(() => {
            UpdatePage(0);
            RefreshStoriesList();
        }));
        ClassNameFilter.value = 0;
        datePicker.Ref_InputField.text = "";
    }

    bool SelectMode = false;

    public void OnClickSelectButton()
    {
        Debug.Log("onClick: OnClickSelectButton");
        SelectMode = !SelectMode;
        ButtonSelect.GetComponentInChildren<Text>().text = SelectMode ?  "退出多选": "多选";
        ButtonSelectAll.gameObject.SetActive(SelectMode);
        Server.selectedStories = new Dictionary<string, bool>();
        RefreshStoriesList();
    }

    public void OnClickSelectAllButton()
    {
        Debug.Log("onClick: OnClickSelectAllButton");
        if (SelectMode)
        {
            Server.selectedStories = new Dictionary<string, bool>();
            foreach(var story in Server.storiesList.stories)
            {
                Server.selectedStories.Add(story._id, true);
            }
            RefreshStoriesList();
        }
    }

    public void UpdatePage(int newPageNum)
    {
        page = newPageNum;
        ButtonLastPage.gameObject.SetActive(true);
        ButtonNextPage.gameObject.SetActive(false);
        if (page == 0)
        {
            ButtonLastPage.gameObject.SetActive(false);
        }
        int maxPage = Server.storiesList.stories.Length / 8;
        if (maxPage * 8 < Server.storiesList.stories.Length)
        {
            maxPage += 1;
        }
        if (page < maxPage - 1)
        {
            ButtonNextPage.gameObject.SetActive(true);
        }
        RefreshStoriesList();
    }

    public Dictionary<string, Action> UpdateActions = new Dictionary<string, Action>();
    public void RefreshStoriesList()
    {
        int pageStart = 8 * page;
        int pageEnd = 8 * (page + 1);
        if (pageEnd > Server.storiesList.stories.Length)
        {
            pageEnd = Server.storiesList.stories.Length;
        }
        int cur = pageStart;
        foreach (RawImage r in rawImages)
        {
            if (cur < pageEnd)
            {
                r.gameObject.SetActive(true);
                r.GetComponentInChildren<VideoPlayer>().targetTexture = new RenderTexture(300, 169, 16, RenderTextureFormat.ARGB32);
                r.GetComponentInChildren<VideoPlayer>().url =
                    $"{Server.HOST}api/v2/uploadedFiles/{Server.storiesList.stories[cur]._id}_preview.mp4";
                r.texture = r.GetComponentInChildren<VideoPlayer>().targetTexture;
                var timeStr = Server.storiesList.stories[cur].time.Substring(2, 17).Replace("T", " ");

                var SelectBkg = r.GetComponentInParent<Image>().GetComponentsInChildren<Image>()[1];
                var oid = Server.storiesList.stories[cur]._id;

                UpdateActions[oid] = () =>
                {
                    Server.selectedStories.TryGetValue(oid, out bool contains);
                    if (SelectMode && contains)
                    {
                        SelectBkg.type = Image.Type.Simple;
                        SelectBkg.fillAmount = 1;
                    }
                    else
                    {
                        SelectBkg.type = Image.Type.Filled;
                        SelectBkg.fillAmount = 0;
                    }
                };

                UpdateActions[oid]();

                r.GetComponentInChildren<Text>().text =
                    $"名称：{Server.storiesList.stories[cur].name}\n作者：{Server.storiesList.stories[cur].creator} 时长：{Server.storiesList.stories[cur].duration}\n（{timeStr}）";
                var videoUrl = $"{Server.HOST}api/v2/uploadedFiles/{Server.storiesList.stories[cur]._id}.mp4";

                r.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                r.gameObject.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (SelectMode)
                    {
                        bool contains = false;
                        Server.selectedStories.TryGetValue(oid, out contains);
                        Server.selectedStories[oid] = !contains;
                        UpdateActions[oid]();
                    }
                    else
                    {
                        if (Server.CheckProcessedFinished(videoUrl))
                        {
                            ReRecord.lastSceneFromStoryList = true;
                            AndroidUtils.RecordFileName = videoUrl;
                            ReRecord.uploaded = true;
                            Debug.Log("Watch story:" + AndroidUtils.RecordFileName);
                            SceneManager.LoadScene("SceneStoryDetails");
                        }
                        else
                        {
                            AndroidUtils.ShowToast("服务器仍在处理该条目，请稍候...");
                        }
                    }
                });
            }
            else
            {
                r.gameObject.SetActive(false);
                var SelectBkg = r.GetComponentInParent<Image>().GetComponentsInChildren<Image>()[1];
                SelectBkg.type = Image.Type.Filled;
                SelectBkg.fillAmount = 0;
            }
            cur++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
