using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden;
using SEGarden.Logging;

namespace GP.Concealment {

    public class Settings {

        public static Settings Instance { get; private set;}

        public static void Load() {
            Log.Trace("Loading Settings", "Load");

            if (!GardenGateway.Files.Exists(Filename)) {
                Log.Trace("No existing file, starting fresh.", "Load");
                Instance = new Settings();
                return;
            }

            Log.Trace("Found file, loading.", "Load");
            Instance = GardenGateway.Files.ReadXML<Settings>(Filename);

            Log.Trace("Finished loading settings", "Load");
        }

        private static readonly Logger Log = new Logger("GP.Concealment.Settings");
        private static readonly string Filename = "concealment_settings.txt";

        public uint ControlledRevealCacheMeters = 1; //500
        public uint RevealVisibilityMeters = 10; //35
        public uint RevealDetectabilityMeters = 10; //50;
        public uint RevealCommunicationMeters = 10; //50;
        public uint RevealCollisionMeters = 10; //10;
        public bool ConcealNearAsteroids = false;

        public void Save() {
            Log.Trace("Saving Settings", "Save");
            GardenGateway.Files.WriteXML<Settings>(Filename, this);
            Log.Trace("Finished saving settings", "Save");
        }


    }

}
