using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions;

namespace GP.Concealment.Messages.Requests {
    class ChangeSettingRequest : Request {

        private static readonly int SIZE = Request.SIZE + sizeof(byte) + sizeof(ushort);

        public static ChangeSettingRequest FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);

            ChangeSettingRequest request = new ChangeSettingRequest();
            request.Index = stream.getByte();
            request.Value = stream.getUShort();

            return request;
        }

        public byte Index;
        public ushort Value;

        public ChangeSettingRequest() :
            base((ushort)MessageType.ChangeSettingRequest) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(SIZE);
            stream.addByte(Index);
            stream.addUShort(Value);
            return stream.Data;
        }

    }
}
