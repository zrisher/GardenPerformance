using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {

    class LoginRequest : Request {

        public static readonly int SIZE = sizeof(long) + Request.SIZE;

        public LoginRequest(byte[] bytes) :
            base((ushort)MessageType.LoginRequest) 
        {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            PlayerId = stream.getLong();
            FactionId = stream.getLong();
        }

        public LoginRequest(long playerId, long factionId) : 
            base((ushort)MessageType.LoginRequest) 
        {
            PlayerId = playerId;
            FactionId = factionId;
        }

        public long PlayerId;
        public long FactionId;

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            stream.addLong(PlayerId);
            stream.addLong(FactionId);
            return stream.Data;
        }

    }
}
