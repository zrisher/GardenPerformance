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

        public IMyCubeGrid Grid { get; private set; }
        public List<long> SpawnOwners { get; private set; }
        public List<long> BigOwners { get; private set; }
        public bool IsInsideAsteroid { get; set; }

        // Working
        private bool IsProducing;
        private bool UpdateProducingNextUpdate;
        private Dictionary<long, Ingame.IMyProductionBlock> ProductionBlocks =
            new Dictionary<long, Ingame.IMyProductionBlock>();

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
        private bool NeededForSpawn;
        private List<long> SpawnablePlayers = new List<long>();
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

        //public EntityConcealability Concealability { get; private set; }

        #endregion
        #region Properties

        // ObserveableEntity
        public override EntityType TypeOfEntity {
            get { return EntityType.Grid; }
        }

        public override Dictionary<uint, Action> UpdateActions {
            get { return base.UpdateActions; }
        }

        public override String ComponentName { get { return "RevealedGrid"; } }

        public override bool IsConcealable {
            get {
                return base.IsConcealable && !IsProducing && !NeededForSpawn;
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

            /*
            var cockpit = fatblock as Ingame.IMyCockpit;
            if (cockpit != null) {
                // for some reason I think this is throwing the error, typeid null?
                if (fatblock.BlockDefinition.TypeId == typeof(MyObjectBuilder_CryoChamber)) {
                    Cyrochambers.Add(cockpit.EntityId, cockpit);
                    return;
                }
                Cockpits.Add(cockpit.EntityId, cockpit);
                return;
            }
            */

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

            /*
            var cockpit = fatblock as Ingame.IMyCockpit;
            if (cockpit != null) {
                if (fatblock.BlockDefinition.TypeId == typeof(MyObjectBuilder_CryoChamber)) {
                    Cyrochambers.Remove(cockpit.EntityId);
                    return;
                }

                Cockpits.Remove(cockpit.EntityId);
                return;
            }
            */

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


        private bool UpdateInsideAsteroid() {
            bool InsideAsteroid = false; 

            // Change number to variable later - half max asteroid size?
            BoundingSphereD bounds = new BoundingSphereD(Entity.GetPosition(), 1250);
            Log.Trace("Getting entities given a bound", "RefreshNearbyAsteroids");
            List<IMyEntity> nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);
            Log.Trace("Got entities given a bound", "RefreshNearbyAsteroids");

            List<IMyVoxelMap> nearbyRoids = nearbyEntities.
                Select((e) => e as IMyVoxelMap).Where((e) => e != null).ToList();

            foreach (IMyVoxelMap roid in nearbyRoids) {
                Log.Trace("Entity is near asteroid " + roid.EntityId, "RefreshNearbyAsteroids");
                BoundingSphere AsteroidHr = roid.WorldVolumeHr;
                BoundingSphere Asteroid = roid.WorldVolume;
                BoundingSphere AsteroidLocal = roid.LocalVolume;
                Log.Trace("Center of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Center, "CanConceal");
                Log.Trace("Radius of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Radius, "CanConceal");

                Log.Trace("Center of Asteroid using WorldVol BoundingSphere: " + Asteroid.Center, "CanConceal");
                Log.Trace("Radius of Asteroid using WorldVol BoundingSphere: " + Asteroid.Radius, "CanConceal");

                Log.Trace("Center of Asteroid using Local BoundingSphere: " + AsteroidLocal.Center, "CanConceal");
                Log.Trace("Radius of Asteroid using Local BoundingSphere: " + AsteroidLocal.Radius, "CanConceal");

                Log.Trace("Center of Asteroid using IMyEntity: " + roid.GetPosition(), "CanConceal");

                bounds = new BoundingSphereD(Asteroid.Center, Asteroid.Radius);
                nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);

                if (nearbyEntities.Contains(Entity)) {
                    InsideAsteroid = true;
                    return InsideAsteroid;
                }

                //concealability |= ConcealableEntity.EntityConcealability.NearAsteroid;
            }
            return InsideAsteroid;
        }

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

        protected override void Conceal() {
            //throw new NotImplementedException();
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
            result += "    TODO: Observed? \n";
            /*
            if (IsObserved) {
                result += "    Observed by:\n";
                foreach (long id in EntitiesViewedBy.Keys) {
                    result += "      " + id;
                }
            }
            else {
                result += "    Not Observed. (OK to conceal.)\n";
            }
            */

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

            // NearAsteroid
            // TODO
            result += "    TODO: Inside asteroid? \n";
            /*
            if (IsInsidesteroid) {
                result += "    Entities within bounding box, couldn't reveal.\n";
            }
            else {
                result += "    Not Blocked. (OK to conceal.)\n";
            }
             * */


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
