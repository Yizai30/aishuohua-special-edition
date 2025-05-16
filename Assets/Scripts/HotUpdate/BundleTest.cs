using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BundleTest : MonoBehaviour
{
    public string bundleName = "db_boy";
    public string assetName = "db_boy";
    // Start is called before the first frame update
    void Start()
    {
        InstantiateBundle();
    }

   protected void InstantiateBundle()
    {
        AssetBundle asset = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName));
        var asset_go = asset.LoadAsset<GameObject>(assetName);
        Instantiate(asset_go);
    }
}
