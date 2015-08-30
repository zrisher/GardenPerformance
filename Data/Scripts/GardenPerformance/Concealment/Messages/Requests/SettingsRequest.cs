using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class SettingsRequest : Request {

        public static SettingsRequest FromBytes(byte[] bytes) {
            return new SettingsRequest();
        }

        public SettingsRequest() :
            base((ushort)MessageType.SettingsRequest) { }

        protected override byte[] ToBytes() {
            return new byte[0];
        }

    }
}
