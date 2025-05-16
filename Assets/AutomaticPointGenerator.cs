using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class AutomaticPointGenerator : MonoBehaviour
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
        fileName = Application.streamingAssetsPath + "/data/JsonFiles/bkgPrivateSettingList_gn.json";

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
        print("�ѻ�ȡ���µĵ�λ��Ϣ�����أ�"+fileName);
        setPanel(curBkgName);
        showBkgPoint(curBkgName);
    }

    //IEnumerator uploadPointFIle()
    //{
     //   yield return StartCoroutine(serverPoster.)
    //}

    //����panel��ʾ��ǰ����
    void setPanel(string bkgName)
    {
        try
        {
            Image bkgImage = panel.GetComponent<Image>();
            Sprite bkgSprite = Resources.Load<Sprite>("Environment/2D/" + bkgName);
            bkgImage.sprite = bkgSprite;
            if (bkgSprite == null)
            {
                //�������û��Լ����ɵ��Ǹ��ļ�����ȥ��һ��
                // ���飬���߹�������·��
                string folderPath = Path.Combine(Application.persistentDataPath, "images");
                print("folderPath is"+folderPath);
                
                string savePath = Path.Combine(folderPath, bkgName + ".png");
                print("savePath is"+savePath);
                // ͼƬ sprite
                
                //bkgSprite = Resources.Load<Sprite>(folderPath + bkgName);
                byte[] fileData = File.ReadAllBytes(savePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                // ����Sprite
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                bkgImage.sprite = sprite;
                if (bkgSprite is null)
                {
                    print("bkgSprite is null");
                }
                else
                {
                    print("ok...");
                }
                print("�������û��Լ����ɵ��Ǹ��ļ�����ȥ��һ��" + bkgName+ ".png");
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    //��ȡ��������

    public List<BkgSettingElement> getExistedBkgSetting()
    {
        List<BkgSettingElement> bkgSettingElements= JsonOperator.parseObj<List<BkgSettingElement>>(fileName);
       
        if (bkgSettingElements != null) return bkgSettingElements;
        else
        {
            Debug.Log("��ȡ������λ��Ϣʧ��");
            return null;
        }

    }

    //����б��
    List<List<float>> genTypePointsk(float height1, float height2, float width1, float width2, int widthNum)
    {
        //BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);      
        //���ɵ�λ
        
        //���ɵĵ�λ
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

    //����ĳһ��pointtype�ĵ�λ���ϣ�����Ѿ����ڣ��򸲸�
    List<List<float>> genTypePointsk0(float height1,float height2,float width1,float width2,int widthNum)
    {
        //BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);      
        //���ɵ�λ
        float y = height1 + (height2 - height1) / 2;
        //���ɵĵ�λ
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

    //�������ɵĵ�λ�����޸�element
    void editBkgElement(string bkgName,string pointType,List<List<float>> points)
    {
        BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (bkgSettingElement == null)
        {
            bkgSettingElement = new BkgSettingElement(bkgName, new List<List<float>>(), new List<PointMark>());
            existedBkgSettingList.bkgSettingList.Add(bkgSettingElement);
        }
        //�����pointType��ɾ����pointtype�Լ�����
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
            Debug.Log("��ǰ����������" + bkgName);
            return;
        }
        if (bkgSettingElement.containsPointType(pointType))
        {
            bkgSettingElement.addPointsToType(pointType,points);
        }
    }

    //���ݵ�ǰbkgSettingList��������json
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

    //��ʾ�ض������ĵ�λ
    void showBkgPoint(string bkgName)
    {
        clearScreenPoint();

        GameObject markGo = (GameObject)Resources.Load("Test/mark", typeof(GameObject));               
        BkgSettingElement bkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (bkgSettingElement == null)
        {
            Debug.Log("û���ҵ�������Ϣ" + bkgName);
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
                        Debug.Log("δ�ҵ�image");
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

    //�����ض���λ����ɫ
    Color getMarkColor(string pointType)
    {
        if (pointType.Contains("enterPoint") || pointType.Contains("escapePoint") || pointType.Contains("exitPoint"))
        {
            //��ɫ
            return getColorByList(new List<float> { 0.4f, 0.4f, 0.4f });
        }else if (pointType.EndsWith("defaultPoint"))
        {
            //��ɫ
            return getColorByList(new List<float> { 1, 1, 1 });
        }else if (pointType.Contains("waterPoint"))
        {
            //��ɫ
            return getColorByList(new List<float> { 0, 1, 0 });
        }
        else if (pointType.Contains("skyPoint"))
        {
            //��ɫ
            return getColorByList(new List<float> { 0, 0, 1 });
        }
        else if (pointType.Contains("propPoint"))
        {
            //��ɫ
            return getColorByList(new List<float> { 1, 0.38f, 0 });
        }
        else
        {
            //��������ɫ
            return getColorByList(new List<float> { 0.627f, 0.125f, 0.941f });
        }
    }


    //�����ض����͵ĵ�λ
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
            Debug.Log("color��������");
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
            Debug.Log("�����뱳���͵�λ������Ϣ");
            return;
        }

        Debug.Log("���ɵ�λ...");
        List<List<float>> genPosList = genTypePointsk(height1, height2, width1, width2, widthNum);
        List<List<float>> normGenPosList = new List<List<float>>();
        foreach (List<float> pos in genPosList)
        {
           normGenPosList.Add(screen2norm(pos));
        }
        editBkgElement(curBkgName, pointType, normGenPosList);
        Debug.Log("bkgName:" + curBkgName + " pointType:" + pointType + " points:" + normGenPosList);
        
        //�ļ�       
        copySeason(curBkgName);     
        
        genBkgSettingJson();
        showBkgPoint(curBkgName);
    }

    public void onClickChangeBkg()
    {
        curBkgName = bkgNameField.text;
        if (curBkgName.Equals(""))
        {
            Debug.Log("�����뱳������");
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
            Debug.Log("����дreSourcePointType��reTargetPointType");
            return;
        }
        copyReference(curBkgName, reSourcePointType, reTargetPointType);
        //existedBkgSettingList = getExistedBkgSetting();
        Debug.Log("�����õ�λ" + reSourcePointType + "��" + reTargetPointType);
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
            Debug.Log("�����뱳���͵�λ������Ϣ");
            return;
        }

        Debug.Log("���ɵ�λ...");
        List<List<float>> genPosList = genTypePointsk(height1, height2, width1, width2, widthNum);
        List<List<float>> normGenPosList = new List<List<float>>();
        foreach(List<float>pos in genPosList)
        {
            normGenPosList.Add(screen2norm(pos));
        }
        addPointToPointType(curBkgName, pointType, normGenPosList);
        Debug.Log("bkgName:" + curBkgName + " pointType:" + pointType + " points:" + normGenPosList);

        //�ļ�       
        copySeason(curBkgName);

        genBkgSettingJson();
        showBkgPoint(curBkgName);
    }

    void copySeason(string bkgName)
    {
       
        BkgSettingElement curBkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if(curBkgSettingElement==null)
        {
            Debug.Log("�Ҳ�������" + bkgName);
        }
        List<BkgSettingElement> samePointBkgList = existedBkgSettingList.getElementsContainBkgName(curBkgSettingElement.backgroundName);
        foreach(BkgSettingElement bkgSettingElement in samePointBkgList)
        {
            if (!containSeaasonName(bkgSettingElement.backgroundName))
            {
                continue;
            }
            bkgSettingElement.copyElementInfo(curBkgSettingElement);
            Debug.Log("��ͬ����λ��Ϣ�� " + bkgSettingElement.backgroundName);
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

    //����һ����λ�ı���
    void copyReference(string bkgName,string sourcePointType,string targetPointType)
    {
        BkgSettingElement curBkgSettingElement = existedBkgSettingList.getElementByBkgName(bkgName);
        if (curBkgSettingElement == null)
        {
            Debug.Log("�Ҳ�������" + bkgName);
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
