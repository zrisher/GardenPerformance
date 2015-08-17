using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Responses {
    class ConcealResponse : Response {

        private const int SIZE = sizeof(long) + sizeof(bool);

        public ConcealResponse() :
            base((ushort)MessageType.ConcealResponse) { }

        public static ConcealResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealResponse response = new ConcealResponse();
            response.EntityId = stream.getLong();
            response.Success = stream.getBoolean();

            Log.Info("Deserialized Conceal Response " + response.EntityId + " ? " + response.Success, "ToBytes");

            return response;
        }

        public long EntityId;
        public bool Success;

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);

            stream.addLong(EntityId);
            stream.addBoolean(Success);

            Log.Info("Serialized Conceal Response " + EntityId + " ? " + Success, "ToBytes");

            return stream.Data;
        }

    }
}
