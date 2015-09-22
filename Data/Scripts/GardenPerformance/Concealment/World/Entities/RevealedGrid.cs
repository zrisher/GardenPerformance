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
        // Plus we can't detect AI pilots at all.
        // So we just base control on moving or moved in past X minutes
        // Unfortunately ControllerInfo isn't whitelisted, so we just guess if 
        // it's controlled by whether it's moving. 
        private bool IsPiloted;
        private bool UpdatePilotedNextUpdate;
        private Dictionary<long, Ingame.IMyCockpit> Cockpits =
            new Dictionary<long, Ingame.IMyCockpit>();
        private Dictionary<long, Ingame.IMyRemoteControl> RemoteControls =
            new Dictionary<long, Ingame.IMyRemoteControl>();

        public override bool IsControlled {
            get { return base.IsControlled || IsPiloted; }
        }


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
            Log.ClassName = "GP.Concealment.World.Entities.RevealedGrid";
            Grid = Entity as IMyCubeGrid;
            SpawnOwners = new List<long>();
            BigOwners = new List<long>();
            Log.Trace("New CubeGrid " + DisplayName, "ctr");
        }

        // Byte Deserialization
        public RevealedGrid(ByteStream stream) : base(stream) {
            Log.ClassName = "GP.Concealment.World.Entities.RevealedGrid";

            Log.Trace("Deserializing revealed grid", "stream ctr");
            //Log.Info("Pos " + stream.Position + " / " + stream.Length, "FromBytes");
            Grid = Entity as IMyCubeGrid;
            //Log.Info("Getting spawnowners ", "FromBytes");
            SpawnOwners = stream.getLongList();
            //Log.Info("Getting bigowners ", "FromBytes");
            BigOwners = stream.getLongList();
            //Log.Info("Finished, pos " + stream.Position + " / " + stream.Length, "FromBytes");

            if (SpawnOwners == null) {
                Log.Error("Deserialized with null spawnowners", "stream ctr");
                SpawnOwners = new List<long>();
            }

            if (BigOwners == null) {
                Log.Error("Deserialized with null BigOwners", "stream ctr");
                BigOwners = new List<long>();
            }

            NeededForSpawn = stream.getBoolean();
            IsProducing = stream.getBoolean();
            IsChargingBatteries = stream.getBoolean();
            IsPiloted = stream.getBoolean();


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
            stream.addBoolean(NeededForSpawn);
            stream.addBoolean(IsProducing);
            stream.addBoolean(IsChargingBatteries);
            stream.addBoolean(IsPiloted);
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
                Log.Trace("Adding medbay owned by " + medbay.OwnerId, "BlockAdded");
                //Log.Trace("Fatblock ownerId " + fatblock.OwnerId, "BlockAdded");
                //Log.Trace("Fatblock cubeblock builder ownerId " + fatblock.GetObjectBuilderCubeBlock().Owner, "BlockAdded");
                MedBays.Add(medbay.EntityId, medbay);
                UpdateNeededForSpawnNextUpdate = true;
                return;
            }

            var cryochamber = fatblock as Sandbox.Game.Entities.Blocks.MyCryoChamber;
            if (cryochamber != null) {
                Log.Trace("Adding cryochamber owned by " + cryochamber.OwnerId, "BlockAdded");

                Cryochambers.Add(cryochamber.EntityId, cryochamber);
                UpdateNeededForSpawnNextUpdate = true;
                return;
            }

            // Must be after cryochamber - they are also cockpits
            var cockpit = fatblock as Ingame.IMyCockpit;
            if (cockpit != null) {
                Log.Trace("Adding cockpit owned by " + cockpit.OwnerId, "BlockAdded");
                Cockpits.Add(cockpit.EntityId, cockpit);
                return;
            }

            var battery = fatblock as Ingame.IMyBatteryBlock;
            if (battery != null) {
                Log.Trace("Adding battery owned by " + battery.OwnerId, "BlockAdded");
                BatteryBlocks.Add(battery.EntityId, battery);
                return;
            }

            var remoteControl = fatblock as Ingame.IMyRemoteControl;
            if (remoteControl != null) {
                Log.Trace("Adding remote control owned by " + remoteControl.OwnerId, "BlockAdded");
                RemoteControls.Add(remoteControl.EntityId, remoteControl);
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

            fatblock.IsWorkingChanged -= BlockWorkingChanged;

            var producer = fatblock as Ingame.IMyProductionBlock;
            if (producer != null) {
                ProductionBlocks.Remove(producer.EntityId);
                return;
            }

            var medbay = fatblock as Ingame.IMyMedicalRoom;
            if (medbay != null) {
                Log.Trace("Removing medbay owned by " + medbay.OwnerId, "BlockRemoved");
                MedBays.Remove(medbay.EntityId);
                UpdateNeededForSpawnNextUpdate = true;
                return;
            }

            var cryochamber = fatblock as Sandbox.Game.Entities.Blocks.MyCryoChamber;
            if (cryochamber != null) {
                Log.Trace("Removing cryochamber owned by " + cryochamber.OwnerId, "BlockRemoved");
                Cryochambers.Remove(cryochamber.EntityId);
                UpdateNeededForSpawnNextUpdate = true;
                return;
            }

            // Must be after cryochamber - they are also cockpits
            var cockpit = fatblock as Ingame.IMyCockpit;
            if (cockpit != null) {
                Log.Trace("Removing cockpit owned by " + cockpit.OwnerId, "BlockRemoved");
                Cockpits.Remove(cockpit.EntityId);
                return;
            }

            var battery = fatblock as Ingame.IMyBatteryBlock;
            if (battery != null) {
                Log.Trace("Removing battery owned by " + battery.OwnerId, "BlockRemoved");
                BatteryBlocks.Remove(battery.EntityId);
                return;
            }

            var remoteControl = fatblock as Ingame.IMyRemoteControl;
            if (remoteControl != null) {
                Log.Trace("Removing remote control owned by " + remoteControl.OwnerId, "BlockRemoved");
                RemoteControls.Remove(remoteControl.EntityId);
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

        private void BlockOwnerChanged(IMyCubeGrid fatblock) {
            Log.Trace("Owner changed for some block on this grid.", "BlockOwnerChanged");
            UpdateNeededForSpawnNextUpdate = true;
        }

        private void BlockWorkingChanged(IMyCubeBlock fatblock) {
            Log.Trace("IsWorking changed for block " + fatblock.BlockDefinition.TypeId + " " + fatblock.EntityId, "IsWorkingChanged");
            // TODO use this to keep only updated lists of working medbays, production, etc
            // Maybe this can let us know when producion stops?
        }

        #endregion
        #region Updates

        public override void Initialize() {
            base.Initialize();

            List<IMySlimBlock> allBlocks = new List<IMySlimBlock>();
            Grid.GetBlocks(allBlocks);
            foreach (var block in allBlocks) {
                BlockAdded(block);
            }

            Grid.OnBlockAdded += BlockAdded;
            Grid.OnBlockRemoved += BlockRemoved;
            Grid.OnBlockOwnershipChanged += BlockOwnerChanged;
        }

        protected override void UpdateConcealabilityAuto(){
            base.UpdateConcealabilityAuto();
            if (UpdateNeededForSpawnNextUpdate) {
                UpdateNeededForSpawn();
                UpdateNeededForSpawnNextUpdate = false;
            }
        }

        public override void UpdateConcealabilityManual() {
            base.UpdateConcealabilityManual();
            UpdateIsProducing();
            UpdateIsChargingBatteries();
        }

        protected override void UpdateControl() {
            base.UpdateControl();
            UpdateIsPiloted();
        }


        public override void Terminate() {
            base.Terminate();

            Grid.OnBlockAdded -= BlockAdded;
            Grid.OnBlockRemoved -= BlockRemoved;
            Grid.OnBlockOwnershipChanged -= BlockOwnerChanged;
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

        private void UpdateIsPiloted() {
            bool wasControlled = IsControlled;
            IsPiloted = CheckIsPiloted();

            if (IsControlled && !wasControlled) {
                ControlAcquired();
            }
            else if (!IsControlled && wasControlled) {
                ControlReleased();
            }
        }

        private bool CheckIsPiloted() {

            foreach (var cockpit in Cockpits.Values) {
                //Log.Trace("Looping cockpit", "UpdateIsPiloted");
                if (cockpit.IsUnderControl || cockpit.GetPilot() != null) {
                    Log.Trace("Is Piloted via cockpit", "UpdateIsPiloted");
                    return true;
                }
            }

            foreach (var cryochamber in Cryochambers.Values) {
                //Log.Trace("Looping cryochamber", "UpdateIsPiloted");
                var asCockpit = cryochamber as Ingame.IMyCockpit;
                if (asCockpit.IsUnderControl || asCockpit.GetPilot() != null) {
                    Log.Trace("Is Piloted via cryochamber", "UpdateIsPiloted");
                    return true;
                }
            }

            foreach (var remoteControl in RemoteControls.Values) {
                if (remoteControl.IsUnderControl) {
                    Log.Trace("Is Piloted via remote control", "UpdateIsPiloted");
                    return true;
                }
            }

            Log.Trace("Not piloted", "UpdateIsPiloted");
            return false;
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
            //Log.Trace("Update needed for spawn, marking as unneeded to start", "UpdateNeededForSpawn");
            //Log.Trace("Medbay owners: " + String.Join(", ", MedBays.Values.Select((x) => x.OwnerId).ToList()), "UpdateNeededForSpawn");
            //Log.Trace("Cryo owners: " + String.Join(", ", Cryochambers.Values.Select((x) => x.OwnerId).ToList()), "UpdateNeededForSpawn");
            NeededForSpawn = false;

            // If we want to only use Working blocks, need hooks

            foreach (var medbay in MedBays.Values) {
                if (Sector.SpawnOwnerNeeded(medbay.OwnerId)) {
                    Log.Trace("Medbay needed for spawn", "UpdateNeededForSpawn");
                    NeededForSpawn = true;
                    return;
                }
            }

            foreach (var cryochamber in Cryochambers.Values) {
                if (Sector.SpawnOwnerNeeded(cryochamber.OwnerId)) {
                    Log.Trace("Cryochamber needed for spawn", "UpdateNeededForSpawn");
                    NeededForSpawn = true;
                    return;
                }
            }

            Log.Trace("Not needed for spawn", "UpdateNeededForSpawn");
        }

        #endregion
        #region Public Marking

        public void MarkSpawnUpdateNeeded() {
            Log.Trace("Marked for spawn update", "MarkSpawnUpdateNeeded");
            UpdateNeededForSpawnNextUpdate = true;
        }

        #endregion
        #region Conceal

        protected override bool Conceal()  {
            Log.Trace("Concealing.", "Conceal");

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
            if (OldEnoughForConceal) {
                result += "      Y Not recently revealed\n";
            }
            else {
                result += "      N Too recently revealed\n";
            }

            // Control
            if (IsControlled) {
                result += "      N Controlled:\n";

                if (IsPiloted) {
                    result += "        Piloted";
                }
                else if (IsMoving) {
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
                    result += "            " + id + "\n";
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
                result += "      N Producing:\n";
                //foreach (long id in ProductionBlocks.Keys) {
                //    result += "      " + id + "\n";
                //}
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
            if (IsInAsteroid) {
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
