using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Extensions.VRageMath;
using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Messaging;
using SEGarden.Notifications;

using GP.Concealment.Messages;
using GP.Concealment.Messages.Responses;
using GP.Concealment.Sessions;
using GP.Concealment.World.Entities;





namespace GP.Concealment.MessageHandlers {
    class ClientMessageHandler : MessageHandlerBase {

        private static Logger Log =
            new Logger("GP.Concealment.Messaging.Handlers.ClientMessageHandler");

        private static ClientConcealSession Session {
            get { return ClientConcealSession.Instance; }
        }

        public ClientMessageHandler() : base((ushort)MessageDomain.ConcealClient) { }

        public override void HandleMessage(ushort messageTypeId, byte[] body,
            ulong senderSteamId, RunLocation sourceType) {

            Log.Trace("Received message typeId " + messageTypeId, "HandleMessage");
            MessageType messageType = (MessageType)messageTypeId;
            Log.Trace("Received message type " + messageType, "HandleMessage");

            switch (messageType) {
                case MessageType.ConcealedGridsResponse:
                    ReceiveConcealedGridsResponse(body);
                    break;
                case MessageType.ConcealResponse:
                    ReceiveConcealResponse(body);
                    break;
                case MessageType.RevealedGridsResponse:
                    ReceiveRevealedGridsResponse(body);
                    break;
                case MessageType.RevealResponse:
                    ReceiveRevealResponse(body);
                    break;
                case MessageType.StatusResponse:
                    ReceiveStatusResponse(body);
                    break;
                case MessageType.SettingsResponse:
                    ReceiveSettingsResponse(body);
                    break;
                case MessageType.ChangeSettingResponse:
                    ReceiveChangeSettingResponse(body);
                    break;
                case MessageType.ObservingEntitiesResponse:
                    ReceiveObservingEntitiesResponse(body);
                    break;
            }

        }

        private void ReceiveConcealedGridsResponse(byte[] body) {
            Log.Trace("Receiving Concealed Grids Response", 
                "ReceiveConcealedGridsResponse");

            ConcealedGridsResponse response = ConcealedGridsResponse.FromBytes(body);

            Session.ConcealedGrids = response.ConcealedGrids;
            String result = Session.ConcealedGrids.Count + " Concealed Grids:\n\n";

            int i = 1;
            foreach (World.Entities.ConcealedGrid grid in Session.ConcealedGrids) {
                result += String.Format("{0}: \"{1}\" - Revealability: {2}\n", 
                    i, grid.DisplayName, grid.IsRevealable);
                i++;
            }

            Notification notice = new WindowNotification() {
                Text = result,
                BigLabel = "Garden Performance",
                SmallLabel = "Concealed Grids"
            };

            notice.Raise();

        }

        private void ReceiveRevealedGridsResponse(byte[] body) {
            Log.Trace("Receiving Revealed Grids Response",
                "ReceiveRevealedGridsResponse");

            RevealedGridsResponse response = RevealedGridsResponse.FromBytes(body);

            Session.RevealedGrids = response.RevealedGrids;

            String result = Session.RevealedGrids.Count + " Revealed Grids:\n\n";

            int i = 1;
            foreach (RevealedGrid grid in Session.RevealedGrids) {

                // Ids
                result += i + ": \"" + grid.ConcealDetails() +  "\n";


                i++;
            }

            Notification notice = new WindowNotification() {
                Text = result,
                BigLabel = "Garden Performance",
                SmallLabel = "Revealed Grids"
            };

            notice.Raise();
        }

        private void ReceiveConcealResponse(byte[] body) {
            Log.Trace("Receiving Conceal Response", "ReceiveConcealResponse");

            ConcealResponse response = ConcealResponse.FromBytes(body);

            String result = response.Success ? 
                "Successfully concealed" :
                "Failed to conceal";

            result += " grid " + response.EntityId;

            Notification notice = new ChatNotification() {
                Text = result,
                Sender = "GP"
            };

            notice.Raise();
        }

        private void ReceiveRevealResponse(byte[] body) {
            Log.Trace("Receiving Reveal Response", "ReceiveRevealResponse");

            RevealResponse response = RevealResponse.FromBytes(body);

            String result = response.Success ?
                "Successfully revealed" :
                "Failed to reveal";

            result += " grid " + response.EntityId;

            Notification notice = new ChatNotification() {
                Text = result,
                Sender = "GP"
            };

            notice.Raise();
        }

        private void ReceiveStatusResponse(byte[] body) {
            Log.Trace("Receiving Reveal Response", "ReceiveRevealResponse");

            StatusResponse response = StatusResponse.FromBytes(body);

            String result = "Server is ";
            result += response.ServerRunning ? "Running." : "Terminated.";

            Notification notice = new ChatNotification() {
                Text = result,
                Sender = "GP"
            };

            notice.Raise();
        }

        private void ReceiveObservingEntitiesResponse(byte[] body) {
            Log.Trace("Receiving Observing Entities Response",
                "ReceiveObservingEntitiesResponse");

            ObservingEntitiesResponse response = ObservingEntitiesResponse.FromBytes(body);

            Session.ObservingEntities = response.ObservingEntities;

            String result = Session.ObservingEntities.Count + " Observing Entities:\n\n";

            int i = 1;
            foreach (ObservingEntity e in Session.ObservingEntities) {

                // Ids
                result += i + ": \"" + e.Details() + "\n";

                i++;
            }

            Notification notice = new WindowNotification() {
                Text = result,
                BigLabel = "Garden Performance",
                SmallLabel = "Observing Entities"
            };

            notice.Raise();
        }

        private void ReceiveSettingsResponse(byte[] body) {
            Log.Trace("Receiving Observing Entities Response",
                "ReceiveObservingEntitiesResponse");

            SettingsResponse response = SettingsResponse.FromBytes(body);

            Session.Settings = response.Settings;

            Notification notice = new WindowNotification() {
                Text = Session.Settings.ToString(),
                BigLabel = "Garden Performance",
                SmallLabel = "Settings"
            };

            notice.Raise();
        }

        private void ReceiveChangeSettingResponse(byte[] body) {
            Log.Trace("Receiving Reveal Response", "ReceiveRevealResponse");

            ChangeSettingResponse response = ChangeSettingResponse.FromBytes(body);

            String result = response.Success ?
                "Successfully changed setting" :
                "Failed to change setting (not yet implemented)";

            Notification notice = new ChatNotification() {
                Text = result,
                Sender = "GP"
            };

            notice.Raise();
        }

    }
}
