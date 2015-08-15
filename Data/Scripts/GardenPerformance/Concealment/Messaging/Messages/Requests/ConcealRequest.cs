using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Requests {
    class ConcealRequest : Request {

        public static ConcealRequest FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealRequest request = new ConcealRequest();
            request.EntityId = stream.getLong();

            return request;
        }

        private const int Size = sizeof(long);

        public long EntityId;// { get; private set; }

        public ConcealRequest() :
            base((ushort)MessageType.ConcealRequest) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(Size);

            stream.addLong(EntityId);

            return stream.Data;
        }



    }
}
