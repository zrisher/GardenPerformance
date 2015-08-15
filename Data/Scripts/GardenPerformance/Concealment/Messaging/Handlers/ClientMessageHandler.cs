using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance.Concealment.Messaging.Handlers {
    class ClientMessageHandler : SEGarden.Messaging.MessageHandlerBase {

        public ClientMessageHandler() : base(MessageDomains.ConcealClient) { }

        public override void HandleMessage(ushort MessageTypeId, byte[] body,
long senderId, SEGarden.Logic.Common.RunLocation sourceType) {

        }

    }
}
