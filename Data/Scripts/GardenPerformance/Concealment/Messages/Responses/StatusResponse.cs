using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Responses {
    class StatusResponse : Response {

        public StatusResponse() :
            base((ushort)MessageType.StatusResponse) { }

        public static StatusResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            StatusResponse response = new StatusResponse();
            response.LoadFromByteStream(stream);
            return response;
        }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            base.AddToByteStream(stream);
            return stream.Data;
        }

    }
}
