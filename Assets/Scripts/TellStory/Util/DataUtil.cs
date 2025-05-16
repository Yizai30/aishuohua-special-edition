using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Jint;
using System;
using System.Text;

public class DataUtil : MonoBehaviour
{
    //判断两个列表完全相等
    public static bool compareList<T>(List<T> list1, List<T> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }
        for (int i = 0; i < list1.Count; i++)
        {
            if (!list1[i].Equals(list2[i]))
            {
                return false;
            }
        }
        return true;
    }

    //判断两个列表乱序相等
    public static bool compareListNoOrder(List<string> list1, List<string> list2)
    {
        if (list1.Count != list2.Count) return false;
        for (int i = 0; i < list1.Count; i++)
        {
            if (!list2.Contains(list1[i]))
            {
                return false;
            }
        }
        return true;
    }

    //判断一个List1中是否包含另一个List2的所有元素
    public static bool isContainList(List<string> list1, List<string> list2)
    {
        for (int i = 0; i < list2.Count; i++)
        {
            if (!list1.Contains(list2[i]))
            {
                return false;
            }
        }
        return true;
    }
    //返回相同个数
    public static int getSameCount(List<string> list1, List<string> list2)
    {
        int count = 0;
        foreach (string str in list1)
        {
            if (list2.Contains(str))
            {
                count++;
            }
        }
        return count;
    }

    //列表数乘
    public static List<float> listMul(List<float> list,List<float> sc)
    {
        List<float> re = new List<float>();
        for(int i = 0; i < list.Count; i++)
        {
        
            re.Add(list[i] * sc[i]);
        }
        return re;
    }

    public static List<float> listMul(List<float> list, float sc)
    {
        List<float> re = new List<float>();
        for (int i = 0; i < list.Count; i++)
        {

            re.Add(list[i] * sc);
        }
        return re;
    }

    //列表数乘
    public static List<float> listDiv(List<float> list, List<float> sc)
    {
        
        List<float> re = new List<float>();
        for (int i = 0; i < list.Count; i++)
        {
            if (sc[i].Equals(0)) throw new System.Exception("除零");
            re.Add(list[i] / sc[i]);
        }
        return re;
    }

    public static List<float> listDiv(List<float> list, float sc)
    {
        List<float> re = new List<float>();
        if (sc.Equals(0)) throw new System.Exception("除零");
        for (int i = 0; i < list.Count; i++)
        {

            re.Add(list[i] / sc);
        }
        return re;
    }

    public static List<float> listAdd(List<float> list1, List<float> list2)
    {
        if (list1.Count != list2.Count) throw new System.Exception("列表长度不同");
        List<float> re = new List<float>();
        for (int i = 0; i < list1.Count; i++)
        {

            re.Add(list2[i]+list1[i]);
        }
        return re;
    }

    public static List<float> listSub(List<float> list1, List<float> list2)
    {
        if (list1.Count != list2.Count) throw new System.Exception("列表长度不同");
        List<float> re = new List<float>();
        for (int i = 0; i < list1.Count; i++)
        {

            re.Add(list1[i] - list2[i]);
        }
        return re;
    }


    public static string getMatnameByGoname(string goname)
    {
        string re = "";
        int stopFlag = 0;
        for(int i = goname.Length-1; i >= 0; i--)
        {
            if(goname[i]>='0' && goname[i] <= '9')
            {
                continue;
            }
            else
            {
                stopFlag = i;
                break;
            }
        }

        re = goname.Substring(0, stopFlag+1);
        return re;
    }

    public static T CloneByJson<T>(T baseObject)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(baseObject));
    }

    public static T Clone<T>(T obj)
    {
        object retval;
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            retval = bf.Deserialize(ms);
            ms.Close();
        }
        return (T)retval;
    }

    public static bool containEn(string str)
    {
        for(int i = 0; i < str.Length; i++)
        {
            if(str[i]>='a' && str[i] <= 'z')
            {
                return true;
            }
        }
        return false;

    }

    public static float getDis(List<float> pos1, List<float> pos2)
    {
        if (pos1.Count == 0 || pos2.Count == 0) return 0;
        
        return (Mathf.Sqrt(
            (pos1[0]- pos2[0])*(pos1[0]-pos2[0]) + 
            (pos1[1] - pos2[1])*(pos1[1]-pos2[1]) +
            (pos1[2] - pos2[2])*(pos1[2]-pos2[2])
            ));
    }


    public static void TestCondition(string conditionCode,MoveState moveState,ref bool res)
    {
        Eval(conditionCode, moveState,ref res);
       
    }


    // Eval > Evaluates C# sourcelanguage
    public static void Eval(string sCSCode, MoveState moveState,ref bool res)
    {


        StringBuilder sb = new StringBuilder();
        sb.Append("re=");
        sb.Append(sCSCode);
        sCSCode = sb.ToString();
        try {
            bool re = false;
            Engine engine = new Engine();
            engine.SetValue("moveState", moveState);
            engine.SetValue("re", re);
            //engine.SetValue("log", new Action<object>(msg => Debug.Log(msg)));

            engine.SetValue("StrContains", new Func<string, string,bool>((s1, s2) =>  s1.Contains(s2)));
            engine.SetValue("ListContains", new Func<List<string>,string,bool>((l1, l2) =>  l1.Contains(l2)));

            engine.Execute(sCSCode);
            res= engine.GetValue("re").AsBoolean();
            //res = re;

        }
        catch(Exception e)
        {
            res = false;
        }
       


        /*
        CodeDomProvider pro = new Microsoft.CSharp.CSharpCodeProvider();
        CompilerParameters cp = new CompilerParameters();
        cp.ReferencedAssemblies.Add("system.dll");
        cp.CompilerOptions = "/t:library";
        cp.GenerateInMemory = true;
        

        System.Text.StringBuilder sb = new System.Text.StringBuilder("");
        sb.Append("using System;\n");
        sb.Append("using System.Collections.Generic;\n");
        sb.Append("namespace CSCodeEvaler{ \n");
        sb.Append("public class CSCodeEvaler{ \n");
        sb.Append("public object EvalCode(){\n");
        sb.Append(param);
        sb.Append("return " + sCSCode + "; \n");
        sb.Append("} \n");
        sb.Append("} \n");
        sb.Append("}\n");

        CompilerResults cr = pro.CompileAssemblyFromSource(cp, sb.ToString());
        if (cr.Errors.Count > 0)
        {
            foreach(CompilerError err in cr.Errors)
            {
                if (!err.IsWarning)
                {
                    System.Console.WriteLine("ERROR: " + cr.Errors[0].ErrorText);
                    return null;
                }
            }
           

            //return null;
        }

        System.Reflection.Assembly a = cr.CompiledAssembly;
        object o = a.CreateInstance("CSCodeEvaler.CSCodeEvaler");

        System.Type t = o.GetType();
        System.Reflection.MethodInfo mi = t.GetMethod("EvalCode");

        object s = mi.Invoke(o, null);
        return s;
        */

    }
}
