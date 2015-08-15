using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance.Concealment.Messaging.Handlers {
    class ServerMessageHandler : SEGarden.Messaging.MessageHandlerBase {

        public ServerMessageHandler() : base(MessageDomains.ConcealServer) { }

        public override void HandleMessage(ushort MessageTypeId, byte[] body,
    long senderId, SEGarden.Logic.Common.RunLocation sourceType) {

        }

    }
}
