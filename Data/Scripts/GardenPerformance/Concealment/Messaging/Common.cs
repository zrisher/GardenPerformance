using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance.Concealment.Messaging {

    public enum MessageType : ushort {
        ConcealRequest,
        ConcealRequestResponse,
        RevealRequest,
        RevealRequestResponse
    }

    public class MessageDomains {
        public static readonly ushort ConcealClient = 14747;
        public static readonly ushort ConcealServer = 14748;
        public static readonly ushort ConcealExternal = 4747;
    }

    class Common {

        public static ushort MessageId = 13838;
        public static ushort PublicMessageId = 14747;


    }
}
