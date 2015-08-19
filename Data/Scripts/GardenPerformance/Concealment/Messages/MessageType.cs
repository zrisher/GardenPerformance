using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messages {

    public enum MessageType : ushort {
        RequestBase,
        ResponseBase,
        ConcealRequest,
        ConcealResponse,
        ConcealedGridsRequest,
        ConcealedGridsResponse,
        RevealRequest,
        RevealResponse,
        RevealedGridsRequest,
        RevealedGridsResponse,
        StatusResponse,
    }

}
