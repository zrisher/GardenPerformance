
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.ObjectBuilders;
using VRage.Library.Utils;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;

using SEGarden;
using SEGarden.Chat;
using SEGarden.Logging;
using Commands = SEGarden.Chat.Commands;
using SEGarden.Notifications;

using GardenPerformance.Concealment;
using GardenPerformance.Concealment.Common;
using GardenPerformance.Concealment.Entities;

//using Sandbox.Common.Components;
//using InGame = Sandbox.ModAPI.Ingame;

using VRage.ModAPI;
using VRageMath;

namespace GardenPerformance {

	public static class Concealer {

        #region Fields

        private static Logger Log = 
            new Logger("GardenPerformance.Components.Concealer");

        private static String WorldName;

        public static ConcealableSector Sector;


        #endregion

        #region Lifecycle

        public static void Initialize() {
            //DebugWorldNames();

            Sector = new ConcealableSector(
                MyAPIGateway.Session.Name,
                MyAPIGateway.Session.GetWorld().Sector.Position
                );

            //Sector.Load();
            MyAPIGateway.Entities.OnEntityAdd += Sector.EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += Sector.EntityRemoved;
        }

        public static void DebugWorldNames(){
            Log.Info("Trying to figure out what to use for name:", "SetSectorFromWorldName");          
            Log.Info("Sector.ToString " + MyAPIGateway.Session.GetWorld().Sector.ToString(), "UpdateWorldName");
            Log.Info("Sector.Position " + MyAPIGateway.Session.GetWorld().Sector.Position, "UpdateWorldName");
            Log.Info("World.ToString" + MyAPIGateway.Session.GetWorld().ToString(), "UpdateWorldName");
            Log.Info("Session.Name " + MyAPIGateway.Session.Name, "UpdateWorldName");
            Log.Info("World.Session.Name  " + MyAPIGateway.Session.GetWorld().Checkpoint.SessionName, "UpdateWorldName");
            Log.Info("Session.ToString " + MyAPIGateway.Session.ToString(), "UpdateWorldName");
            Log.Info("Session.WorkshopID " + MyAPIGateway.Session.WorkshopId, "UpdateWorldName");
            Log.Info("Session.WorkshopID " + MyAPIGateway.Session.WorkshopId, "UpdateWorldName");
        }

        public static void Terminate() {
            //Sector.Save();
            MyAPIGateway.Entities.OnEntityAdd -= Sector.EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove -= Sector.EntityRemoved;
        }

        #endregion
        #region Lists

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

        public static void ReceiveRevealedGrids(List<RevealedGrid> grids){
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

        #endregion
        #region Conceal

        /// <summary>
        /// Conceal request called from outside this class, i.e. a chat command
        /// </summary>
        /// <param name="grid"></param>
        public static bool RequestConceal(long entityId) {
            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);
            
            // TODO: check concealability and pass some sort of info back?
            // or should we just do it since the user is asking?
            QueueConceal(entity);
            return true;
        }

        private static void QueueConceal(IMyEntity entity) {
            // TODO: wait to conceal after notifying other mods for a few frames
            //ConcealEntity(entity);
        }

        #endregion
        #region Reveal

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
             * */
            return true;
        }

        /// <summary>
        /// TODO: Actually queue, just like conceal
        /// </summary>
        /// <param name="entity"></param>
        public static void QueueReveal(ConcealedEntity entity) {
            //revealEntity(entity);
        }


        #endregion
        #region Saving

        private static void QueueSave() {
            // TODO: Set a flag to do this during update instead to multiple
            // Want to wait a while too, this flag gets hit every conceal
            //Save();
        }

        #endregion
        #region Loading

        private static void QueueLoad() {
            // TODO: Set a flag to do this during update instead to multiple
            // Want to wait a while too, this flag gets hit every conceal
            //Save();
        }

        #endregion

    }
}
