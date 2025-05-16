
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JsonOperator
{
   


    public static T parseObj<T>(string jsonfile)
    {
        string jsonStr = File.ReadAllText(jsonfile,Encoding.UTF8);
        T re = JsonConvert.DeserializeObject<T>(jsonStr);
        return re;
    }

    public static T parseObjFormStr<T>(string jsonStr)
    {
        
        T re = JsonConvert.DeserializeObject<T>(jsonStr);
        return re;
    }

    public static object parseObjFormStr2(string jsonStr,Type objType)
    {
        object result=JsonConvert.DeserializeObject(jsonStr, objType);
        return result;
        
    }

    public static string Obj2jsonStr<T>(T obj)
    {
        string jsonStr = JsonConvert.SerializeObject(obj);
        return jsonStr;




    }


    public static void Obj2Json<T>(T obj, string jsonPath)
    {
        
        if (File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
            File.Create(jsonPath).Dispose();
        }
        StreamWriter writer = new StreamWriter(jsonPath);
        try
        {
            string json = JsonConvert.SerializeObject(obj);
            //Debug.Log("jsonstr" + json);
            
            writer.Write(json);
            //File.WriteAllText(jsonPath, json);
        }catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        finally
        {
            writer.Close();
        }
        
       
        
        
    }
  
}
