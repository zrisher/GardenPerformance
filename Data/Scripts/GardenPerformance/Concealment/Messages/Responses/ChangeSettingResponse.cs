using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Responses {
    class ChangeSettingResponse : Response {

        private static readonly int SIZE = Response.SIZE + sizeof(bool);

        public ChangeSettingResponse() :
            base((ushort)MessageType.ChangeSettingResponse) { }

        public static ChangeSettingResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            ChangeSettingResponse response = new ChangeSettingResponse();
            response.LoadFromByteStream(stream);
            response.Success = stream.getBoolean();
            return response;
        }

        public bool Success;

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            base.AddToByteStream(stream);
            stream.addBoolean(Success);
            return stream.Data;
        }

    }
}
