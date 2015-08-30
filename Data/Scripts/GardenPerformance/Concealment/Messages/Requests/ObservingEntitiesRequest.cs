using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class ObservingEntitiesRequest : Request {

        public static ObservingEntitiesRequest FromBytes(byte[] bytes) {
            return new ObservingEntitiesRequest();
        }

        public ObservingEntitiesRequest() :
            base((ushort)MessageType.ObservingEntitiesRequest) { }

        protected override byte[] ToBytes() {
            return new byte[0];
        }

    }
}
