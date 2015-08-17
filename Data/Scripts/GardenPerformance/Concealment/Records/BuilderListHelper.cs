/*
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

    // I would love to store the builder within our ConcealEntitiy representations,
    // but we can't serialize them together
    // (error when you make an objectbuilder a field on any new object you try to 
    // serialize. They work fine by themselves tho)
    //
    // TODO: Try Rynchodon's solution to this problem
    public static class BuilderListHelper {

        private const String FILENAME_SUFFIX = "_concealed_grids";
        private const String FILE_EXT = ".txt";

        private static Logger Log =
            new Logger("GP.Concealment.Records.ConcealedGridBuildersList");
        
        public static String FileName(String sectorName) {
            return sectorName + FILENAME_SUFFIX + FILE_EXT;
        }

        public static void Save(List<MyObjectBuilder_EntityBase> builders, 
            String fileName) {

            Log.Trace("Saving Builders", "SaveOverwrite");

            GardenGateway.Files.Overwrite(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<MyObjectBuilder_EntityBase>>(builders),
                fileName
            );

            Log.Trace("Saved Builders", "SaveOverwrite");
        }

        public static List<MyObjectBuilder_EntityBase> Load(String fileName) {
            Log.Trace("Loading Grid Builders from " + fileName, "Load");

            if (!GardenGateway.Files.Exists(fileName)) {
                Log.Error("No file found", "Load");
                return null;
            }

            String serialized = "";
            GardenGateway.Files.Read<String>(fileName, ref serialized);

            if (serialized == null || serialized.Length < 1) {
                Log.Error("Existing file blank", "Load");
                return null;
            }

            List<MyObjectBuilder_EntityBase> loaded = MyAPIGateway.Utilities.
                SerializeFromXML<List<MyObjectBuilder_EntityBase>>(serialized);

            if (loaded == null) {
                Log.Error("Failed to load grid builders", "Load");
                return null;
            }

            Log.Trace("Successfully loaded grid builders", "LoadBuilders");
            return loaded;
        }     


    }

}
*/