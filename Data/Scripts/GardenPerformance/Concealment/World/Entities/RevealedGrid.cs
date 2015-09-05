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

    public class RevealedGrid : ObservingEntity, ConcealableGrid {

        #region Fields

        private Dictionary<long, Ingame.IMyProductionBlock> ProductionBlocks =
            new Dictionary<long, Ingame.IMyProductionBlock>();
        /*
        // Oxy production - oxy farms or generators might be generating oxy
        // TODO Break these out from production blocks and only block conceal
        // if they're both producing AND actually generating oxygen - could be leaks
        private Dictionary<long, Ingame.IMyOxygenFarm> OxyFarms =
            new Dictionary<long, Ingame.IMyOxygenFarm>();

        private Dictionary<long, Ingame.IMyOxygenGenerator> OxyGenerators =
            new Dictionary<long, Ingame.IMyOxygenGenerator>();
        */

        // Power production - solar panels might be charging batteries
        private Dictionary<long, Ingame.IMyBatteryBlock> BatteryBlocks =
            new Dictionary<long, Ingame.IMyBatteryBlock>();

        // Control
        // We can't get the pilots from the grid, we have to go from the character
        // But we already reveal near a character.
        // Plus we can't detect AI pilots at all.
        // So we just base control on moving or moved in past X minutes
        // Unfortunately ControllerInfo isn't whitelisted, so we just guess if 
        // it's controlled by whether it's moving. Character entities report presence separately. 
        /*
        private bool Piloted;
        private bool UpdatePilotedNextUpdate;
        private Dictionary<long, Ingame.IMyCockpit> Cockpits =
            new Dictionary<long, Ingame.IMyCockpit>();
        public bool Controlled { get { return ControlsInUse.Count > 0; } }
        */

        // Spawn
        private bool UpdateNeededForSpawnNextUpdate;
        private Dictionary<long, Ingame.IMyMedicalRoom> MedBays =
            new Dictionary<long, Ingame.IMyMedicalRoom>();
        private Dictionary<long, Sandbox.Game.Entities.Blocks.MyCryoChamber> Cryochambers =
            new Dictionary<long, Sandbox.Game.Entities.Blocks.MyCryoChamber>();

        // Comms
        // These are all just to keep us from having to reveal at greater distances 
        /*
        // TODO: beacons and antenna broadcast to both friends and enemies
        // laser antennae just to friends
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
        /*
        public override Dictionary<uint, Action> UpdateActions {
            get { return base.UpdateActions; }
        }
        */

        public override String ComponentName { get { return "RevealedGrid"; } }

        // ConcealableGrid
        public IMyCubeGrid Grid { get; private set; }
        public List<long> SpawnOwners { get; private set; }
        public List<long> BigOwners { get; private set; }

        // RevealedGrid
        public bool NeededForSpawn { get; private set; }
        public bool IsProducing { get; private set; }
        public bool IsChargingBatteries { get; private set; }

        public override bool IsConcealableAuto {
            get { return base.IsConcealableAuto && !NeededForSpawn; }
        }

        public override bool IsConcealableManual {
            get {
                return base.IsConcealableManual && !IsProducing && !IsChargingBatteries;
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
            Log.Info("Pos " + stream.Position + " / " + stream.Length, "FromBytes");
            Grid = Entity as IMyCubeGrid;
            Log.Info("Getting spawnowners ", "FromBytes");
            SpawnOwners = stream.getLongList();
            Log.Info("Getting bigowners ", "FromBytes");
            BigOwners = stream.getLongList();
            Log.Info("Finished, pos " + stream.Position + " / " + stream.Length, "FromBytes");

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
        #region Serialization

        // Byte Serialization
        public virtual void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);

            Log.Trace("Adding Revealed Grid to byte stream", "AddToByteStream");

            if (SpawnOwners == null) {
                Log.Error("Serializing with null spawnowners", "AddToByteStream");
                SpawnOwners = new List<long>();
            }

            if (BigOwners == null) {
                Log.Error("Serializing with null BigOwners", "AddToByteStream");
                BigOwners = new List<long>();
            }

            Log.Info("Adding spawnowners ", "FromBytes");
            stream.addLongList(SpawnOwners);
            Log.Info("Adding BigOwners ", "FromBytes");
            stream.addLongList(BigOwners);
        }

        #endregion
        #region Instance Event Helpers

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
                UpdateNeededForSpawnNextUpdate = true;
                return;
            }

            var cryochamber = fatblock as Sandbox.Game.Entities.Blocks.MyCryoChamber;
            if (cryochamber != null) {
                Cryochambers.Add(cryochamber.EntityId, cryochamber);
                UpdateNeededForSpawnNextUpdate = true;
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
                UpdateNeededForSpawnNextUpdate = true;
                return;
            }

            var cryochamber = fatblock as Sandbox.Game.Entities.Blocks.MyCryoChamber;
            if (cryochamber != null) {
                Cryochambers.Remove(cryochamber.EntityId);
                UpdateNeededForSpawnNextUpdate = true;
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
        #region Updates

        public override void Initialize() {
            base.Initialize();

            Grid.OnBlockAdded += BlockAdded;
            Grid.OnBlockRemoved += BlockRemoved;
        }

        protected override void UpdateConcealabilityAuto(){
            base.UpdateConcealabilityAuto();
            if (UpdateNeededForSpawnNextUpdate) {
                UpdateNeededForSpawn();
                UpdateNeededForSpawnNextUpdate = false;
            }
        }

        protected override void UpdateConcealabilityManual() {
            base.UpdateConcealabilityManual();
            UpdateIsProducing();
            UpdateIsChargingBatteries();
        }


        public override void Terminate() {
            base.Terminate();

            Grid.OnBlockAdded -= BlockAdded;
            Grid.OnBlockRemoved -= BlockRemoved;
        }

        #endregion
        #region Concealability Updates

        private void UpdateIsChargingBatteries() {
            // TODO: implement this
            IsChargingBatteries = false;
            // loop through battery blocks and return with true if charging one
            // Cache what production was last time and only mark charging if 
            // charge is going up
        }

        private void UpdateIsProducingOxygen() {
            // TODO: implement this
            // loop through oxy blocks and return with true if charging one
            // Cache what production was last time and only mark charging if 
            // charge is going up
        }

        private void UpdateIsProducing() {
            IsProducing = false;
            foreach (Ingame.IMyProductionBlock producer in ProductionBlocks.Values) {
                if (producer.Enabled && producer.IsProducing) {
                    IsProducing = true;
                    return;
                }
            }
        }

        // TODO: beacons, other types of antennae
        private void UpdateBroadcastRange() {
            /*
            RadioRange = 0;

            foreach(Ingame.IMyRadioAntenna antenna in RadioAntennae.Values) {
                if (!antenna.Enabled) continue;
                if (antenna.Radius > RadioRange) RadioRange = antenna.Radius;
            }
            */ 
        }

        private void UpdateNeededForSpawn() {
            NeededForSpawn = false;

            // If we want to only use Working blocks, need hooks

            foreach (var medbay in MedBays.Values) {
                if (Sector.SpawnOwnerNeeded(medbay.OwnerId)) {
                    NeededForSpawn = true;
                    return;
                }
            }

            foreach (var cryochamber in Cryochambers.Values) {
                if (Sector.SpawnOwnerNeeded(cryochamber.OwnerId)) {
                    NeededForSpawn = true;
                    return;
                }
            }
        }

        #endregion
        #region Public Marking

        public void MarkSpawnUpdateNeeded() {
            UpdateNeededForSpawnNextUpdate = true;
        }

        #endregion
        #region Conceal

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

        #endregion
        #region Describe
    
        public String ConcealDetails() {
            // Revealed entities cannot be concealed if they
            // Are controlled (i.e. moving)
            // Are near a controlled entity
            // Are "working" (refining, assembling, oxy creating, battery charging)
            // Are needed as a spawn point for a logged-in player

            String result = "";

            // Ids
            result += "\"" + DisplayName + "\" - " + EntityId + "\n";

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
                result += "  Concealable:\n";
                //return result;
            }
            else {
                result += "  Not concealable:\n";
            }

            //result += "    Frequently Updated:\n";

            // Control
            if (IsControlled) {
                result += "      N Controlled:\n";

                if (IsMoving) {
                    result += "        Moving at " + 
                        System.Math.Truncate(LinearVelocity.Length()) + " m/s";
                }
                else if (RecentlyMoved) {
                    result += "        Recently moved until " + RecentlyMovedEnds;
                }

                result += "\n";
            }
            else {
                result += "      Y Not Controlled.\n";
            }

            // Observed
            if (IsObserved) {
                result += "      N Observed by:\n";
                foreach (long id in EntitiesViewedBy.Keys) {
                    result += "        " + id + "\n";
                }
            }
            else {
                result += "      Y Not Observed.\n";
            }

            // Spawn
            // TODO: show owner names instead of playerIds
            // TODO: actually implement updates to these details
            if (NeededForSpawn) {
                result += "      N Needed for spawn.\n";
                /*
                result += "    Needed as a spawn point by:\n";
                foreach (long id in SpawnablePlayers) {
                    result += "      " + id;
                }
                */
            }
            else {
                result += "      Y Not Needed for spawn.\n";
            }

            //result += "    Infrequently Updated:\n";

            // Working
            // TODO: send block entity ids
            // TODO: show block types instead of entity Ids
            if (IsProducing) {
                result += "    N Producing:\n";
                foreach (long id in ProductionBlocks.Keys) {
                    result += "      " + id + "\n";
                }
            }
            else {
                result += "      Y Not Producing.\n";
            }

            if (IsChargingBatteries) {
                result += "      N Charging Batteries.\n";
            }
            else {
                result += "      Y Not Charging Batteries.\n";
            }

            // NearAsteroid
            if (IsInsideAsteroid) {
                result += "      N Inside Asteroid.\n";
            }
            else {
                result += "      Y Not in Asteroid.\n";
            }

            // Blocked
            if (IsRevealBlocked) {
                result += "      N Entities within bounding box.\n";
            }
            else {
                result += "      Y No entities in bounding box.\n";
            }

            return result;
        }

        #endregion

    }

}
