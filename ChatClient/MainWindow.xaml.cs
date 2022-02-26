using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ChatClient.ServiceChat;
using WCFChat.Entities;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IServiceChatCallback
    {
        private bool isConnected = false;
        private ServiceChatClient client;
        private DataBase dataBase = new DataBase();
        private User user = new User();
        private Chat chat = new Chat();
        private int userServerId;
        //List<Message> test = new List<Message>();
        //int chatId;
        //string chatName;
        //int ID;
        //string nickname;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectUser()
        {
            if (!isConnected)
            {
                client = new ServiceChatClient(new System.ServiceModel.InstanceContext(this));
                userServerId = client.Connect(user.Nickname);
                isConnected = true;
            }
        }

        private void DisconnectUser()
        {
            if (isConnected)
            {
                try
                {
                    client.Disconnect(userServerId);
                }
                catch
                {
                    System.Windows.MessageBox.Show("Internal server error.\nRestart the server and client application", "Internal erver error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                client = null;
                isConnected = false;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginBox.Text.Length < 15 && LoginBox.Text.Length > 0)
            {
                if (PasswordBox.Password.Length > 0)
                {
                    DataTable dt = Select("SELECT TOP(1) [users_id] FROM [dbo].[users] WHERE [login] = '" + LoginBox.Text + "' AND [password] = '" + PasswordBox.Password + "'");
                    if (dt.Rows.Count > 0)
                    {
                        user.Nickname = LoginBox.Text;
                        user.Id = Convert.ToInt32(dt.Rows[0][0].ToString());
                        try
                        {
                            ConnectUser();
                            LoadChats();
                        }
                        catch
                        {
                            System.Windows.MessageBox.Show("Server error. Try restarting the server.", "Server error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else LoginMessageBlock.Text = "Wrong login or password!";
                }
                else LoginMessageBlock.Text = "Enter password!";
            }
            else LoginMessageBlock.Text = "Enter a nickname consisting of 1 to 15 characters!";

            if (LoginMessageBlock.Text.Length != 0)
                LoginMessageBlock.Visibility = Visibility.Visible;
            else
                LoginMessageBlock.Visibility = Visibility.Hidden;
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthNicknameBox.Text.Length < 15 && AuthNicknameBox.Text.Length > 0)
            {
                if (AuthPasswordBox.Password.Length > 0)
                {
                    if (AuthRepeatPasswordBox.Password.Length > 0)
                    {
                        if (AuthPasswordBox.Password == AuthRepeatPasswordBox.Password)
                        {
                            DataTable dt = Select("SELECT * FROM [dbo].[users] WHERE [login] = '" + AuthNicknameBox.Text + "'");
                            if (dt.Rows.Count > 0)
                            {
                                AuthMessageBlock.Text = "A user with this nickname already exists.\nPlease sign up with another one.";
                                AuthMessageBlock.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                Insert("INSERT INTO [dbo].[users] ([login],[password]) VALUES ('" + AuthNicknameBox.Text + "', '" + AuthPasswordBox.Password + "')");
                                System.Windows.MessageBox.Show("The user is registered.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                Open(ContactsScreen);
                            }
                        }
                        else AuthMessageBlock.Text = "Passwords don't match!";
                    }
                    else AuthMessageBlock.Text = "Repeat the password!";
                }
                else AuthMessageBlock.Text = "Enter the password!";
            }
            else AuthMessageBlock.Text = "Enter a nickname consisting of 1 to 15 characters!";

            if (AuthMessageBlock.Text.Length != 0)
                AuthMessageBlock.Visibility = Visibility.Visible;
            else
                AuthMessageBlock.Visibility = Visibility.Hidden;
        }

        private void LoadChats()
        {
            ContactsList.Items.Clear();
            ChatsMessageBlock.Visibility = Visibility.Hidden;
            DataTable dt = Select("SELECT [chats_id] FROM [dbo].[chats_users] WHERE [users_id] = '" + user.Id + "'");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dt2 = Select("SELECT [chats_name] FROM [dbo].[chats] WHERE [chats_id] = '" + dt.Rows[i][0].ToString() + "'");
                    if (dt2.Rows.Count > 0)
                    {
                        for (int j = 0; j < dt2.Rows.Count; j++)
                        {
                            ContactsList.Items.Add(dt2.Rows[j][0].ToString());
                            ContactsList.ScrollIntoView(ContactsList.Items[ContactsList.Items.Count - 1]);
                        }
                    }
                }
            }
            else ChatsMessageBlock.Visibility = Visibility.Visible;

            Open(ContactsScreen);
        }

        private void ContactsList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (ContactsList.SelectedItem != null)
            {
                chat.Name = ContactsList.SelectedItem.ToString();
                DataTable dt = Select("SELECT TOP(1) [chats_id],[chats_admin_id] FROM [dbo].[chats] WHERE [chats_name] = '" + chat.Name + "'");
                chat.Id = Convert.ToInt32(dt.Rows[0][0].ToString());
                chat.AdminId = Convert.ToInt32(dt.Rows[0][1].ToString());
                ChatName.Text = chat.Name;
                LoadMessages();
                if (user.Id == chat.AdminId)
                    Moderate.Visibility = Visibility.Visible;
                else
                    Moderate.Visibility = Visibility.Hidden;
                Open(ChatScreen);
            }
        }

        private void AddMessage(string msg, DateTime dt, HorizontalAlignment ha, string nn)
        {
            MessagesList.Items.Add(new Message()
            {
                Msg = msg,
                DateTime = dt,
                Alignment = ha,
                UserNickname = nn
            });
            MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
        }

        public void MessageCallback(Message message)
        {
            if (chat.Id == message.ChatRecieverId)
            {
                //MessagesList.Items.Add(message.Msg);
                //MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                HorizontalAlignment ha;
                if (user.Id.Equals(message.UserSenderId))
                    ha = HorizontalAlignment.Right;
                else
                    ha = HorizontalAlignment.Left;

                AddMessage(message.Msg, message.DateTime, ha, message.UserNickname);
            }
        }


        private void LoadMessages()
        {
            MessagesList.Items.Clear();
            DataTable dt = Select("SELECT [messages_text],[users_id_sender],[messages_datetime] FROM [dbo].[messages] WHERE [chats_id_reciever] = " + chat.Id + " ORDER BY [messages_datetime] ASC");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //MessagesList.Items.Add(dt.Rows[i][0].ToString());
                    //MessagesList.ScrollIntoView(MessagesList.Items[MessagesList.Items.Count - 1]);
                    DataTable dt2 = Select("SELECT TOP(1) [login] FROM [dbo].[users] WHERE [users_id] = " + dt.Rows[i][1].ToString() + "");
                    if (dt2.Rows.Count > 0)
                    {
                        HorizontalAlignment ha;
                        if (user.Id.Equals(Convert.ToInt32(dt.Rows[i][1].ToString())))
                            ha = HorizontalAlignment.Right;
                        else
                            ha = HorizontalAlignment.Left;

                        AddMessage(dt.Rows[i][0].ToString(), (DateTime)dt.Rows[i][2], ha, dt2.Rows[0][0].ToString());
                    }
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Text.Trim().Length == 0)
                System.Windows.MessageBox.Show("Enter the text of the message", "Empty input", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (MessageBox.Text.Length > 999)
                System.Windows.MessageBox.Show("The message is too long.\nMaximum number of characters : 1000", "Message overflow", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                if (client != null)
                {
                    Message message = new Message()
                    {
                        Msg = MessageBox.Text,
                        UserSenderId = user.Id,
                        ChatRecieverId = chat.Id,
                        UserNickname = user.Nickname,
                        DateTime = DateTime.Now
                    };

                    Insert("INSERT INTO [dbo].[messages] ([messages_text],[users_id_sender],[chats_id_reciever],[messages_datetime]) VALUES ('"
                        + message.Msg + "',"
                        + message.UserSenderId + ","
                        + message.ChatRecieverId + ",'"
                        + message.DateTime + "')");

                    try
                    {
                        client.SendMessage(message);
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("Internal server error.\nRestart the server and client application", "Internal server error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    MessageBox.Text = string.Empty;
                }
            }
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            if (NewChatName.Text.Length < 15 && NewChatName.Text.Length > 0)
            {
                DataTable dt = Select("SELECT [chats_id] FROM [dbo].[chats] WHERE [chats_name] = '" + NewChatName.Text + "'");
                if (dt.Rows.Count > 0)
                {
                    NewChatMessageBlock.Text = "A chat with this name already exists.\nPlease choose a different name.";
                    NewChatMessageBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    Insert("INSERT INTO [dbo].[chats] ([chats_name],[chats_admin_id]) VALUES ('" + NewChatName.Text + "', " + user.Id + ")");
                    DataTable dt2 = Select("SELECT [chats_id] FROM [dbo].[chats] WHERE [chats_name] = '" + NewChatName.Text + "'");
                    Insert("INSERT INTO [dbo].[chats_users] ([chats_id],[users_id]) VALUES (" + dt2.Rows[0][0] + ", " + user.Id + ")");
                    System.Windows.MessageBox.Show("The chat has been created.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadChats();
                    Open(ContactsScreen);
                }
            }
            else
            {
                NewChatMessageBlock.Text = "Enter a name consisting of 1 to 15 characters!";
                NewChatMessageBlock.Visibility = Visibility.Visible;
            }
        }

        private void BrowseNetwork_Click(object sender, RoutedEventArgs e)
        {
            ContactsAll.Items.Clear();
            NetworkChatsMessageBlock.Visibility = Visibility.Hidden;
            DataTable dt = Select("SELECT [chats_name],[chats_admin_id],[chats_id] FROM [dbo].[chats]");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool isParticipant = false;
                    DataTable dt2 = Select("SELECT [chats_id] FROM [dbo].[chats_users] WHERE [users_id] = " + user.Id + "");
                    if (dt2.Rows.Count > 0)
                    {
                        for (int k = 0; k < dt2.Rows.Count; k++)
                            if (dt.Rows[i][2].ToString() == dt2.Rows[k][0].ToString())
                                isParticipant = true;
                    }

                    bool isSent = false;
                    DataTable dt3 = Select("SELECT [invitations_to_chat_id] FROM [dbo].[invitations] WHERE [invitations_from_user_id] = " + user.Id + "");
                    if (dt3.Rows.Count > 0)
                    {
                        for (int k = 0; k < dt3.Rows.Count; k++)
                            if (dt.Rows[i][2].ToString() == dt3.Rows[k][0].ToString())
                                isSent = true;
                    }

                    TextBlock tb = new TextBlock()
                    {
                        Text = dt.Rows[i][0].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(5),
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Color.FromRgb(246, 246, 246)),
                        FontSize = 14
                    };

                    Button btn = CheckUserRights(Convert.ToInt32(dt.Rows[i][1].ToString()), isParticipant, isSent);
                    DockPanel dp = new DockPanel();
                    dp.Children.Add(tb);
                    dp.Children.Add(btn);

                    ContactsAll.Items.Add(dp);
                    ContactsAll.ScrollIntoView(ContactsAll.Items[ContactsAll.Items.Count - 1]);
                }
            }
            else NetworkChatsMessageBlock.Visibility = Visibility.Visible;

            Open(NetworkChats);
        }

        private Button CheckUserRights(int adminId, bool isParticipant, bool isSent)
        {
            Button btn = new Button();

            if (user.Id == adminId && isParticipant)
            {
                btn.Content = "You are the chat admin";
                btn.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (isParticipant)
            {
                btn.Content = "You are already a member";
                btn.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (isSent)
            {
                btn.Content = "You've already sent a request";
                btn.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                btn.Content = "Send a request to join";
                btn.Foreground = new SolidColorBrush(Color.FromRgb(246, 246, 246));
                btn.IsEnabled = true;
                btn.Click += SendRequest_Click;
            }

            return btn;
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DockPanel dp = (DockPanel)button.Parent;
            TextBlock txt = (TextBlock)dp.Children[0];

            DataTable dt = Select("SELECT [chats_id] FROM [dbo].[chats] WHERE [chats_name] = '" + txt.Text + "'");
            if (dt.Rows.Count > 0)
            {
                Insert("INSERT INTO [dbo].[invitations] ([invitations_to_chat_id],[invitations_from_user_id]) VALUES (" + dt.Rows[0][0].ToString() + ", " + user.Id + ")");
            }
            System.Windows.MessageBox.Show("The request to add to the chat\nwas successfully sent to the chat admin", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            BrowseNetwork_Click(null, null);
        }

        private void Moderate_Click(object sender, RoutedEventArgs e)
        {
            Requests.Items.Clear();
            UsersList.Items.Clear();
            DataTable dt = Select("SELECT [invitations_from_user_id] FROM [dbo].[invitations] WHERE [invitations_to_chat_id] = " + chat.Id + "");
            if (dt.Rows.Count > 0)
            {
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    DataTable dt2 = Select("SELECT [login] FROM [dbo].[users] WHERE [users_id] = " + dt.Rows[k][0].ToString() + "");
                    if (dt2.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            TextBlock tb = new TextBlock()
                            {
                                Text = dt2.Rows[i][0].ToString(),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Padding = new Thickness(5),
                                Margin = new Thickness(5),
                                Foreground = new SolidColorBrush(Color.FromRgb(246, 246, 246)),
                                FontSize = 14
                            };

                            Button btn = new Button()
                            {
                                Content = "Add user to chat"
                            };
                            btn.Click += AcceptRequest_Click;
                            DockPanel dp = new DockPanel();
                            dp.Children.Add(tb);
                            dp.Children.Add(btn);

                            Requests.Items.Add(dp);
                            Requests.ScrollIntoView(Requests.Items[Requests.Items.Count - 1]);
                        }
                    }
                }
            }

            DataTable dt3 = Select("SELECT [users_id] FROM [dbo].[chats_users] WHERE [chats_id] = " + chat.Id);
            if (dt3.Rows.Count > 0)
            {
                for (int i = 0; i < dt3.Rows.Count; i++)
                {
                    DataTable users = Select("SELECT [login] FROM [dbo].[users] WHERE [users_id] = " + dt3.Rows[i][0].ToString() + "");
                    if (users.Rows.Count > 0)
                    {
                        for (int k = 0; k < users.Rows.Count; k++)
                        {
                            UsersList.Items.Add(users.Rows[k][0].ToString());
                            UsersList.ScrollIntoView(UsersList.Items[UsersList.Items.Count - 1]);
                        }
                    }
                }
            }

            Open(ModerateScreen);
        }

        private void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DockPanel dp = (DockPanel)button.Parent;
            TextBlock txt = (TextBlock)dp.Children[0];

            DataTable dt = Select("SELECT TOP(1) [users_id] FROM [dbo].[users] WHERE [login] = '" + txt.Text + "'");
            if (dt.Rows.Count > 0)
            {
                Insert("INSERT INTO [dbo].[chats_users] ([chats_id],[users_id]) VALUES (" + chat.Id + ", " + dt.Rows[0][0] + ")");
                Insert("DELETE FROM [dbo].[invitations] WHERE [invitations_from_user_id] = " + dt.Rows[0][0] + " AND [invitations_to_chat_id] = " + chat.Id);
            }
            System.Windows.MessageBox.Show("The user was successfully\nadded to the chat", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Moderate_Click(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisconnectUser();
        }

        public DataTable Select(string selectSQL)
        {
            DataTable dataTable = new DataTable("dataBase");
            dataBase.OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = dataBase.GetConnection();
            cmd.CommandText = selectSQL;
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
            try
            {
                sqlDataAdapter.Fill(dataTable);
            }
            catch
            {
                System.Windows.MessageBox.Show("Error when executing a query to the database.\nCheck the correctness of the request.", "Database query error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return dataTable;
        }

        public void Insert(string insertSQL)
        {
            dataBase.OpenConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = dataBase.GetConnection();
            cmd.CommandText = insertSQL;
            cmd.ExecuteNonQuery();
        }

        private void Open(Border screen)
        {
            LoginScreen.Visibility = Visibility.Hidden;
            AuthScreen.Visibility = Visibility.Hidden;
            ContactsScreen.Visibility = Visibility.Hidden;
            ChatScreen.Visibility = Visibility.Hidden;
            NewChatScreen.Visibility = Visibility.Hidden;
            NetworkChats.Visibility = Visibility.Hidden;
            ModerateScreen.Visibility = Visibility.Hidden;

            screen.Visibility = Visibility.Visible;
        }

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Content)
            {
                case "← Back to Sign in":
                    LoginMessageBlock.Text = "";
                    Open(LoginScreen);
                    break;
                case "Go sign up":
                    AuthMessageBlock.Text = "";
                    Open(AuthScreen);
                    break;
                case "← Log out":
                    DisconnectUser();
                    LoginMessageBlock.Text = "";
                    Open(LoginScreen);
                    break;
                case "Create new chat":
                    Open(NewChatScreen);
                    break;
                case "← Chat":
                    Open(ChatScreen);
                    break;
                case "← Chats":
                    ContactsList.UnselectAll();
                    Open(ContactsScreen);
                    break;
                default:
                    break;
            }
        }
    }
}
