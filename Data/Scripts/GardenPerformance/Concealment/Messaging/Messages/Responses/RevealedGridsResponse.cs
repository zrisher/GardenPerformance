using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Responses {
    class RevealedGridsResponse : Response {

        //private const int Size = sizeof(long);


        public RevealedGridsResponse() :
            base((ushort)MessageType.RevealedGridsResponse) { }

        public static RevealedGridsResponse FromBytes(byte[] bytes) {
            //VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            RevealedGridsResponse request = new RevealedGridsResponse();
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
