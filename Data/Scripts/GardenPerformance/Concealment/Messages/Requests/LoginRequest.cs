using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {

    class LoginRequest : Request {

        public static LoginRequest FromBytes(byte[] bytes) {
            return new LoginRequest();
        }

        public LoginRequest() :
            base((ushort)MessageType.LoginRequest) { }


        protected override byte[] ToBytes() {
            return new byte[0];
        }

    }
}
