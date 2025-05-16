using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectList
{
    public List<CorrectListElement> correctList { set; get; }

    public CorrectList(List<CorrectListElement> correctList)
    {
        this.correctList = correctList;
    }
    public CorrectList()
    {
        this.correctList = new List<CorrectListElement>();
    }

    public string getCorrectFlag(string word)
    {
        string re = "";
        foreach(CorrectListElement correctListElement in correctList)
        {
            if (correctListElement.wordList.Contains(word))
            {
                re = correctListElement.wordFlag;
                break;
            }
        }
        return re;
    }
}


public class CorrectListElement
{
    public List<string> wordList { set; get; }
    public string wordFlag { set; get; }

    public CorrectListElement(List<string> wordList, string wordFlag)
    {
        this.wordList = wordList;
        this.wordFlag = wordFlag;
    }
}