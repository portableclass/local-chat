using System.ServiceModel;
using WCFChat.Entities;

namespace WCFChat
{
    [ServiceContract(CallbackContract = typeof(IServiceChatCallback))]
    public interface IServiceChat
    {
        [OperationContract]
        int Connect(string nickname);
        //[OperationContract]
        //int ConnectToChat(List<User> users, int chatId);
        [OperationContract]
        void Disconnect(int id);
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
        //[OperationContract(IsOneWay = true)]
        //void SendMessageToChat(string message, int chatId, int userId);
    }

    public interface IServiceChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void MessageCallback(Message message);
        //[OperationContract(IsOneWay = true)]
        //void MessageCallbackToChat(string message);
    }
}
