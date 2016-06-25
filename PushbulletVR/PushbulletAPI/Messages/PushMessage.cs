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
    class PushMessage : MessageBase
    {
        public enum PushType
        {
            Note,
            File,
            Link,
            Unknown
        }

        public enum PushDirection
        {
            Incoming,
            Outgoing,
            Self,
            Unknown
        }

        #region Public Properties
        [DataMember(Name = "iden")]
        public string Iden { get; set; }

        [DataMember(Name = "active")]
        public bool Active { get; set; }

        [DataMember(Name = "created")]
        public string SCreated { get; set; }
        public DateTime Created
        {
            get
            {
                return !String.IsNullOrEmpty(SCreated)
                ? PushUtils.UnixTimeStampToDateTime(PushUtils.ParseFloatTime(SCreated))
                : DateTime.Now;
            }
        }

        [DataMember(Name = "modified")]
        public string SModified { get; set; }
        public DateTime Modified
        {
            get
            {
                return !String.IsNullOrEmpty(SModified)
                ? PushUtils.UnixTimeStampToDateTime(PushUtils.ParseFloatTime(SModified))
                : DateTime.Now;
            }
        }
        
        [DataMember(Name = "type")]
        public string SType { private get; set; }
        public PushType Type
        {
            get
            {
                return EnumUtils.ParseNonCaseSensitiveOrDefault(SType, PushType.Unknown);
            }
        }

        [DataMember(Name = "dismissed")]
        public bool Dismissed { get; set; }

        [DataMember(Name = "guid")]
        public string Guid { get; set; }

        [DataMember(Name = "direction")]
        public string SDirection { private get; set; }
        public PushDirection Direction
        {
            get
            {
                return EnumUtils.ParseNonCaseSensitiveOrDefault(SDirection, PushDirection.Unknown);
            }
        }

        [DataMember(Name = "sender_iden")]
        public string SenderIden { get; set; }

        [DataMember(Name = "sender_email")]
        public string SenderEmail { get; set; }

        [DataMember(Name = "sender_email_normalized")]
        public string SenderEmailNormalized { get; set; }

        [DataMember(Name = "sender_name")]
        public string SenderName { get; set; }

        [DataMember(Name = "receiver_iden")]
        public string ReceiverIden { get; set; }

        [DataMember(Name = "receiver_email")]
        public string ReceiverEmail { get; set; }

        [DataMember(Name = "receiver_email_normalized")]
        public string ReceiverEmailNormalized { get; set; }

        [DataMember(Name = "target_device_iden")]
        public string TargetDeviceIden { get; set; }

        [DataMember(Name = "source_device_iden")]
        public string SourceDeviceIden { get; set; }

        [DataMember(Name = "client_iden")]
        public string ClientIden { get; set; }

        [DataMember(Name = "channel_iden")]
        public string ChannelIden { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "url")]
        public string URL { get; set; }

        [DataMember(Name = "file_name")]
        public string FileName { get; set; }

        [DataMember(Name = "file_type")]
        public string FileType { get; set; }

        [DataMember(Name = "file_url")]
        public string FileURL { get; set; }

        [DataMember(Name = "image_url")]
        public string ImageURL { get; set; }

        [DataMember(Name = "image_width")]
        public string ImageWidth { get; set; }

        [DataMember(Name = "image_height")]
        public string ImageHeight { get; set; }

        #endregion

        public override string GetIdentifier()
        {
            return Iden;
        }
    }
}
