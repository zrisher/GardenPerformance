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
        public String WorldName = "";

        [XmlElement("SectorPosition")]
        public VRageMath.Vector3D SectorPosition = new Vector3D();

        //[XmlElement("GridBuildersFileName")]
        /*
        [XmlIgnore]
        public String GridBuildersFileName { get { return BuilderListHelper.FileName(Name);  } }
        */

        // Can't serialize dictionaries
        [XmlIgnore]
        public Dictionary<long, ConcealableGrid> ConcealedGrids =
            new Dictionary<long, ConcealableGrid>();

        // Don't use this, only used to hold the above dictionary for storage
        [XmlArray("ConcealedGrids")]
        [XmlArrayItem("ConcealedGrid")]
        public List<ConcealableGrid> CachedConcealedGridsList =
            new List<ConcealableGrid>();

        // Having trouble saving this as part of an existing class,
        // see BuilderListHelper
        /*
        [XmlIgnore]
        public Dictionary<long, MyObjectBuilder_EntityBase> ConcealedGridBuilders =
            new Dictionary<long, MyObjectBuilder_EntityBase>();
        */

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
            Log.Trace("Retrieving revealed grids list", "RevealedGridsList");
            Log.Trace("RevealedGrids.Keys: " + RevealedGrids.Keys, "RevealedGridsList");
            Log.Trace("RevealedGrids.Count: " + RevealedGrids.Count, "RevealedGridsList");

            return RevealedGrids.Select((x) => x.Value).ToList();
        }

        #region Save

        public void Save() {
            Log.Trace("Saving", "Save");

            CachedConcealedGridsList = ConcealedGridsList();

            CachedConcealedGridsList = new List<ConcealableGrid>() {
                new ConcealableGrid { },
                new ConcealableGrid { },
                new ConcealableGrid { },
                new ConcealableGrid { },
            };

            // check for nulls
            if (WorldName == null || SectorPosition == null || CachedConcealedGridsList == null) {
                Log.Error("Sector object had a null, aborting", "Save");
                return;
            }

            foreach (ConcealableGrid grid in CachedConcealedGridsList) {
                if (grid.Builder == null) {

                    Log.Error("ConcealableGrid had a null builder, aborting", "Save");
                    return;
                }
            }

            foreach (ConcealableGrid grid in CachedConcealedGridsList) {
                if (grid.Builder.AngularVelocity == null ||
                    grid.Builder.BlockGroups == null ||
                    grid.Builder.ComponentContainer == null ||
                    grid.Builder.ConveyorLines == null ||
                    grid.Builder.CreatePhysics == null ||
                    grid.Builder.CubeBlocks == null ||
                    grid.Builder.DampenersEnabled == null ||                    
                    grid.Builder.DestructibleBlocks == null ||
                    grid.Builder.DisplayName == null ||
                    grid.Builder.EnableSmallToLargeConnections == null ||                    
                    grid.Builder.EntityDefinitionId == null ||
                    grid.Builder.EntityId == null ||
                    grid.Builder.GridSizeEnum == null ||      
                    grid.Builder.Handbrake == null ||
                    grid.Builder.IsStatic == null ||
                    grid.Builder.JumpDriveDirection == null ||       
                    grid.Builder.JumpElapsedTicks == null ||
                    grid.Builder.LinearVelocity == null ||
                    grid.Builder.Name == null ||       
                    grid.Builder.OxygenAmount == null ||
                    grid.Builder.PersistentFlags == null ||
                    grid.Builder.PositionAndOrientation== null ||       
                    grid.Builder.Skeleton == null ||
                    grid.Builder.SubtypeId == null ||
                    grid.Builder.SubtypeName == null //||       
                    //grid.Builder.TypeId == null ||       
                    ) {

                    Log.Error("ConcealableGrid builder had a null value, aborting", "Save");
                    return;
                }
            }

            /*
            foreach (ConcealableGrid grid in CachedConcealedGridsList) {
                if (grid.BigOwners == null || 
                    grid.Builder == null ||
                    grid.Concealability == null ||
                    grid.DisplayName == null ||
                    grid.EntityId == null ||
                    grid.IsStatic == null ||
                    grid.Position == null ||
                    grid.Revealability == null ||
                    grid.SpawnOwners == null ||
                    grid.Status == null ||
                    grid.Transparent == null ||
                    grid.Type == null) {

                    Log.Error("ConcealableGrid had a null, aborting", "Save");
                    return;
                }
            }
            */

            //CachedConcealedGridsList = new List<ConcealableGrid>();

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.SerializeToXML<ConcealableSector>(this),
                FileName
            );

            //SaveBuilders();

            Log.Trace("Finished saving", "Save");
        }

        private void SaveBuilders(){

            /*
            HashSet<IMyEntity> allEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(allEntities);

            List<MyObjectBuilder_CubeGrid> concealedGridBuilderList =
                allEntities.Select(x => x.GetObjectBuilder() as MyObjectBuilder_CubeGrid).ToList();
            
            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedGridBuilderList),
                GridBuildersFileName
            );

            *//*

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
                Log.Trace("Found entity " + entityId, "ConcealEntity");
            }

            if (concealableGrid == null) {
                Log.Error("Failed to find grid, aborting", "ConcealEntity");
                return false;
            }

            IMyCubeGrid grid = concealableGrid.IngameGrid;

            if (grid == null) {
                Log.Error("Received null grid, aborting", "ConcealEntity");
                return false;
            }

            if (grid.SyncObject == null) {
                Log.Error("SyncObject missing, aborting", "ConcealEntity");
                return false;
            }

            concealableGrid.LoadFromCubeGrid(grid);


            //RevealedGrids.Remove(concealableGrid.EntityId);
            // Should be taken care of automatically by onRemoved hook
            
            ConcealedGrids.Add(concealableGrid.EntityId, concealableGrid);

            /*
            MyObjectBuilder_EntityBase builder = grid.GetObjectBuilder() as MyObjectBuilder_EntityBase;

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + grid.EntityId +
                    ", aborting", "ConcealEntity");
                return;
            }
            */

            // Track it
            /*
            if (ConcealedGridBuilders.ContainsKey(builder.EntityId)) {
                Log.Error("Attempting to store already-stored entity " +
                    grid.EntityId, "ConcealEntity");
                return;
            }

            ConcealedGridBuilders.Add(builder.EntityId, builder);
            */
            //ConcealedGrids.Add(builder.EntityId, concealableGrid);

            //NeedsSave = true;
            Save();

            // Remove it from the world
            grid.SyncObject.SendCloseRequest();

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

            /*
            MyObjectBuilder_CubeGrid builder = concealableGrid.Builder;

            if (builder == null) {
                Log.Error("Unable to retrieve builder for " + concealableGrid.EntityId +
                    ", aborting", "RevealEntity");
                return;
            }

            ConcealedGrids.Remove(concealableGrid.EntityId);

            // Should be taken care of automatically by onAdded hook
            //ConcealedGrids.Add(concealableGrid.EntityId, concealableGrid);


            Log.Trace("Start reveal " + concealableGrid.EntityId, "revealEntity");

            if (MyAPIGateway.Entities.EntityExists(concealableGrid.EntityId)) {
                concealableGrid.EntityId = 0;
                Log.Trace("Reallocating entityId", "revealEntity");
            }

            //builder.LinearVelocity = VRageMath.Vector3D.Zero;
            //builder.AngularVelocity = VRageMath.Vector3D.Zero;

            MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(builder);

            Log.Trace("Created object", "revealEntity");

            //Reveal
            */

            Log.Trace("End reveal " + concealableGrid.EntityId, "revealEntity");
            return true;
        }

        #endregion
        #region Entity Add/Remove from game world

        public void AddIngameEntity(IMyEntity entity) {

            // Add cubegrids
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null) return;
            var revealedGrid = new ConcealableGrid();
            revealedGrid.LoadFromCubeGrid(grid);
            RevealedGrids[grid.EntityId] = revealedGrid;
            Log.Trace("Added " + grid.EntityId, "AddIngameEntity");

        }

        public void AddIngameEntities(HashSet<IMyEntity> entities) {
            Log.Trace("Adding " + entities.Count + " entities", "AddIngameEntities");

            List<ConcealableGrid> revealedGrids = entities.
                Select(x => x as IMyCubeGrid).
                Where(x => x != null).
                Select((x) => { 
                    var grid= new ConcealableGrid(); 
                    grid.LoadFromCubeGrid(x);
                    return grid;
                }). 
                ToList();

            foreach (ConcealableGrid grid in revealedGrids) {
                RevealedGrids[grid.EntityId] = grid;
                Log.Trace("Added " + grid.EntityId, "AddIngameEntities");
            }
        }

        public void RemoveIngameEntity(IMyEntity entity) {

            // Remove cubegrid
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null) return;
            RevealedGrids.Remove(entity.EntityId);
            Log.Trace("Added " + entity.EntityId, "AddIngameEntities");

        }


        #endregion

    }

}
