using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Requests {
    class RevealedGridsRequest : SEGarden.Messaging.MessageBase {

        public static RevealedGridsRequest FromBytes(byte[] bytes) {
            //VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealedGridsRequest request = new RevealedGridsRequest();
            //request.EntityId = stream.getLong();

            return request;
        }

        //private const int Size = sizeof(long);

        /*
        public ConcealedGridsRequest() :
            base((ushort)MessageDomains.ConcealServer, (ushort)MessageType.ConcealedGridsRequest) { }
        */


        protected override ushort TypeId {
            get { return (ushort)MessageType.RevealedGridsRequest; } 
        }

        protected override ushort DomainId {
            get { return MessageDomain.ConcealServer; }
        }

        public long EntityId; // { get; private set; }


        protected override byte[] ToBytes() {
            //VRage.ByteStream stream = new VRage.ByteStream(Size);

            //stream.addLong(EntityId);

            //return stream.Data;
            return new byte[0];
        }



    }
}
