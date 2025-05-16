using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ZipUtil
{
    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="zipFilePath">压缩文件的路径与名称</param>
    /// <param name="FilePath">被压缩的文件路径</param>
    /// <param name="ZipPWD">解压密码（null代表无密码）</param>
    /// <returns></returns>
    
    public static string FileToZip(string zipFilePath, string FilePath, string ZipPWD)
    {
        //FastZip fz
        string state = "Fail...";
        try
        {
            FastZip fz = new FastZip();
            FileInfo fi = new FileInfo(FilePath);
            string filename = fi.Name;
            string dirname = fi.DirectoryName;
            fz.Password = ZipPWD;
            fz.CreateZip(zipFilePath, dirname, false, filename);

            state = "Success !";
        }
        catch (Exception ex)
        {
            state += "," + ex.Message;
        }
        return state;
    }


    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationZipFilePath"></param>
    public static void DirToZip(string sourceFilePath, string destinationZipFilePath)
    {
        if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            sourceFilePath += System.IO.Path.DirectorySeparatorChar;
        ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
        zipStream.SetLevel(9);  // 压缩级别 0-9
        CreateZipFiles(sourceFilePath, zipStream);
        zipStream.Finish();
        zipStream.Close();
    }
    /// <summary>
    /// 递归压缩文件
    /// </summary>
    /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
    /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名</param>
    /// <param name="staticFile"></param>
    private static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream)
    {
        Crc32 crc = new Crc32();
        string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
        foreach (string file in filesArray)
        {
            if (Directory.Exists(file))                     //如果当前是文件夹，递归
            {
                CreateZipFiles(file, zipStream);
            }
            else                                            //如果是文件，开始压缩
            {
                FileStream fileStream = File.OpenRead(file);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                string tempFile = file.Substring(sourceFilePath.LastIndexOf("\\") + 1);
                ZipEntry entry = new ZipEntry(tempFile);
                entry.DateTime = DateTime.Now;
                entry.Size = fileStream.Length;
                fileStream.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                zipStream.PutNextEntry(entry);
                zipStream.Write(buffer, 0, buffer.Length);
            }
        }
    }


    /// <summary>
    /// 解压Zip
    /// </summary>
    /// <param name="DirPath">解压后存放路径</param>
    /// <param name="ZipPath">Zip的存放路径</param>
    /// <param name="ZipPWD">解压密码（null代表无密码）</param>
    /// <returns></returns>
    public static string Compress(string DirPath, string ZipPath, string ZipPWD)
    {
        string state = "Fail...";
        try
        {
            FastZip fz = new FastZip();
            fz.Password = ZipPWD;
            fz.ExtractZip(ZipPath, DirPath, null);

            state = "Success !";
        }
        catch (Exception ex)
        {
            state += "," + ex.Message;
        }
        return state;
    }






}
