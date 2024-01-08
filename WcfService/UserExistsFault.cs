using System.Runtime.Serialization;

namespace WcfService
{
    public class UserExistsFault
    {
        [DataMember]
        public string message { get; set; }
    }
}