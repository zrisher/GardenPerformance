using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using SEGarden.Extensions;
using SEGarden.Logging;

namespace GP.Concealment.Messages.Responses {
    abstract class Response : SEGarden.Messaging.MessageBase {

        protected static Logger Log = 
            new Logger("GP.Concealment.Messaging.Messages.Responses.Response");

        public const int SIZE = sizeof(bool);

        public bool ServerRunning = true;

        public Response(ushort typeId = (ushort)MessageType.RequestBase) : 
            base((ushort)MessageDomain.ConcealClient, typeId) { }

        /*
        public Response(VRage.ByteStream stream, ushort typeId = (ushort)MessageType.RequestBase) :
            base((ushort)MessageDomain.ConcealClient, typeId) { }
        */

        protected void AddToByteStream(VRage.ByteStream stream) {
            stream.addBoolean(ServerRunning);
        }

        protected void LoadFromByteStream(VRage.ByteStream stream) {
            ServerRunning = stream.getBoolean();
        }

    }
}

