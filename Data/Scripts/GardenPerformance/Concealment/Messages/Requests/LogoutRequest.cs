using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class LogoutRequest : Request {

        public static LogoutRequest FromBytes(byte[] bytes) {
            return new LogoutRequest();
        }

        public LogoutRequest() :
            base((ushort)MessageType.LogoutRequest) { }


        protected override byte[] ToBytes() {
            return new byte[0];
        }


    }
}
