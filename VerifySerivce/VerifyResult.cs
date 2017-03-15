
namespace VerifySerivce
{
    /// <summary>
    /// 服务端验证的结果
    /// </summary>
    public class VerifyResult
    {
        public VerifyResult()
        {
            Key = "";
            ErrorCode = 0;
            AnnexData = "";
            Flags = new ImageFlag();
        }

        /// <summary>
        /// 健，每次验证时传入的值(通常使用唯一的文件名)
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 错误代码
        /// 0：没有错误
        /// 1：输入参数错误
        /// 2：验证服务返回错误
        /// 3：返回值格式错误
        /// 4：其它未知错误
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        public string AnnexData { get; set; }

        public ImageFlag Flags { get; set; }
    }
}
