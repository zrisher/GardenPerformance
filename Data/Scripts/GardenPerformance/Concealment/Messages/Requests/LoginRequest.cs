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
        }

        public LoginRequest(long playerId) : base((ushort)MessageType.LoginRequest) {
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
