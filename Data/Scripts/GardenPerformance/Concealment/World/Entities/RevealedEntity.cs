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
        public Dictionary<long, int> ProxyIdsByTree { 
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
        public bool IsInAsteroid { get; set; }
        protected bool MovedSinceIsInAsteroidCheck { get; set; }

        public bool IsConcealable {
            get { return IsConcealableAuto && IsConcealableManual; } 
        }

        public virtual bool IsConcealableAuto {
            get { return !IsObserved && OldEnoughForConceal; }
        }

        public virtual bool IsConcealableManual {
            get { return !IsRevealBlocked && !IsInAsteroid; }
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

            MovedSinceIsInAsteroidCheck = true;
            Log.ClassName = "GP.Concealment.World.Entities.RevealedEntity";
            Log.Trace("Finished RevealedEntity deserialize constructor", "ctr");
        }

        // Creation from ingame entity
        public RevealedEntity(IMyEntity entity) : base(entity) {
            Log.Trace("Start RevealedEntity constructor", "ctr");
            MovedSinceIsInAsteroidCheck = true;
            Log.ClassName = "GP.Concealment.World.Entities.RevealedEntity";
            MarkNearbyObservingEntitiesForUpdate();
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
        #region Marking external world

        private void MarkNearbyObservingEntitiesForUpdate() {
            Log.Trace("begin", "MarkNearbyObservingEntitiesForUpdate");
            // Observing entities need to know there's a new entity to observe.
            // If they're not moving, they won't realize this has been added.
            var viewingSphere = new BoundingSphereD(Position, Settings.Instance.RevealVisibilityMeters);
            List<ObservingEntity> nearbyObserving = Sector.ObservingInSphere(viewingSphere);

            if (nearbyObserving.Count == 0) {
                Log.Trace("No nearby observing entities", "MarkNearbyObservingEntitiesForUpdate");
                //Log.Trace("viewingSphere has center " + Position + " and radius " + RevealVisibilityMeters, "MarkNearbyObservingEntitiesForUpdate");
                return;
            }

            Log.Trace("Marking " + nearbyObserving.Count + " nearby observing entities for observe update", "MarkNearbyObservingEntitiesForUpdate");
            foreach (ObservingEntity observing in nearbyObserving) {
                observing.MarkForObservingUpdate();
            }

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
            if (MovedSinceIsInAsteroidCheck) {
                UpdateIsInAsteroid();
            }
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

            //Log.Trace("Begin UpdateRevealBlocked", "UpdateRevealBlocked");
            BoundingBoxD boxCopy = BoundingBox;
            List<IMyEntity> boundedEntities = MyAPIGateway.Entities.
                GetElementsInBox(ref boxCopy);

            if (boundedEntities.Count == 0) {
                Log.Trace("No nearby entities, reveal is not blocked", "UpdateRevealBlocked");
                return;
            }

            foreach (IMyEntity e in boundedEntities) {
                if (e.GetTopMostParent() != Entity) {
                    Log.Trace("Found an entity in bounding box that's not a child, reveal is blocked", "UpdateRevealBlocked");
                    IsRevealBlocked = true;
                    return;
                }
            }

            //Log.Trace("All entities in bounds are children.", "UpdateRevealBlocked");

            List<ConcealedEntity> concealedEntities = Concealed.EntitiesInBox(boxCopy);

            //Log.Trace("concealed boundedEntities count " + concealedEntities.Count, "UpdateRevealBlocked");

            if (concealedEntities.Count > 0) {
                Log.Trace("Found a concealed entity in bounding box, reveal is blocked", "UpdateRevealBlocked");
                IsRevealBlocked = true;
                return;
            }

            Log.Trace("All entities in concealed bounds are children.", "UpdateRevealBlocked");

        }

        private void UpdateIsInAsteroid() {
            //Log.Trace("Begin UpdateInsideAsteroid", "UpdateIsInAsteroid");
            bool InsideAsteroid = false;

            // Change number to variable later - half max asteroid size?
            BoundingSphereD bounds = new BoundingSphereD(Entity.GetPosition(), 1250);
            //Log.Trace("Getting entities given a bound", "RefreshNearbyAsteroids");
            List<IMyEntity> nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);
            //Log.Trace("Got entities given a bound", "RefreshNearbyAsteroids");

            List<IMyVoxelMap> nearbyRoids = nearbyEntities.
                Select((e) => e as IMyVoxelMap).Where((e) => e != null).ToList();

            foreach (IMyVoxelMap roid in nearbyRoids) {
                //Log.Trace("Entity is near asteroid " + roid.EntityId, "UpdateIsInAsteroid");
                BoundingSphere worldSphere = roid.WorldVolume;

                //BoundingSphere AsteroidHr = roid.WorldVolumeHr;
                //BoundingSphere AsteroidLocal = roid.LocalVolume;
                /*
                Log.Trace("Center of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Center, "UpdateIsInAsteroid");
                Log.Trace("Radius of Asteroid using WorldVolHr BoundingSphere: " + AsteroidHr.Radius, "UpdateIsInAsteroid");
                Log.Trace("Center of Asteroid using WorldVol BoundingSphere: " + Asteroid.Center, "UpdateIsInAsteroid");
                Log.Trace("Radius of Asteroid using WorldVol BoundingSphere: " + Asteroid.Radius, "UpdateIsInAsteroid");
                Log.Trace("Center of Asteroid using Local BoundingSphere: " + AsteroidLocal.Center, "UpdateIsInAsteroid");
                Log.Trace("Radius of Asteroid using Local BoundingSphere: " + AsteroidLocal.Radius, "UpdateIsInAsteroid");
                Log.Trace("Center of Asteroid using IMyEntity: " + roid.GetPosition(), "UpdateIsInAsteroid");
                */

                bounds = new BoundingSphereD(worldSphere.Center, worldSphere.Radius);
                nearbyEntities = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);

                if (nearbyEntities.Contains(Entity)) {
                    Log.Trace("Entity is inside asteroid " + roid.EntityId, "UpdateIsInAsteroid");
                    InsideAsteroid = true;
                    return;
                }

                //concealability |= ConcealableEntity.EntityConcealability.NearAsteroid;
            }


            Log.Trace("Entity is not inside asteroid", "UpdateIsInAsteroid");
        }

        #endregion
        #region Conceal

        public virtual bool TryConceal() {
            if (!IsConcealableAuto) {
                Log.Trace("Grid failed auto concealable checks, can't conceal", "TryConceal");
                return false;
            }

            UpdateConcealabilityManual();

            if (!IsConcealableManual) {
                Log.Trace("Grid failed manual concealable checks, can't conceal", "TryConceal");
                return false;
            }

            return Conceal();
        }

        protected abstract bool Conceal();

        #endregion

    }

}
