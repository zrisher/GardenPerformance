using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messaging.Messages.Responses {

    class ConcealedGridsResponse : Response {

        private const int SizeInBytes = sizeof(long);

        public List<Records.Entities.ConcealedGrid> ConcealedGrids =
            new List<Records.Entities.ConcealedGrid>();

        public ConcealedGridsResponse() :
            base((ushort)MessageType.ConcealedGridsResponse) { }

        public static ConcealedGridsResponse FromBytes(byte[] bytes) {
            //VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ConcealedGridsResponse request = new ConcealedGridsResponse();

            // TODO: Deserialize concealed grids

            //request.EntityId = stream.getLong();

            return request;
        }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SizeInBytes);

            // TODO : Serialize concealed grids

            //stream.addLong(EntityId);

            //return stream.Data;
            return new byte[0];
        }

    }
}
