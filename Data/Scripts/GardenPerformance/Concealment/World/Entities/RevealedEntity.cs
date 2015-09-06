using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

using SEGarden.Extensions;
using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

using GP.Concealment.World.Sectors;
using GP.Concealment.Sessions;

namespace GP.Concealment.World.Entities {

    public abstract class RevealedEntity : EntityComponent, ObservableEntity, AABBEntity {

        #region Static

        protected static RevealedSector Sector {
            get { return ServerConcealSession.Instance.Manager.Revealed; }
        }

        protected static ConcealedSector Concealed {
            get { return ServerConcealSession.Instance.Manager.Concealed; }
        }

        #endregion
        #region Fields

        // ObserveableEntity
        private bool UpdateIsObservedNextUpdate = true;
        protected Dictionary<long, RevealedEntity> EntitiesViewedBy =
            new Dictionary<long, RevealedEntity>();

        /*
        private Dictionary<long, RevealedEntity> EntitiesDetectedBy =
            new Dictionary<long, RevealedEntity>();
        private Dictionary<long, RevealedEntity> EntitiesBroadcastingTo =
            new Dictionary<long, RevealedEntity>();
        */

        private DateTime RevealedAt = DateTime.Now;

        #endregion
        #region Properties

        // The properties that we send to client are set instead of dynamically calculated
        // We could calculate these on the fly, but storing and updating them instead
        // allows us to let the client know exactly what the server sees when they're
        // sent in messages. It also allows us to delay updating them until needed.

        // ObserveableEntity
        public Vector3D Position { 
            get {
                return Entity.GetPosition(); 
            } 
        }
        public bool IsObserved { get; private set; }
        public abstract EntityType TypeOfEntity { get; }

        // AABBEntity
        public BoundingBoxD BoundingBox {
            get { return Entity.PositionComp.WorldAABB; } 
        }
        public int TreeProxyID { 
            get; set; 
        }
        public Vector3D WorldTranslation {
            get { return Entity.PositionComp.WorldMatrix.Translation; }
        }
        public Vector3D LinearVelocity { 
            get { return Entity.Physics.LinearVelocity; } 
        }

        // ConcealableEntity
        public bool IsRevealBlocked { get; set; }

        // EntityComponent
        public override Dictionary<uint, Action> UpdateActions {
            get {
                var actions = base.UpdateActions;
                actions.Add(65, UpdateConcealabilityAuto); // TODO tweak resolution
                return actions;
            }
        }

        // RevealedEntity
        public bool IsInsideAsteroid { get; set; }

        public bool IsConcealable {
            get { return IsConcealableAuto && IsConcealableManual; } 
        }

        public virtual bool IsConcealableAuto {
            get { return !IsObserved && OldEnoughForConceal; }
        }

        public virtual bool IsConcealableManual {
            get { return !IsRevealBlocked && !IsInsideAsteroid; }
        }

        protected bool OldEnoughForConceal {
            get {
                return DateTime.Now > RevealedAt.AddSeconds(Settings.Instance.
                    RevealedMinAgeSeconds);
            }
        }

        #endregion
        #region Constructors

        // Byte Deserialization
        public RevealedEntity(VRage.ByteStream stream) : base(stream) {
            // Nearly everything is available from the ingame Entity
            IsObserved = stream.getBoolean();
            IsRevealBlocked = stream.getBoolean();
            
            List<long> entitiesViewedByList = stream.getLongList();
            foreach (long id in entitiesViewedByList) {
                EntitiesViewedBy.Add(id, null);
            }

            RevealedAt = stream.getDateTime();

            Log.ClassName = "GP.Concealment.World.Entities.RevealedEntity";
            Log.Trace("Finished RevealedEntity deserialize constructor", "ctr");
        }

        // Creation from ingame entity
        public RevealedEntity(IMyEntity entity) : base(entity) {
            Log.ClassName = "GP.Concealment.World.Entities.RevealedEntity";
            Log.Trace("Finished RevealedEntity constructor", "ctr");
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
            UpdateConcealabilityManual();
            stream.addBoolean(IsObserved);
            stream.addBoolean(IsRevealBlocked);
            stream.addLongList(EntitiesViewedBy.Keys.ToList());
            stream.addDateTime(RevealedAt);
        }

        #endregion
        #region Public Marking

        public void MarkViewedBy(ObservingEntity e) {
            long id = e.EntityId;

            if (id == EntityId) {
                Log.Warning("Tried to view itself " + id, "MarkViewedBy");
                return;
            }

            if (EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewedBy");
                return;
            }

            Log.Trace("Viewed by " + id, "MarkViewedBy");
            EntitiesViewedBy.Add(id, e);
            UpdateIsObservedNextUpdate = true;
        }

        public void UnmarkViewedBy(ObservingEntity e) {
            long id = e.EntityId;

            if (id == EntityId) {
                Log.Warning("Tried to unview itself " + id, "UnmarkViewedBy");
                return;
            }

            if (!EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewedBy");
                return;
            }

            Log.Trace("No longer viewed by " + id, "UnmarkViewedBy");
            EntitiesViewedBy.Remove(id);
            UpdateIsObservedNextUpdate = true;
        }

        /*
        public void MarkDetectedBy(RevealedEntity e) {
            long id = e.EntityId;
            if (EntitiesDetectedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkDetectedBy");
                return;
            }

            Log.Error("Adding " + id, "MarkDetectedBy");
            EntitiesDetectedBy.Add(id, e);
            UpdateObserveability();
        }

        public void UnmarkDetectedBy(RevealedEntity e) {
            long id = e.EntityId;
            if (!EntitiesDetectedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkDetectedBy");
                return;
            }

            Log.Error("Removing " + id, "UnmarkDetectedBy");
            EntitiesDetectedBy.Remove(id);
            UpdateObserveability();
        }

        public void MarkBroadcastingTo(RevealedEntity e) {
            long id = e.EntityId;
            if (EntitiesBroadcastingTo.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkBroadcastingTo");
                return;
            }

            Log.Error("Adding " + id, "MarkBroadcastingTo");
            EntitiesBroadcastingTo.Add(id, e);
            UpdateObserveability();
        }

        public void UnmarkBroadcastingTo(RevealedEntity e) {
            long id = e.EntityId;
            if (!EntitiesBroadcastingTo.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkBroadcastingTo");
                return;
            }

            Log.Error("Removing " + id, "UnmarkBroadcastingTo");
            EntitiesBroadcastingTo.Remove(id);
            UpdateObserveability();
        }
        */

        #endregion
        #region Concealability Updates

        /// <summary>
        /// Should be called before concealing and before sending to clients
        /// Call this to update things that aren't automatically kept up to date
        /// </summary>
        public virtual void UpdateConcealabilityManual() {
            UpdateRevealBlocked();
            UpdateInsideAsteroid();
        }

        protected virtual void UpdateConcealabilityAuto() {
            if (UpdateIsObservedNextUpdate) {
                UpdateObserveability();
                UpdateIsObservedNextUpdate = false;
            }
        }

        private void UpdateObserveability() {
            IsObserved = (
                EntitiesViewedBy.Count > 0 //||
                //EntitiesDetectedBy.Count > 0 ||
                //EntitiesBroadcastingTo.Count > 0
                );
        }

        private void UpdateRevealBlocked() {
            IsRevealBlocked = false;

            Log.Trace("Begin UpdateRevealBlocked", "UpdateRevealBlocked");
            BoundingBoxD boxCopy = BoundingBox;
            List<IMyEntity> boundedEntities = MyAPIGateway.Entities.
                GetElementsInBox(ref boxCopy);

            //Log.Trace("boundedEntities count " + boundedEntities.Count, "UpdateRevealBlocked");

            foreach (IMyEntity e in boundedEntities) {
                if (e.GetTopMostParent() != Entity) {
                    Log.Trace("Found an entity that's not a child", "UpdateRevealBlocked");
                    IsRevealBlocked = true;
                    return;
                }
            }

            //Log.Trace("All entities in bounds are children.", "UpdateRevealBlocked");

            List<ConcealedEntity> concealedEntities = Concealed.EntitiesInBox(boxCopy);

            //Log.Trace("concealed boundedEntities count " + concealedEntities.Count, "UpdateRevealBlocked");

            if (concealedEntities.Count > 0) {
                Log.Trace("Found a concealed entity in the way", "UpdateRevealBlocked");
                IsRevealBlocked = true;
                return;
            }

            Log.Trace("All entities in concealed bounds are children.", "UpdateRevealBlocked");

        }

        private bool UpdateInsideAsteroid() {
            Log.Trace("Begin UpdateInsideAsteroid", "UpdateInsideAsteroid");
            bool InsideAsteroid = false;

            // Change number to variable later - half max asteroid size?
            BoundingSphereD bounds = new BoundingSphereD(Entity.GetPosition(), 1250);
            //Log.Trace("Getting entities given a bound", "RefreshNearbyAsteroids");
            List<IMyEntity> nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);
            //Log.Trace("Got entities given a bound", "RefreshNearbyAsteroids");

            List<IMyVoxelMap> nearbyRoids = nearbyEntities.
                Select((e) => e as IMyVoxelMap).Where((e) => e != null).ToList();

            foreach (IMyVoxelMap roid in nearbyRoids) {
                Log.Trace("Entity is near asteroid " + roid.EntityId, "UpdateInsideAsteroid");

                //BoundingSphere AsteroidHr = roid.WorldVolumeHr;
                BoundingSphere Asteroid = roid.WorldVolume;
                //BoundingSphere AsteroidLocal = roid.LocalVolume;
                /*
                Log.Trace("Center of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Center, "UpdateInsideAsteroid");
                Log.Trace("Radius of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Radius, "UpdateInsideAsteroid");
                */
                Log.Trace("Center of Asteroid using WorldVol BoundingSphere: " + Asteroid.Center, "UpdateInsideAsteroid");
                Log.Trace("Radius of Asteroid using WorldVol BoundingSphere: " + Asteroid.Radius, "UpdateInsideAsteroid");
                /*
                Log.Trace("Center of Asteroid using Local BoundingSphere: " + AsteroidLocal.Center, "UpdateInsideAsteroid");
                Log.Trace("Radius of Asteroid using Local BoundingSphere: " + AsteroidLocal.Radius, "UpdateInsideAsteroid");
                */
                Log.Trace("Center of Asteroid using IMyEntity: " + roid.GetPosition(), "UpdateInsideAsteroid");

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

        #endregion
        #region Conceal

        public virtual bool TryConceal() {
            UpdateConcealabilityManual();

            if (!IsConcealable) {
                Log.Trace("Grid is not concealable", "TryConceal");
                return false;
            }

            return Conceal();
        }

        protected abstract bool Conceal();

        #endregion

    }

}
