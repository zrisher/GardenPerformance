using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;

namespace GP.Concealment.Messaging.Messages.Responses {
    abstract class Response : SEGarden.Messaging.MessageBase {

        protected static Logger Log = 
            new Logger("GP.Concealment.Messaging.Messages.Responses.Response");

        public Response(ushort typeId) : 
            base((ushort)MessageDomain.ConcealClient, typeId) { }

    }
}

