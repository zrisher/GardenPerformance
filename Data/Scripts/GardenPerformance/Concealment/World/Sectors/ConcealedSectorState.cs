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
    [XmlType("ConcealableSector")]
    public class ConcealedSectorState {

        #region Static

        private const String FILENAME_SUFFIX = "_concealment_state";
        private const String GRIDS_SUFFIX = "_concealed_grids";
        private const String FILE_EXT = ".txt";

        private static Logger Log =
            new Logger("GP.Concealment.World.Sectors.ConcealedSectorState");

        private static String SectorName(String worldName, Vector3D sectorPos) {
            return String.Format("{0}_{1}_{2}_{3}", worldName, sectorPos.X, sectorPos.Y, sectorPos.Z);
        }

        private static String GenFileName(String sectorName) {
            return (sectorName + FILENAME_SUFFIX + FILE_EXT).CleanFileName();
        }

        private static String GenGridBuildersFileName(String sectorName) {
            return (sectorName + GRIDS_SUFFIX + FILE_EXT).CleanFileName();
        }

        public static ConcealedSectorState Load(String worldName, Vector3D sectorPos) {

            String sectorName = SectorName(worldName,sectorPos);
            String fileName = GenFileName(sectorName);
            String gridBuildersFileName = GenGridBuildersFileName(sectorName);

            // Load main state object
            Log.Info("Attempting to load from " + fileName, "Load");

            ConcealedSectorState loaded = GardenGateway.Files.
                ReadXML<ConcealedSectorState>(fileName);

            if (loaded == null) {
                Log.Error("Failed to deserialize XML", "Load");
                return null;
            }

            loaded.FileName = fileName;
            loaded.BuildersFileName = gridBuildersFileName;
            loaded.WorldName = worldName;
            loaded.SectorPosition = sectorPos;

            // Load concealed grid builders
            loaded.ConcealedGridBuilders = GardenGateway.Files.
                ReadXML<List<MyObjectBuilder_CubeGrid>>(gridBuildersFileName);

            if (loaded.ConcealedGridBuilders == null) {
                Log.Error("Failed to load grid builders from file!", "Load");
                return null;
            }

            Log.Trace("Concealed Sector State successfully loaded", "LoadState");
            return loaded;
        }

        #endregion
        #region Instance Fields

        // We cache these on load so we don't have to regen on save
        private String FileName;
        private String BuildersFileName;

        [XmlElement("WorldName")]
        public String WorldName = "Unknown";

        [XmlElement("SectorPosition")]
        public VRageMath.Vector3D SectorPosition = new Vector3D();

        [XmlArray("ConcealedGrids")]
        [XmlArrayItem("ConcealedGrid")]
        public List<ConcealableGrid> ConcealedGrids =
            new List<ConcealableGrid>();

        // Load these separately per remarks
        [XmlIgnore]
        public List<MyObjectBuilder_CubeGrid> ConcealedGridBuilders =
            new List<MyObjectBuilder_CubeGrid>();

        public void Save() {
            Log.Trace("Saving " + WorldName + " " + SectorPosition, "Save");

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
            GardenGateway.Files.WriteXML<ConcealedSectorState>(
                FileName, this);
            Log.Trace("Successful sector save.", "Save");

            Log.Trace("Finished saving", "Save");
        }

        #endregion

    }

}
