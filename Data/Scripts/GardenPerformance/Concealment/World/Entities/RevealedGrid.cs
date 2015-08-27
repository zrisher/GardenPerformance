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
    public class RevealedGrid : ControllableEntity {

        /*
        private static ServerConcealSession Session {
            get { return ServerConcealSession.Instance; }
        }
        */

        #region Instance Fields

        private IMyCubeGrid Grid;
        private Dictionary<long, Ingame.IMyProductionBlock> ProductionBlocks;
        private Dictionary<long, Ingame.IMyRadioAntenna> Antennae;
        private Dictionary<long, Ingame.IMyLaserAntenna> LaserAntennae;

        public EntityConcealability Concealability { get; private set; }

        #endregion
        #region Instance Properties

        public override Dictionary<uint, Action> UpdateActions {
            get { return base.UpdateActions; }
        }

        #endregion
        #region Instance Event Helpers


        #endregion
        #region Constructors

        public RevealedGrid(IMyEntity entity) : base(entity) {
            Log = new Logger("GP.Concealment.World.Entities.CubeGrid",
                Entity.EntityId.ToString());

            Grid = Entity as IMyCubeGrid;

            Log.Trace("New CubeGrid " + Entity.EntityId + " " + DisplayName, "ctr");
        }

        public RevealedGrid(ByteStream stream) : base(stream) {
            // TODO: grab more details here and from controllable

            Log = new Logger("GP.Concealment.World.Entities.CubeGrid",
                Entity.EntityId.ToString());

            Grid = Entity as IMyCubeGrid;

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
            }

        }

        private void BlockRemoved(IMySlimBlock block) {

        }

        #endregion

        public virtual void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
        }

        private bool InsideAsteroid;
        private bool Producing;
        private float RadioRange;

        private void RefreshNearbyAsteroids() {
            InsideAsteroid = false; 

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
                    return;
                }

                //concealability |= ConcealableEntity.EntityConcealability.NearAsteroid;
            }

        }

        private void RefreshProducing() {
            Producing = false;
            foreach (Ingame.IMyProductionBlock producer in ProductionBlocks.Values) {
                if (producer.Enabled && producer.IsProducing) {
                    Producing = true;
                    return;
                }
            }
        }

        // TODO: beacons, other types of antennae
        private void RefreshBroadcastRange() {
            RadioRange = 0;

            foreach(Ingame.IMyRadioAntenna antenna in Antennae.Values) {
                if (!antenna.Enabled) continue;
                if (antenna.Radius > RadioRange) RadioRange = antenna.Radius;
            }
        }

    }

}
