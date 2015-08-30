using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messages.Requests {
    abstract class Request : SEGarden.Messaging.MessageBase {

        public static readonly int SIZE = 0;

        public Request(ushort typeId = (ushort)MessageType.RequestBase) : 
            base((ushort)MessageDomain.ConcealServer, typeId) { }

    }
}

