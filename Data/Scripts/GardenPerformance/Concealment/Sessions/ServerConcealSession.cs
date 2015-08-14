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

using GardenPerformance.Concealment;
using GardenPerformance.Concealment.Common;
using GardenPerformance.Concealment.Records;
using GardenPerformance.Concealment.Records.Entities;
using GardenPerformance.Concealment.Messaging.Messengers;

namespace GardenPerformance.Concealment.Sessions {

    class ServerConcealSession {

        private static Logger Log =
            new Logger("GardenPerformance.Concealment.Sessions.ServerConcealSession");

        private const ushort CONCEAL_MESSAGE_ID = 4747;

        private String WorldName;
        private ConcealableSector Sector;
        private ServerMessenger Messenger = new ServerMessenger(CONCEAL_MESSAGE_ID);

        public void Initialize() {
            //DebugWorldNames();

            // TODO: Load via static method and stop init if fail
            Sector = new ConcealableSector(
                MyAPIGateway.Session.Name,
                MyAPIGateway.Session.GetWorld().Sector.Position
                );
            //Sector.Load();

            MyAPIGateway.Entities.OnEntityAdd += Sector.EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += Sector.EntityRemoved;
        }

        public void Terminate() {
            //Sector.Save();
            MyAPIGateway.Entities.OnEntityAdd -= Sector.EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove -= Sector.EntityRemoved;
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
