using System.Collections;
using UnityEngine;

namespace Assets.Scripts.TellStory.Util
{
    public class ResourceUtils
    {
        public static string persistentDataPath = Application.persistentDataPath;

        public static void CopyToPersistentDataPath(string resourceFileName)
        {
            WWW reader = new WWW(getReaderPath(resourceFileName));
            while (!reader.isDone) { }
            var realPath = Application.persistentDataPath + "/" + resourceFileName;
            System.IO.Directory.CreateDirectory(realPath.Substring(0, realPath.LastIndexOf("/")));
            System.IO.File.WriteAllBytes(realPath, reader.bytes);
            //Debug.Log("Write " + reader.bytes.Length + " bytes to " + realPath);
        }

        public static void CopyToPersistentDataPath(string resourceFileName, byte[] bytes)
        {
            var realPath = persistentDataPath + "/" + resourceFileName;
            System.IO.Directory.CreateDirectory(realPath.Substring(0, realPath.LastIndexOf("/")));
            System.IO.File.WriteAllBytes(realPath, bytes);
            //Debug.Log("Write " + bytes.Length + " bytes to " + realPath);
        }

        public static string getReaderPath(string resourceFileName)
        {
            string oriPath = Application.streamingAssetsPath + "/" + resourceFileName;
            if (!oriPath.StartsWith("jar:"))
            {
                oriPath = "file://" + oriPath;
            }
            //Debug.Log("Use reader path for: " + resourceFileName + " => " + oriPath);
            return oriPath;
        }
    }
}