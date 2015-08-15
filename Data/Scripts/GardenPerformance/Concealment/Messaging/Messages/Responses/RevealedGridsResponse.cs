using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Responses {
    class RevealedGridsResponse : Response {

        public static RevealedGridsResponse FromBytes(byte[] bytes) {
            //VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealedGridsResponse request = new RevealedGridsResponse();
            //request.EntityId = stream.getLong();

            return request;
        }

        //private const int Size = sizeof(long);

        /*
        public ConcealedGridsRequest() :
            base((ushort)MessageDomains.ConcealServer, (ushort)MessageType.ConcealedGridsRequest) { }
        */


        protected override ushort TypeId {
            get { return (ushort)MessageType.RevealedGridsResponse; } 
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
