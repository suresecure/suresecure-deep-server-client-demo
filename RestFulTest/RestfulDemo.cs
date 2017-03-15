using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using VerifySerivce;
using System.Threading.Tasks;
using System.Reflection;

namespace RestFullTest
{
    public partial class RestfulDemo : Form
    {
        public RestfulDemo()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "JPG|*.jpg|BMP|*.bmp|JPEG|*.jpeg";
                DialogResult result = dialog.ShowDialog();
                dialog.RestoreDirectory = false;
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtPicPath.Text = dialog.FileName;

                    Image imgtmp = pic1.Image;
                    pic1.Image = null;
                    if (imgtmp != null)
                        imgtmp.Dispose();

                    Image imgtmp2 = pic2.Image;
                    pic2.Image = null;
                    if (imgtmp2 != null)
                        imgtmp2.Dispose();

                    pic1.Image = Image.FromFile(dialog.FileName);
                }
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "btnSelect_Click","", exp);
            }
        }

        /// <summary>
        /// 在图片上绘制标记
        /// </summary>
        /// <param name="flag"></param>
        private void DrawFlag(ImageFlag flag)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                Image img = null;
                if (pic1.Image != null)
                {
                    img = pic1.Image.Clone() as Image;
                }
                if (img != null)
                {
                    Pen pen = new Pen(Color.Blue, 2.0f);
                    Graphics g = Graphics.FromImage(img);

                    for (int i = 0; i < flag.targets.Count; i++)
                    {
                        Flag flagInfo = flag.targets[i];
                        //绘制检测类型
                        g.DrawString(flagInfo.label + "[" + flagInfo.conf + "]", new Font("宋体", 14f, FontStyle.Bold), Brushes.Red, flagInfo.x, flagInfo.y - 20);
                        //绘制目标框
                        g.DrawRectangle(pen, flagInfo.x, flagInfo.y, flagInfo.w, flagInfo.h);
                    }
                    pic2.Image = img;
                }
            }));
        }

        VerifyProcess verProcess = new VerifyProcess();
        private void PostImage()
        {
            try
            {
                string filePath = "";
                string url = "";
                this.Invoke(new MethodInvoker(delegate ()
                {
                    filePath = txtPicPath.Text;
                    url = txtUrl.Text;
                    btnExecute.Enabled = false;
                }));

                if (!File.Exists(filePath))
                {
                    return;
                }
                var fileName = filePath.Substring(filePath.LastIndexOf("\\")+1);
                byte[] imgData = File.ReadAllBytes(filePath);
                VerifyResult result = verProcess.VerifyImage(txtUrl.Text, fileName,imgData);
                ImageFlag flag = null;
                if (result != null)
                {
                    flag = result.Flags;
                }

                if (flag != null && flag.targets != null)
                {
                    DrawFlag(flag);
                }
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "PostImage","", exp);
            }
            finally
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    btnExecute.Enabled = true;
                }));
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                bw.RunWorkerCompleted += delegate (object bwsender, RunWorkerCompletedEventArgs bwe) { };
                bw.DoWork += delegate (object bwsender, DoWorkEventArgs bwe) { PostImage(); };
                bw.RunWorkerAsync();
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            int w = (this.Width - 48) / 2;
            int h = (this.Height - 255);
            pic1.Width = w;
            pic1.Height = h;

            label4.Left = w + 20;
            pic2.Left = w + 20;
            pic2.Width = w;
            pic2.Height = h;
        }

        #region 压力测试

        byte[] imgData2 = null;
        string urlTmp = "";
        private void btnExecute2_Click(object sender, EventArgs e)
        {
            string filePath = "";
            filePath = txtPicPath.Text;
            urlTmp = txtUrl.Text;

            if (!File.Exists(filePath))
                return;

            var fileName = filePath.Substring(filePath.LastIndexOf("\\")+1);
            imgData2 = File.ReadAllBytes(filePath);
            button1.Enabled = false;
            StartThread();
        }
        int sentCnt = 0;
        int successCnt = 0;
        int errorCnt = 0;
        int timeoutCnt = 0;
        int otherErrCnt = 0;
        int total = 0;
        int unKnow = 0;
        DateTime bgTime = DateTime.Now;
        DateTime edTime = DateTime.Now;
        int completeCnt = 0;

        private VerifyResult PostImage2(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                guid = Guid.NewGuid().ToString();
            try
            {
                VerifyResult result = verProcess.VerifyImage(urlTmp, guid, imgData2);
                ImageFlag flag = null;
                if (result != null)
                {
                    flag = result.Flags;
                }
                if (flag != null && flag.targets != null)
                {
                    DrawFlag(flag);
                }
                return result;
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "PostImage2","", exp);
                return null;
            }
            finally
            {
            }                
        }
        #endregion

        DateTime dt1 = DateTime.Now;
        int Interval = 100;
        List<SendRecord> times = new List<SendRecord>();
        private void StartThread()
        {
            lbHdTimes.Text = "";
            label18.Text = "";
            lbSentCnt.Text = "0";
            lbErrorCnt.Text = "0";
            lbSuccessCnt.Text = "0";
            lbTimeout.Text = "0";
            lbOtherErr.Text = "0";
            lbundnow.Text = "0";

            sentCnt = 0;
            successCnt = 0;
            errorCnt = 0;
            timeoutCnt = 0;
            otherErrCnt = 0;
            completeCnt = 0;
            unKnow = 0;

            int time = Convert.ToInt32(nudSendCount.Value);
            int rate = Convert.ToInt32(nudRate.Value);
            Interval = 1000 / rate;
            total = (int)nudSendCount.Value;// time * rate;

            btnExecute2.Enabled = false;
            times.Clear();
            Thread th = new Thread(delegate() {
                dt1 = DateTime.Now;
                for (int i = 0; i <  total; i++)
                {
                    Interlocked.Increment(ref sentCnt);
                    if ((Interval < 100 && sentCnt % 10 == 0) || Interval >= 100 || sentCnt == total)
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            lbSentCnt.Text = sentCnt.ToString();
                        }));
                    }
                    Thread thsub = new Thread(new ThreadStart(StartSubThd));
                    thsub.Start();
                    Thread.Sleep(Interval);
                }
                DateTime dt2 = DateTime.Now;
                this.Invoke(new MethodInvoker(delegate ()
                {
                    label18.Text ="发送完成，耗时："+ (dt2 - dt1).TotalSeconds.ToString()+" 秒";
                    btnExecute2.Enabled = true;
                }));
            });
            th.Start();
        }

        
        public void StartSubThd()
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                SendRecord record = new SendRecord() { SendTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), GUID = guid };

                VerifyResult result = PostImage2(guid);

                record.CompleteTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                record.RespString = result != null ? result.AnnexData : "";
                times.Add(record);
                Interlocked.Increment(ref completeCnt);
                if (result == null)
                {
                    Interlocked.Increment(ref otherErrCnt);
                }
                else
                {
                    string respString = result.AnnexData.ToLower();
                    if (respString.Contains("targets"))
                    {
                        Interlocked.Increment(ref successCnt);
                    }
                    else if (respString.Contains("time is out"))
                    {
                        Interlocked.Increment(ref timeoutCnt);
                    }
                    else if (respString.Contains("image is invalid"))
                    {
                        Interlocked.Increment(ref errorCnt);
                    }
                    else if (respString.Contains("error"))
                    {
                        Interlocked.Increment(ref otherErrCnt);
                    }
                    else
                    {
                        Interlocked.Increment(ref unKnow);
                        Logger.AddLog(this.GetType(), "StartSubThd", respString);
                    }
                }

                if ((Interval < 100 && completeCnt % 10 == 0) || Interval >= 100 || completeCnt == total)
                {
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        lbSuccessCnt.Text = successCnt.ToString();
                        lbTimeout.Text = timeoutCnt.ToString();
                        lbErrorCnt.Text = errorCnt.ToString();
                        lbOtherErr.Text = otherErrCnt.ToString();
                        lbundnow.Text = unKnow.ToString();
                        if (completeCnt == total)
                        {
                            lbHdTimes.Text = "处理完成，共耗时：" + (DateTime.Now - dt1).TotalSeconds.ToString() + " 秒";
                            button1.Enabled = true;
                        }
                    }));
                }
            }
            catch (Exception exp)
            {
                Logger.AddLog(this.GetType(), "StartSubThd", "", exp);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void nudTime_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestResult f2 = new TestResult(times);
            f2.ShowDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }

    public class SendRecord
    {
        public string GUID { get; set; }
        public string SendTime { get;set; }

        public string CompleteTime { get; set; }

        public string RespString { get;set;}
    }
}
