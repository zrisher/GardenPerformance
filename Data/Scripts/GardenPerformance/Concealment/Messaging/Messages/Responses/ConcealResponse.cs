using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GP.Concealment.Messaging.Messages.Responses {
    class ConcealResponse : Response {

        private const int SizeInBytes = sizeof(long);

        public ConcealResponse() :
            base((ushort)MessageType.ConcealResponse) { }

        public static ConcealResponse FromBytes(byte[] bytes) {
            return new ConcealResponse();
        }

        protected override byte[] ToBytes() {
            //throw new NotImplementedException();
            return new byte[0];
        }

    }
}
