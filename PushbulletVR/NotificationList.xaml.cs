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

using PushbulletVR.Extensions;
using PushbulletVR.PushbulletAPI.Messages;

using SteamVR_HUDCenter.Elements.WPF;
using System.Drawing;

namespace PushbulletVR
{
    /// <summary>
    /// Logica di interazione per NotificationList.xaml
    /// </summary>
    public partial class NotificationList : UserControl, IVRUserControl
    {
        public Dictionary<MessageBase, NotificationItem> Items { get; set; }
        private Dictionary<MessageBase, uint> DataSet;

        public int RenderWidth = 500;

        public event VRUserControl.RenderEvent OnRendered;

        public NotificationList(Dictionary<MessageBase,uint> DataSet)
        {
            InitializeComponent();
            Items = new Dictionary<MessageBase, NotificationItem>();
            
            this.DataSet = DataSet;
        }

        private void NotificationList_Loaded(object sender, RoutedEventArgs e)
        {
            DataSetChanged();
        }

        public void ClearItems()
        {
            lsNotifications.Children.Clear();
            Items.Clear();
        }

        public void DataSetChanged()
        {
            ClearItems();
            for(int i = DataSet.Keys.Count - 1; i >= 0; i--)
            {
                NotificationItem item = new NotificationItem(DataSet.Keys.ElementAt(i));
                lsNotifications.Children.Add(item);
                Items.Add(DataSet.Keys.ElementAt(i), item);

                if(i == 0)
                    item.OnFinishedLoading += LastItem_OnFinishedLoading;
            }

            if(Items.Count == 0)
                if (OnRendered != null)
                    OnRendered(null);
        }

        private void LastItem_OnFinishedLoading()
        {
            PresentationSource source = PresentationSource.FromVisual(lsNotifications);
            double dpiX, dpiY;
            if (source != null && lsNotifications.ActualHeight.HasValue())
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
                // get the bounding rectangle of the element
                // create a new render target bitmap
                
                lsNotifications.Measure(new System.Windows.Size(lsNotifications.ActualWidth, lsNotifications.ActualHeight));
                lsNotifications.Arrange(new Rect(0, 0, lsNotifications.ActualWidth, lsNotifications.ActualHeight));
                foreach(UIElement child in lsNotifications.Children)
                {
                    child.Measure(new System.Windows.Size(lsNotifications.ActualWidth, lsNotifications.ActualHeight));
                    //child.Arrange(new Rect(0, 0, lsNotifications.ActualWidth, lsNotifications.ActualHeight));
                }
                //lsNotifications.UpdateLayout();

                RenderTargetBitmap bitmap;
                if ((int)lsNotifications.ActualHeight != 0 && (int)lsNotifications.ActualWidth != 0)
                {
                    bitmap = new RenderTargetBitmap((int)(lsNotifications.ActualWidth * dpiX / 96.0), (int)(lsNotifications.ActualHeight * dpiY / 96.0), dpiX, dpiY, PixelFormats.Pbgra32);

                    // create the draw visual
                    DrawingVisual drawingVisual = new DrawingVisual();
                    // draw the visual via visual brush into the context
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        VisualBrush brush = new VisualBrush(lsNotifications);
                        drawingContext.DrawRectangle(brush, null, new Rect(new System.Windows.Point(), new System.Windows.Size(lsNotifications.ActualWidth, lsNotifications.ActualHeight)));
                    }
                    // render the DrawingVisual to the RenderTargetBitmap
                    bitmap.Render(drawingVisual);
                }
                else
                    bitmap = new RenderTargetBitmap((int)(lsNotifications.ActualWidth * dpiX / 96.0), 1, dpiX, dpiY, PixelFormats.Pbgra32);

                if (OnRendered != null)
                    OnRendered(bitmap);
            }
        }
    }
}
