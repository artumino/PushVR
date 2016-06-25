using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.Requests
{
    public class OAuthRequest
    {
        public String ClientID;
        public String ClientSecret;
        public String RedirectURL;
        public String Type = "token";
        public Uri OAuthService;
    }
}
