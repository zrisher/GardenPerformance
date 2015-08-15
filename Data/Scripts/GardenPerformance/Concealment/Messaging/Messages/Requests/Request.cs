using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messaging.Messages.Requests {
    abstract class Request : SEGarden.Messaging.MessageBase {

        public Request(ushort typeId) : 
            base((ushort)MessageDomain.ConcealServer, typeId) { }

    }
}

