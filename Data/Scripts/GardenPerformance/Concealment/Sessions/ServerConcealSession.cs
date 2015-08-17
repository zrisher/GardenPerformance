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


        public ConcealableSector Sector;
        private String SaveFileName;
        private ServerMessageHandler Messenger;
        private RunStatus Status = RunStatus.NotInitialized;

        public void Initialize() {
            Log.Trace("Initializing Server Conceal Session", "Initialize");

            if (Status == RunStatus.Initialized) {
                Log.Warning("Duplicate initialization attempt, already initialized.", 
                    "Initialize");
                return;
            }

            // === Load Sector

            SaveFileName = ConcealableSector.GenFileName(
                MyAPIGateway.Session.Name,
                MyAPIGateway.Session.GetWorld().Sector.Position);

            if (!GardenGateway.Files.Exists(SaveFileName)) {
                Log.Info("No existing save file, starting fresh.", "Load");
                Sector = new ConcealableSector();
                Sector.FileName = SaveFileName;
                //Sector.WorldName = 
            }
            else {
                Sector = ConcealableSector.Load(SaveFileName);

                if (Sector == null) {
                    Log.Error("Error loading sector! Aborting functionality", "Load");
                    Terminate();
                    return;
                }
            }

            // Add all in-world entities
            Log.Trace("Registering existing entities into sector", "Load");
            HashSet<IMyEntity> allEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(allEntities);
            Sector.AddIngameEntities(allEntities);

            MyAPIGateway.Entities.OnEntityAdd += Sector.AddIngameEntity;
            MyAPIGateway.Entities.OnEntityRemove += Sector.RemoveIngameEntity;

            // === Load Messenger
            Log.Trace("Registering message handler", "Load");
            Messenger = new ServerMessageHandler();

            Status = RunStatus.Initialized;
            Log.Trace("Finished Initializing Server Conceal Session", "Initialize");
        }

        public void Terminate() {
            Log.Trace("Terminating Server Conceal Session", "Initialize");

            if (Status == RunStatus.Terminated) return;

            if (Sector != null) {
                Sector.Save();
                MyAPIGateway.Entities.OnEntityAdd -= Sector.AddIngameEntity;
                MyAPIGateway.Entities.OnEntityRemove -= Sector.RemoveIngameEntity;
            }

            Status = RunStatus.Terminated;
            Log.Trace("Finished Terminating Server Conceal Session", "Initialize");
        }


        public bool QueueConceal(long entityId) {
            // TODO: wait to conceal after notifying other mods for a few frames
            //ConcealEntity(entity);
            return Sector.ConcealEntity(entityId);
        }

        /// <summary>
        /// TODO: Actually queue, just like conceal
        /// </summary>
        /// <param name="entity"></param>
        public bool QueueReveal(long entityId) {
            //revealEntity(entity);
            return Sector.RevealEntity(entityId);
        }

        public bool CanConceal(long entityId) {
            return true;
        }

        public bool CanReveal(long entityId) {
            return true;
        }

        #region Saving

        public void LoadSector() { }

        public void SaveSector() {
            if (Sector != null) Sector.Save();
        }

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
