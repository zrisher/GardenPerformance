using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance.Concealment.Messaging.Messengers {
    class ClientMessenger : SEGarden.Messaging.MessengerBase {

        public ClientMessenger(ushort messageId) : base(messageId) { }

        public override void ReceiveBytes(byte[] buffer) {

        }

        protected override void ReceiveMessage(SEGarden.Messaging.MessageBase msg) {

        }

        /*
        public static void RequestConcealedGrids() {
            Log.Info("Requesting Concealed Grids", "RequestConcealedGrids");

            if (Sector == null)
                Log.Info("Whoops, null sector", "RequestConcealedGrids");
            else
                ReceiveConcealedGrids(Sector.ConcealedGrids());
        }

        public static void ReceiveConcealedGrids(List<ConcealedGrid> grids) {
            Log.Info("Received Concealed Grids", "ReceiveConcealedGrids");

            String result = "Concealed Grids:\n\n";

            foreach (ConcealedGrid grid in grids) {
                result += grid.EntityId + "\n";
            }

            Notification notice = new WindowNotification() {
                Text = result,
                BigLabel = "Garden Performance",
                SmallLabel = "Concealed Grids"
            };

            notice.Raise();
        }



        public static void RequestRevealedGrids() {
            Log.Info("Requesting Revealed Grids", "RequestRevealedGrids");

            if (Sector == null)
                Log.Info("Whoops, null sector", "RequestRevealedGrids");
            else
                ReceiveRevealedGrids(Sector.RevealedGrids());
        }

        public static void ReceiveRevealedGrids(List<RevealedGrid> grids) {
            String result = grids.Count + " Revealed Grids:\n\n";
            int i = 0;

            foreach (RevealedGrid grid in grids) {
                i++;
                result += i + " - " + grid.EntityId + "\n";
            }

            Notification notice = new WindowNotification() {
                Text = result,
                BigLabel = "Garden Performance",
                SmallLabel = "Revealed Grids"
            };

            notice.Raise();
        }

        /// <summary>
        /// Conceal request called from outside this class, i.e. a chat command
        /// </summary>
        /// <param name="grid"></param>
        public static bool RequestReveal(long entityId) {
            /*
            if (!ConcealedEntities.ContainsKey(entityId)) {
                // TODO: notify requestor somehow
                return false;
            }

            ConcealedEntity entity = ConcealedEntities[entityId];

            // TODO: check revealability and pass some sort of info back?
            // or should we just do it since the user is asking?
            QueueReveal(entity);
             * *//*
            return true;
        }


        /// <summary>
        /// Conceal request called from outside this class, i.e. a chat command
        /// </summary>
        /// <param name="grid"></param>
        public static bool RequestConceal(long entityId) {
            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);

            // GardenGateway.Messenger.Send(new ConcealRequest());
            // TODO: check concealability and pass some sort of info back?
            // or should we just do it since the user is asking?
            QueueConceal(entity);
            return true;
        }

    */

    }
}
