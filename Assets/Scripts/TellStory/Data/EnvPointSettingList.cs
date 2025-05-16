using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvPointSettingList 
{
    public List<EnvPointSettingElement> envPointSettingList;

    public EnvPointSettingList(List<EnvPointSettingElement> envPointSettingList)
    {
        this.envPointSettingList = envPointSettingList;
    }
    public EnvPointSettingList()
    {
        this.envPointSettingList = new List<EnvPointSettingElement>();
    }

    public EnvPointSettingElement getElementByEnvPropMatName(string envPropMatName)
    {
        foreach(EnvPointSettingElement envPointSettingElement in envPointSettingList)
        {
            if (envPointSettingElement.envPropList.Contains(envPropMatName))
            {
                return envPointSettingElement;
            }
        }
        return null;
    }

    public bool containEnvPropMatName(string envpropMatName)
    {
        foreach (EnvPointSettingElement envPointSettingElement in envPointSettingList)
        {
            if (envPointSettingElement.envPropList.Contains(envpropMatName))
            {
                return true;
            }
        }
        return false;
    }
}

public class EnvPointSettingElement
{
    public List<string> envPropList { get; }

    public List<string> pointNameList { get; }

    public List<string> defaultBackgroundList { get; }
    public string appear { get; }

    public EnvPointSettingElement(List<string> envPropList, List<string> pointNameList, List<string> defaultBackgroundList,string appear)
    {
        this.envPropList = envPropList;
        this.pointNameList = pointNameList;
        this.defaultBackgroundList = defaultBackgroundList;
        this.appear = appear;

        if (envPropList == null) this.envPropList = new List<string>();
        if (pointNameList == null) this.pointNameList = new List<string>();
        if (defaultBackgroundList == null) this.defaultBackgroundList = new List<string>();

    }
}
