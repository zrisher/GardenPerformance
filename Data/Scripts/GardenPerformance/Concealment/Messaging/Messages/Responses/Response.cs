using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GP.Concealment.Messaging.Messages.Responses {
    abstract class Response : SEGarden.Messaging.MessageBase {

        protected override ushort DomainId {
            get { return MessageDomain.ConcealClient; }
        }

    }
}

