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

using PushbulletVR.PushbulletAPI.Messages;
using System.Windows.Threading;

namespace PushbulletVR
{
    /// <summary>
    /// Logica di interazione per NotificationItem.xaml
    /// </summary>
    public partial class NotificationItem : UserControl
    {
        public delegate void FinishedLoading();
        public event FinishedLoading OnFinishedLoading;

        public MessageBase linkedMessage;

        public NotificationItem(MessageBase linkedMessage)
        {
            InitializeComponent();
            this.linkedMessage = linkedMessage;
            this.Loaded += NotificationItem_Loaded;
        }

        private void NotificationItem_Loaded(object sender, RoutedEventArgs e)
        {
            if(linkedMessage is InstaPushMessage)
            {
                InstaPushMessage message = linkedMessage as InstaPushMessage;

                if (message.Icon != null)
                {
                    BitmapImage iconBMP = new BitmapImage();
                    iconBMP.BeginInit();
                    iconBMP.StreamSource = new System.IO.MemoryStream(message.Icon);
                    iconBMP.EndInit();
                    this.imgIcon.Source = iconBMP;
                    this.imgIcon.Width = iconBMP.Width;
                    this.imgIcon.Height = iconBMP.Height;
                    this.imgIcon.VerticalAlignment = VerticalAlignment.Center;
                    this.imgIcon.HorizontalAlignment = HorizontalAlignment.Center;
                }

                this.txtTitle.FontSize = 32;
                this.txtTitle.Inlines.Add(new Bold(new Run(message.ApplicationName)));

                this.txtContent.FontSize = 25; // 24 points
                this.txtContent.Inlines.Add(new Bold(new Run(message.Title)));
                this.txtContent.Inlines.Add("\n");
                this.txtContent.Inlines.Add(message.Body);
                this.txtContent.TextWrapping = TextWrapping.Wrap;
            }
            if(linkedMessage is PushMessage)
            {
                PushMessage message = linkedMessage as PushMessage;
                this.txtTitle.FontSize = 32;
                this.txtTitle.Inlines.Add(new Bold(new Run(message.SenderName)));

                this.txtContent.FontSize = 25; // 24 points
                this.txtContent.Inlines.Add(new Bold(new Run(message.Title)));
                this.txtContent.Inlines.Add("\n");
                this.txtContent.Inlines.Add(message.Body);
                this.txtContent.TextWrapping = TextWrapping.Wrap;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => {
                if (OnFinishedLoading != null)
                    OnFinishedLoading();
            }));
        }
    }
}
