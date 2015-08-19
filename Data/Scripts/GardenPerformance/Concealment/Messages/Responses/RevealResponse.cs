using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Responses {
    class RevealResponse : Response {

        private const int SIZE = sizeof(long) + sizeof(bool);

        public static RevealResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealResponse response = new RevealResponse();
            response.LoadFromByteStream(stream);
            response.EntityId = stream.getLong();
            response.Success = stream.getBoolean();

            return response;
        }

        public long EntityId;
        public bool Success;

        public RevealResponse() :
            base((ushort)MessageType.RevealResponse) { }


        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            base.AddToByteStream(stream);

            stream.addLong(EntityId);
            stream.addBoolean(Success);

            return stream.Data;
        }

    }
}
