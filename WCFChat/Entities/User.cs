using System.ServiceModel;

namespace WCFChat.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public OperationContext OperationContext { get; set; }
    }
}
