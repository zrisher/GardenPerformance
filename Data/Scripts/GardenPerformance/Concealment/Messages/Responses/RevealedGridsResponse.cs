using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;
using SEGarden.Extensions;

using GP.Concealment.World.Entities;

namespace GP.Concealment.Messages.Responses {
    class RevealedGridsResponse : Response {

        private static Logger Log =
            new Logger("GP.Concealment.Messaging.Messages.Responses.RevealedGridsResponse");

        public static RevealedGridsResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            RevealedGridsResponse response = new RevealedGridsResponse();
            response.LoadFromByteStream(stream);
  
            ConcealedGrid grid;
            ushort count = stream.getUShort();
            //Log.Trace("Retrieving " + count + " grids from response", "ToBytes");
            for (int i = 0; i < count; i++) {
                grid = new ConcealedGrid();
                grid.RemoveFromByteStream(stream);
                response.RevealedGrids.Add(grid);
                //Log.Trace("Added grid " + grid.EntityId, "ToBytes");
            }

            return response;
        }

        public List<ConcealedGrid> RevealedGrids = new List<ConcealedGrid>();

        public RevealedGridsResponse() :
            base((ushort)MessageType.RevealedGridsResponse) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(32, true);
            base.AddToByteStream(stream);

            //Log.Trace("Adding grids to response", "ToBytes");
            stream.addUShort((ushort)RevealedGrids.Count);
            foreach (ConcealedGrid grid in RevealedGrids) {
                grid.AddToByteStream(stream);
                //Log.Trace("Added Grid", "ToBytes");
            }

            return stream.Data;
        }

    }
}
