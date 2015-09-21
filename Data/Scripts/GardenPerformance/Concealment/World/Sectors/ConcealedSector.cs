using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

using SEGarden;
using SEGarden.Logging;
using SEGarden.Extensions;
using SEGarden.Math;

using GP.Concealment.World.Entities;

namespace GP.Concealment.World.Sectors {

    ///<summary>
    /// Holds all info about the currently concealed sector
    ///</summary>
    ///<remarks>
    /// Save load issues will stop conceal from running to ensure we don't muck up
    /// or lose the existing conceal state
    /// 
    /// We include XML attributes here, even though all the names are the same,
    /// to ensure backwards compatability with saves.
    ///
    /// I would love to store the builder within our ConcealableEntities, or even as
    /// a list within the sector. But I've run into problems time and time again 
    /// storing ObjectBuilders within a another object. Sometimes they just fail to
    /// serialize, and then always fail to deserialize. It can work on some and not 
    /// others. See commit 4fc00f4ecd2c8a2362892e22f86ebaa07b47f977 and the example
    /// world from zrisher. If we can fix this and it works without issue for an 
    /// extended period, maybe the bug in the vanilla object's serialization pattern
    /// is fixed. :/
    /// </remarks>
    [XmlType("ConcealedSector")]
    public class ConcealedSector {

        /*
        public class SectorLoadError : System.Exception {
            public SectorLoadError(String message) : base(message) { }
        }
        */

        #region Static

        private const String FILENAME_SUFFIX = "_concealment_state";
        private const String GRIDS_SUFFIX = "_concealed_grids";
        private const String FILE_EXT = ".txt";

        private static Logger Log =
            new Logger("GP.Concealment.World.Sectors.ConcealedSector");

        private static String SectorName(String worldName, Vector3D sectorPos) {
            return String.Format("{0}_{1}_{2}_{3}", worldName, sectorPos.X, sectorPos.Y, sectorPos.Z);
        }

        private static String GenFileName(String worldName, Vector3D sectorPos) {
            return (SectorName(worldName, sectorPos) + FILENAME_SUFFIX + FILE_EXT).
                CleanFileName();
        }

        private static String GenGridBuildersFileName(String worldName, Vector3D sectorPos) {
            return (SectorName(worldName, sectorPos) + GRIDS_SUFFIX + FILE_EXT).
                CleanFileName();
        }

        public static ConcealedSector LoadOrNew(String worldName, Vector3D sectorPos) {
            String fileName = GenFileName(worldName, sectorPos);

            if (!GardenGateway.Files.Exists(fileName)) {
                Log.Trace("No existing save, starting fresh.", "LoadOrNew");
                return new ConcealedSector(worldName, sectorPos);
                /*
                newSector.WorldName = worldName;
                newSector.SectorPosition = sectorPos;
                newSector.FileName = GenFileName(worldName, sectorPos);
                newSector.BuildersFileName = GenGridBuildersFileName(worldName, sectorPos);
                */
            }

            return Load(worldName, sectorPos);
        }

        public static ConcealedSector Load(String worldName, Vector3D sectorPos) {
            String fileName = GenFileName(worldName, sectorPos);

            // Load main state object
            Log.Info("Attempting to load from " + fileName, "Load");
            ConcealedSector loaded = GardenGateway.Files.
                ReadXML<ConcealedSector>(fileName);

            if (loaded == null) {
                Log.Error("Failed to deserialize XML", "Load");
                return null;
            }

            //loaded.FileName = fileName;
            //loaded.BuildersFileName = gridBuildersFileName;
            //loaded.WorldName = worldName;
            //loaded.SectorPosition = sectorPos;

            // Load grid dictionary from serialized list
            loaded.Grids = loaded.ConcealedGrids.
                ToDictionary(x => x.EntityId, x => x);

            // Load concealed grid builders
            Log.Trace("Loading concealed grid builders", "Load");
            loaded.ConcealedGridBuilders = GardenGateway.Files.
                ReadXML<List<MyObjectBuilder_CubeGrid>>(loaded.BuildersFileName);

            if (loaded.ConcealedGridBuilders == null) {
                Log.Error("Failed to load grid builders from file!", "Load");
                return null;
            }

            // load builders into grids
            foreach (MyObjectBuilder_CubeGrid builder in loaded.ConcealedGridBuilders) {
                if (!loaded.Grids.ContainsKey(builder.EntityId)) {
                    Log.Error("Found builder with missing conceal grid " + builder.EntityId, "Load");
                    return null;
                }

                loaded.Grids[builder.EntityId].Builder = builder;
            }

            // ensure every grid has a builder
            foreach (ConcealedGrid grid in loaded.Grids.Values) {
                if (grid.Builder == null) {
                    Log.Error("Found grid with missing builder " + grid.EntityId, "Load");
                    return null;
                }
            }

            Log.Trace("Loading AABB Tree", "LoadState");
            foreach (ConcealedGrid grid in loaded.ConcealedGrids) {
                loaded.GridTree.Add(grid);
            }
            Log.Trace("Finished Loading AABB Tree", "LoadState");


            loaded.NeedsSave = true;
            Log.Trace("Concealed Sector State successfully loaded", "LoadState");
            return loaded;
        }

        public static bool SaveExists(String worldName, Vector3D sectorPos) {
            String fileName = GenFileName(worldName, sectorPos);
            return GardenGateway.Files.Exists(fileName);
        }

        /*
        private static String GenFileName(String sectorName) {
            return (sectorName + FILENAME_SUFFIX + FILE_EXT).CleanFileName();
        }
        private static String GenGridBuildersFileName(String sectorName) {
            return (sectorName + GRIDS_SUFFIX + FILE_EXT).CleanFileName();
        }
        */

        #endregion
        #region Instance Fields

        // Public fields stored in XML
        [XmlElement("WorldName")]
        public String WorldName = "Unknown";

        [XmlElement("SectorPosition")]
        public VRageMath.Vector3D SectorPosition = new Vector3D();

        [XmlArray("ConcealedGrids")]
        [XmlArrayItem("ConcealedGrid")]
        public List<ConcealedGrid> ConcealedGrids =
            new List<ConcealedGrid>();

        public String FileName;
        public String BuildersFileName;

        // Load these separately per remarks
        private List<MyObjectBuilder_CubeGrid> ConcealedGridBuilders =
            new List<MyObjectBuilder_CubeGrid>();

        private Dictionary<long, ConcealedGrid> Grids =
            new Dictionary<long, ConcealedGrid>();

        //private Dictionary<long, MyObjectBuilder_CubeGrid> GridBuilders =
        //    new Dictionary<long, MyObjectBuilder_CubeGrid>();

        private AABBTree GridTree = new AABBTree();

        private bool NeedsSave;
        private bool Loaded;

        #endregion
        #region Field Access Helpers

        public List<ConcealedGrid> ConcealedGridsList() {
            return Grids.Values.ToList();
        }

        public List<ConcealedGrid> ConcealedGridsNeedingReveal() {
            return Grids.Values.Where((g) => g.NeedsReveal).ToList();
        }

        private List<MyObjectBuilder_CubeGrid> ConcealedGridBuildersList() {
            return Grids.Values.Select((x) => x.Builder).ToList();
        }

        public ConcealedGrid GetGrid(long entityId) {
            ConcealedGrid grid;
            Grids.TryGetValue(entityId, out grid);
            return grid;
        }

        public bool AddGrid(ConcealedGrid concealed) {
            // Track it
            if (Grids.ContainsKey(concealed.EntityId)) {
                Log.Error("Attempting to store already-stored entity id " +
                    concealed.EntityId, "ConcealGrid");
                return false;
            }

            Grids.Add(concealed.EntityId, concealed);
            //GridBuilders.Add(concealed.EntityId, concealed.Builder);
            GridTree.Add(concealed);

            NeedsSave = true;
            return true;
        }

        public bool CanRemoveGrid(ConcealedGrid concealed) {
            if (!Grids.ContainsKey(concealed.EntityId)) {
                Log.Error("Failed to find in list " + concealed.EntityId,
                    "RemoveGrid");
                return false;
            }

            return true;
        }

        public void RemoveGrid(ConcealedGrid concealed) {
            Grids.Remove(concealed.EntityId);
            GridTree.Remove(concealed);
            NeedsSave = true;
        }

        public void UpdateSpawn() {
            foreach (ConcealedGrid grid in Grids.Values) {
                grid.MarkSpawnUpdateNeeded();
            }
        }

        #endregion
        #region Constructors

        // Must be parameterless because loaded from XML
        public ConcealedSector() {}

        // Truly new one created by application
        public ConcealedSector(String worldName, Vector3D sectorPos) {
            WorldName = worldName;
            SectorPosition = sectorPos;
            FileName = GenFileName(worldName, sectorPos);
            BuildersFileName = GenGridBuildersFileName(worldName, sectorPos);
            NeedsSave = true;
        }

        #endregion
        #region Save

        public void Save() {
            Log.Trace("Saving " + WorldName + " " + SectorPosition, "Save");

            ConcealedGridBuilders = ConcealedGridBuildersList();
            ConcealedGrids = ConcealedGridsList();

            // check for nulls
            if (WorldName == null || SectorPosition == null ||
                ConcealedGrids == null) {

                Log.Error("Concealed Sector State had a null, aborting", "Save");
                return;
            }

            Log.Trace("Storing grid builders list", "Save");
            GardenGateway.Files.WriteXML<List<MyObjectBuilder_CubeGrid>>(
                BuildersFileName, ConcealedGridBuilders);
            Log.Trace("Finished concealed grid builder list save", "Save");

            Log.Trace("Trying sector save.", "Save");
            GardenGateway.Files.WriteXML<ConcealedSector>(
                FileName, this);
            Log.Trace("Successful sector save.", "Save");

            Log.Trace("Finished saving", "Save");
        }

        #endregion
        #region Marking

        #endregion

        public List<ConcealedEntity> EntitiesInBox(BoundingBoxD bounds) {
            var results = new List<ConcealedEntity>();
            GridTree.GetAllEntitiesInBox<ConcealedEntity>(ref bounds, results);
            /*
            if (results.Count > 0) {
                Log.Trace(results.Count + " entities found in box " + bounds + 
                    " : " + String.Join(", ", results.Select((e) => e.EntityId).ToList()), "EntitiesInBox");
            }
            */
            return results;
        }

        public List<ConcealedEntity> EntitiesInSphere(BoundingSphereD bounds) {
            var results = new List<ConcealedEntity>();
            GridTree.GetAllEntitiesInSphere<ConcealedEntity>(ref bounds, results);
            /*
            if (results.Count > 0) {
                Log.Trace(results.Count + " entities found in sphere " + bounds +
                    " : " + String.Join(", ", results.Select((e) => e.EntityId).ToList()), "EntitiesInSphere");
            }
            */
            return results;
        }



    }

}
