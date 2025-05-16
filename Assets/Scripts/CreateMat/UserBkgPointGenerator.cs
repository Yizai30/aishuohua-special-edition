using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserBkgPointGenerator
{
   

    public static void genAllDefaultPoint(BkgSettingElement bkgSettingElement)
    {
        genPoints(bkgSettingElement, "defaultPoint", -0.6f, -0.6f, -0.7f, 0.7f, 6);

        genPoints(bkgSettingElement, "waterPoint", -0.35f, -0.35f, -0.7f, 0.7f, 6);

        genPoints(bkgSettingElement, "skyPoint", 0.45f, 0.45f, -0.7f, 0.7f, 6);

        genPoints(bkgSettingElement, "env_skyPoint", 0.35f, 0.35f, -0.85f, 0.85f, 6);

        genPoints(bkgSettingElement, "propPoint", -0.8f, -0.8f, -0.75f, 0.75f, 6);

        genPoints(bkgSettingElement, "sky_enterPoint", 0.5f, 0.5f, -0.85f, -0.8f, 5);

        genPoints(bkgSettingElement, "ground_enterPoint", -0.6f, -0.6f, -0.85f, -0.8f, 5);

        genPoints(bkgSettingElement, "exitPoint", -0.6f, -0.6f, 0.85f, 0.8f, 5);
    }

    static void  genPoints(BkgSettingElement bkgSettingElement,string pointType, float height1, float height2, float width1, float width2, int widthNum)
    {

        List<List<float>> points = genTypePointsk(height1, height2, width1, width2, widthNum);
        bkgSettingElement.addPointTypeAndPoint(pointType, points);

    }


    static List<List<float>> genTypePointsk(float height1, float height2, float width1, float width2, int widthNum)
    {
       

        //??????¦Ë
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
}
