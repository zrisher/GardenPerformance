using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.ModAPI;
using VRage.Library.Utils;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using Interfaces = Sandbox.ModAPI.Interfaces;
using InGame = Sandbox.ModAPI.Ingame;

using VRageMath;

using SEGarden;
using SEGarden.Chat;
using SEGarden.Logging;
using Commands = SEGarden.Chat.Commands;
using SEGarden.Notifications;
using SEGarden.Extensions;

using SEGarden.Logging;
using SEGarden.Math;

using GP.Concealment.World.Entities;
using GP.Concealment.World.Sectors;

namespace GP.Concealment {

    ///<summary>
    /// Holds all info about the currently concealed sector
    /// Handles revealing requested grids and offers public methods to mark and
    /// inspect the concealed ones
    ///</summary>
    ///<remarks>
    /// This relies on the ConcealedSectorState object to do most of the heavy lifting
    /// wrt saving and loading.
    ///</remarks>
    public class ConcealmentManager {

        #region Static

        private static Logger Log = new Logger("GP.Concealment.ConcealmentManager");

        #endregion
        #region Instance Fields

        private String WorldName;

        // eventually multiple sectors
        private Vector3D SectorPosition; 
        public ConcealedSector Concealed;
        public RevealedSector Revealed;

        private Queue<ConcealedGrid> GridRevealQueue = new Queue<ConcealedGrid>();
        private Queue<RevealedGrid> GridConcealQueue = new Queue<RevealedGrid>();

        #endregion
        #region Instance Properties

        public bool Loaded { get; private set; }

        #endregion
        #region Constructor

        public ConcealmentManager() {}

        #endregion
        #region Initialize/Terminate

        public void Initialize() {
            Log.Trace("Initializing ConcealmentManager", "Initialize");
            WorldName = MyAPIGateway.Session.Name;
            SectorPosition = MyAPIGateway.Session.GetWorld().Sector.Position;
            Concealed = ConcealedSector.LoadOrNew(WorldName, SectorPosition);

            Revealed = new RevealedSector();
            ControllableEntity.ControllableEntityAdded += Revealed.ControllableEntityAdded;
            ControllableEntity.ControllableEntityMoved += Revealed.ControllableEntityMoved;
            ControllableEntity.ControllableEntityRemoved += Revealed.ControllableEntityRemoved;
            ControllableEntity.ControlAcquired += Revealed.ControllableEntityControlled;
            ControllableEntity.ControlReleased += Revealed.ControllableEntityReleased;

            if (Concealed != null) Loaded = true;
            Log.Trace("Done Initializing ConcealmentManager", "Initialize");
        }

        public void Terminate() {
            Log.Trace("Terminating ConcealmentManager", "Terminate");
            if (Loaded) Concealed.Save();

            ControllableEntity.ControllableEntityAdded -= Revealed.ControllableEntityAdded;
            ControllableEntity.ControllableEntityMoved -= Revealed.ControllableEntityMoved;
            ControllableEntity.ControllableEntityRemoved -= Revealed.ControllableEntityRemoved;
            ControllableEntity.ControlAcquired -= Revealed.ControllableEntityControlled;
            ControllableEntity.ControlReleased -= Revealed.ControllableEntityReleased;

            Log.Trace("Done Terminating ConcealmentManager", "Terminate");
        }

        #endregion
        #region Conceal/Reveal requests


        public bool RequestConcealGrid(long entityId) {
            Log.Trace("Concealing entity " + entityId, "ConcealEntity");

            IMyEntity entity = null;
            IMyCubeGrid grid = null;
            MyAPIGateway.Entities.TryGetEntityById(entityId, out entity);
            grid = entity as IMyCubeGrid;
            if (grid == null) {
                // log
                return false;
            }

            ConcealedGrid concealable = new ConcealedGrid();
            concealable.LoadFromCubeGrid(grid);
            concealable.Concealability = EntityConcealability.Concealable;
            RequestConcealGrid(concealable);
            return true;
        }

        public void RequestConcealGrid(ConcealedGrid grid) {
            /*
            if (grid.Concealability != EntityConcealability.Concealable) {
                Log.Warning("Requested conceal on non-concealable grid",
                    "RequestConcealGrid");
                return;
            }

            //GridConcealQueue.Enqueue(grid);
            Grids.Remove(grid.EntityId);
            */ 
        }

        public bool RequestRevealGrid(long entityId) {
            return false;
        }

        public bool QueueConceal(long entityId) {
            // TODO: wait to conceal after notifying other mods for a few frames
            //ConcealEntity(entity);
            return false; //ConcealedSector0.RequestConcealGrid(entityId);
        }

        // can occur from messaging and entity hooks so safed
        // provide instruction queue for managed resources and do their updates from here
        // keeps them a lot simpler and makes more sense to do that stuff in the session

        /// <summary>
        /// TODO: Actually queue, just like conceal
        /// </summary>
        /// <param name="entity"></param>
        public bool QueueReveal(long entityId) {
            //revealEntity(entity);
            return false; //ConcealedSector0.RequestRevealGrid(entityId);
        }

        public bool CanConceal(long entityId) {
            return true;
        }

        public bool CanReveal(long entityId) {
            return true;
        }

        private static void QueueSave() {
            // TODO: Set a flag to do this during update instead to multiple
            // Want to wait a while too, this flag gets hit every conceal
            //Save();
        }

        #endregion
        #region Conceal/Reveal process

        public void ProcessConcealQueue() {

        }

        public void ProcessRevealQueue() {

        }

        private bool ConcealGrid(IMyCubeGrid grid) {
            /*
            Log.Trace("Concealing grid " + grid.EntityId, "ConcealGrid");

            ConcealedGrid concealableGrid = new ConcealedGrid();

            if (grid == null) {
                Log.Error("Stored cubegrid reference is null, aborting", "ConcealGrid");
                return false;
            }

            if (grid.SyncObject == null) {
                Log.Error("SyncObject missing, aborting", "ConcealGrid");
                return false;
            }

            // Refresh the info before saving
            concealableGrid.LoadFromCubeGrid(grid);

            *//*
            if (!concealableGrid.Saveable()) {
                Log.Error("Won't be able to save this grid, aborting conceal.",
                    "ConcealEntity");
                return false;
            }
            *//*

            // Track it
            if (Grids.ContainsKey(grid.EntityId)) {
                Log.Error("Attempting to store already-stored entity id " +
                    grid.EntityId, "ConcealGrid");
                return false;
            }

            Grids.Add(concealableGrid.EntityId, concealableGrid);
            GridBuilders.Add(concealableGrid.EntityId, 
                grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid);
            // TODO: Add to AABB Tree
            // TODO: Combine this into a function to share with load

            // Remove it from the world
            grid.SyncObject.SendCloseRequest();

            NeedsSave = true;
            return true;
            */
            return false;
        }

        private bool RevealGrid(long entityId) {
            /*
            Log.Error("Revealing entity", "RevealEntity");

            ConcealedGrid concealableGrid;

            // === Get stored concealed grid

            if (Grids.ContainsKey(entityId)) {
                concealableGrid = Grids[entityId];
            }

            if (concealableGrid == null) {
                Log.Error("Failed to find grid, aborting", "ConcealEntity");
                return false;
            }

            // === Get stored builder

            MyObjectBuilder_CubeGrid builder = null;

            if (GridBuilders.ContainsKey(entityId)) {
                builder = GridBuilders[entityId];
            }

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + concealableGrid.EntityId +
                    ", aborting", "RevealEntity");
                return false;
            }

            // Reallocate ID if necessary
            if (MyAPIGateway.Entities.EntityExists(concealableGrid.EntityId)) {
                concealableGrid.EntityId = 0;
                Log.Trace("Reallocating entityId", "revealEntity");
            }

            // === Add it back to game world
            Log.Trace("Adding entity back into game from builder. " +
                concealableGrid.EntityId, "revealEntity");
            MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(builder);
            Log.Trace("Created object", "revealEntity");

            // === Update lists
            Grids.Remove(concealableGrid.EntityId);
            GridBuilders.Remove(concealableGrid.EntityId);

            Log.Trace("End reveal " + concealableGrid.EntityId, "revealEntity");
            return true;
            */
            return false;
        }

        #endregion
        #region Save

        public void Save() {
            if (Concealed == null) {
                Log.Error("Failed to save concealed sector, not loaded.", "Save");
                return;
            }

            Concealed.Save();
        }

        #endregion

    }

}
