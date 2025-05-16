 using System.Collections;
 using System.Collections.Generic;
 using System.Threading;
 using UnityEngine;
 using UnityEngine.UI;
 using MyFtp;
using System.Text;
using FluentFTP;
public class FtpTest : MonoBehaviour
{
  
    
     void Start()
    {


        // create an FTP client and specify the host, username and password
        // (delete the credentials to use the "anonymous" account)
        FtpClient client = new FtpClient("1.117.155.93", "ftpuser1", "123456");

        // connect to the server and automatically detect working FTP settings
        client.AutoConnect();

        // get a list of files and directories in the "/htdocs" folder
        foreach (FtpListItem item in client.GetListing("/"))
        {

            // if this is a file
            if (item.Type == FtpFileSystemObjectType.File)
            {

                // get the file size
                long size = client.GetFileSize(item.FullName);

                // calculate a hash for the file on the server side (default algorithm)
                //FtpHash hash = client.GetChecksum(item.FullName);
            }

            // get modified date/time of the file or folder
            //var time = client.GetModifiedTime(item.FullName);

        }

        // upload a file
        FtpStatus ftpStatus= client.UploadFile(@"D:\Test\test.txt", "/testDir/test.txt", FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
        if (ftpStatus == FtpStatus.Success)
        {
            Debug.Log("上传成功");
        }
        else
        {
            Debug.Log("上传失败");
        }

        // move the uploaded file
        //client.MoveFile("/testDir/test.txt", "/testDir/test/test.txt");

        // download the file again
        client.DownloadFile(@"D:\Test\test2.txt", "/testDir/test.txt");

        // compare the downloaded file with the server
        if (client.CompareFile(@"D:\Test\test2.txt", "/testDir/test.txt") == FtpCompareResult.Equal) { }

        // delete the file
        //client.DeleteFile("/testDir/test.txt");

        // upload a folder and all its files
        client.UploadDirectory(@"D:\Test2\", @"/testDir2/", FtpFolderSyncMode.Update);

        // upload a folder and all its files, and delete extra files on the server
        //client.UploadDirectory(@"C:\website\assets\", @"/public_html/assets", FtpFolderSyncMode.Mirror);

        // download a folder and all its files
        //client.DownloadDirectory(@"C:\website\logs\", @"/public_html/logs", FtpFolderSyncMode.Update);

        // download a folder and all its files, and delete extra files on disk
        //client.DownloadDirectory(@"C:\website\dailybackup\", @"/public_html/", FtpFolderSyncMode.Mirror);

        // delete a folder recursively
        //client.DeleteDirectory("/htdocs/extras/");

        // check if a file exists
        //if (client.FileExists("/htdocs/big2.txt")) { }

        // check if a folder exists
        if (client.DirectoryExists("/Resources/Characters/Animals/duck/")) 
        {
            Debug.Log("exit");
        }
        else
        {
            Debug.Log("not exit");
        }

        // upload a file and retry 3 times before giving up
        //client.RetryAttempts = 3;
        //client.UploadFile(@"C:\MyVideo.mp4", "/htdocs/big.txt", FtpRemoteExists.Overwrite, false, FtpVerify.Retry);

        // disconnect! good bye!
        client.Disconnect();
        Debug.Log("end");

    }

   



 }

