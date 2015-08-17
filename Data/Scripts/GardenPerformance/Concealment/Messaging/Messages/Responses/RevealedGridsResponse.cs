using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;
using SEGarden.Extensions;

using GP.Concealment.Records.Entities;

namespace GP.Concealment.Messaging.Messages.Responses {
    class RevealedGridsResponse : Response {

        private static Logger Log =
            new Logger("GP.Concealment.Messaging.Messages.Responses.RevealedGridsResponse");

        public static RevealedGridsResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            RevealedGridsResponse response = new RevealedGridsResponse();   
  
            ConcealableGrid grid;
            ushort count = stream.getUShort();
            //Log.Trace("Retrieving " + count + " grids from response", "ToBytes");
            for (int i = 0; i < count; i++) {
                grid = new ConcealableGrid();
                grid.RemoveFromByteStream(stream);
                response.RevealedGrids.Add(grid);
                //Log.Trace("Added grid " + grid.EntityId, "ToBytes");
            }

            return response;
        }

        public List<ConcealableGrid> RevealedGrids = new List<ConcealableGrid>();

        public RevealedGridsResponse() :
            base((ushort)MessageType.RevealedGridsResponse) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(32, true);

            //Log.Trace("Adding grids to response", "ToBytes");
            stream.addUShort((ushort)RevealedGrids.Count);
            foreach (ConcealableGrid grid in RevealedGrids) {
                grid.AddToByteStream(stream);
                //Log.Trace("Added Grid", "ToBytes");
            }

            return stream.Data;
        }

    }
}
