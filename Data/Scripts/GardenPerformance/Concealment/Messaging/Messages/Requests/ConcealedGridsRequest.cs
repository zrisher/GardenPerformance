using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;
using SEGarden.Logging;

namespace GP.Concealment.Messaging.Messages.Requests {
    class ConcealedGridsRequest : Request {

        static Logger Log = new Logger("GP.Concealment.ConcealedGridsRequest");

        public static ConcealedGridsRequest FromBytes(byte[] bytes) {
            //VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealedGridsRequest request = new ConcealedGridsRequest();
            //request.EntityId = stream.getLong();

            return request;
        }

        //private const int Size = sizeof(long);

        /*
        public ConcealedGridsRequest() :
            base((ushort)MessageDomains.ConcealServer, (ushort)MessageType.ConcealedGridsRequest) { }
        */


        protected override ushort TypeId {
            get {
                /*
                Log.Trace("TypeId being requested for ConcealedGridsRequest", "TypeId");
                Log.Trace("ushort for each enum:", "TypeId");
                Log.Trace(MessageType.ConcealedGridsRequest + " : " + (ushort)MessageType.ConcealedGridsRequest, "TypeId");
                Log.Trace(MessageType.ConcealedGridsResponse + " : " + (ushort)MessageType.ConcealedGridsResponse, "TypeId");
                Log.Trace(MessageType.ConcealRequest + " : " + (ushort)MessageType.ConcealRequest, "TypeId");
                Log.Trace(MessageType.ConcealResponse + " : " + (ushort)MessageType.ConcealResponse, "TypeId");
                Log.Trace(MessageType.RevealedGridsRequest + " : " + (ushort)MessageType.RevealedGridsRequest, "TypeId");
                Log.Trace(MessageType.RevealedGridsResponse + " : " + (ushort)MessageType.RevealedGridsResponse, "TypeId");
                Log.Trace(MessageType.RevealRequest + " : " + (ushort)MessageType.RevealRequest, "TypeId");
                Log.Trace(MessageType.RevealResponse + " : " + (ushort)MessageType.RevealResponse, "TypeId");
                Log.Trace("ushort for each enum:", "TypeId");
                */
                return (ushort)MessageType.ConcealedGridsRequest; 
            } 
        }

        protected override ushort DomainId {
            get { return MessageDomain.ConcealServer; }
        }

        public long EntityId;// { get; private set; }


        protected override byte[] ToBytes() {
            //VRage.ByteStream stream = new VRage.ByteStream(Size);

            //stream.addLong(EntityId);

            //return stream.Data;
            return new byte[0];
        }



    }
}
