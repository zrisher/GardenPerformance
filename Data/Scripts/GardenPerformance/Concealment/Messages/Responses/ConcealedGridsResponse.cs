using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

using GP.Concealment.World.Entities;

namespace GP.Concealment.Messages.Responses {

    class ConcealedGridsResponse : Response {

        public static ConcealedGridsResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            ConcealedGridsResponse response = new ConcealedGridsResponse();
            response.LoadFromByteStream(stream);

            ConcealedGrid grid;
            ushort count = stream.getUShort();
            for (int i = 0; i < count; i++) {
                grid = new ConcealedGrid();
                grid.RemoveFromByteStream(stream);
                response.ConcealedGrids.Add(grid);
            }

            return response;
        }

        public List<ConcealedGrid> ConcealedGrids = new List<ConcealedGrid>();

        public ConcealedGridsResponse() :
            base((ushort)MessageType.ConcealedGridsResponse) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(32, true);
            base.AddToByteStream(stream);

            stream.addUShort((ushort)ConcealedGrids.Count);
            foreach (ConcealedGrid grid in ConcealedGrids) {
                grid.AddToByteStream(stream);
            }

            return stream.Data;
        }

    }
}
