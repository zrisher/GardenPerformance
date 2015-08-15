using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Requests {
    class RevealRequest : SEGarden.Messaging.MessageBase {

        private const int Size = sizeof(long);

        public static RevealRequest FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealRequest request = new RevealRequest();
            request.EntityId = stream.getLong();

            return request;
        }

        protected override ushort TypeId {
            get { return (ushort)MessageType.RevealRequest; }
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
