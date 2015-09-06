using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {

    class PilotingStoppedRequest : Request {

        public static readonly int SIZE = sizeof(long) + Request.SIZE;

        public PilotingStoppedRequest(byte[] bytes) :
            base((ushort)MessageType.PilotingStoppedRequest) 
        {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            PlayerId = stream.getLong();
            GridId = stream.getLong();
        }

        public PilotingStoppedRequest(long playerId, long gridId) :
            base((ushort)MessageType.PilotingStoppedRequest) 
        {
            PlayerId = playerId;
            GridId = gridId;
        }

        public long PlayerId;
        public long GridId;

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            stream.addLong(PlayerId);
            stream.addLong(GridId);
            return stream.Data;
        }

    }
}
