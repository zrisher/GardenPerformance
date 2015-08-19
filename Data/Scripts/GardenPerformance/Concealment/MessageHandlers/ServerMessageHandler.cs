using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Logging;
using SEGarden.Logic;

using GP.Concealment.Sessions;
using GP.Concealment.Messages;
using GP.Concealment.Messages.Requests;
using GP.Concealment.Messages.Responses;

using SEGarden.Messaging;

namespace GP.Concealment.MessageHandlers {

    class ServerMessageHandler : MessageHandlerBase {

        private static Logger Log = 
            new Logger("GP.Concealment.Messaging.Handlers.ServerMessageHandler");

        public bool Disabled = false;

        public ServerMessageHandler() : base((ushort)MessageDomain.ConcealServer) { }

        public override void HandleMessage(ushort messageTypeId, byte[] body,
            ulong senderSteamId, RunLocation sourceType) {

            Log.Trace("Received message typeId " + messageTypeId, "HandleMessage");
            MessageType messageType = (MessageType)messageTypeId;
            Log.Trace("Received message type " + messageType, "HandleMessage");

            /*
            if (Disabled) {
                SendDisabledNotice(senderSteamId);
                return;
            }


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
             * */

        }

        /*
        private void ReplyToConcealedGridsRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Concealed Grids Request", 
                "ReceiveConcealedGridsRequest");

            Log.Trace("Deserializing request", "ReceiveConcealedGridsRequest");
            // nothing to read, but doing this anyway to test
            ConcealedGridsRequest request = ConcealedGridsRequest.FromBytes(body);

            Log.Trace("Preparing response", "ReceiveConcealedGridsRequest");
            ConcealedGridsResponse response = new ConcealedGridsResponse {
                ConcealedGrids = Session.Server.ConcealedSector0.ConcealedGridsList()
            };

            Log.Trace("Sending to player", "ReceiveConcealedGridsRequest");
            response.SendToPlayer(senderId);
        }

        private void ReceiveRevealedGridsRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Revealed Grids Request",
                "ReceiveRevealedGridsRequest");

            // nothing to read, but doing this anyway to test
            RevealedGridsRequest request = RevealedGridsRequest.FromBytes(body);

            RevealedGridsResponse response = new RevealedGridsResponse() {
                RevealedGrids = Session.Server.RevealedSector0.RevealedGridsList()
            };

            response.SendToPlayer(senderId);
        }

        private void ReceiveConcealRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Conceal Request", "ReceiveConcealRequest");

            ConcealRequest request = ConcealRequest.FromBytes(body);
            bool success = false;

            if (Session.Server.CanConceal(request.EntityId)) {
                success = Session.Server.QueueConceal(request.EntityId);
            }

            ConcealResponse response = new ConcealResponse() {
                EntityId = request.EntityId,
                Success = success
            };

            response.SendToPlayer(senderId);

        }

        private void ReceiveRevealRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Reveal Request", "ReceiveRevealRequest");

            RevealRequest request = RevealRequest.FromBytes(body);
            bool success = false;

            if (Session.Server.QueueReveal(request.EntityId)) {
                success = true;
                Log.Trace("Successfully revealed", "ReceiveRevealRequest");
            }

            RevealResponse response = new RevealResponse() {
                EntityId = request.EntityId,
                Success = success
            };

            Log.Trace("Sending response success ? " + response.Success, "ReceiveRevealRequest");

            response.SendToPlayer(senderId);
        }

        private void SendDisabledNotice(ulong senderId) {
            StatusResponse response = new StatusResponse() {
                ServerRunning = false
            };

            response.SendToPlayer(senderId);
        }

         * */
    }
}
