using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.PushbulletAPI
{
    public static class PushConstants
    {
        public static class Configs
        {
            public static string ClientID = "--CLIENTID--";
            public static string RedirectURL = "--REDIRECT URL--";
        }

        public static class WebServices
        {
            public static string OAuthLogin = "https://www.pushbullet.com/authorize";
            public static string RealtimeEventsURL = "wss://stream.pushbullet.com/websocket/";
        }
    }
}
