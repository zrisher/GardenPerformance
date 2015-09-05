using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messages {

    public enum MessageType : ushort {
        RequestBase,
        ResponseBase,
        ChangeSettingRequest,
        ChangeSettingResponse,
        ConcealRequest,
        ConcealResponse,
        ConcealedGridsRequest,
        ConcealedGridsResponse,
        LoginRequest,
        LogoutRequest,
        FactionChangeRequest,
        ObservingEntitiesRequest,
        ObservingEntitiesResponse,
        RevealRequest,
        RevealResponse,
        RevealedGridsRequest,
        RevealedGridsResponse,
        SettingsRequest,
        SettingsResponse,
        StatusResponse,
    }

}
