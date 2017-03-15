using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace VerifySerivce
{
    [DataContract]
    [Serializable]
    public class Flag
    {

        //{\"targets\": [{\"w\": 136, \"h\": 203, \"label\": \"person\", \"conf\": 54, \"y\": 286, \"x\": 1614}]}

        [DataMember(Order = 0)]
        public int w { get; set; }

        [DataMember(Order = 1)]
        public int h { get; set; }

        [DataMember(Order = 2)]
        public string label { get; set; }
        [DataMember(Order = 3)]
        public int conf { get; set; }


        [DataMember(Order = 4)]
        public int y { get; set; }

        [DataMember(Order = 5)]
        public int x { get; set; }

    }

    [DataContract]
    [Serializable]
    public class ImageFlag
    {
        public ImageFlag()
        {
            targets = new List<Flag>();
        }
        [DataMember(Order = 0)]
        public List<Flag> targets { get; set; }
    }


    /// <summary>
    /// JSON序列化和反序列化辅助类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// JSON序列化
        /// </summary>
        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        /// <summary>
        /// JSON反序列化
        /// </summary>
        public static T JsonDeserialize<T>(string jsonString)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                T obj = (T)ser.ReadObject(ms);
                return obj;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
