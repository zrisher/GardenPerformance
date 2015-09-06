using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {

    class PilotingStartedRequest : Request {

        public static readonly int SIZE = sizeof(long) + Request.SIZE;

        public PilotingStartedRequest(byte[] bytes) :
            base((ushort)MessageType.PilotingStartedRequest) 
        {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            PlayerId = stream.getLong();
            GridId = stream.getLong();
        }

        public PilotingStartedRequest(long playerId, long gridId) :
            base((ushort)MessageType.PilotingStartedRequest) 
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
