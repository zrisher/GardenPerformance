using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Ingame = Sandbox.ModAPI.Ingame;
using VRage;
using VRage.ModAPI;
using VRageMath;

using SEGarden.Extensions;
using SEGarden.Extensions.VRageMath;
using SEGarden.Logic;
using SEGarden.Logging;
//using SEGarden.Math;

using GP.Concealment.Sessions;

namespace GP.Concealment.World.Entities {

    // Revealed entities cannot be concealed if they
    // Are controlled
    // Are "working" (refining, assembling, oxy creating, battery charging)
    // Are moving, 
    // Is near a controlled entity (taken care of by that controlled entity)
    //
    // TODO: beacons broadcast to both friends and enemies
    // actually antennae do too, it's just laser that don't
    // TODO: Power production and oxy production (need to check if these are actually making anything or else they'll just stay stuck revealed)
    //
    // Unfortunately ControllerInfo isn't whitelisted, so we just guess if 
    // it's controlled by whether it's moving. Character entities report presence separately. 
    //public bool Controlled { get { return ControlsInUse.Count > 0; } }
    public class RevealedGrid : ObservingEntity, ConcealableGrid {

        /*
        private static ServerConcealSession Session {
            get { return ServerConcealSession.Instance; }
        }
        */

        #region Fields

        private Dictionary<long, Ingame.IMyProductionBlock> ProductionBlocks =
            new Dictionary<long, Ingame.IMyProductionBlock>();

        private Dictionary<long, Ingame.IMyBatteryBlock> BatteryBlocks =
            new Dictionary<long, Ingame.IMyBatteryBlock>();

        // Control
        // We can't get the pilots from the grid, we have to go from the character
        // But we already reveal near a character.
        // Plus we can't detect AI pilots at all.
        // So we just base control on moving or moved in past X minutes
        /*
        private bool Piloted;
        private bool UpdatePilotedNextUpdate;
        private Dictionary<long, Ingame.IMyCockpit> Cockpits =
            new Dictionary<long, Ingame.IMyCockpit>();
        */

        // Spawn
        private Dictionary<long, Ingame.IMyMedicalRoom> MedBays =
            new Dictionary<long, Ingame.IMyMedicalRoom>();
        private Dictionary<long, Ingame.IMyCockpit> Cyrochambers =
            new Dictionary<long, Ingame.IMyCockpit>();

        // Comms
        // These are all just to keep us from having to reveal at greater distances 
        /*
        private bool HasRadioAntennae;
        private bool HasLaserAntennae;
        private float BroadcastingFriendlyRange;
        private float BroadcastingEnemyRange;
        private Dictionary<long, Ingame.IMyRadioAntenna> RadioAntennae = 
            new Dictionary<long, Ingame.IMyRadioAntenna>();
        private Dictionary<long, Ingame.IMyLaserAntenna> LaserAntennae = 
            new Dictionary<long, Ingame.IMyLaserAntenna>();
        private Dictionary<long, Ingame.IMyBeacon> Beacons = 
            new Dictionary<long, Ingame.IMyBeacon>();
        private Dictionary<long, Ingame.IMyBeacon> RadarBlocks = 
            new Dictionary<long, Ingame.IMyBeacon>();
        */

        #endregion
        #region Properties

        // ObserveableEntity
        public override EntityType TypeOfEntity {
            get { return EntityType.Grid; }
        }

        // ObservingEntity
        public override Dictionary<uint, Action> UpdateActions {
            get { return base.UpdateActions; }
        }
        public override String ComponentName { get { return "RevealedGrid"; } }

        // ConcealableGrid
        public IMyCubeGrid Grid { get; private set; }
        public List<long> SpawnOwners { get; private set; }
        public List<long> BigOwners { get; private set; }

        // RevealedGrid
        public bool NeededForSpawn { get; private set; }
        public bool IsProducing { get; private set; }
        public bool IsChargingBatteries { get; private set; }

        public override bool IsConcealable {
            get {
                return base.IsConcealable && !IsProducing && !NeededForSpawn &&
                    !IsProducing && !IsChargingBatteries;
            }
        }

        /*
        // ObservingEntity
        protected override uint DetectDistance {
            get {
                // TODO: Actually use greatest set radar distance from blocks
                if (HasRadar) return Settings.Instance.RevealVisibilityMeters;
                else return 0;
            }
        }

        protected override uint CommunicateDistance {
            get {
                // TODO: Actually use greatest set comm distance from blocks
                if (HasComms) return Settings.Instance.RevealDetectabilityMeters;
                else return 0;
            }
        }
        */
        #endregion
        #region Instance Event Helpers


        #endregion
        #region Constructors

        // Creation from ingame entity
        public RevealedGrid(IMyEntity entity) : base(entity) {
            Log.ClassName = "GP.Concealment.World.Entities.CubeGrid";
            Grid = Entity as IMyCubeGrid;
            SpawnOwners = new List<long>();
            BigOwners = new List<long>();
            Log.Trace("New CubeGrid " + Entity.EntityId + " " + DisplayName, "ctr");
        }

        // Byte Deserialization
        public RevealedGrid(ByteStream stream) : base(stream) {
            Log.ClassName = "GP.Concealment.World.Entities.CubeGrid";

            Log.Trace("Deserializing revealed grid", "stream ctr");
            Grid = Entity as IMyCubeGrid;
            SpawnOwners = stream.getLongList();
            BigOwners = stream.getLongList();

            if (SpawnOwners == null) {
                Log.Error("Deserialized with null spawnowners", "stream ctr");
                SpawnOwners = new List<long>();
            }

            if (BigOwners == null) {
                Log.Error("Deserialized with null BigOwners", "stream ctr");
                BigOwners = new List<long>();
            }


            Log.Trace("New CubeGrid " + Entity.EntityId + " " + DisplayName, "ctr");
        }

        #endregion
        #region Updates

        public override void Initialize() {
            base.Initialize();

            Grid.OnBlockAdded += BlockAdded;
            Grid.OnBlockRemoved += BlockRemoved;
        }

        protected override void Update() {
            //Log.Trace("Revealed Grid update " + DisplayName, "Update");
            base.Update();
        }

        public override void Terminate() {
            base.Terminate();

            Grid.OnBlockAdded -= BlockAdded;
            Grid.OnBlockRemoved -= BlockRemoved;
        }

        #endregion
        #region Block Events

        private void BlockAdded(IMySlimBlock block) {
            IMyCubeBlock fatblock = block.FatBlock;
            if (fatblock == null) return;  

            var producer = fatblock as Ingame.IMyProductionBlock;
            if (producer != null) {
                ProductionBlocks.Add(producer.EntityId, producer);
                return;
            }

            var medbay = fatblock as Ingame.IMyMedicalRoom;
            if (medbay != null) {
                MedBays.Add(medbay.EntityId, medbay);
                return;
            }

            var battery = fatblock as Ingame.IMyBatteryBlock;
            if (battery != null) {
                BatteryBlocks.Add(battery.EntityId, battery);
                return;
            }

        
            /*
            var radioAntenna = fatblock as Ingame.IMyRadioAntenna;
            if (radioAntenna != null) {
                RadioAntennae.Add(radioAntenna.EntityId, radioAntenna);
                return;
            }

            var laserAntenna = fatblock as Ingame.IMyLaserAntenna;
            if (laserAntenna != null) {
                LaserAntennae.Add(laserAntenna.EntityId, laserAntenna);
                return;
            }

            var beacon = fatblock as Ingame.IMyBeacon;
            if (beacon != null) {
                if (fatblock.BlockDefinition.SubtypeId.
                    IndexOf("radar", StringComparison.OrdinalIgnoreCase) >= 0) {

                    RadarBlocks.Add(beacon.EntityId, beacon);
                    return;
                }

                Beacons.Add(beacon.EntityId, beacon);
                return;
            }
            */
        }

        private void BlockRemoved(IMySlimBlock block) {
            IMyCubeBlock fatblock = block.FatBlock;
            if (fatblock == null) return;

            var producer = fatblock as Ingame.IMyProductionBlock;
            if (producer != null) {
                ProductionBlocks.Remove(producer.EntityId);
                return;
            }

            var medbay = fatblock as Ingame.IMyMedicalRoom;
            if (medbay != null) {
                MedBays.Remove(medbay.EntityId);
                return;
            }

            var battery = fatblock as Ingame.IMyBatteryBlock;
            if (battery != null) {
                BatteryBlocks.Remove(battery.EntityId);
                return;
            }

            /*
            var radioAntenna = fatblock as Ingame.IMyRadioAntenna;
            if (radioAntenna != null) {
                RadioAntennae.Remove(radioAntenna.EntityId);
                return;
            }

            var laserAntenna = fatblock as Ingame.IMyLaserAntenna;
            if (laserAntenna != null) {
                LaserAntennae.Remove(laserAntenna.EntityId);
                return;
            }

            var beacon = fatblock as Ingame.IMyBeacon;
            if (beacon != null) {
                if (fatblock.BlockDefinition.SubtypeId.
                    IndexOf("radar", StringComparison.OrdinalIgnoreCase) >= 0) {

                    RadarBlocks.Remove(beacon.EntityId);
                    return;
                }

                Beacons.Remove(beacon.EntityId);
                return;
            }
            */
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public virtual void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);

            Log.Trace("Serializing revealed grid", "AddToByteStream");
            if (SpawnOwners == null) {
                Log.Error("Serializing with null spawnowners", "AddToByteStream");
                SpawnOwners = new List<long>();
            }

            if (BigOwners == null) {
                Log.Error("Serializing with null BigOwners", "AddToByteStream");
                BigOwners = new List<long>();
            }

            stream.addLongList(SpawnOwners);
            stream.addLongList(BigOwners);
        }

        #endregion
        #region Update from Ingame state

        protected override void UpdateConcealability() {
            base.UpdateConcealability();
            UpdateIsProducing();
            UpdateIsChargingBatteries();
        }


        private void UpdateIsProducing() {
            // TODO: implement this
            IsProducing = false;


            // loop through production blocks and return with true if producing one
        }

        private void UpdateIsChargingBatteries() {
            // TODO: implement this
            IsChargingBatteries = false;

            // loop through battery blocks and return with true if charging one
        }

        #endregion


        private void RefreshProducing() {
            IsProducing = false;
            foreach (Ingame.IMyProductionBlock producer in ProductionBlocks.Values) {
                if (producer.Enabled && producer.IsProducing) {
                    IsProducing = true;
                    return;
                }
            }
        }

        // TODO: beacons, other types of antennae
        private void RefreshBroadcastRange() {
            /*
            RadioRange = 0;

            foreach(Ingame.IMyRadioAntenna antenna in RadioAntennae.Values) {
                if (!antenna.Enabled) continue;
                if (antenna.Radius > RadioRange) RadioRange = antenna.Radius;
            }
            */ 
        }

        protected override bool Conceal()  {
            Log.Trace("Concealing grid " + EntityId, "Conceal");

            if (Grid.SyncObject == null) {
                Log.Error("SyncObject missing, aborting", "Conceal");
                return false;
            }

            ConcealedGrid concealed = new ConcealedGrid(this);

            if (!concealed.IsXMLSerializable) {
                Log.Error("Won't be able to save this grid, aborting conceal.",
                    "Conceal");
                return false;
            }

            if (!ServerConcealSession.Instance.Manager.Concealed.AddGrid(concealed)) {
                Log.Error("Unable to add to concealed sector, aborting", "Conceal");
                return false;
            }

            // Remove it from the world
            Grid.SyncObject.SendCloseRequest();
            return true;
        }

        public String ConcealDetails() {
            String result = "";

            // Ids
            result += DisplayName + "\" - " + EntityId + "\n";

            // Owners
            // TODO: show owner names instead of playerIds
            result += "  Owners: TODO\n";
            /*
            if (BigOwners != null) {
                result += "  Owners: " + String.Join(", ", BigOwners) + "\n";
            }
            else {
                Log.Error("Grid had null BigOwners", "ReceiveRevealedGridsResponse");
            }
             * */

            // Position
            result += "  Position: " + Position.ToRoundedString() + "\n";

            // Concealability
            if (IsConcealable) {
                result += "  Concealable! Details:\n";
                //return result;
            }
            else {
                result += "  Not concealable:\n";
            }

            // Control
            if (IsControlled) {
                result += "    Controlled:\n";

                if (IsMoving) {
                    result += "      Moving at " + 
                        System.Math.Truncate(LinearVelocity.Length()) + " m/s";
                }
                else if (RecentlyMoved) {
                    result += "      Recently moved until " + RecentlyMovedEnds;
                }

                result += "\n";
            }
            else {
                result += "    Not Controlled. (OK to conceal.)\n";
            }

            // Observed
            if (IsObserved) {
                result += "    Observed by:\n";
                foreach (long id in EntitiesViewedBy.Keys) {
                    result += "      " + id;
                }
            }
            else {
                result += "    Not Observed. (OK to conceal.)\n";
            }

            // Spawn
            // TODO: show owner names instead of playerIds
            // TODO: actually implement updates to these details
            result += "    TODO: Needed for spawn? \n";
            /*
            if (NeededForSpawn) {
                result += "    Needed as a spawn point by:\n";
                foreach (long id in SpawnablePlayers) {
                    result += "      " + id;
                }
            }
            else {
                result += "    Not Needed as a spawn point. (OK to conceal.)\n";
            }
            */

            // Working
            // TODO: send block entity ids
            // TODO: show block types instead of entity Ids
            // TODO: actually implement updates to these details
            result += "    TODO: Producing? \n";
            result += "      TODO: Assembling? \n";
            /*
            if (IsProducing) {
                result += "    Some blocks are currently producing: by:\n";
                foreach (long id in ProductionBlocks.Keys) {
                    result += "      " + id;
                }
            }
            else {
                result += "    No blocks producing. (OK to conceal.)\n";
            }
            */

            result += "      TODO: Refining? \n";
            result += "      TODO: Charging? \n";

            result += "      TODO: Producing Oxygen? \n";

            // NearAsteroid
            result += "    Inside asteroid? \n";
            if (IsInsideAsteroid) {
                result += "    Yes, might affect asteroid respawn.\n";
            }
            else {
                result += "    Not in Asteroid. (OK to conceal.)\n";
            }


            // Blocked
            if (IsRevealBlocked) {
                result += "    Entities within bounding box, couldn't reveal.\n";
            }
            else {
                result += "    Not Blocked. (OK to conceal.)\n";
            }

            return result;
        }
    }

}
