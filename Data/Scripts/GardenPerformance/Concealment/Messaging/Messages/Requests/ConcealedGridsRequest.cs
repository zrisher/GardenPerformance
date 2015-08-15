using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;
using SEGarden.Logging;

namespace GP.Concealment.Messaging.Messages.Requests {
    class ConcealedGridsRequest : Request {

        static Logger Log = new Logger("GP.Concealment.ConcealedGridsRequest");

        //private const int Size = sizeof(long);

        public ConcealedGridsRequest() :
            base((ushort)MessageType.ConcealedGridsRequest) { }


        public static ConcealedGridsRequest FromBytes(byte[] bytes) {
            //VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealedGridsRequest request = new ConcealedGridsRequest();
            //request.EntityId = stream.getLong();

            return request;
        }


        protected override byte[] ToBytes() {
            //VRage.ByteStream stream = new VRage.ByteStream(Size);

            //stream.addLong(EntityId);

            //return stream.Data;
            return new byte[0];
        }



    }
}
