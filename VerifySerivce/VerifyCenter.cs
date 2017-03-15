using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using AssistFK;

namespace VerifySerivce
{
    public class VerifyCenter
    {
        #region 属性和事件的定义

        /// <summary>
        /// 最大线程数
        /// </summary>
        public int MaxThreads { get; private set; }

        /// <summary>
        /// 控制最大并发数
        /// </summary>
        private Semaphore maxAccepted = null;

        /// <summary>
        /// 处理线程队列
        /// </summary>
        private PoolQueue<VerifyProcess> evtHandlers = null;

        /// <summary>
        /// 暂存未返回验证结果的回掉函数
        /// </summary>
        private Dictionary<string, Action<VerifyResult>> tmpRequest = null;

        /// <summary>
        /// 验证完成事件
        /// </summary>
        public event Action<VerifyResult> OnVerifyCompleted = null;

        #endregion

        #region 单例模式

        private static readonly object SynObject = new object();

        private static VerifyCenter instance = null;

        public static VerifyCenter Instance
        {
            get
            {
                return CreateInstance();
            }
        }

        public static VerifyCenter CreateInstance(int maxTask = 32)
        {
            // Double-Checked Locking
            if (instance == null)
            {
                lock (SynObject)
                {
                    if (instance == null)
                    {
                        instance = new VerifyCenter(maxTask);
                    }
                }
            }
            return instance;
        }

        #endregion

        #region 构造函数及初始化
        private VerifyCenter(int maxTask)
        {
            try
            {
                MaxThreads = maxTask;

                this.maxAccepted = new Semaphore(MaxThreads, MaxThreads);

                tmpRequest = new Dictionary<string, Action<VerifyResult>>();

                evtHandlers = new PoolQueue<VerifyProcess>(MaxThreads + 2);
                for (int i = 0; i < MaxThreads; i++)
                {
                    VerifyProcess verTask = new VerifyProcess();
                    verTask.OnCompleted += verTask_OnCompleted;
                    evtHandlers.Enqueue(verTask);
                }
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "VerifyCenter", exp.Message);
            }
        }

        //操作完成后
        private void verTask_OnCompleted(VerifyProcess sender, VerifyResult arg)
        {
            evtHandlers.Enqueue(sender);
            maxAccepted.Release();

            if (tmpRequest.ContainsKey(arg.Key))
            {
                try
                {
                    Action<VerifyResult> callback = tmpRequest[arg.Key];
                    if (callback != null)
                    {
                        callback(arg);
                    }
                    else
                    {
                        if (OnVerifyCompleted != null)
                        {
                            OnVerifyCompleted(arg);
                        }
                    }
                }
                catch (Exception exp)
                {
                    Logger.AddLog(this.GetType(), "VerifyCenter.Part1", exp.Message);
                }
                finally
                {
                    tmpRequest.Remove(arg.Key);
                }
            }
            else
            {
                try
                {
                    if (OnVerifyCompleted != null)
                    {
                        OnVerifyCompleted(arg);
                    }
                }
                catch (Exception exp)
                {
                    Logger.AddLog(this.GetType(), "VerifyCenter.Part2", exp.Message);
                }
            }
        }

        #endregion

        #region 验证过程

        public VerifyResult ExecuteVerify(string url, string fileName, Image alarmImg)
        {
            maxAccepted.WaitOne(); //获取信号量
            VerifyProcess tmpHandler = evtHandlers.Dequeue();
            VerifyResult result = null;
            if (tmpHandler != null)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = Guid.NewGuid().ToString();
                }
                result = tmpHandler.VerifyImage(url, fileName, alarmImg);
            }
            evtHandlers.Enqueue(tmpHandler);
            maxAccepted.Release();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">验证服务的URL</param>
        /// <param name="fileName">验证图片的文件名</param>
        /// <param name="alarmImg">验证图片</param>
        /// <param name="callback">回调函数(回调函数为null，则会出发验证完成事件)</param>
        public void ExecuteVerifyAsync(string url, string fileName, Image alarmImg,  Action<VerifyResult> callback = null)
        {
            maxAccepted.WaitOne(); //获取信号量
            VerifyProcess tmpHandler = evtHandlers.Dequeue();
            if (tmpHandler != null)
            {
                if (!tmpRequest.ContainsKey(fileName) && callback != null)
                {
                    tmpRequest.Add(fileName, callback);
                }
                tmpHandler.VerifyPersonAsync(url, fileName, alarmImg);
            }
        }

        #endregion
    }
}
