using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PathTransformChanger
{

    public static void faceToRight( PathTransform pathTransform,string matName)
    {
        if (matName.Contains("3D"))
        {
            
            List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(matName).initRotation);
            pathTransform.endRotation = originRotation;
            pathTransform.startRotation = DataUtil.Clone<List<float>>(pathTransform.endRotation);
        }
        else
        {
            float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(matName).initScale[0]);
            float subScale = Mathf.Abs(pathTransform.endScale[0]);
            pathTransform.endScale[0]= subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
            
        }
    }

    public static void faceToLeft( PathTransform pathTransform, string matName)
    {
        if (matName.Contains("3D"))
        {

            List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(matName).initRotation);
            pathTransform.endRotation[1] = originRotation[1]+120f;
            pathTransform.startRotation = DataUtil.Clone<List<float>>(pathTransform.endRotation);
        }
        else
        {
            float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(matName).initScale[0]);
            float subScale = Mathf.Abs(pathTransform.endScale[0]);
            pathTransform.endScale[0] =-1* subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);

        }
    }

    public static bool isFaceRight(PathTransform pathTransform,string matName)
    {
        bool re = false;
        if (matName.Contains("3D"))
        {
            List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(matName).initRotation);
            if (DataUtil.compareList<float>(originRotation, pathTransform.endRotation))
            {
                re = true;
            }
            else
            {
                re = false;
            }
        }
        else
        {
            float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(matName).initScale[0]);
            if (subInitXScale * pathTransform.endScale[0] > 0)
            {
                re = true;
            }
            else
            {
                re = false;
            }
        }
        return re;
    }

    //让obj和sub相对静止移动
    public static void movePositionTogether( PathTransform pathTransformSub,ref PathTransform pathTransformObj)
    {
        List<float> gap = DataUtil.listSub(pathTransformObj.startPosition, pathTransformSub.startPosition);
        pathTransformObj.endPosition = DataUtil.Clone<List<float>>(DataUtil.listAdd(gap, pathTransformSub.endPosition));

    }


    public static void moveToDis(string subMatName, PathTransform pathTransform,List<float>dis)
    {
        pathTransform.endPosition = dis;
        faceTo( pathTransform, subMatName, dis);
    }
    public static void tumble( PathTransform pathTransform)
    {
        pathTransform.endRotation[2] = 90;
        pathTransform.startRotation[2] = 90;
    }

    public static void tumbleRecover( PathTransform pathTransform,string subName)
    {
        List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(subName).initRotation);
        pathTransform.endRotation[2] = originRotation[2];
        pathTransform.startRotation[2] = originRotation[2];
    }

    public static void turnBack(PathTransform pathTransformSub,string subMatName)
    {
        float subScale = Mathf.Abs(pathTransformSub.endScale[0]);
        if (subMatName.Contains("3D"))
        {
            List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(subMatName).initRotation);
            if (!DataUtil.compareList<float>( pathTransformSub.endRotation , originRotation))
            {
                pathTransformSub.endRotation = originRotation;
                pathTransformSub.startRotation = DataUtil.Clone<List<float>>(originRotation);
            }
            else
            {
                pathTransformSub.endRotation[1] = originRotation[1] + 120;
                pathTransformSub.startPosition = DataUtil.Clone<List<float>>(pathTransformSub.endRotation);
            }
        }
        else
        {
            float subInitXScale = DataUtil.Clone<float>(pathTransformSub.endScale[0]);
            pathTransformSub.endScale[0]= -1 * subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
            pathTransformSub.startScale = DataUtil.Clone<List<float>>(pathTransformSub.endScale);
        }
    }

    //向左返回false 向右返回true
    public static bool faceTo( PathTransform pathTransformSub,string subName,List<float> endPos)
    {
        float subScale = Mathf.Abs(pathTransformSub.endScale[0]);
        bool isRight = true;
        if (subName.Contains("3D"))
        {
            if (endPos[0] - pathTransformSub.startPosition[0] < 0)
            {
                List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(subName).initRotation);
                pathTransformSub.startRotation[1] = originRotation[1] + 120;

                isRight = false;
            }
            else
            {
                List<float> originRotation = DataUtil.Clone<List<float>>(DataMap.matSettingList.getMatSettingMap(subName).initRotation);
                pathTransformSub.startRotation[1] = originRotation[1];
                isRight = true;
            }
            pathTransformSub.endRotation[1] = DataUtil.Clone<float>( pathTransformSub.startRotation[1]);
        }
        else
        {
            float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initScale[0]);
            if (pathTransformSub.startPosition[0] - endPos[0] > 0)
            {
                pathTransformSub.endScale[0] = -1 * subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
                isRight = false;
            }
            else
            {
                pathTransformSub.endScale[0] = subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
                isRight = true;

            }
            pathTransformSub.startScale = DataUtil.Clone<List<float>>(pathTransformSub.endScale);
        }
        return isRight;
    }

    public static void faceTo( PathTransform pathTransformSub, PathTransform pathTransformObj,string subName)
    {
        float subScale = Mathf.Abs(pathTransformSub.endScale[0]);
        if (subName.Contains("3D"))
        {
            faceTo3D(ref pathTransformSub, ref pathTransformObj, subName);
        }
        else
        {
            faceTo2D(ref pathTransformSub, ref pathTransformObj, subName, subScale);
        }
    }

    public static void faceToFace(ref PathTransform pathTransformSub, ref PathTransform pathTransformObj,
        string subName, string objName, float subScale, float objScale)
    {
        if (subName.Contains("3D")&&objName.Contains("3D"))
        {
            faceToFace3D3D(ref pathTransformSub, ref pathTransformObj, subName, objName);
        }
        else if(subName.Contains("3D")&& !objName.Contains("3D"))
        {
            faceToFace3D2D(ref pathTransformSub,ref pathTransformObj,subName,objName,objScale);
        }
        else if (!subName.Contains("3D") && objName.Contains("3D"))
        {
            faceToFace2D3D(ref pathTransformSub, ref pathTransformObj, subName, objName, subScale);
        }
        else
        {
            faceToFace2D2D(ref pathTransformSub, ref pathTransformObj, subName, objName, subScale, objScale);
        }
    }

    //将from面向to
    public static void faceTo2D(ref PathTransform pathTransformSub,ref PathTransform pathTransformObj,string subName,float subScale)
    {
        float subInitXScale= DataUtil.Clone<float> (DataMap.matSettingList.getMatSettingMap(subName).initScale[0]);
        if (pathTransformSub.endPosition[0] - pathTransformObj.endPosition[0] > 0)
        {
            pathTransformSub.endScale[0] =-1* subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);  
        }
        else
        {
            pathTransformSub.endScale[0] = subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
           
        }
        pathTransformSub.startScale = DataUtil.Clone<List<float>>(pathTransformSub.endScale);

    }

    public static void faceTo3D(ref PathTransform pathTransformSub, ref PathTransform pathTransformObj,string subName)
    {
        float subInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initRotation[1]);
        if (pathTransformSub.endPosition[0] - pathTransformObj.endPosition[0] > 0)
        {
            pathTransformSub.endRotation[1] = subInitYRotation+120;
        }
        else
        {
            pathTransformSub.endRotation[1] = subInitYRotation;
        }
        pathTransformSub.startRotation = DataUtil.Clone<List<float>>(pathTransformSub.endRotation);
    }

    public static void faceToFace2D2D(ref PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName, string objName, 
        float subScale,float objScale)
    {
        float subInitXScale= DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initScale[0]);
        float objInitXScale= DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initScale[0]);

        if (pathTransformSub.endPosition[0] - pathTransformObj.endPosition[0] > 0)
        {
            pathTransformSub.endScale[0] = -1* subInitXScale /Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
            pathTransformObj.endScale[0] = objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);
        }
        else
        {
            pathTransformSub.endScale[0] =  subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
            pathTransformObj.endScale[0] = -1*objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);

        }
        pathTransformSub.startScale = DataUtil.Clone<List<float>>(pathTransformSub.endScale);
        pathTransformObj.startScale = DataUtil.Clone < List<float> > (pathTransformObj.endScale);
    }

    public static void faceToFace2D3D(ref PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName,string objName,
        float subScale)
    {
        float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initScale[0]);
        float objInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initRotation[1]);
        if (pathTransformSub.endPosition[0] - pathTransformObj.endPosition[0] > 0)
        {
            pathTransformSub.endScale[0] = -1 * subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
            pathTransformObj.endRotation[1] = objInitYRotation;
        }
        else
        {
            pathTransformSub.endScale[0] = subInitXScale / Mathf.Abs(subInitXScale) * Mathf.Abs(subScale);
            pathTransformObj.endRotation[1] = objInitYRotation + 120;

        }
        pathTransformSub.startScale = DataUtil.Clone<List<float>>(pathTransformSub.endScale);
        pathTransformObj.startRotation = DataUtil.Clone<List<float>>(pathTransformObj.endRotation);
    }

    public static void faceToFace3D2D(ref PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName,string objName,
        float objScale)
    {
        float subInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initRotation[1]);
        float objInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initScale[0]);
        if (pathTransformSub.endPosition[0] - pathTransformObj.endPosition[0] > 0)
        {
            pathTransformSub.endRotation[1] = subInitYRotation + 120;
            pathTransformObj.endScale[0] = objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);
        }
        else
        {
            pathTransformSub.endRotation[1] = subInitYRotation;
            pathTransformObj.endScale[0] = -1 * objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);

        }
        pathTransformSub.startRotation = DataUtil.Clone<List<float>>(pathTransformSub.endRotation);
        pathTransformObj.startScale = DataUtil.Clone<List<float>>(pathTransformObj.endScale);
    }

    public static void faceToFace3D3D(ref PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName, string objName)
    {
        float subInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initRotation[1]);
        float objInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initRotation[1]);
        if (pathTransformSub.endPosition[0] - pathTransformObj.endPosition[0] > 0)
        {
            pathTransformSub.endRotation[1] = subInitYRotation + 120;
            pathTransformObj.endRotation[1] = objInitYRotation;
        }
        else
        {
            pathTransformSub.endRotation[1] = subInitYRotation;
            pathTransformObj.endRotation[1] = objInitYRotation+120;

        }
        pathTransformSub.startRotation = DataUtil.Clone<List<float>>(pathTransformSub.endRotation);
        pathTransformObj.startRotation = DataUtil.Clone<List<float>>(pathTransformObj.endRotation); 
    }

    //使得obj面向sub同方向
    public static void faceSameDirection(PathTransform pathTransformSub,ref PathTransform pathTransformObj,string subName,string objName)
    {
        float objScale = Mathf.Abs( pathTransformObj.endScale[0]);
        if (subName.Contains("3D") && objName.Contains("3D"))
        {
            faceSameDirection3D3D( pathTransformSub, ref pathTransformObj, subName, objName);
        }
        else if (subName.Contains("3D") && !objName.Contains("3D"))
        {
            faceSameDirection3D2D( pathTransformSub, ref pathTransformObj, subName, objName, objScale);
        }
        else if (!subName.Contains("3D") && objName.Contains("3D"))
        {
            faceSameDirection2D3D( pathTransformSub, ref pathTransformObj, subName, objName);
        }
        else
        {
            faceSameDirection2D2D( pathTransformSub, ref pathTransformObj, subName, objName, objScale);
        }
    }

  
    public static void faceSameDirection2D2D( PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName, string objName, float objScale)
    {
        float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initScale[0]);
        float objInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initScale[0]);
        if (pathTransformSub.endScale[0] * subInitXScale >= 0)
        {
            //sub向右
            pathTransformObj.endScale[0]= objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);
        }
        else
        {
            pathTransformObj.endScale[0] = -1*objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);
        }       
        
        pathTransformObj.startScale = DataUtil.Clone<List<float>>(pathTransformObj.endScale);
    }

    public static void faceSameDirection2D3D( PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName, string objName)
    {
        float subInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initScale[0]);
        float objInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initRotation[1]);
        if (pathTransformSub.endScale[0] * subInitXScale >= 0)
        {
            //sub向右
            pathTransformObj.endRotation[1] = objInitYRotation;
        }
        else
        {
            pathTransformObj.endRotation[1] = objInitYRotation + 120;
        }

        pathTransformObj.startRotation = DataUtil.Clone<List<float>>(pathTransformObj.endRotation);
    }

    public static void faceSameDirection3D2D( PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName, string objName,
        float objScale)
    {
        float subInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initRotation[1]);
        float objInitXScale = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initScale[0]);
        if (pathTransformSub.endRotation[1] .Equals(subInitYRotation))
        {
            //sub向右
            pathTransformObj.endScale[0] = objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);
        }
        else
        {
            pathTransformObj.endScale[0] = -1 * objInitXScale / Mathf.Abs(objInitXScale) * Mathf.Abs(objScale);
        }

        pathTransformObj.startScale = DataUtil.Clone<List<float>>(pathTransformObj.endScale);
    }

    public static void faceSameDirection3D3D( PathTransform pathTransformSub, ref PathTransform pathTransformObj, string subName, string objName)
    {
        float subInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(subName).initRotation[1]);
        float objInitYRotation = DataUtil.Clone<float>(DataMap.matSettingList.getMatSettingMap(objName).initRotation[1]);
        if (pathTransformSub.endRotation[1].Equals(subInitYRotation))
        {
            //sub向右
            pathTransformObj.endRotation[1] = objInitYRotation;
        }
        else
        {
            pathTransformObj.endRotation[1] = objInitYRotation+120;
        }

        pathTransformObj.startRotation = DataUtil.Clone<List<float>>(pathTransformObj.endRotation);
    }

    public static void makeTransformStatic( PathTransform pathTransform)
    {
        pathTransform.startPosition = DataUtil.Clone<List<float>>(pathTransform.endPosition);
        pathTransform.startRotation = DataUtil.Clone<List<float>>(pathTransform.endRotation);
        pathTransform.startScale = DataUtil.Clone<List<float>>(pathTransform.endScale);
    }


}
