using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Requests {
    class ConcealRequest : SEGarden.Messaging.MessageBase {

        public static ConcealRequest FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealRequest request = new ConcealRequest();
            request.EntityId = stream.getLong();

            return request;
        }

        private const int Size = sizeof(long);


        protected override ushort TypeId {
            get { return (ushort)MessageType.ConcealRequest; } 
        }

        protected override ushort DomainId {
            get { return MessageDomain.ConcealServer; }
        }

        public long EntityId;// { get; private set; }


        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(Size);

            stream.addLong(EntityId);

            return stream.Data;
        }



    }
}
