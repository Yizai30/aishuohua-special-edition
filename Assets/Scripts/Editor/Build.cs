using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    static void PerformBuild()
    {
        string[] defaultScene = { 
            "Assets/Scenes/Screens/SceneMainMenu.unity",
            "Assets/Scenes/Screens/SceneMyStories.unity",
            "Assets/Scenes/Screens/SceneReport.unity",
            "Assets/Scenes/Screens/SceneStoryDetails.unity",
            "Assets/Scenes/Screens/SceneMainMenu.unity",
            "Assets/Scenes/Screens/SceneTellStory.unity",
            };
EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        BuildPipeline.BuildPlayer(defaultScene, "TellStory.apk" ,
            BuildTarget.Android, BuildOptions.None);
    }

    static void PerformExport()
    {
        string[] defaultScene = { 
            "Assets/Scenes/Screens/SceneMainMenu.unity",
            "Assets/Scenes/Screens/SceneMyStories.unity",
            "Assets/Scenes/Screens/SceneReport.unity",
            "Assets/Scenes/Screens/SceneStoryDetails.unity",
            "Assets/Scenes/Screens/SceneMainMenu.unity",
            "Assets/Scenes/Screens/SceneTellStory.unity",
            };
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        BuildPipeline.BuildPlayer(defaultScene, "Export" ,
            BuildTarget.Android, BuildOptions.None);
    }

}

