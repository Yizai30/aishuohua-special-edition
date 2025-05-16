rm -f TellStory.apk # Ensure a fresh build
Unity -nographics -batchmode -projectPath . -executeMethod BuildScript.PerformBuild -quit # -logFile "build.log"
ls -l TellStory.apk
python upload.py TellStory.apk
