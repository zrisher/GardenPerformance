using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GP.Concealment.Messaging.Messages.Responses {
    class ConcealResponse : Response {

        protected override ushort TypeId {
            get { return (ushort)MessageType.ConcealResponse; }
        }

        public static ConcealResponse FromBytes(byte[] bytes) {
            return new ConcealResponse();
        }

        protected override byte[] ToBytes() {
            //throw new NotImplementedException();
            return new byte[0];
        }

    }
}
