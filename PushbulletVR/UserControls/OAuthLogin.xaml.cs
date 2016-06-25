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
using System.Web;

using System.Collections.Specialized;
using Gecko;
using System.Windows.Forms;

using PushbulletVR.Requests;

namespace PushbulletVR.UserControls
{
    /// <summary>
    /// Logica di interazione per OAuthLogin.xaml
    /// </summary>
    public partial class OAuthLogin : System.Windows.Controls.UserControl
    {
        public delegate void LoginEnded(NameValueCollection response, String Address);
        public event LoginEnded OnLoginEnded;

        private GeckoWebBrowser geckoWebBrowser;

        private OAuthRequest Request { get; set; }

        public OAuthLogin(OAuthRequest Request)
        {
            InitializeComponent();
            this.Request = Request;
            this.Loaded += OAuthLogin_Loaded;
        }

        private void OAuthLogin_Loaded(object sender, RoutedEventArgs e)
        {
            cntBox.Visibility = Visibility.Hidden;
            Xpcom.Initialize("Firefox");
            geckoWebBrowser = new GeckoWebBrowser { Dock = DockStyle.Fill };
            geckoWebBrowser.Navigating += GeckoWebBrowser_Navigating;
            geckoWebBrowser.Navigated += GeckoWebBrowser_Navigated;
            cntBox.Child = geckoWebBrowser;
            cntBox.Background = Brushes.Transparent;
            NavigateToLogin();
        }

        public void NavigateToLogin()
        {
            UriBuilder uriBuilder = new UriBuilder(Request.OAuthService);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["client_id"] = Request.ClientID;
            query["redirect_uri"] = Request.RedirectURL;
            query["response_type"] = Request.Type;
            if (!String.IsNullOrEmpty(Request.ClientSecret))
                query["client_secret"] = Request.ClientSecret;
            uriBuilder.Query = query.ToString();
            geckoWebBrowser.Navigate(uriBuilder.Uri.ToString());
        }

        private void GetLoginResult(string Url)
        {
            UriBuilder uriBuilder = new UriBuilder(Url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            if (OnLoginEnded != null)
                OnLoginEnded(query, Url);
        }

        private void GeckoWebBrowser_Navigating(object sender, Gecko.Events.GeckoNavigatingEventArgs e)
        {
            cntBox.Visibility = Visibility.Hidden;
            if (e.Uri.ToString().Contains(Request.RedirectURL))
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    GetLoginResult(e.Uri.ToString());
                }));
                e.Cancel = true;
            }
        }

        private void GeckoWebBrowser_Navigated(object sender, GeckoNavigatedEventArgs e)
        {
            cntBox.Visibility = Visibility.Visible;
        }
    }
}
