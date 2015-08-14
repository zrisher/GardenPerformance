using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance.Concealment.Messaging.Messengers {
    class ServerMessenger : SEGarden.Messaging.MessengerBase {

        public ServerMessenger(ushort MessageId) : base(MessageId) { }

        public override void ReceiveBytes(byte[] buffer) {

        }

        protected override void ReceiveMessage(SEGarden.Messaging.MessageBase msg) {

        }
    }
}
