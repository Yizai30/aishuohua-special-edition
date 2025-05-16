using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointGenerator : MonoBehaviour
{
    public string fileName = "";
    public string editFileName = "";
    public string curBkgName;
    public string pointType;

    public ServerPoster serverPoster;

    [SerializeField]
     public float screenHeight = 510f;
     public float screenWidth = 1040f;
     public float pivotX=0;
     public float pivotY = 0;

    [SerializeField]
    float height1, height2, width1, width2;
    int widthNum;


    public GameObject panel;
    public InputField bkgNameField, pointTypeField, height1Field, height2Field, width1Field,
        width2Field, widthNumField, referenceSourcePointTypeField, referenceTargetPointTypeField;

    public Button delBtn, genBtn, changeBkg, referenceBtn, addPointBtn, getServeBtn;

    //public Button genButton;
    BkgSettingList existedBkgSettingList;
    void Start()
    {
        existedBkgSettingList = new BkgSettingList();
        serverPoster = GetComponent<ServerPoster>();
        // genButton.enabled = true;
        fileName = Application.streamingAssetsPath + "/data/JsonFiles/bkgSettingList_gn.json";

        delBtn.onClick.AddListener(onClickDelete);
        genBtn.onClick.AddListener(onClickGenPoint);
        changeBkg.onClick.AddListener(onClickChangeBkg);
        referenceBtn.onClick.AddListener(onClickReference);
        addPointBtn.onClick.AddListener(onClickAddPoint);
        getServeBtn.onClick.AddListener(onClickgetServerFile);
    }

    public void onClickgetServerFile()
    {
        StartCoroutine(getPointFile());
    }

    IEnumerator getPointFile()
    {
        yield return StartCoroutine(serverPoster.FindElemFromDataBase("Huiben_Setting", "bkgSettingList", "", "", existedBkgSettingList.bkgSettingList));
        JsonOperator.Obj2Json<List<BkgSettingElement>>(existedBkgSettingList.bkgSettingList,fileName);
        print("已获取最新的点位信息到本地："+fileName);
        setPanel(curBkgName);
        showBkgPoint(curBkgName);
    }

    //IEnumerator uploadPointFIle()
    //{
     //   yield return StartCoroutine(serverPoster.)
    //}

    //控制panel显示当前背景
    void setPanel(string bkgName)
    {
        try
        {
            Image bkgImage = panel.GetComponent<Image>();
            Sprite bkgSprite = Resources.Load<Sprite>("Environment/2D/" + bkgName);
            bkgImage.sprite = bkgSprite;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    //获取已有内容

    public List<BkgSettingElement> getExistedBkgSetting()
    {
        List<BkgSettingElement> bkgSettingElements= JsonOperator.parseObj<List<BkgSettingElement>>(fileName);
       
        if (bkgSettingElements != null) return bkgSettingElements;
        else
        {
            Debug.Log("读取背景点位信息失败");
            return null;
        }

    }

    //根据斜率
    List<List<float>> genTypePointsk(float height1, float height2, float width1, float width2, int widthNum)
    {
        //BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);      
        //生成点位
        
        //生成的点位
        List<List<float>> genPos = new List<List<float>>();
        if (widthNum == 1)
        {
            float y = height1 + (height2 - height1) / 2;
            float x = width1 + (width2 - width1) / 2;
            genPos.Add(new List<float> { x, y, 0 });

        }
        else
        {
            float k = (height2 - height1) / (width2 - width1);
            float b = (height1 * width2 - height2 * width1) / (width2 - width1);
            float gap = (width2 - width1) / (widthNum - 1);
            
            //genPos.Add(new List<float> { y, width1 });
            for (int i = 0; i < widthNum; i++)
            {
                float x = width1 + i * gap;
                float y = k * x + b;
                genPos.Add(new List<float> { width1 + i * gap, y, 0 });
            }
        }

        return genPos;

    }

    //生成某一个pointtype的点位集合，如果已经存在，则覆盖
    List<List<float>> genTypePointsk0(float height1,float height2,float width1,float width2,int widthNum)
    {
        //BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);      
        //生成点位
        float y = height1 + (height2 - height1) / 2;
        //生成的点位
        List<List<float>> genPos = new List<List<float>>();
        if (widthNum == 1)
        {
            float x = width1 + (width2 - width1) / 2;
            genPos.Add(new List<float> { x, y, 0 });

        }
        else
        {
            float gap = (width2 - width1) / (widthNum - 1);
            //genPos.Add(new List<float> { y, width1 });
            for(int i = 0; i < widthNum; i++)
            {
                genPos.Add(new List<float> {  width1 + i * gap,y,0 });
            }
        }

        return genPos;
         
    }

    //根据生成的点位集合修改element
    void editBkgElement(string bkgName,string pointType,List<List<float>> points)
    {
        BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (bkgSettingElement == null)
        {
            bkgSettingElement = new BkgSettingElement(bkgName, new List<List<float>>(), new List<PointMark>());
            existedBkgSettingList.bkgSettingList.Add(bkgSettingElement);
        }
        //如果有pointType，删除此pointtype以及坐标
        if (bkgSettingElement.containsPointType(pointType))
        {
            bkgSettingElement.deletePointTypeAndPoint(pointType);
        }
        bkgSettingElement.addPointTypeAndPoint(pointType, points);
    }


    void addPointToPointType(string bkgName,string pointType,List<List<float>>points)
    {
        BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (bkgSettingElement == null)
        {
            Debug.Log("当前背景不存在" + bkgName);
            return;
        }
        if (bkgSettingElement.containsPointType(pointType))
        {
            bkgSettingElement.addPointsToType(pointType,points);
        }
    }

    //根据当前bkgSettingList重新生成json
    void genBkgSettingJson()
    {
        JsonOperator.Obj2Json<List<BkgSettingElement>>(existedBkgSettingList.bkgSettingList, fileName);
    }
    

    void clearScreenPoint()
    {
        foreach (GameObject mark in GameObject.FindGameObjectsWithTag("test"))
        {
            GameObject.Destroy(mark);
        }
    }

    //显示特定背景的点位
    void showBkgPoint(string bkgName)
    {
        clearScreenPoint();

        GameObject markGo = (GameObject)Resources.Load("Test/mark", typeof(GameObject));               
        BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (bkgSettingElement == null)
        {
            Debug.Log("没有找到背景信息" + bkgName);
            return;
        }
        else
        {
            foreach(PointMark pointMark in bkgSettingElement.pointMarks)
            {
                List<int> indexList = pointMark.pointList;
                foreach(int index in indexList)
                {
                    List<float> pos = bkgSettingElement.initPositionList[index];
                    //pos[0] = pos[0] * screenWidth / 2f + pivotX;
                    //pos[1] = pos[1] * screenHeight / 2f + pivotY;
                    List<float> screenPos= norm2screen(pos);
                    Color markColor = getMarkColor(pointMark.pointType);
                    
                    GameObject go= GameObject.Instantiate(markGo, new Vector3(screenPos[0], screenPos[1], screenPos[2]), new Quaternion(0, 0, 0, 0),panel.transform);
                    
                    go.transform.localPosition = new Vector3(screenPos[0], screenPos[1], screenPos[2]);
                    //go.transform.SetParent(panel.transform);
                    
                    go.name = pointMark.pointType;
                    Image image = go.GetComponent<Image>();

                    if (image == null)
                    {
                        Debug.Log("未找到image");
                        return;
                    }
                    image.color = markColor;
                    //foreach (Material material in image.materials)
                    //{
                        //material.color = spriteColor;
                    //}

                }
            }

           
        }
        //GameObject.Instantiate(actorRes.prefab, startPosition, startRotation)
    }

    List<float> norm2screen(List<float> pos)
    {
        List<float> re = new List<float>();
        re.Add( pos[0] * screenWidth / 2f + pivotX);
        re.Add( pos[1] * screenHeight / 2f + pivotY);
        re.Add(pos[2]);
        return re;
    }

    List<float> screen2norm(List<float> pos)
    {
        List<float> re = new List<float>();
        re.Add( (pos[0] - pivotX) / (screenWidth / 2f));
        re.Add( (pos[1] - pivotY) / (screenHeight / 2f));
        re.Add(pos[2]);
        return re;
    }

    //返回特定点位的颜色
    Color getMarkColor(string pointType)
    {
        if (pointType.Contains("enterPoint") || pointType.Contains("escapePoint") || pointType.Contains("exitPoint"))
        {
            //灰色
            return getColorByList(new List<float> { 0.4f, 0.4f, 0.4f });
        }else if (pointType.EndsWith("defaultPoint"))
        {
            //白色
            return getColorByList(new List<float> { 1, 1, 1 });
        }else if (pointType.Contains("waterPoint"))
        {
            //绿色
            return getColorByList(new List<float> { 0, 1, 0 });
        }
        else if (pointType.Contains("skyPoint"))
        {
            //蓝色
            return getColorByList(new List<float> { 0, 0, 1 });
        }
        else if (pointType.Contains("propPoint"))
        {
            //橙色
            return getColorByList(new List<float> { 1, 0.38f, 0 });
        }
        else
        {
            //其他、紫色
            return getColorByList(new List<float> { 0.627f, 0.125f, 0.941f });
        }
    }


    //增加特定类型的点位
    Color getColorByList(List<float> rgb)
    {
        Color color = new Color(0, 0, 0);
        if (rgb.Count == 3)
        {
            return new Color(rgb[0], rgb[1], rgb[2]);

        }
        else if (rgb.Count == 4)
        {
            return new Color(rgb[0], rgb[1], rgb[2], rgb[3]);
        }
        else
        {
            Debug.Log("color参数错误");
            return color;
        }
    }



    public void onClickDelete()
    {
        curBkgName = bkgNameField.text;
        pointType = pointTypeField.text;
        BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(curBkgName);
        if (bkgSettingElement == null) return;
        bkgSettingElement.deletePointTypeAndPoint(pointType);
        
        copySeason(curBkgName);
        
        genBkgSettingJson();
        showBkgPoint(curBkgName);
    }

    public void onClickGenPoint()
    {
        height1 =Convert.ToSingle( height1Field.text);
        height2 =Convert.ToSingle( height2Field.text);
        width1 = Convert.ToSingle(width1Field.text);
        width2 = Convert.ToSingle(width2Field.text);
        widthNum =Convert.ToInt32(widthNumField.text);
        curBkgName = bkgNameField.text;
        pointType = pointTypeField.text;

        if(curBkgName.Equals("") || pointType.Equals(""))
        {
            Debug.Log("请输入背景和点位种类信息");
            return;
        }

        Debug.Log("生成点位...");
        List<List<float>> genPosList = genTypePointsk(height1, height2, width1, width2, widthNum);
        List<List<float>> normGenPosList = new List<List<float>>();
        foreach (List<float> pos in genPosList)
        {
           normGenPosList.Add(screen2norm(pos));
        }
        editBkgElement(curBkgName, pointType, normGenPosList);
        Debug.Log("bkgName:" + curBkgName + " pointType:" + pointType + " points:" + normGenPosList);
        
        //四季       
        copySeason(curBkgName);     
        
        genBkgSettingJson();
        showBkgPoint(curBkgName);
    }

    public void onClickChangeBkg()
    {
        curBkgName = bkgNameField.text;
        if (curBkgName.Equals(""))
        {
            Debug.Log("请输入背景名称");
            return;
        }
        
        existedBkgSettingList.bkgSettingList = getExistedBkgSetting();
        setPanel(curBkgName);
        showBkgPoint(curBkgName);
    }

    public void onClickReference()
    {
        string reSourcePointType = referenceSourcePointTypeField.text;
        string reTargetPointType = referenceTargetPointTypeField.text;
        if(reSourcePointType==""||reTargetPointType=="")
        {
            Debug.Log("请填写reSourcePointType和reTargetPointType");
            return;
        }
        copyReference(curBkgName, reSourcePointType, reTargetPointType);
        //existedBkgSettingList = getExistedBkgSetting();
        Debug.Log("已引用点位" + reSourcePointType + "到" + reTargetPointType);
        copySeason(curBkgName);
        genBkgSettingJson();
        showBkgPoint(curBkgName);
    }


    public void onClickAddPoint()
    {
        height1 = Convert.ToSingle(height1Field.text);
        height2 = Convert.ToSingle(height2Field.text);
        width1 = Convert.ToSingle(width1Field.text);
        width2 = Convert.ToSingle(width2Field.text);
        widthNum = Convert.ToInt32(widthNumField.text);
        curBkgName = bkgNameField.text;
        pointType = pointTypeField.text;

        if (curBkgName.Equals("") || pointType.Equals(""))
        {
            Debug.Log("请输入背景和点位种类信息");
            return;
        }

        Debug.Log("生成点位...");
        List<List<float>> genPosList = genTypePointsk(height1, height2, width1, width2, widthNum);
        List<List<float>> normGenPosList = new List<List<float>>();
        foreach(List<float>pos in genPosList)
        {
            normGenPosList.Add(screen2norm(pos));
        }
        addPointToPointType(curBkgName, pointType, normGenPosList);
        Debug.Log("bkgName:" + curBkgName + " pointType:" + pointType + " points:" + normGenPosList);

        //四季       
        copySeason(curBkgName);

        genBkgSettingJson();
        showBkgPoint(curBkgName);
    }

    void copySeason(string bkgName)
    {
       
        BkgSettingElement curBkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if(curBkgSettingElement==null)
        {
            Debug.Log("找不到背景" + bkgName);
        }
        List<BkgSettingElement> samePointBkgList = existedBkgSettingList.getElementsContainBkgName(curBkgSettingElement.backgroundName);
        foreach(BkgSettingElement bkgSettingElement in samePointBkgList)
        {
            if (!containSeaasonName(bkgSettingElement.backgroundName))
            {
                continue;
            }
            bkgSettingElement.copyElementInfo(curBkgSettingElement);
            Debug.Log("已同步点位信息到 " + bkgSettingElement.backgroundName);
        }
    }

    bool containSeaasonName(string bkgName)
    {
        List<string> seasonNameList = new List<string> { "winter","autumn","summer","spring" };
        foreach (string seasonName in seasonNameList)
        {
            if (bkgName.Contains(seasonName)) return true;
        }
        return false;
    }

    //新增一个点位的别名
    void copyReference(string bkgName,string sourcePointType,string targetPointType)
    {
        BkgSettingElement curBkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (curBkgSettingElement == null)
        {
            Debug.Log("找不到背景" + bkgName);
            return;
        }
        curBkgSettingElement.referPointType(sourcePointType, targetPointType);

    }

    void fixNewPoints2(float height,float width,float pivotX,float pivotY)
    {
        existedBkgSettingList.bkgSettingList = getExistedBkgSetting();

        foreach (BkgSettingElement bkgSettingElement in existedBkgSettingList.bkgSettingList)
        {
            foreach (List<float> pos in bkgSettingElement.initPositionList)
            {
                pos[0] = (pos[0] - pivotX) / (width / 2f);
                pos[1] = (pos[1] - pivotY) / (height / 2f);
                
            }
        }

        JsonOperator.Obj2Json<BkgSettingList>(existedBkgSettingList, fileName);


    }

    void fixNewPoints(float offsetX,float offsetY,float stretchX,float stretchY)
    {
        existedBkgSettingList.bkgSettingList = getExistedBkgSetting();

        foreach(BkgSettingElement bkgSettingElement in existedBkgSettingList.bkgSettingList)
        {
            foreach(List<float> pos in bkgSettingElement.initPositionList)
            {
                pos[0] += offsetX;
                pos[1] += offsetY;
                pos[0] *= stretchX;
                pos[1] *= stretchY;
            }
        }

        JsonOperator.Obj2Json<BkgSettingList>(existedBkgSettingList, fileName);
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
