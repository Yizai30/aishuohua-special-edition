 using System;
 using System.Collections.Generic;
using FluentFTP;
  
 namespace MyFtp
 {
    class FtpManager
    {
        private static string FTPCONSTR = "1.117.155.93";//shiyanshi
        //private static string FTPCONSTR = "47.100.88.221";//my
        private static string FTPUSERNAME = "ftpuser1";//我的FTP服务器的用户名
        private static string FTPPASSWORD = "123456";//我的FTP服务器的密码
        public static float uploadProgress;//上传进度
        public static float downloadProgress;//下载进度


        //@"C:\website\videos\", @"/public_html/videos"

        public FtpManager() { }
        public FtpManager(string url, string username, string password)
        {
            FTPCONSTR = url;
            FTPUSERNAME = username;
            FTPPASSWORD = password;
        }

        /// <summary>
        /// upload a file
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="ftpPath"></param>
        /// <returns></returns>
        public static FtpStatus UploadFile(string localFilePath, string ftpFilePath)
        {
            using (var ftp = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                
                ftp.AutoConnect();

                // upload a file to an existing FTP directory
                //ftp.UploadFile(@"D:\Github\FluentFTP\README.md", "/public_html/temp/README.md");

                // upload a file and ensure the FTP directory is created on the server
                
                return ftp.UploadFile(localFilePath, ftpFilePath, FtpRemoteExists.Overwrite, true);

                // upload a file and ensure the FTP directory is created on the server, verify the file after upload
                //ftp.UploadFile(@"D:\Github\FluentFTP\README.md", "/public_html/temp/README.md", FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
            }

        }

        /// <summary>
        /// upload files
        /// </summary>
        /// <param name="filenames"></param>
        /// <param name="ftpPath"></param>
        /// <returns>the number of upload successfully files</returns>
        public static int UploadFiles(List<string> filenames, string ftpDirPath)
        {
            using (var ftp = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                ftp.AutoConnect();

                // upload many files, skip if they already exist on server
                return ftp.UploadFiles(filenames, ftpDirPath, FtpRemoteExists.Skip);

            }

        }

        /// <summary>
        /// upload a directory
        /// </summary>
        public static void UploadDirectory(string localDirPath,string ftpDirPath)
        {
            using (var ftp = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                ftp.AutoConnect();


                // upload a folder and all its files
                List<FtpResult> results= ftp.UploadDirectory(localDirPath, ftpDirPath, FtpFolderSyncMode.Update);

                // upload a folder and all its files, and delete extra files on the server
                //ftp.UploadDirectory(@"C:\website\assets\", @"/public_html/assets", FtpFolderSyncMode.Mirror);

            }
        }


        public static FtpStatus DownloadFile(string localFilePath,string ftpFilePath)
        {
            using (var ftp = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                ftp.AutoConnect();

                // download a file and ensure the local directory is created
                return ftp.DownloadFile(localFilePath, ftpFilePath);

                // download a file and ensure the local directory is created, verify the file after download
                //ftp.DownloadFile(@"D:\Github\FluentFTP\README.md", "/public_html/temp/README.md", FtpLocalExists.Overwrite, FtpVerify.Retry);

            }
        }

        public static void DownloadFiles(string localDirPath,List<string> ftpFilesPath)
        {
            using (var ftp = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                ftp.AutoConnect();

                // download many files, skip if they already exist on disk
                ftp.DownloadFiles(localDirPath,ftpFilesPath, FtpLocalExists.Skip);

            }
        }

        public static void DownloadDirectory(string localDirPath,string ftpDirPath)
        {
            using (var ftp = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                ftp.AutoConnect();


                // download a folder and all its files
                ftp.DownloadDirectory(localDirPath, ftpDirPath, FtpFolderSyncMode.Update);

                // download a folder and all its files, and delete extra files on disk
                //ftp.DownloadDirectory(@"C:\website\dailybackup\", @"/public_html/", FtpFolderSyncMode.Mirror);

            }
        }

        public static bool DirectoryExists(string ftpDirPath)
        {
            using (var conn = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                conn.AutoConnect();

                if (conn.DirectoryExists(ftpDirPath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool FileExists(string ftpFilePath)
        {
            using (var conn = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                conn.AutoConnect();

                if (conn.FileExists(ftpFilePath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void DeleteFile(string ftpFilePath)
        {
            using (var conn = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                conn.AutoConnect();

                conn.DeleteFile(ftpFilePath);
            }
        }


        public static void DeleteDirectory(string ftpDirPath)
        {
            using (var conn = new FtpClient(FTPCONSTR, FTPUSERNAME, FTPPASSWORD))
            {
                conn.AutoConnect();

                // Remove the directory and all files and subdirectories inside it
                conn.DeleteDirectory(ftpDirPath);
            }
        }




    }
}
