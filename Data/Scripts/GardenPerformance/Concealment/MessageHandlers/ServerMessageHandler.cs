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

        private static ServerConcealSession Session {
            get { return ServerConcealSession.Instance; }
        }

        public bool Disabled = false;

        public ServerMessageHandler() : base((ushort)MessageDomain.ConcealServer) { }

        public override void HandleMessage(ushort messageTypeId, byte[] body,
            ulong senderSteamId, RunLocation sourceType) {

            Log.Trace("Received message typeId " + messageTypeId, "HandleMessage");
            MessageType messageType = (MessageType)messageTypeId;
            Log.Trace("Received message type " + messageType, "HandleMessage");


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
                case MessageType.LoginRequest:
                    ReceiveLoginRequest(body, senderSteamId);
                    break;
                case MessageType.LogoutRequest:
                    ReceiveLogoutRequest(body, senderSteamId);
                    break;
                case MessageType.FactionChangeRequest:
                    ReceiveFactionChangeRequest(body, senderSteamId);
                    break;
                case MessageType.RevealedGridsRequest:
                    ReceiveRevealedGridsRequest(body, senderSteamId);
                    break;
                case MessageType.RevealRequest:
                    ReceiveRevealRequest(body, senderSteamId);
                    break;
                case MessageType.SettingsRequest:
                    ReceiveSettingsRequest(body, senderSteamId);
                    break;
                case MessageType.ChangeSettingRequest:
                    ReceiveChangeSettingRequest(body, senderSteamId);
                    break;
                case MessageType.ObservingEntitiesRequest:
                    ReceiveObservingEntitiesRequest(body, senderSteamId);
                    break;
            }
        }

        private void ReplyToConcealedGridsRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Concealed Grids Request", 
                "ReceiveConcealedGridsRequest");

            Log.Trace("Deserializing request", "ReceiveConcealedGridsRequest");
            // nothing to read, but doing this anyway to test
            ConcealedGridsRequest request = ConcealedGridsRequest.FromBytes(body);

            Log.Trace("Preparing response", "ReceiveConcealedGridsRequest");
            ConcealedGridsResponse response = new ConcealedGridsResponse {
                ConcealedGrids = Session.Manager.Concealed.ConcealedGridsList()
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
                RevealedGrids = Session.Manager.Revealed.RevealedGridsList()
            };

            response.SendToPlayer(senderId);
        }

        private void ReceiveConcealRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Conceal Request", "ReceiveConcealRequest");

            ConcealRequest request = ConcealRequest.FromBytes(body);
            bool success = Session.Manager.QueueConceal(request.EntityId);

            ConcealResponse response = new ConcealResponse() {
                EntityId = request.EntityId,
                Success = success
            };

            response.SendToPlayer(senderId);

        }

        private void ReceiveLoginRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Login Request", "ReceiveLoginRequest");

            LoginRequest request = new LoginRequest(body);
            Session.Manager.Revealed.PlayerLoggedIn(request.PlayerId, request.FactionId);
        }

        private void ReceiveLogoutRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Logout Request", "ReceiveLogoutRequest");
            LogoutRequest request = new LogoutRequest(body);
            Session.Manager.Revealed.PlayerLoggedOut(request.PlayerId, request.FactionId);
        }

        private void ReceiveFactionChangeRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Faction Change Request", "ReceiveFactionChangeRequest");
            FactionChangeRequest request = new FactionChangeRequest(body);
            Session.Manager.Revealed.PlayerChangedFactions(
                request.PlayerId, request.OldFactionId, request.NewFactionId);
        }


        private void ReceiveRevealRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Reveal Request", "ReceiveRevealRequest");

            RevealRequest request = RevealRequest.FromBytes(body);
            bool success = Session.Manager.QueueReveal(request.EntityId);

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

        private void ReceiveObservingEntitiesRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Observing Entities Request",
                "ReceiveObservingEntitiesRequest");

            // nothing to read, but doing this anyway to test
            ObservingEntitiesRequest request = ObservingEntitiesRequest.FromBytes(body);

            // Convert them to just observing form 
            ObservingEntitiesResponse response = new ObservingEntitiesResponse() {
                ObservingEntities = Session.Manager.Revealed.ObservingEntitiesList()
            };

            response.SendToPlayer(senderId);
        }

        private void ReceiveChangeSettingRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Conceal Request", "ReceiveConcealRequest");

            ChangeSettingRequest request = ChangeSettingRequest.FromBytes(body);

            Settings.Instance.ChangeSetting(request.Index, request.Value);

            /*
             * We actually resend settings, incase player is relying on them for data
            ChangeSettingResponse response = new ChangeSettingResponse() {
                Success = true
            };
            */

            SettingsResponse response = new SettingsResponse() {
                Settings = Settings.Instance
            };

            response.SendToPlayer(senderId);
        }

        private void ReceiveSettingsRequest(byte[] body, ulong senderId) {
            Log.Trace("Receiving Conceal Request", "ReceiveConcealRequest");

            SettingsRequest request = SettingsRequest.FromBytes(body);

            SettingsResponse response = new SettingsResponse() {
                Settings = Settings.Instance
            };

            response.SendToPlayer(senderId);
        }

    }

}
