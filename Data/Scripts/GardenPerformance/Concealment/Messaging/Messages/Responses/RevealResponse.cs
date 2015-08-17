using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Responses {
    class RevealResponse : Response {

        private const int SIZE = sizeof(long) + sizeof(bool);

        public static RevealResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealResponse response = new RevealResponse();
            response.EntityId = stream.getLong();

            return response;
        }

        public long EntityId;
        public bool Success;

        public RevealResponse() :
            base((ushort)MessageType.RevealResponse) { }


        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);

            stream.addLong(EntityId);
            stream.addBoolean(Success);

            return stream.Data;
        }

    }
}
