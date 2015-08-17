using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


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

using GP.Concealment.Records.Entities;

namespace GP.Concealment.Records {

    // We include XML attributes here, even though all the names are the same,
    // to ensure backwards compatability with saves.
    // Losing concealed grids is NOT ok.
    [XmlType("ConcealableSector")]
    public class ConcealableSector {

        #region Static

        private const String FILENAME_SUFFIX = "_concealment_state";
        private const String FILE_EXT = ".txt";

        private static Logger Log =
            new Logger("GardenPerformance.Concealment.ConcealableSector");

        public static String SectorName(String worldName, Vector3D sectorPos) {
            return String.Format("{0}_{1}_{2}_{3}", worldName, sectorPos.X, sectorPos.Y, sectorPos.Z);
        }

        public static String GenFileName(String worldName, Vector3D sectorPos) {
            return SectorName(worldName, sectorPos) + FILENAME_SUFFIX + FILE_EXT;
        }

        public static ConcealableSector Load(String fileName) {
            Log.Info("Attempting to loading from " + fileName, "Load");

            if (!GardenGateway.Files.Exists(fileName)) {
                Log.Error("File does not exist.", "Load");
                return null;
            }

            String loadedSerialized = "";
            GardenGateway.Files.Read<String>(fileName, ref loadedSerialized);

            if (loadedSerialized == null || loadedSerialized.Length < 1) {
                Log.Error("File blank.", "Load");
                return null;
            }

            ConcealableSector loaded = MyAPIGateway.Utilities.
                SerializeFromXML<ConcealableSector>(loadedSerialized);

            if (loaded == null) {
                Log.Error("Failed to deserialize XML", "Load");
                return loaded;
            }

            // Load builders
            //if (!loaded.LoadBuilders()) return null;

            loaded.FileName = fileName;

            Log.Trace("Concealed Sector State successfully loaded", "LoadState");
            return loaded;
        }

        #endregion
        #region Instance Fields

        [XmlElement("WorldName")]
        public String WorldName = "Test world name";

        [XmlElement("SectorPosition")]
        public VRageMath.Vector3D SectorPosition = new Vector3D();

        // Can't serialize dictionaries
        [XmlIgnore]
        public Dictionary<long, ConcealableGrid> ConcealedGrids =
            new Dictionary<long, ConcealableGrid>();

        // Only used to hold the above dictionary for storage
        [XmlArray("ConcealedGrids")]
        [XmlArrayItem("ConcealedGrid")]
        public List<ConcealableGrid> CachedConcealedGridsList =
            new List<ConcealableGrid>();

        // No point in saving this, much easier to load it from the world
        [XmlIgnore]
        public Dictionary<long, ConcealableGrid> RevealedGrids =
            new Dictionary<long, ConcealableGrid>();

        [XmlIgnore]
        public bool NeedsSave;

        [XmlIgnore]
        public String FileName;

        #endregion

        private String Name { get { return SectorName(WorldName, SectorPosition); } }

        public List<ConcealableGrid> ConcealedGridsList() {
            return ConcealedGrids.Select((x) => x.Value).ToList();
        }

        public List<ConcealableGrid> RevealedGridsList() {
            //Log.Trace("Retrieving revealed grids list", "RevealedGridsList");
            //Log.Trace("RevealedGrids.Keys: " + RevealedGrids.Keys, "RevealedGridsList");
            //Log.Trace("RevealedGrids.Count: " + RevealedGrids.Count, "RevealedGridsList");

            return RevealedGrids.Select((x) => x.Value).ToList();
        }

        #region Save

        public void Save() {
            Log.Trace("Saving", "Save");

            CachedConcealedGridsList = ConcealedGridsList();

            // check for nulls
            if (WorldName == null || SectorPosition == null || 
                CachedConcealedGridsList == null) {

                Log.Error("Sector object had a null, aborting", "Save");
                return;
            }

           // WorldName = "Test World"; // why isnt this applied from init?

            bool canSaveAllGrids = true;
            CachedConcealedGridsList = CachedConcealedGridsList.Where(
                (grid) => {
                    if (!grid.Saveable()) {
                        canSaveAllGrids = false;
                        return false;
                    }
                    return true; 
                }).ToList();

            if (!canSaveAllGrids) {
                Log.Error("Errors make some grids impossible to save! Continuing.", 
                    "Save");
            }


            //Log.Trace("Trying ingame builder list save.", "Save");
            //SaveBuilders();
            //Log.Trace("Finished saving ingame builder list", "Save");


            Log.Trace("Trying concealed grid builder list save", "Save");
            String GridBuildersFileName2 = "concealed_grid_builder_list.txt";
            List<MyObjectBuilder_CubeGrid> concealedGridBuilderList;
            concealedGridBuilderList = ConcealedGrids.Select((x) => {
                return x.Value.Builder;
            }).ToList();

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedGridBuilderList),
                GridBuildersFileName2
            );
            Log.Trace("Finished concealed grid builder list save", "Save");

            /*
            Log.Trace("Trying concealed grid list unsafe", "Save");
            String GridBuildersFileName3 = "concealed_grid_list_unsafe.txt";

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<ConcealableGrid>>(ConcealedGridsList()),
                GridBuildersFileName3
            );
            Log.Trace("Finished concealed grid list save", "Save");
            */

            Log.Trace("Trying concealed grid list safe", "Save");
            String GridBuildersFileName4 = "concealed_grid_list_safe.txt";
            List<ConcealableGrid> saveableConcealedGridList;
            saveableConcealedGridList = ConcealedGridsList().
                Where((x) => x.Saveable()).ToList();

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<ConcealableGrid>>(saveableConcealedGridList),
                GridBuildersFileName4
            );
            Log.Trace("Finished concealed grid list safe", "Save");



            Log.Trace("Trying sector save next.", "Save");
            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.SerializeToXML<ConcealableSector>(this),
                FileName
            );
            Log.Trace("Successful sector save!", "Save");



            Log.Trace("Finished saving", "Save");
        }

        private void SaveBuilders(){

            String GridBuildersFileName = "build_list_test.txt";
            List<MyObjectBuilder_CubeGrid> concealedGridBuilderList;

            HashSet<IMyEntity> allGrids = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(allGrids, (x) => x is IMyCubeGrid);

            /*
            concealedGridBuilderList =
                allGrids.Select(x => x.GetObjectBuilder() as MyObjectBuilder_CubeGrid).ToList();

            Log.Trace("Trying to save pulled from world", "Save");
            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedGridBuilderList),
                GridBuildersFileName
            );
            Log.Trace("Finished saving pulled from world!!!", "Save");
            */

            // Fails
            //concealedGridBuilderList =
            //    RevealedGrids.Select(kvp => kvp.Value.Builder).ToList();
            /*
            concealedGridBuilderList =
                allGrids.Select((x) => {
                    var concealable = new ConcealableGrid();
                    concealable.LoadFromCubeGrid(x)
                    return x.GetObjectBuilder() as MyObjectBuilder_CubeGrid;
                }).ToList();
            */

            concealedGridBuilderList =
                allGrids.Select((x) => {
                    IMyCubeGrid grid = x as IMyCubeGrid;

                    //return x.GetObjectBuilder() as MyObjectBuilder_CubeGrid;
                    //return grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;

                    var concealable = new ConcealableGrid();
                    concealable.LoadFromCubeGrid(grid);

                    //return concealable.IngameGrid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;
  
                    // /\ success

                    return concealable.Builder;

                    // \/ failure

                    //return concealable.Builder;
                }).ToList();


            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedGridBuilderList),
                "real_" + GridBuildersFileName
            );

            /*

            List<MyObjectBuilder_EntityBase> concealedGridBuilderList =
                ConcealedGrids.Values.Select(x => x.Builder).ToList();

            BuilderListHelper.Save(concealedGridBuilderList, GridBuildersFileName);
             * 
            *//*

            if (!GardenGateway.Files.Exists(GridBuildersFileName)) {
                Log.Error("Error saving grid builders", "SaveBuilders");
            } else {
                Log.Trace("Saved Builders", "SaveBuilders");
            }
               * */
        }

        #endregion
        #region Load

        private bool LoadBuilders() {
            Log.Trace("Loading Builders", "LoadBuilders");

            bool success = true;

            if (!LoadGridBuilders()) success = false;

            Log.Trace("Finished loading builders", "LoadBuilders");
            return success;
        }

        private bool LoadGridBuilders() {
            /*

            bool buildersExpected = (ConcealedGrids.Keys.Count != 0);

            if (GridBuildersFileName == "" || 
                !GardenGateway.Files.Exists(GridBuildersFileName)) {

                if (buildersExpected) {
                    Log.Error("Missing builders file for existing concealed grids!","LoadGridBuilders");
                    return false;
                }

                //GridBuildersFileName = BuilderListHelper.FileName(Name);
                return true;
            }

            Log.Trace("Loading Grid Builders from " + GridBuildersFileName, "LoadBuilders");

            List<MyObjectBuilder_EntityBase> loaded =  BuilderListHelper.Load(GridBuildersFileName);

            if (loaded == null) {
                if (buildersExpected) {
                    Log.Error("Failed to load existing builders from file!", "LoadBuilders");
                    return false;
                }

                return true;
            }

            foreach (MyObjectBuilder_EntityBase builder in loaded) {
                if (ConcealedGrids.ContainsKey(builder.EntityId)) {
                    ConcealedGrids[builder.EntityId].Builder = builder as MyObjectBuilder_CubeGrid;
                } else {
                    Log.Warning("Orphan builder in file", "LoadBuilders");
                }
            }

            bool missingExpected = false;
            foreach (ConcealableGrid grid in ConcealedGrids.Values) {
                if (grid.Builder == null) {
                    Log.Error("Missing builder for conceal grid " + grid.EntityId + " !", "LoadBuilders");
                    missingExpected = true;
                }
            }

            if (missingExpected) {
                Log.Error("Failed to load due to missing builders", "LoadBuilders");
                return false;
            }

            Log.Trace("Successfully loaded builders", "LoadBuilders");
            */

            return true;
        }

        #endregion
        #region Conceal/Reveal

        // TODO: more entity types
        public bool ConcealEntity(long entityId) {
            Log.Trace("Concealing entity " + entityId, "ConcealEntity");

            ConcealableGrid concealableGrid = null;

            if (RevealedGrids.ContainsKey(entityId)) {
                concealableGrid = RevealedGrids[entityId];
                Log.Trace("Found stored revealed entity " + 
                    concealableGrid.EntityId, "ConcealEntity");
            }

            if (concealableGrid == null) {
                Log.Error("Failed to find grid, aborting", "ConcealEntity");
                return false;
            }

            //IMyCubeGrid grid = concealableGrid.IngameGrid;
            IMyEntity entity = null;
            IMyCubeGrid grid = null;
            MyAPIGateway.Entities.TryGetEntityById(entityId, out entity);
            grid = entity as IMyCubeGrid;


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

            if (!concealableGrid.Saveable()) {
                Log.Error("Won't be able to save this grid, aborting conceal.",
                    "ConcealEntity");
                return false;
            }

            // Track it
            if (ConcealedGrids.ContainsKey(grid.EntityId)) {
                Log.Error("Attempting to store already-stored entity " +
                    grid.EntityId, "ConcealEntity");
                return false;
            }

            ConcealedGrids.Add(concealableGrid.EntityId, concealableGrid);

            // Should be taken care of automatically by onRemoved hook
            //RevealedGrids.Remove(concealableGrid.EntityId);

            // Remove it from the world
            grid.SyncObject.SendCloseRequest();

            //NeedsSave = true;
            Save();

            return true;
        }

        public bool RevealEntity(long entityId) {
            Log.Error("Revealing entity", "RevealEntity");

            ConcealableGrid concealableGrid = null;

            if (ConcealedGrids.ContainsKey(entityId)) {
                concealableGrid = ConcealedGrids[entityId];
            }

            if (concealableGrid == null) {
                Log.Error("Failed to find grid, aborting", "ConcealEntity");
                return false;
            }

            MyObjectBuilder_CubeGrid builder = concealableGrid.Builder;

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + concealableGrid.EntityId +
                    ", aborting", "RevealEntity");
                return false;
            }

            ConcealedGrids.Remove(concealableGrid.EntityId);

            // Should be taken care of automatically by onAdded hook
            //RevealedGrids.Add(concealableGrid.EntityId, concealableGrid);

            Log.Trace("Start reveal " + concealableGrid.EntityId, "revealEntity");

            if (MyAPIGateway.Entities.EntityExists(concealableGrid.EntityId)) {
                concealableGrid.EntityId = 0;
                Log.Trace("Reallocating entityId", "revealEntity");
            }

            //builder.LinearVelocity = VRageMath.Vector3D.Zero;
            //builder.AngularVelocity = VRageMath.Vector3D.Zero;

            MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(builder);
            Log.Trace("Created object", "revealEntity");

            Log.Trace("End reveal " + concealableGrid.EntityId, "revealEntity");
            return true;
        }

        #endregion
        #region Entity Add/Remove from game world

        public void AddIngameEntity(IMyEntity entity) {
            Log.Trace("Adding entity " + entity.EntityId + " of type " + 
                entity.GetType(), "AddIngameEntity");

            if (entity.Transparent) {
                Log.Trace("It's Transparent, skipping", "AddIngameEntity");
                return;
            }

            // TODO: Store other types of entities
            if (entity is IMyCubeGrid) {
                Log.Trace("It's a CubeGrid.", "AddIngameEntity");
                IMyCubeGrid grid = entity as IMyCubeGrid;
                var revealedGrid = new ConcealableGrid();
                revealedGrid.LoadFromCubeGrid(grid);
                RevealedGrids[grid.EntityId] = revealedGrid;
            }
            if (entity is IMyCharacter) {
                Log.Trace("It's a Character", "AddIngameEntity");
            }
            if (entity is IMyCubeBlock) {
                Log.Trace("It's a CubeBlock", "AddIngameEntity");
            }
            if (entity is IMyCubeBuilder) {
                Log.Trace("It's a CubeBuilder", "AddIngameEntity");
            }

        }

        public void AddIngameEntities(HashSet<IMyEntity> entities) {
            Log.Trace("Adding " + entities.Count + " entities", "AddIngameEntities");

            foreach (IMyEntity entity in entities) {
                AddIngameEntity(entity);
            }
        }

        public void RemoveIngameEntity(IMyEntity entity) {
            Log.Trace("Removing entity " + entity.EntityId + " of type " +
                entity.GetType(), "RemoveIngameEntity");

            if (entity.Transparent) {
                Log.Trace("It's Transparent, skipping", "AddIngameEntity");
                return;
            }

            // TODO: Store other types of entities
            if (entity is IMyCubeGrid) {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                Log.Trace("Removing CubeGrid " + grid.EntityId, "RemoveIngameEntity");

                if (!RevealedGrids.ContainsKey(entity.EntityId)) {
                    Log.Trace("Removed CubeGrid wasn't stored " + grid.EntityId, "RemoveIngameEntity");
                }
                else {
                    RevealedGrids.Remove(entity.EntityId);
                    Log.Trace("Removed " + entity.EntityId, "RemoveIngameEntity");
                }
            }
        }


        #endregion

    }

}
