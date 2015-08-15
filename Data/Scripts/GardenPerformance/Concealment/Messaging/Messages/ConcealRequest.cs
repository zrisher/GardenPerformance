using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GardenPerformance.Concealment.Messaging.Messages {
    class ConcealRequest : SEGarden.Messaging.MessageBase {

        protected override ushort TypeId {
            get { return (ushort)MessageType.ConcealRequest; } 
        }

        protected override ushort DomainId {
            get { return MessageDomains.ConcealServer; }
        }

        public static ConcealRequest FromBytes(byte[] bytes) {
            return new ConcealRequest();
        }

        protected override byte[] ToBytes() {
            //throw new NotImplementedException();
            return new byte[0];
        }

    }
}
