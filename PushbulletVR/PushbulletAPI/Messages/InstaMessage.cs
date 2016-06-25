using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PushbulletVR.PushbulletAPI.Utils;

namespace PushbulletVR.PushbulletAPI.Messages
{
    [DataContract]
    class InstaMessage : MessageBase
    {
        public enum InstaType
        {
            Tickle,
            Push,
            Nop,
            Unknown
        }
        
        public enum InstaSubType
        {
            Push,
            Unknown
        }

        #region Public Properties

        [DataMember(Name = "type")]
        public string SType { private get; set; }
        public InstaType Type
        {
            get
            {
                return EnumUtils.ParseNonCaseSensitiveOrDefault(SType, InstaType.Unknown);
            }
        }


        [DataMember(Name = "subtype")]
        public string SSubType { private get; set; }
        public InstaSubType SubType
        {
            get
            {
                return EnumUtils.ParseNonCaseSensitiveOrDefault(SSubType, InstaSubType.Unknown);
            }
        }

        [DataMember(Name = "push")]
        public InstaPushMessage Push { get; set; }

        #endregion

        public override string GetIdentifier()
        {
            return null;
        }
    }
}
