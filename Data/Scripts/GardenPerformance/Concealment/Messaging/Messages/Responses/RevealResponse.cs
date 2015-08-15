using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GP.Concealment.Messaging.Messages.Responses {
    class RevealResponse : Response {

        //private const int Size = sizeof(long);

        public RevealResponse() :
            base((ushort)MessageType.RevealResponse) { }

        public static RevealResponse FromBytes(byte[] bytes) {
            return new RevealResponse();
        }

        protected override byte[] ToBytes() {
            //throw new NotImplementedException();
            return new byte[0];
        }

    }
}
