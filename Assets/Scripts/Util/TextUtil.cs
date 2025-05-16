using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextUtil : MonoBehaviour
{

    public static bool IsHanziChar(string value)
    {
        bool re = true;
        Regex regex = new Regex("^[\u4E00-\u9FA5]+$");
        if (regex.IsMatch(value))
        {
            re = true;
        }
        else re = false;
        return re;
       
    }
}
