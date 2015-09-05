using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {

    class FactionChangeRequest : Request {

        public static readonly int SIZE = sizeof(long) + Request.SIZE;

        public FactionChangeRequest(byte[] bytes) :
            base((ushort)MessageType.FactionChangeRequest) 
        {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            PlayerId = stream.getLong();
            OldFactionId = stream.getLong();
            NewFactionId = stream.getLong();
        }

        public FactionChangeRequest(long playerId, long oldFactionId, long newFactionId)
            : base((ushort)MessageType.FactionChangeRequest) 
        {
            PlayerId = playerId;
            OldFactionId = oldFactionId;
            NewFactionId = newFactionId;
        }

        public long PlayerId;
        public long OldFactionId;
        public long NewFactionId;

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            stream.addLong(PlayerId);
            stream.addLong(OldFactionId);
            stream.addLong(NewFactionId);
            return stream.Data;
        }

    }
}
