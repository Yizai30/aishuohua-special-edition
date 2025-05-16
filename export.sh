rm -fr Export # Ensure a fresh build
Unity -nographics -batchmode -projectPath . -executeMethod BuildScript.PerformExport -quit # -logFile "build.log"
python fix-export.py
rm -f Export.zip
zip -r -9 Export.zip Export
ls -l Export.zip
python upload.py Export.zip
