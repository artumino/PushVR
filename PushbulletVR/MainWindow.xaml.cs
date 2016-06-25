using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

using MahApps.Metro.Controls;

using SteamVR_HUDCenter;
using SteamVR_HUDCenter.Elements;

using PushbulletVR.Overlays;
using PushbulletVR.Requests;
using PushbulletVR.UserControls;
using PushbulletVR.PushbulletAPI;
using PushbulletVR.PushbulletAPI.Messages;

namespace PushbulletVR
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public HUDCenterController VRController { get; private set; }
        public Overlay VRDummyOverlay { get; private set; }

        private OAuthLogin loginForm;
        private NotificationList notificationList;
        private PushbulletNetworkManager PushClient;

        public Dictionary<MessageBase, uint> SentNotifications;

        public MainWindow()
        {
            InitializeComponent();
            this.VRController = new HUDCenterController();
            this.SentNotifications = new Dictionary<MessageBase, uint>();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(VRController._IsRunning)
                VRController.Stop();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                VRController.Init();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (String.IsNullOrEmpty(Properties.Settings.Default["PushToken"].ToString()))
                TryLogin();
            else
                StartNotificationListener();
        }

        #region PushbulletLogin

        public void TryLogin()
        {
            this.contentCard.Children.Clear();
            txtStatus.Foreground = Brushes.Red;
            txtStatus.Text = "Status: Logged-Out";
            loginForm = new OAuthLogin(new OAuthRequest()
            {
                OAuthService = new Uri(PushConstants.WebServices.OAuthLogin),
                ClientID = PushConstants.Configs.ClientID,
                RedirectURL = PushConstants.Configs.RedirectURL
            });
            loginForm.OnLoginEnded += Login_OnLoginEnded;
            this.contentCard.Children.Add(loginForm);
        }

        private void Login_OnLoginEnded(System.Collections.Specialized.NameValueCollection response, String Url)
        {
            if (response["error"] != null)
            {
                MessageBox.Show("Error during authentication, please try again!");
                loginForm.NavigateToLogin();
            }
            else
            {
                response = System.Web.HttpUtility.ParseQueryString(Url.Split(new char[1] { '#' })[1]);
                Properties.Settings.Default["PushToken"] = response["access_token"];
                Properties.Settings.Default.Save();
                StartNotificationListener();
            }
        }

        #endregion

        #region Realtime Events
        public void StartNotificationListener()
        {
            this.contentCard.Children.Clear();
            notificationList = new NotificationList(SentNotifications);

            if(VRController._IsRunning)
            {
                VRDummyOverlay = new WPFOverlay("PushVR", "", notificationList, Dispatcher);
                VRController.RegisterNewItem(VRDummyOverlay);
            }

            this.contentCard.Children.Add(notificationList);

            PushClient = new PushbulletNetworkManager(Properties.Settings.Default["PushToken"].ToString());
            PushClient.OnMessageReceived += PushClient_OnMessageReceived;
            //Tries to connect to Pushbullet Realtime-Events WebSocket
            PushClient.StartListeningForRealtimeEvents(new Action(() =>
            {
                //On Success
                Dispatcher.Invoke(new Action(() =>
                {
                    txtStatus.Foreground = Brushes.Green;
                    txtStatus.Text = "Status: Listening...";
                }));
            }), new Action<object>((object ex) =>
            {
                //On Fail
                Dispatcher.Invoke(new Action(() =>
                {
                    if (ex is HttpWebResponse)
                    {
                        if (((HttpWebResponse)ex).StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Properties.Settings.Default["PushToken"] = null;
                            Properties.Settings.Default.Save();
                            MessageBox.Show("Failed to login to Pushbullet, please log-in again!");
                            TryLogin();
                            return;
                        }
                    }
                    else
                        MessageBox.Show(ex.ToString());
                }));
            }));
        }

        private void PushClient_OnMessageReceived(InstaMessage Message)
        {
            switch (Message.Type)
            {
                case InstaMessage.InstaType.Tickle:
                    if (Message.SubType == InstaMessage.InstaSubType.Push)
                    {
                        PushbulletNetworkManager.GetPushesFilter filter = new PushbulletNetworkManager.GetPushesFilter()
                        {
                            ModifiedAfter = PushClient.LastPushModified,
                            Active = true
                        };

                        List<PushMessage> results = PushClient.GetPushes(filter);

                        foreach (PushMessage push in results)
                        {
                            if (!push.Dismissed && !SentNotifications.ContainsKey(push) && push.Direction != PushMessage.PushDirection.Outgoing)
                            {
                                string DisplayMessage = String.Format("{0}\n{1}\n{2}", push.SenderName, push.Title, push.Body);
                                uint id = 0;
                                if (VRController._IsRunning)
                                    id = VRController.DisplayNotification(DisplayMessage, VRDummyOverlay, Valve.VR.EVRNotificationType.Transient, Valve.VR.EVRNotificationStyle.Application, new Valve.VR.NotificationBitmap_t());
                                AddNotification(push, id);
                            }
                        }
                    }
                    break;
                case InstaMessage.InstaType.Push:
                    InstaPushMessage response = Message.Push;
                    if (response != null && response.Type != InstaPushMessage.InstaPushType.Dismissal)
                    {
                        if (SentNotifications.ContainsKey(response))
                        {
                            foreach (MessageBase msg in SentNotifications.Keys)
                                if (msg is InstaPushMessage && msg.Equals(response))
                                {
                                    (msg as InstaPushMessage).Body += response.Body;
                                    break;
                                }
                        }

                        string DisplayMessage = String.Format("{0}\n{1}\n{2}", response.ApplicationName, response.Title, response.Body);
                        Valve.VR.NotificationBitmap_t bitmap = new Valve.VR.NotificationBitmap_t();
                        uint id = 0;
                        if (response.Icon != null && VRController._IsRunning)
                        {
                            using (System.Drawing.Bitmap iconOriginal = new System.Drawing.Bitmap(new System.IO.MemoryStream(response.Icon)))
                            using (System.Drawing.Bitmap iconBMP = new System.Drawing.Bitmap(iconOriginal.Width, iconOriginal.Height,
                                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
                            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(iconBMP))
                            {
                                gr.DrawImage(iconOriginal, new System.Drawing.Rectangle(0, 0, iconBMP.Width, iconBMP.Height));
                                System.Drawing.Imaging.BitmapData TextureData =
                                iconBMP.LockBits(
                                    new System.Drawing.Rectangle(0, 0, iconBMP.Width, iconBMP.Height),
                                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                                );

                                bitmap.m_nBytesPerPixel = 4;
                                bitmap.m_nHeight = TextureData.Height;
                                bitmap.m_nWidth = TextureData.Width;
                                bitmap.m_pImageData = TextureData.Scan0;

                                iconBMP.UnlockBits(TextureData);

                                Valve.VR.EVRNotificationType type = response.Dismissable ? Valve.VR.EVRNotificationType.Transient : Valve.VR.EVRNotificationType.Persistent;

                                id = VRController.DisplayNotification(DisplayMessage, VRDummyOverlay, Valve.VR.EVRNotificationType.Transient, Valve.VR.EVRNotificationStyle.Contact_Enabled, bitmap);
                            }
                        }
                        if (SentNotifications.ContainsKey(response))
                            UpdateNotification(response, id);
                        else
                            AddNotification(response, id);
                    }
                    else if (response != null && response.Type == InstaPushMessage.InstaPushType.Dismissal)
                        RemoveNotification(response);
                    break;
            }
        }
        #endregion

        #region Notification Management

        private void AddNotification(MessageBase notification, uint ID)
        {
            if (!SentNotifications.ContainsKey(notification))
            {
                SentNotifications.Add(notification, ID);
                notificationList.DataSetChanged();
            }
        }

        private void UpdateNotification(MessageBase notification, uint newID)
        {
            if (SentNotifications.ContainsKey(notification))
            {
                SentNotifications[notification] = newID;
                notificationList.DataSetChanged();
            }
        }

        private void RemoveNotification(MessageBase notification)
        {
            if (SentNotifications.ContainsKey(notification))
            {
                uint id = SentNotifications[notification];
                if(VRController._IsRunning)
                    VRController.CloseNotification(id);
                SentNotifications.Remove(notification);
                notificationList.DataSetChanged();
            }
        }

        #endregion
    }
}
