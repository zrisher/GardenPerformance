using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class RevealedGridsRequest : Request {

        public static RevealedGridsRequest FromBytes(byte[] bytes) {
            return new RevealedGridsRequest();
        }

        public RevealedGridsRequest() :
            base((ushort)MessageType.RevealedGridsRequest) { }

        public long EntityId;

        protected override byte[] ToBytes() {
            return new byte[0];
        }

    }
}
