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

        #region Static

        private static readonly uint DefaultControlledMovingGraceTimeSeconds = 30; //500
        private static readonly uint DefaultControlledMovementGraceDistanceMeters = 1; //500
        private static readonly uint DefaultRevealVisibilityMeters = 10; //35
        //private static readonly  uint DefaultRevealDetectabilityMeters = 10; //50;
        //private static readonly  uint DefaultRevealCommunicationMeters = 10; //50;
        //private static readonly  uint DefaultRevealCollisionMeters = 10; //10;
        private static readonly bool DefaultConcealNearAsteroids = false;
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

        public uint ControlledMovingGraceTimeSeconds = 
            DefaultControlledMovingGraceTimeSeconds;
        public uint ControlledMovementGraceDistanceMeters = 
            DefaultControlledMovementGraceDistanceMeters;
        public uint RevealVisibilityMeters = DefaultRevealVisibilityMeters;
        //public uint RevealDetectabilityMeters = DefaultRevealDetectabilityMeters;
        //public uint RevealCommunicationMeters = DefaultRevealCommunicationMeters;
        //public uint RevealCollisionMeters = DefaultRevealCollisionMeters;
        public bool ConcealNearAsteroids = DefaultConcealNearAsteroids;
        public readonly byte Count = 4;

        #endregion
        #region Constructors

        public Settings() {
            Log.ClassName = "GP.Concealment.World.Entities.RevealedEntity";
            Log.Trace("Finished RevealedEntity deserialize constructor", "ctr");
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



        public string ToString() {
            // TODO: implement
            return "To implement - settings as text with indicies for changing";
        }


    }

}
