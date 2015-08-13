using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
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

using SEGarden.Logging;

using GardenPerformance.Concealment.Common;
using GardenPerformance.Concealment.Entities;

namespace GardenPerformance.Concealment {

    public class ConcealableSector {

        #region Subclassses

        public class ConcealableSectorState {
            public String SectorName;
            public String StateFileName;
            public String GridBuildersFileName;
            public Dictionary<long, ConcealedEntity> ConcealedEntities =
                new Dictionary<long, ConcealedEntity>();
        }

        #endregion
        #region Static Fields

        private static readonly String STATE_FILENAME_SUFFIX = "_concealment_state";
        private static readonly String GRIDS_FILENAME_SUFFIX = "_concealed_grids";
        private static readonly String FILE_EXT = ".txt";

        private static Logger Log =
            new Logger("GardenPerformance.Concealment.ConcealableSector");

        #endregion
        #region Static Methods
        
        /*
        public static String FileName(String worldName, Vector3D sectorPos) {
            return worldName + sectorPos.ToString() + STATE_FILENAME_SUFFIX + FILE_EXT;
        }
         * */

        #endregion
        #region Instance Fields

        private ConcealableSectorState State = new ConcealableSectorState();

        // I would love to store the builder within our ConcealEntitiy representations,
        // but that means we have to pass them for external commands, and we can't
        // serialize them together anyway (error when you make an objectbuilder a field
        // on any new object you try to serialize. They work fine by themselves tho)
        private Dictionary<long, MyObjectBuilder_EntityBase> ConcealedBuilders
            = new Dictionary<long, MyObjectBuilder_EntityBase>();

        #endregion
        #region Constructor

        public ConcealableSector(String worldName, Vector3D sectorPos) {
            State.SectorName = worldName + sectorPos.ToString();
            State.StateFileName = State.SectorName + STATE_FILENAME_SUFFIX + FILE_EXT;
            State.GridBuildersFileName = State.SectorName + GRIDS_FILENAME_SUFFIX + FILE_EXT;
        }

        #endregion
        #region Save

        public void Save() {
            Log.Trace("Saving", "Save");

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                SerializeToXML<ConcealableSectorState>(State),
                State.StateFileName
            );

            SaveBuilders();

            Log.Trace("Finished saving", "Save");
        }

        // can't store object builders as a member of another object,
        // until we can store object builders in a higher level object,
        // serializing all the different types will require a few different steps
        // and unfortunately a few different files.
        private void SaveBuilders(){
            List<MyObjectBuilder_CubeGrid> concealedGridBuilderList =
                ConcealedBuilders.Values.Select(x => x as MyObjectBuilder_CubeGrid).
                Where(x => x != null).ToList();

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedGridBuilderList),
                State.GridBuildersFileName
            );

            Log.Trace("Saved Builders", "SaveBuilders");
        }

        #endregion
        #region Load

        /// <summary>
        /// Don't call this until MyAPIGateway is initialized!
        /// </summary>
        public void Load() {
            Log.Trace("Loading", "Load");

            LoadState();
            LoadBuilders();
            ValidateLoaded();

            Log.Trace("Finished loading", "Load");
        }

        private void LoadState() {

            // Cache filename from initialized object state
            String stateFileName = State.StateFileName;

            Log.Info("Loading Concealed Sector State from " + stateFileName, 
                "LoadState");

            // Empty current state
            State = new ConcealableSectorState();

            if (!GardenGateway.Files.Exists(State.StateFileName)) {
                Log.Warning("No existing state file found, using initialized",
                    "Load");
                return;
            }

            String serializedState = null;
            GardenGateway.Files.Read<String>(stateFileName, ref serializedState);

            if (serializedState == null || serializedState.Length < 1) {
                Log.Error("Existing State file blank, keeping init state", "Load");
                return;
            }

            ConcealableSectorState loaded = MyAPIGateway.Utilities.
                SerializeFromXML<ConcealableSectorState>(serializedState);

            if (loaded == null) {
                Log.Error("Failed to load state, keeping init state", "Load");
                return;
            }

            State = loaded;
            Log.Info("Concealed Sector State successfully loaded", "LoadState");
        }

        private void LoadBuilders() {
            Log.Info("Loading Builders for sector from " + State.SectorName, "LoadBuilders");

            LoadGridBuilders();

            Log.Info("Finished loading builders", "LoadBuilders");
        }

        private void LoadGridBuilders() {
            // Load Grids
            Log.Trace("Loading Grid Builders from " + State.GridBuildersFileName, "LoadBuilders");

            if (!GardenGateway.Files.Exists(State.GridBuildersFileName)) {
                Log.Info("No existing grid builders file found", "LoadBuilders");
                return;
            }

            String serializedGrids = null;
            GardenGateway.Files.Read<String>(State.GridBuildersFileName,
                ref serializedGrids);

            if (serializedGrids == null || serializedGrids.Length < 1) {
                Log.Error("Existing Grid Builders file blank", "LoadBuilders");
                return;
            }

            List<MyObjectBuilder_CubeGrid> loadedGridBuilders = MyAPIGateway.Utilities.
                SerializeFromXML<List<MyObjectBuilder_CubeGrid>>(serializedGrids);

            if (loadedGridBuilders == null) {
                Log.Error("Failed to load grid builders", "Load");
                return;
            }

            foreach (MyObjectBuilder_CubeGrid builder in loadedGridBuilders)
                ConcealedBuilders.Add(builder.EntityId, builder);

            Log.Trace("Successfully loaded builders", "LoadBuilders");
        }

        private bool ValidateLoaded() {
            Log.Info("Validating Loaded Concealable Sector", "LoadState");

            bool Validates = true;


            // From state perspective
            foreach (KeyValuePair<long, ConcealedEntity> kvp in State.ConcealedEntities) {

                if (!ConcealedBuilders.ContainsKey(kvp.Key)) {
                    Log.Error("Missing a builder for entity " + kvp.Key, "LoadState");
                    Validates = false;
                }
                else {
                    // TODO: Validate the builders against the saved grid details
                    switch (kvp.Value.EntityType) {

                        case EntityType.Grid:

                            break;

                    }
                }
            }

            // TODO: Validate from builders perspective

            if (Validates)
                Log.Info("Concealed Sector successfully validated", "LoadState");
            else
                Log.Info("Concealed Sector failed to validate", "LoadState");

            return Validates;
        }

        #endregion
        #region Lists

        /// <summary>
        /// Get a copy of the Grid info as list, for admin tasks
        /// </summary>
        /// <returns></returns>
        /// Dictionary<long, ConcealedGrid> ConcealedGrids() {
        public List<ConcealedGrid> ConcealedGrids() {
            Log.Info("Get list of Concealed Grids for public consumption", 
                "ConcealedGrids()");

            if (State == null) {
                Log.Error("Null state", "ConcealedGrids()");
                return new List<ConcealedGrid>();
            }

            if (State.ConcealedEntities == null) {
                Log.Error("Null Concealed Entities", "ConcealedGrids()");
                return new List<ConcealedGrid>();
            }     

            return State.ConcealedEntities.Values.Select(x => x as ConcealedGrid).
                Where(x => x != null).ToList();
            //.Where(kvp => kvp.Value is ConcealedGrid).Values.ToList();
            //ToDictionary(kvp => kvp.Key, kvp => kvp.Value as ConcealedGrid);
        }

        /*
        private Dictionary<long, MyObjectBuilder_CubeGrid> ConcealedGridBuilders() {
            return ConcealedBuilders.Where(
                kvp => kvp.Value as MyObjectBuilder_CubeGrid != null
                ).ToDictionary<long, MyObjectBuilder_CubeGrid>(kvp => kvp.Key, kvp => kvp.Value as MyObjectBuilder_CubeGrid);
        }
         * */

        public List<RevealedGrid> RevealedGrids() {
            Log.Info("Get list of Revealed Grids for public consumption", 
                "RevealedGrids()");

            if (State == null) {
                Log.Error("Null state", "RevealedGrids()");
                return new List<RevealedGrid>();
            }

            if (State.ConcealedEntities == null) {
                Log.Error("Null Concealed Entities", "RevealedGrids()");
                return new List<RevealedGrid>();
            }   

            var gridEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(gridEntities, e => e is IMyCubeGrid);

            // TODO: Store revealed grids in advance from EntityAdded
            List<IMyCubeGrid> gridsList = gridEntities.Select(x => x as IMyCubeGrid).ToList();

            List<RevealedGrid> revealedGrids = gridsList.Select(
                x =>  new RevealedGrid(x)
                ).ToList();

            return revealedGrids;
        }

        #endregion
        #region Conceal/Reveal

        private static void ConcealEntity(IMyEntity entity) {
            /*
            if (entity == null) {
                Log.Error("Received null entity, aborting", "ConcealEntity");
                return;
            }

            if (entity.SyncObject == null) {
                Log.Error("SyncObject missing, aborting", "ConcealEntity");
                return;
            }

            MyObjectBuilder_EntityBase builder = entity.GetObjectBuilder();

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + entity.EntityId +
                    ", aborting", "ConcealEntity");
                return;
            }

            // Track it

            if (ConcealedBuilders.ContainsKey(builder.EntityId)) {
                Log.Error("Attempting to store already-stored entity " +
                    entity.EntityId, "ConcealEntity");
                return;
            }

            ConcealedBuilders.Add(builder.EntityId, builder);

            if (builder is MyObjectBuilder_CubeGrid) {
                ConcealedEntities.Add(builder.EntityId,
                    new ConcealedGrid() {
                        EntityId = builder.EntityId,
                        SpawnPoints = new List<ConcealedGrid.SpawnPoint>(), // TODO: Check for spawn points
                        Position = new VRageMath.Vector3D(), // TODO: Get position
                    }
                );
            }
            else {
                ConcealedEntities.Add(builder.EntityId,
                    new ConcealedGrid() {
                        EntityId = builder.EntityId,
                        Position = new VRageMath.Vector3D(), // TODO: Get position
                    }
                );
            }

            QueueSave();

            // Remove it from the world
            entity.SyncObject.SendCloseRequest();
             * */
        }

        private void revealEntity(ConcealedEntity entity) {
            /*
            Log.Trace("Start reveal " + entity.EntityId, "revealEntity");

            if (entity == null) {
                Log.Error("Received null entity, aborting", "RevealEntity");
                return;
            }

            MyObjectBuilder_EntityBase builder = ConcealedBuilders[entity.EntityId];

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + entity.EntityId +
                    ", aborting", "RevealEntity");
                return;
            }

            // Remove from trackers

            if (entity is ConcealedGrid) {
                ConcealedGrid grid = entity as ConcealedGrid;
                MyObjectBuilder_CubeGrid gridBuilder =
                    builder as MyObjectBuilder_CubeGrid;

                if (builder == null) {
                    Log.Error("Wrong builder type stored for " + entity.EntityId +
                        ", aborting", "RevealEntity");
                    return;
                }

                ConcealedEntities.Remove(entity.EntityId);
                ConcealedBuilders.Remove(entity.EntityId);

                if (MyAPIGateway.Entities.EntityExists(builder.EntityId)) {
                    gridBuilder.EntityId = 0;
                    Log.Trace("Reallocating entityId", "revealEntity");
                }

                //builder.LinearVelocity = VRageMath.Vector3D.Zero;
                //builder.AngularVelocity = VRageMath.Vector3D.Zero;

                MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(gridBuilder);

                Log.Trace("Created object", "revealEntity");

            }

            //Reveal

            Log.Trace("End reveal " + entity.EntityId, "revealEntity");
             *              * */
        }

        #endregion
        #region Entity Add/Remove from game world

        public void EntityAdded(IMyEntity entity) {

        }

        public void EntityRemoved(IMyEntity entity) {

        }


        #endregion

    }

}
