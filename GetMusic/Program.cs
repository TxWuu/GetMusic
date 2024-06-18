using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;

namespace UriParserApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mylist = File.ReadAllLines(@"D:\Code\GetMusic\mylist.txt");
            var lineIndex = 0;
            var lineCount = mylist.Length;

            foreach (var line in mylist)
            {
                if(!line.Contains('&'))
                {
                    break;
                }

                var aLine = line.Split('&');
                var name = aLine[0];
                var uriStr = aLine[1];

                var queryParams = ParseUri(uriStr);

                Console.WriteLine("{2}/{3} id:{0}-{1}",queryParams["id"],name,lineIndex++,lineCount);

                var listId = queryParams["id"];

                var baseUri = "" + listId;
                #if true
                    #error "未添加API"
                #endif

                var jsonArray = await FetchJsonArrayAsync<Song>(baseUri);
                var songIndex = 0;
                var songCount = jsonArray.Count;

                foreach (var item in jsonArray)
                {
                    var fileName = item.Name + "-" + item.Artist + ".mp3";
                    await DownloadFileAsync(item.Url,name,fileName);
                    Console.WriteLine("{2}/{3} {0:20},{1}",item.Name,item.Url,songIndex++,songCount);
                    
                }

            }

        }

        static void delay()
        {
            var rd = new Random();
            int time = rd.Next(30, 120);
            Thread.Sleep(time);
        }

        
        static Dictionary<string, string> ParseUri(string uri)
        {
            var queryParams = new Dictionary<string, string>();

            // 解析URI
            Uri myUri = new Uri(uri);

            // 获取片段部分（#后的部分）
            string fragment = myUri.Fragment;

            // 检查片段是否以#/开头
            if (fragment.StartsWith("#/"))
            {
                // 去掉开头的#/
                fragment = fragment.Substring(2);
            }

            // 分离路径和查询部分
            string[] parts = fragment.Split(new[] { '?' }, 2);
            if (parts.Length > 1)
            {
                string query = parts[1];

                // 解析查询参数
                var queryCollection = HttpUtility.ParseQueryString(query);

                // 填充字典
                foreach (string key in queryCollection.AllKeys)
                {
                    queryParams[key] = queryCollection[key];
                }
            }

            return queryParams;
        }

        static async Task<List<T>> FetchJsonArrayAsync<T>(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 发起GET请求
                    HttpResponseMessage response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // 解析JSON数组
                    var jsonArray = JsonConvert.DeserializeObject<List<T>>(responseBody);

                    return jsonArray;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"请求异常: {e.Message}");
                    return new List<T>();
                }
            }
        }

        static async Task DownloadFileAsync(string fileUrl, string relativePath, string fileName)
        {
            //将文件名中的/改为&以避免Windows文件系统错误
            fileName = fileName.Replace("/", "&");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // 确保相对路径存在
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }

                    // 检查文件是否已经存在
                    string fullFileName = Path.Combine(fullPath, fileName);
                    if (File.Exists(fullFileName))
                    {
                        Console.WriteLine("File already exists, skipping download.");
                        return;
                    }

                    // 发起GET请求
                    HttpResponseMessage response = await client.GetAsync(fileUrl);
                    response.EnsureSuccessStatusCode();

                    // 读取文件内容
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                    // 保存文件
                    await File.WriteAllBytesAsync(fullFileName, fileBytes);
                    delay();
                    Console.WriteLine("File downloaded successfully.");
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
                catch (IOException e)
                {
                    Console.WriteLine($"File error: {e.Message}");
                }
            }
        }
    }
}
