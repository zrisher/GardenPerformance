using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRageMath;

using SEGarden;
using SEGarden.Extensions;
using SEGarden.Logging;

namespace GP.Concealment {

    public class Settings {

        public enum Setting : byte {
            ControlledMovingGraceTime,
            ControlledMovementGraceDistance,
            RevealVisibility,
            ConcealNearAsteroids,
            RevealedMinAge,
        }

        #region Static

        private static readonly uint DefaultControlledMovingGraceTimeSeconds = 30; //500
        private static readonly uint DefaultControlledMovementGraceDistanceMeters = 5; //500
        private static readonly uint DefaultRevealVisibilityMeters = 100; //35
        //private static readonly  uint DefaultRevealDetectabilityMeters = 10; //50;
        //private static readonly  uint DefaultRevealCommunicationMeters = 10; //50;
        //private static readonly  uint DefaultRevealCollisionMeters = 10; //10;
        private static readonly bool DefaultConcealNearAsteroids = false;
        private static readonly uint DefaultRevealedMinAgeSeconds = 20;
        private static readonly Logger Log = new Logger("GP.Concealment.Settings");
        private static readonly string Filename = "concealment_settings.txt";

        public static Settings Instance { get; private set; }

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

        #endregion
        #region Fields

        private Dictionary<Setting, ulong> CurrentSettings = 
            new Dictionary<Setting, ulong>();

        #endregion
        #region Properties

        public ulong ControlledMovingGraceTimeSeconds {
            get { return CurrentSettings[Setting.ControlledMovingGraceTime]; }
            set { CurrentSettings[Setting.ControlledMovingGraceTime] = value; }
        }

        public ulong ControlledMovementGraceDistanceMeters {
            get { return CurrentSettings[Setting.ControlledMovementGraceDistance]; }
            set { CurrentSettings[Setting.ControlledMovementGraceDistance] = value; }
        }

        public ulong RevealVisibilityMeters {
            get { return CurrentSettings[Setting.RevealVisibility]; }
            set { CurrentSettings[Setting.RevealVisibility] = value; }
        }

        public bool ConcealNearAsteroids {
            get { return CurrentSettings[Setting.ConcealNearAsteroids] == 1; }
            set { CurrentSettings[Setting.ConcealNearAsteroids] = (ulong)((value) ? 1 : 0); }
        }

        public ulong RevealedMinAgeSeconds {
            get { return CurrentSettings[Setting.RevealedMinAge]; }
            set { CurrentSettings[Setting.RevealedMinAge] = value; }
        }

        public byte Count {
            get { return (byte)CurrentSettings.Keys.Count; }
        }

        #endregion
        #region Constructors

        public Settings() {
            Log.ClassName = "GP.Concealment.World.Entities.RevealedEntity";
            ControlledMovingGraceTimeSeconds = DefaultControlledMovingGraceTimeSeconds;
            ControlledMovementGraceDistanceMeters = DefaultControlledMovingGraceTimeSeconds;
            RevealVisibilityMeters = DefaultRevealVisibilityMeters;
            ConcealNearAsteroids = DefaultConcealNearAsteroids;
            RevealedMinAgeSeconds = DefaultRevealedMinAgeSeconds;
        }

        // Byte Deserialization
        public Settings(VRage.ByteStream stream) {
            ControlledMovingGraceTimeSeconds = (ushort)stream.getUlong();
            ControlledMovementGraceDistanceMeters = (ushort)stream.getUlong();
            RevealVisibilityMeters = (ushort)stream.getUlong();
            //RevealDetectabilityMeters = 10; //50;
            //RevealCommunicationMeters = 10; //50;
            //RevealCollisionMeters = 10; //10;
            ConcealNearAsteroids = stream.getBoolean();

        }

        #endregion
        #region Serialization

        // Byte Serialization
        public void AddToByteStream(VRage.ByteStream stream) {
            stream.addUlong(ControlledMovingGraceTimeSeconds);
            stream.addUlong(ControlledMovementGraceDistanceMeters);
            stream.addUlong(RevealVisibilityMeters);
            //RevealDetectabilityMeters = 10; //50;
            //RevealCommunicationMeters = 10; //50;
            //RevealCollisionMeters = 10; //10;
            stream.addBoolean(ConcealNearAsteroids);
        }

        public void Save() {
            Log.Trace("Saving Settings", "Save");
            GardenGateway.Files.WriteXML<Settings>(Filename, this);
            Log.Trace("Finished saving settings", "Save");
        }

        #endregion

        public void ChangeSetting(byte id, ulong value) {
            id--; // we use 1-basis for user simplicity
            CurrentSettings[(Setting)id] = value;
        }

        public string Describe() {
            string result = "Boolean settings (i.e. ConcealNearAsteroids) take a 1 or 0.\n";
            foreach (var kvp in CurrentSettings) {
                result += " " + ((byte)kvp.Key + 1) + " : " + kvp.Key + " - " + 
                    kvp.Value + "\n";
            }
            return result;
        }


    }

}
