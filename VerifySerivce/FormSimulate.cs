﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace VerifySerivce
{
    internal class FormItem
    {
        /// <summary>  
        /// 表单键，request["key"]  
        /// </summary>  
        public string Key { set; get; }
        /// <summary>  
        /// 表单值,上传文件时忽略，request["key"].value  
        /// </summary>  
        public string Value { set; get; }
        /// <summary>  
        /// 是否是文件  
        /// </summary>  
        public bool IsFile
        {
            get
            {
                if (FileContent == null || FileContent.Length == 0)
                    return false;

                if (FileContent != null && FileContent.Length > 0 && (FileName == null || FileName.Trim() == ""))
                    throw new Exception("上传文件时 FileName 属性值不能为空");
                return true;
            }
        }
        /// <summary>  
        /// 上传的文件名  
        /// </summary>  
        public string FileName { set; get; }
        /// <summary>  
        /// 上传的文件内容  
        /// </summary>  
        public byte[] FileContent { set; get; }
    }


    /// <summary>
    /// 模拟表单提交
    /// </summary>
    internal class FormSimulate
    {
        /// <summary>  
        /// 使用Post方法获取字符串结果  
        /// </summary>  
        /// <param name="url"></param>  
        /// <param name="formItems">Post表单内容</param>  
        /// <param name="cookieContainer"></param>  
        /// <param name="timeOut">默认20秒</param>  
        /// <param name="encoding">响应内容的编码类型（默认utf-8）</param>  
        /// <returns></returns>  
        public string PostForm(string url, List<FormItem> formItems, CookieContainer cookieContainer = null, string refererUrl = null, Encoding encoding = null, int timeOut = 20000)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                #region 初始化请求对象

                request.Proxy = null;
                request.Method = "POST";
                request.Timeout = timeOut;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.KeepAlive = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
                if (!string.IsNullOrEmpty(refererUrl))
                    request.Referer = refererUrl;
                if (cookieContainer != null)
                    request.CookieContainer = cookieContainer;

                #endregion

                string boundary = "----" + DateTime.Now.Ticks.ToString("x");//分隔符  
                request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                //请求流  
                var postStream = new MemoryStream();
                #region 处理Form表单请求内容
                //是否用Form上传文件  
                var formUploadFile = formItems != null && formItems.Count > 0;
                if (formUploadFile)
                {
                    //文件数据模板  
                    string fileFormdataTemplate =
                        "\r\n--" + boundary +
                        "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                        "\r\nContent-Type: application/octet-stream" +
                        "\r\n\r\n";
                    //文本数据模板  
                    string dataFormdataTemplate =
                        "\r\n--" + boundary +
                        "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                        "\r\n\r\n{1}";
                    foreach (var item in formItems)
                    {
                        string formdata = null;
                        if (item.IsFile)
                        {
                            //上传文件  
                            formdata = string.Format(
                                fileFormdataTemplate,
                                item.Key, //表单键  
                                item.FileName);
                        }
                        else
                        {
                            //上传文本  
                            formdata = string.Format(
                                dataFormdataTemplate,
                                item.Key,
                                item.Value);
                        }

                        //统一处理  
                        byte[] formdataBytes = null;
                        //第一行不需要换行  
                        if (postStream.Length == 0)
                            formdataBytes = Encoding.UTF8.GetBytes(formdata.Substring(2, formdata.Length - 2));
                        else
                            formdataBytes = Encoding.UTF8.GetBytes(formdata);
                        postStream.Write(formdataBytes, 0, formdataBytes.Length);

                        //写入文件内容  
                        if (item.FileContent != null && item.FileContent.Length > 0)
                        {
                            postStream.Write(item.FileContent, 0, item.FileContent.Length);
                        }
                    }
                    //结尾  
                    var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                    postStream.Write(footer, 0, footer.Length);

                }
                else
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                }
                #endregion

                request.ContentLength = postStream.Length;

                #region 输入二进制流
                if (postStream != null)
                {
                    postStream.Position = 0;
                    //直接写入流  
                    Stream requestStream = request.GetRequestStream();

                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                    postStream.Close();//关闭文件访问  
                }
                #endregion

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (cookieContainer != null)
                {
                    response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
                }

                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.UTF8))
                    {
                        string retString = myStreamReader.ReadToEnd();
                        return retString;
                    }
                }
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "PostForm", exp.Message);
                return "";
            }
        }
    }
}
