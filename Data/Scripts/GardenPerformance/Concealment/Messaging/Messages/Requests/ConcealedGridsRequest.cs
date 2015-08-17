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
            return new ConcealedGridsRequest();
        }

        public ConcealedGridsRequest() :
            base((ushort)MessageType.ConcealedGridsRequest) { }

        protected override byte[] ToBytes() {
            return new byte[0];
        }

    }
}
