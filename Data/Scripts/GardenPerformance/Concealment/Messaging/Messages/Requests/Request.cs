using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messaging.Messages.Requests {
    abstract class Request : SEGarden.Messaging.MessageBase {

        protected override ushort DomainId {
            get { return MessageDomain.ConcealServer; }
        }

    }
}

