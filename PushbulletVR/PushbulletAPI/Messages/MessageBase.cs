using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace PushbulletVR.PushbulletAPI.Messages
{
    [DataContract]
    public abstract class MessageBase
    {
        public abstract string GetIdentifier();

        public override int GetHashCode()
        {
            if (this.GetIdentifier() == null) return 0;
            return this.GetIdentifier().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MessageBase)
                return this.GetIdentifier().Equals((obj as MessageBase).GetIdentifier());
            if (obj is String)
                return this.GetIdentifier().Equals(obj.ToString());

            return false;
        }
    }
}
