using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class ConcealRequest : Request {

        private static readonly int SIZE = Request.SIZE + sizeof(long);

        public static ConcealRequest FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealRequest request = new ConcealRequest();
            request.EntityId = stream.getLong();

            return request;
        }

        public long EntityId;// { get; private set; }

        public ConcealRequest() :
            base((ushort)MessageType.ConcealRequest) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);

            stream.addLong(EntityId);

            return stream.Data;
        }



    }
}
