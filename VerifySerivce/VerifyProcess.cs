using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;

namespace VerifySerivce
{
    public class VerifyProcess
    {
        #region 定义部分

        /// <summary>
        /// 模拟表单对象
        /// </summary>
        private FormSimulate frmSimulate = null;

        /// <summary>
        /// 委托(验证完成时执行)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="result"></param>
        public delegate void CompletedHandle(VerifyProcess sender, VerifyResult result);
        /// <summary>
        /// 验证完成事件
        /// </summary>
        public event CompletedHandle OnCompleted;

        #endregion

        #region 构造函数和初始化

        public VerifyProcess()
        {
            frmSimulate = new FormSimulate();
        }

        #endregion

        #region 异步操作

        public void VerifyPersonAsync(string url, string fileName, byte[] fileContent, Dictionary<string, string> formItems = null,CompletedHandle callback=null)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                VerifyResult result = VerifyImage(url, fileName, fileContent, formItems);
                TriggerEvent(fileName, result, callback);
            });
        }

        public void VerifyPersonAsync(string url, string fileName, Image alarmImg, Dictionary<string, string> formItems = null, CompletedHandle callback = null)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                VerifyResult result = VerifyImage(url, fileName, alarmImg, formItems);
                TriggerEvent(fileName, result, callback);
            });
        }

        private void TriggerEvent(string fileName, VerifyResult result, CompletedHandle callback = null)
        {
            if (callback != null)
            {
                try
                {
                    callback(this, result);
                }
                catch (Exception exp)
                {
                    Logger.AddLog(this.GetType(), "TriggerEvent", exp.Message);
                }
            }
            else if (OnCompleted != null)
            {
                try
                {
                    OnCompleted(this, result);
                }
                catch (Exception exp)
                {
                    Logger.AddLog(this.GetType(), "TriggerEvent", exp.Message);
                }
            }
        }

        #endregion

        #region 同步操作
        public VerifyResult VerifyImage(string url, string imgFileName, byte[] imgFileContent, Dictionary<string, string> formItems = null)
        {
            VerifyResult result = new VerifyResult();
            result.Key = imgFileName;
            try
            {
                if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(imgFileName) || imgFileContent == null || imgFileContent.Length == 0)
                {
                    result.ErrorCode = 1;
                    return result;
                }

                var formDatas = new List<FormItem>();
                //添加图片文件
                formDatas.Add(new FormItem()
                {
                    Key = "image",
                    Value = "",
                    FileName = imgFileName,
                    FileContent = imgFileContent
                });

                //添加文本  
                if (formItems != null && formItems.Count > 0)
                {
                    foreach (var key in formItems.Keys)
                    {
                        formDatas.Add(new FormItem()
                        {
                            Key = key,
                            Value = formItems[key]
                        });
                    }
                }

                //提交表单  
                var respString = frmSimulate.PostForm(url, formDatas);

                if (string.IsNullOrWhiteSpace(respString))
                {
                    result.ErrorCode = 2;
                    return result;
                }
                result.AnnexData = respString;
                ImageFlag imgFlags = JsonHelper.JsonDeserialize<ImageFlag>(respString);
                result.Flags = imgFlags;
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "VerifyPerson", exp.Message);
                result.ErrorCode = 4;
            }
            finally
            {
                imgFileContent = null;
            }
            return result;
        }

        public VerifyResult VerifyImage(string url, string imgFileName, Image alarmImage, Dictionary<string, string> formItems = null)
        {
            byte[] fileContent = null;
            Bitmap bitmap = null;
            try
            {
                System.Drawing.Imaging.ImageFormat format = alarmImage.RawFormat;
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap = new Bitmap(alarmImage);
                    bitmap.Save(ms, format);
                    fileContent = ms.ToArray();
                }
                return VerifyImage(url, imgFileName, fileContent, formItems);
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "VerifyPerson[ImageConvert]", exp.Message);
                return new VerifyResult() { Key = imgFileName, ErrorCode = 1 };
            }
            finally
            {
                if (alarmImage != null)
                {
                    alarmImage.Dispose();
                    alarmImage = null;
                }
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
        }

        #endregion

    }
}
