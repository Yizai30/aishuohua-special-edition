using Jint;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEval : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
     
        int re = -1;
        
        MoveState moveState = new MoveState("test1", new List<string> { "t", "t" }, "test2", new List<string> { "d", "d" });   
        try
        {
            bool res = false;
            Engine engine = new Engine();
            engine.SetValue("moveState", moveState);

            engine.SetValue("log", new Action<object>(msg => Debug.Log(msg)));

            engine.SetValue("Contains", new Action<string,string>((s1,s2)=>res=s1.Contains(s2)));


            //engine.Execute(@"De=" + "subCurPointFlag.includes(\"test\")" + ";");

            engine.Execute(@"var str=moveState.subCurPointFlag;
                            log(str);
                            Contains(str,'test');");
            Debug.Log(res);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        //DataUtil.Eval("subCurPointFlag.includes(\"test\")", moveState,ref re);
        //Debug.Log("ssssssssssss"+re);
      
       
    }

   


    private void Update()
    {
        
    }


}
