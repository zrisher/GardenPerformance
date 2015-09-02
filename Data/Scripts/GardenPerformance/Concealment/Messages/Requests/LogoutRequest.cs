using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class LogoutRequest : Request {

        public static readonly int SIZE = sizeof(long) + Request.SIZE;

        public LogoutRequest(byte[] bytes) :
            base((ushort)MessageType.LogoutRequest) 
        {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            PlayerId = stream.getLong();
        }

        public LogoutRequest(long playerId)
            : base((ushort)MessageType.LogoutRequest) 
        {
            PlayerId = playerId;
        }

        public long PlayerId;

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            stream.addLong(PlayerId);
            return stream.Data;
        }


    }
}
