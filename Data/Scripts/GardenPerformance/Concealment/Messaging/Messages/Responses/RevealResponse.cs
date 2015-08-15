using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GP.Concealment.Messaging.Messages.Responses {
    class RevealResponse : Response {

        protected override ushort TypeId {
            get { return (ushort)MessageType.RevealRequest; }
        }

        public static RevealResponse FromBytes(byte[] bytes) {
            return new RevealResponse();
        }

        protected override byte[] ToBytes() {
            //throw new NotImplementedException();
            return new byte[0];
        }

    }
}
