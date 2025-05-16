using System;
using Newtonsoft.Json;
using System.IO;

namespace TellStory.Util
{
    public class JsonUtils
    {
        // 使用 Newtonsoft.Json 来序列化对象并写入 JSON 文件
        public static void WriteToJsonFile<T>(T objectToSerialize, string filePath)
        {
            try
            {
                // 设置序列化选项，例如格式化输出
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented // 可选，为了更好的可读性
                };

                // 序列化对象到 JSON 字符串
                string jsonString = JsonConvert.SerializeObject(objectToSerialize, settings);

                // 写入文件
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志等
                Console.WriteLine("An error occurred while writing the JSON file: " + ex.Message);
            }
        }
        
        // 清空文件内容
        public static void ClearFileContent(string filePath)
        {
            // 确保文件存在
            if (File.Exists(filePath))
            {
                // 使用 File.WriteAllText 方法将文件内容清空
                File.WriteAllText(filePath, string.Empty);
            }
            else
            {
                // 文件不存在，可以选择创建一个空文件或者抛出异常
                // File.Create(testResultPath); // 如果你想创建一个空文件
                throw new FileNotFoundException("The file was not found: " + filePath);
            }
        }
    }
}