using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;

using PushbulletVR.Extensions;
using PushbulletVR.PushbulletAPI.Messages;
using System.Net.Http;
using System.Net.WebSockets;
using System.Windows.Threading;

namespace PushbulletVR.PushbulletAPI
{
    class PushbulletNetworkManager
    {
        public delegate void MessageReceived(InstaMessage Message);
        public event MessageReceived OnMessageReceived;

        private string AccessToken { get; set; }
        public DateTime LastPushModified { get; private set; }

        public Networking.WebSocketManager RealtimeManager { get; set; }

        public class GetPushesFilter
        {
            public DateTime? ModifiedAfter { get; set; }
            public int? Limit { get; set; }
            public bool? Active { get; set; }
            public string Cursor { get; set; }
        }

        public PushbulletNetworkManager()
        {

        }

        public PushbulletNetworkManager(string AccessToken)
        {
            this.AccessToken = AccessToken;
            this.LastPushModified = DateTime.Now;
        }

        public List<PushMessage> GetPushes(GetPushesFilter filter = null)
        {

            if (String.IsNullOrEmpty(this.AccessToken))
                return null;

            List<PushMessage> Pushes = new List<PushMessage>();

            UriBuilder uriBuilder = new UriBuilder(new Uri("https://api.pushbullet.com/v2/pushes"));
            if (filter != null)
            {
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                if(filter.Active.HasValue)
                    query["active"] = filter.Active.Value.ToString();
                if(filter.Limit.HasValue)
                    query["limit"] = filter.Limit.Value.ToString();
                if (!String.IsNullOrEmpty(filter.Cursor))
                    query["cursor"] = filter.Cursor;
                if (filter.ModifiedAfter.HasValue)
                    query["modified_after"] = PushUtils.ConvertToFloatTime(PushUtils.DateTimeToUnixTimeStamp(filter.ModifiedAfter.Value));
                uriBuilder.Query = query.ToString();
            }
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", this.AccessToken));
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.SendAsync(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                PushMessageContainer container = result.JsonToOjbect<PushMessageContainer>();
                if (container.Pushes != null)
                {
                    foreach(PushMessage msg in container.Pushes)
                    {
                        if (msg.Modified != null && msg.Modified > LastPushModified)
                            LastPushModified = new DateTime(msg.Modified.Ticks);
                        if (msg.Created != null && msg.Created > LastPushModified)
                            LastPushModified = new DateTime(msg.Created.Ticks);
                    }
                    Pushes = container.Pushes;
                }
            }

            return Pushes;
        }

        public void StartListeningForRealtimeEvents(Action connectionSuccess, Action<object> connectionFailed)
        {
            RealtimeManager = new Networking.WebSocketManager();
            RealtimeManager.OnMessageReceived += RealtimeManager_OnMessageReceived; ;
            Task<Exception> connect = RealtimeManager.Connect(new Uri(PushConstants.WebServices.RealtimeEventsURL + Properties.Settings.Default["PushToken"]));
            connect.GetAwaiter().OnCompleted(new Action(() =>
            {
                if (connect.Result != null)
                {

                    if (connect.Result is WebSocketException)
                    {
                        WebSocketException ex = connect.Result as WebSocketException;
                        if (ex.InnerException != null && ex.InnerException is WebException)
                            connectionFailed(((WebException)ex.InnerException).Response as HttpWebResponse);
                    }
                    else
                        connectionFailed(connect.Result.Message);
                }
                else
                    connectionSuccess();
            }));
        }

        private void RealtimeManager_OnMessageReceived(Networking.WebSocketManager.StateObject state, string value)
        {
            InstaMessage message = value.JsonToOjbect<InstaMessage>();
            if (OnMessageReceived != null)
                OnMessageReceived(message);
        }
    }
}
