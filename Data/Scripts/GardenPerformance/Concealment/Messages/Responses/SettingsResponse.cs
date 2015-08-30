using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;
using SEGarden.Extensions;

using GP.Concealment.World.Entities;

namespace GP.Concealment.Messages.Responses {
    class SettingsResponse : Response {

        private static Logger Log =
            new Logger("GP.Concealment.Messaging.Messages.Responses.SettingsResponse");

        public static SettingsResponse FromBytes(byte[] bytes) {
            VRage.ByteStream stream = new VRage.ByteStream(bytes, bytes.Length);
            SettingsResponse response = new SettingsResponse();
            response.LoadFromByteStream(stream);
            response.Settings = new Settings(stream);
            return response;
        }

        public Settings Settings = new Settings();

        public SettingsResponse() :
            base((ushort)MessageType.SettingsResponse) { }

        protected override byte[] ToBytes() {
            VRage.ByteStream stream = new VRage.ByteStream(32, true);
            base.AddToByteStream(stream);
            Settings.AddToByteStream(stream);
            return stream.Data;
        }

    }
}
