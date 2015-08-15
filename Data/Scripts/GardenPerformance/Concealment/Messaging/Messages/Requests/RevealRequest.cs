using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Requests {
    class RevealRequest : Request {

        private const int Size = sizeof(long);

        public static RevealRequest FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealRequest request = new RevealRequest();
            request.EntityId = stream.getLong();

            return request;
        }

        public RevealRequest() :
            base((ushort)MessageType.RevealRequest) { }


        public long EntityId;// { get; private set; }


        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(Size);

            stream.addLong(EntityId);

            return stream.Data;
        }


    }
}
