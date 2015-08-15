using System;
using System.Collections.Generic;
using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;

using Sandbox.Common;
//using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
//using Interfaces = Sandbox.ModAPI.Interfaces;
//using InGame = Sandbox.ModAPI.Ingame;


using VRage.Library.Utils;
using VRage.ModAPI;
//using VRage.ObjectBuilders;
//using VRageMath;



using SEGarden;
using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Logic.Common;
//using SEGarden.Notifications;

using GP.Concealment;
using GP.Concealment.Records;
using GP.Concealment.Records.Entities;
using GP.Concealment.Messaging.Handlers;

namespace GP.Concealment.Sessions {

    class ServerConcealSession {

        private static Logger Log =
            new Logger("GardenPerformance.Concealment.Sessions.ServerConcealSession");


        private String WorldName;
        private ConcealableSector Sector;
        private ServerMessageHandler Messenger = new ServerMessageHandler();

        public void Initialize() {
            Log.Trace("Initializing Server Conceal Session", "Initialize");
            //DebugWorldNames();

            // TODO: Load via static method and stop init if fail
            Sector = new ConcealableSector(
                MyAPIGateway.Session.Name,
                MyAPIGateway.Session.GetWorld().Sector.Position
                );
            //Sector.Load();

            MyAPIGateway.Entities.OnEntityAdd += Sector.EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += Sector.EntityRemoved;
            Log.Trace("Finished Initializing Server Conceal Session", "Initialize");
        }

        public void Terminate() {
            Log.Trace("Terminating Server Conceal Session", "Initialize");
            //Sector.Save();
            MyAPIGateway.Entities.OnEntityAdd -= Sector.EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove -= Sector.EntityRemoved;
            Log.Trace("Finished Terminating Server Conceal Session", "Initialize");
        }



        private static void QueueConceal(IMyEntity entity) {
            // TODO: wait to conceal after notifying other mods for a few frames
            //ConcealEntity(entity);
        }



        /// <summary>
        /// TODO: Actually queue, just like conceal
        /// </summary>
        /// <param name="entity"></param>
        public static void QueueReveal(ConcealedEntity entity) {
            //revealEntity(entity);
        }


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
