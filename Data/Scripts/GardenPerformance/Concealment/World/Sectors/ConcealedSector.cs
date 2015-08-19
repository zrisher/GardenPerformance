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

using GP.Concealment.World.Entities;

namespace GP.Concealment.World.Sectors {

    ///<summary>
    /// Holds all info about the currently concealed sector
    /// Handles revealing requested grids and offers public methods to mark and
    /// inspect the concealed ones
    ///</summary>
    ///<remarks>
    /// This relies on the ConcealedSectorState object to do most of the heavy lifting
    /// wrt saving and loading.
    ///</remarks>
    public class ConcealedSector {

        #region Static

        private static Logger Log =
            new Logger("GP.Concealment.World.Sectors.ConcealableSector");

        #endregion
        #region Instance Fields

        private ConcealedSectorState State;

        private bool NeedsSave = false;

        private Dictionary<long, ConcealableGrid> Grids =
            new Dictionary<long, ConcealableGrid>();

        private Dictionary<long, MyObjectBuilder_CubeGrid> GridBuilders =
            new Dictionary<long, MyObjectBuilder_CubeGrid>();

        // uuuuggh MyConstants.GAME_PRUNING_STRUCTURE_AABB_EXTENSION not allowed in script
        private MyDynamicAABBTreeD GridTree = new MyDynamicAABBTreeD(new Vector3D(3.0f));

        private Queue<ConcealableGrid> GridRevealQueue = new Queue<ConcealableGrid>();
        private Queue<ConcealableGrid> GridConcealQueue = new Queue<ConcealableGrid>();

        #endregion
        #region Instance Properties

        public bool Loaded { get; private set; }

        public List<ConcealableGrid> ConcealedGridsList() {
            return Grids.Select((x) => x.Value).ToList();
        }

        private List<MyObjectBuilder_CubeGrid> ConcealedGridBuildersList() {
            return GridBuilders.Select((x) => x.Value).ToList();
        }

        #endregion
        #region Save / Load

        public void Save() {
            State.ConcealedGridBuilders = ConcealedGridBuildersList();
            State.ConcealedGrids = ConcealedGridsList();
            State.Save();
        }

        public void Load(String worldName, Vector3D sectorPos) {
            Log.Info("Attempting to load", "Load");
            Loaded = false;

            State = ConcealedSectorState.Load(worldName, sectorPos);
            if (State == null) return;

            // Load concealed grids
            Grids = State.ConcealedGrids.ToDictionary(x => x.EntityId, x => x);

            GridBuilders = State.ConcealedGridBuilders.
                ToDictionary(x => x.EntityId, x => x);

            // TODO: validate concealed grids vs concealed builders

            Log.Trace("Loading AABB Tree", "LoadState");
            foreach (ConcealableGrid grid in State.ConcealedGrids) {
                // TODO: Add grid to tree
                // We might need to store details on the AABB box in the concealed
                // grid. That could be helpful for getting sizing anyway for
                // radar detection range.
            }
            Log.Trace("Finished Loading AABB Tree", "LoadState");

            NeedsSave = false;
            Loaded = true;
        }

        #endregion
        #region Updates

        public void Update100() {
            if (GridRevealQueue.Count > 0) {
                RevealGrid(GridRevealQueue.Dequeue());
            }
        }

        public void Update1000() {
            if (GridConcealQueue.Count > 0) {
                RevealGrid(GridRevealQueue.Dequeue());
            }
        }

        #endregion
        #region Conceal/Reveal

        /// <summary>
        /// Helper method for testing
        /// </summary>
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

            ConcealableGrid concealable = new ConcealableGrid();
            concealable.LoadFromCubeGrid(grid);
            concealable.Concealability = EntityConcealability.Concealable;
            RequestConcealGrid(concealable);
            return true;
        }

        public void RequestConcealGrid(ConcealableGrid grid) {
            if (grid.Concealability != EntityConcealability.Concealable) {
                Log.Warning("Requested conceal on non-concealable grid",
                    "RequestConcealGrid");
                return;
            }

            GridConcealQueue.Enqueue(grid);
            Grids.Remove(grid.EntityId);
        }

        public void RequestRevealGrid(ConcealableGrid concealable) {

        }

        private bool ConcealGrid(ConcealableGrid concealableGrid) {
            Log.Trace("Concealing entity " + concealableGrid.EntityId, "ConcealEntity");

            IMyCubeGrid grid = concealableGrid.IngameGrid;

            if (grid == null) {
                Log.Error("Stored cubegrid reference is null, aborting", "ConcealEntity");
                return false;
            }

            if (grid.SyncObject == null) {
                Log.Error("SyncObject missing, aborting", "ConcealEntity");
                return false;
            }

            // Refresh the info before saving
            concealableGrid.LoadFromCubeGrid(grid);

            /*
            if (!concealableGrid.Saveable()) {
                Log.Error("Won't be able to save this grid, aborting conceal.",
                    "ConcealEntity");
                return false;
            }
            */

            // Track it
            if (Grids.ContainsKey(grid.EntityId)) {
                Log.Error("Attempting to store already-stored entity id " +
                    grid.EntityId, "ConcealEntity");
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
        }

        private bool RevealGrid(ConcealableGrid concealableGrid) {
            Log.Error("Revealing entity", "RevealEntity");

            long entityId = concealableGrid.EntityId;

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
        }

        #endregion
        #region Marking

        public void MarkAllConcealable() {
            foreach (var kvp in Grids) {
                kvp.Value.Concealability = EntityConcealability.Concealable;
            }
        }

        public void MarkConcealabilityNear(Vector3D center, int meters, EntityConcealability flag) {
            // TODO: Use AABB tree to mark all grids within meters of center for reason
        }

        #endregion

    }

}
