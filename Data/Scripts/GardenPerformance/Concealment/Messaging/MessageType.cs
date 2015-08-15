using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messaging {

    public enum MessageType : ushort {
        ConcealRequest,
        ConcealResponse,
        ConcealedGridsRequest,
        ConcealedGridsResponse,
        RevealRequest,
        RevealResponse,
        RevealedGridsRequest,
        RevealedGridsResponse,
    }

}
