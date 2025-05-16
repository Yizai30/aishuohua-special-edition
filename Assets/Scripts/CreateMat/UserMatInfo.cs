using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserMatInfoList
{
    public UserMatInfoList()
    {
        this.userMatInfoList = new List<UserMatInfoElement>();
    }
    public List<UserMatInfoElement> userMatInfoList { set; get; }

    public UserMatInfoElement FindUserMatInfoByRawName(string rawName)
    {
        foreach(UserMatInfoElement userMatInfoElement in userMatInfoList)
        {
            if (userMatInfoElement.rawName == rawName)
            {
                return userMatInfoElement;
            }
        }
        return null;
    }

    public UserMatInfoElement FindUserMatInfoByMatName(string matName)
    {
        foreach (UserMatInfoElement userMatInfoElement in userMatInfoList)
        {
            if (userMatInfoElement.matName == matName)
            {
                return userMatInfoElement;
            }
        }
        return null;
    }

    public bool ContainsUserMatInfoByMatName(string matName)
    {
        foreach (UserMatInfoElement userMatInfoElement in userMatInfoList)
        {
            if (userMatInfoElement.matName == matName)
            {
                return true;
            }
        }
        return false;
    }

    public bool ContainsUserMatInfoByRawName(string rawName)
    {
        foreach (UserMatInfoElement userMatInfoElement in userMatInfoList)
        {
            if (userMatInfoElement.rawName == rawName)
            {
                return true;
            }
        }
        return false;
    }

    public void AddNewUserMatInfo(UserMatInfoElement userMatInfoElement)
    {
        if (this.ContainsUserMatInfoByMatName(userMatInfoElement.matName))
        {
            this.RemoveUserMat(userMatInfoElement.matName);
        }
        this.userMatInfoList.Add(userMatInfoElement);
    }

    public void RemoveUserMat(string matName)
    {
        List<UserMatInfoElement> newUserList = new List<UserMatInfoElement>();
        foreach(UserMatInfoElement userMatInfoElement in this.userMatInfoList)
        {
            if (!userMatInfoElement.matName.Equals(matName))
            {
                newUserList.Add(userMatInfoElement);
            }
        }
        this.userMatInfoList = newUserList;
    }

    public List<string> getAllRawName()
    {
        List<string> re = new List<string>();
        foreach(UserMatInfoElement userMatInfoElement in this.userMatInfoList)
        {
            if (!re.Contains(userMatInfoElement.rawName))
            {
                re.Add(userMatInfoElement.rawName);
            }
        }
        return re;
    }
}

[Serializable]
public class UserMatInfoElement 
{
    public string creator { set; get; }
    public string rawName { set; get; }
    public string matName { set; get; }
    public string url { set; get; }
    public string className { set; get; }

    public UserMatInfoElement(string creator, string rawName, string matName, string url, string className)
    {
        this.creator = creator;
        this.rawName = rawName;
        this.matName = matName;
        this.url = url;
        this.className = className;
    }
}
