using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class UsrDicElement
{
    public string name { set; get; }
    public string type { set; get; }
    public string freq { set; get; }

    public UsrDicElement(string name, string type, string freq)
    {
        this.name = name;
        this.type = type;
        this.freq = freq;
    }

    public UsrDicElement(string name, string type)
    {
        this.name = name;
        this.type = type;
        this.freq = "";
    }

   
}

public class UsrDicGenerator : MonoBehaviour
{

    //��Դ��
    //convmap��diyaction��pointSetting
    List<UsrDicElement> usrDicElementList;

    private void Awake()
    {
        usrDicElementList = new List<UsrDicElement>();
    }

    void genFromConvMap()
    {
        //DataMap.convMatMapList.
       
        List<string> actionRawName = DataMap.convMatMapList.getAllActionRawName();
        addUsrEle(actionRawName, "v");

        List<string> actorPropRawName = DataMap.convMatMapList.getAllActorAndPropRawName();
        addUsrEle(actorPropRawName, "n");

        List<string> bkgRawName = DataMap.convMatMapList.getAllBackgroundRawName();
        addUsrEle(bkgRawName, "s");
       

        List<string> seasonRawName = DataMap.convMatMapList.getAllSeasonRawName();
        addUsrEle(seasonRawName, "t");

    }

    void genFromConvChildMap()
    {
        //DataMap.convMatMapList.

        List<string> actionRawName = DataMap.convMatMapListChild.getAllActionRawName();
        addUsrEle(actionRawName, "v");

        List<string> actorPropRawName = DataMap.convMatMapListChild.getAllActorAndPropRawName();
        addUsrEle(actorPropRawName, "n");

        List<string> bkgRawName = DataMap.convMatMapListChild.getAllBackgroundRawName();
        addUsrEle(bkgRawName, "s");

    }

    void genFromDiyAction()
    {
        List<string> diyRawNameList = DataMap.diyActList.getAllDiyRawName();
        addUsrEle(diyRawNameList, "v");
    }

    void genFromPointName()
    {
        List<string>actionFlag= DataMap.pointSettingList.getAllActionFlags();
        addUsrEle(actionFlag, "v");
       

        List<string> locationFlag = DataMap.pointSettingList.getAllLocationFlags();
        addUsrEle(locationFlag, "s");
        
    }

    void genFromFlag()
    {
        
        addUsrEle(DataMap.aroundFlag, "s");
        addUsrEle(DataMap.attrChangeFlag, "v");
        addUsrEle(DataMap.speakFlag, "v");
        addUsrEle(DataMap.denyFlag, "v");
        addUsrEle(DataMap.pluralPronounList, "r");

    }

    void addUsrEle(List<string> strList,string type)
    {
        foreach (string rawName in strList)
        {
            UsrDicElement usrDicElement = new UsrDicElement(rawName, type);
            this.usrDicElementList.Add(usrDicElement);
        }
    }


    void addUserMatName()
    {
        List<string> privateActorNameList = DataMap.privateActorList.GetAllRawName();
        List<string> privatePropNameList = DataMap.privatePropList.GetAllRawName();
        List<string> privateBackgroundNameList = DataMap.privateBackgroundList.GetAllRawName();
        addUsrEle(privatePropNameList, "n");
        addUsrEle(privateActorNameList, "n");
        addUsrEle(privateBackgroundNameList, "n");
    }

    void addExtra()
    {
        List<string> vadj = new List<string> { "�Ե�", "���", "����" };
        addUsrEle(vadj, "a");
    }

    void assembleUsrDic()
    {
        genFromConvMap();
        genFromConvChildMap();
        genFromDiyAction();
        genFromPointName();
        genFromFlag();
        addExtra();
        addUserMatName();
        quchong();
    }

    //ȥ��
    void quchong()
    {
        List<UsrDicElement> re = new List<UsrDicElement>();
        foreach(UsrDicElement element in this.usrDicElementList)
        {
            if (containsName(re, element.name)){
                continue;
            }
            re.Add(element);
        }
        this.usrDicElementList = re;
    }

    bool containsName(List<UsrDicElement>eleList, string name)
    {
        foreach(UsrDicElement usrDicElement in eleList)
        {
            if (name == usrDicElement.name)
            {
                return true;
            }
        }
        return false;
    }

    void getUsrDicFreq()
    {
       
        List<List<UsrDicElement>> tmpList = new List<List<UsrDicElement>>();
        foreach(UsrDicElement ele in usrDicElementList)
        {
            bool added = false;
            string eleName = ele.name;
            foreach(List<UsrDicElement> eleList in tmpList)
            {
                foreach(UsrDicElement element in eleList)
                {
                    string key = element.name;
                    if (eleName == key)
                    {
                        added = true;
                        break;
                    }
                    //�´ʵ�һ���ְ������д�
                    if (eleName.Contains(key) || key.Contains(eleName))
                    {
                        eleList.Add(ele);
                        added = true;
                        break;
                    }
                }
               
                
               
            }
            if (added == false)
            {
                tmpList.Add(new List<UsrDicElement> { ele });
            }
            
           


        }

        //��tmpList�е�eleList���ճ�������
        foreach(List<UsrDicElement> eleList in tmpList)
        {
            eleList.Sort((x, y) => x.name.Length.CompareTo(y.name.Length));
        }

        //Ϊelement��freq��ֵ

        foreach(List<UsrDicElement> eleList in tmpList)
        {
            if (eleList.Count == 1)
            {
                eleList[0].freq = "";
            }
            else
            {
                int baseFreq = 3;
                int add = 0;
                for(int i = 0; i < eleList.Count; i++)
                {
                    if (i > 0 && eleList[i].name.Length == eleList[i - 1].name.Length)
                    {
                        eleList[i].freq = eleList[i - 1].freq;
                        continue;
                    }
                    eleList[i].freq = Convert.ToString(baseFreq + add);
                    add = add + 2;
                }
               
            }
        }
    }

    void writeUsrDicFile()
    {
        string filePath = Application.persistentDataPath + "/jiebaConfig/usr_dic.txt";
        using (StreamWriter writetext = new StreamWriter(filePath))
        {
            foreach(UsrDicElement usrDicElement in usrDicElementList)
            {
                string str = "";
                if (usrDicElement.freq != "")
                {
                    str = usrDicElement.name + " " + usrDicElement.freq + " " + usrDicElement.type;
                }
                else
                {
                    str = usrDicElement.name + " " + usrDicElement.type;
                }
                if (str != "")
                {
                    writetext.WriteLine(str);
                }
               

            }
            
        }

    }
    
    public void initUserDic()
    {
        //DataMap.initData();
        assembleUsrDic();
        getUsrDicFreq();
        writeUsrDicFile();
        AsrMain.regenProcessor();
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    

  
}
