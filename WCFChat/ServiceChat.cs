using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WCFChat.Entities;

namespace WCFChat
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChat : IServiceChat
    {
        List<User> Users = new List<User>();
        int NextId = 1;

		/// <summary>
		/// метод для добавления пользователя в список подключенных к серверу
		/// </summary>
		/// <param name="nickname"></param>
		/// <returns></returns>
		public int Connect(string nickname)
        {
            User user = new User()
            {
                Id = NextId,
                Nickname = nickname,
                OperationContext = OperationContext.Current
            };
            NextId++;

            Users.Add(user);
            return user.Id;
        }

		/// <summary>
		/// метод для исключения пользователя из списка подключенных к серверу
		/// </summary>
		/// <param name="id"></param>
		public void Disconnect(int id)
        {
            User user = Users.FirstOrDefault(i => i.Id == id);

            if (user != null)
                Users.Remove(user);
        }

		/// <summary>
		/// метод для отправки сообщения пользователям, подключенным к серверу
		/// </summary>
		/// <param name="message"></param>
		public void SendMessage(Message message)
        {
            foreach (User item in Users)
            {
                item.OperationContext.
					GetCallbackChannel<IServiceChatCallback>().
					MessageCallback(message);
            }
        }
    }
}
