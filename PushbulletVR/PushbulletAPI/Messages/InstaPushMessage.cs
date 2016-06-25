using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization;
using PushbulletVR.PushbulletAPI.Utils;

namespace PushbulletVR.PushbulletAPI.Messages
{
    [DataContract]
    public class InstaPushMessage : MessageBase
    {
        public enum InstaPushType
        {
            Mirror,
            Dismissal,
            Clip,
            Unknown
        }

        #region Public Properties
        [DataMember(Name = "application_name")]
        public string ApplicationName { get; set; }

        [DataMember(Name = "type")]
        public string SType { private get; set; }
        public InstaPushType Type {
            get
            {
                return EnumUtils.ParseNonCaseSensitiveOrDefault(SType, InstaPushType.Unknown);
            }
        }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "created")]
        public string SCreated { private get; set; }
        public DateTime Created { get { return !String.IsNullOrEmpty(SCreated)
                    ? PushUtils.UnixTimeStampToDateTime(PushUtils.ParseFloatTime(SCreated)) 
                    : DateTime.Now ; } }
        
        [DataMember(Name = "dismissable")]
        public bool Dismissable { get; set; }

        [DataMember(Name = "icon")]
        public string SIcon { private get; set; }
        public byte[] Icon { get { return !String.IsNullOrEmpty(SIcon)
                    ? Convert.FromBase64String(SIcon) 
                    : null; } }
        
        [DataMember(Name = "iden")]
        public string Iden { get; set; }

        [DataMember(Name = "notification_id")]
        public string NotificationID { get; set; }

        [DataMember(Name = "notification_tag")]
        public string NotificationTag { get; set; }

        [DataMember(Name = "package_name")]
        public string PackageName { get; set; }

        [DataMember(Name = "receiver_email")]
        public string ReceiverEmail { get; set; }

        [DataMember(Name = "receiver_email_normalized")]
        public string ReceiverEmailNormalized { get; set; }

        [DataMember(Name = "receiver_iden")]
        public string ReceiverIden { get; set; }

        [DataMember(Name = "sender_email")]
        public string SenderEmail { get; set; }

        [DataMember(Name = "sender_email_normalized")]
        public string SenderEmailNormalized { get; set; }

        [DataMember(Name = "sender_iden")]
        public string SenderIden { get; set; }

        [DataMember(Name = "source_device_iden")]
        public string SourceDeviceIden { get; set; }

        [DataMember(Name = "source_user_iden")]
        public string SourceUserIden { get; set; }

        [DataMember(Name = "conversation_iden")]
        public string ConversationIden { get; set; }

        #endregion

        public override string GetIdentifier()
        {
            if (Iden != null)
                return Iden;
            return NotificationID + NotificationTag;
        }
    }
}
