using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;

using GP.Concealment.Sessions;
using GP.Concealment.Messaging.Messages.Requests;
using GP.Concealment.Messaging.Messages.Responses;

namespace GP.Concealment.Messaging.Handlers {

    class ServerMessageHandler : SEGarden.Messaging.MessageHandlerBase {

        private static Logger Log = 
            new Logger("GP.Concealment.Messaging.Handlers.ServerMessageHandler");

        public ServerMessageHandler() : base((ushort)MessageDomain.ConcealServer) { }

        public override void HandleMessage(ushort messageTypeId, byte[] body,
            ulong senderSteamId, SEGarden.Logic.Common.RunLocation sourceType) {

            Log.Trace("Received message typeId " + messageTypeId, "HandleMessage");
            MessageType messageType = (MessageType)messageTypeId;
            Log.Trace("Received message type " + messageType, "HandleMessage");

            switch (messageType) {
                case MessageType.ConcealedGridsRequest:
                    ReplyToConcealedGridsRequest(body, senderSteamId);
                    break;
                case MessageType.ConcealRequest:
                    ReceiveConcealRequest(body, senderSteamId);
                    break;
                case MessageType.RevealedGridsRequest:
                    ReceiveRevealedGridsRequest(body, senderSteamId);
                    break;
                case MessageType.RevealRequest:
                    ReceiveRevealRequest(body, senderSteamId);
                    break;
            }

        }

        private void ReplyToConcealedGridsRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Concealed Grids Request", 
                "ReceiveConcealedGridsRequest");

            // nothing to read, but doing this anyway to test
            ConcealedGridsRequest request = ConcealedGridsRequest.FromBytes(body);


            ConcealedGridsResponse response = new ConcealedGridsResponse {
                // TODO: add server's concealed grids
            };
            response.SendToPlayer(senderId);
        }

        private void ReceiveRevealedGridsRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Revealed Grids Request",
                "ReceiveRevealedGridsRequest");

            // nothing to read, but doing this anyway to test
            RevealedGridsRequest request = RevealedGridsRequest.FromBytes(body);

            RevealedGridsResponse response = new RevealedGridsResponse() {
                // TODO: add server's revealed grids
            };

            response.SendToPlayer(senderId);
        }

        private void ReceiveConcealRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Conceal Request", "ReceiveConcealRequest");

            ConcealRequest request = ConcealRequest.FromBytes(body);

            ConcealResponse response = new ConcealResponse() {
                // TODO: tell them if we are going to conceal it
            };

            response.SendToPlayer(senderId);

        }

        private void ReceiveRevealRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Reveal Request", "ReceiveRevealRequest");

            RevealRequest request = RevealRequest.FromBytes(body);

            RevealResponse response = new RevealResponse() {
                // TODO: tell them if we are going to reveal it
            };

            response.SendToPlayer(senderId);

        }

    }
}
