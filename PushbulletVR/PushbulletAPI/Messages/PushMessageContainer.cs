using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PushbulletVR.PushbulletAPI.Messages
{
    [DataContract]
    class PushMessageContainer
    {
        [DataMember(Name = "pushes")]
        public List<PushMessage> Pushes { get; set; }

        [DataMember(Name = "cursor")]
        public string Cursor { get; set; }
    }
}
