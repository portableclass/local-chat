using System;
using System.Windows;

namespace WCFChat.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Msg { get; set; }
        public int UserSenderId { get; set; }
        public int ChatRecieverId { get; set; }
        public DateTime DateTime { get; set; }
        public HorizontalAlignment Alignment { get; set; }
        public string UserNickname { get; set; }
    }
}
