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

        #endregion
        #region Fields

        // ObserveableEntity
        protected Dictionary<long, RevealedEntity> EntitiesViewedBy =
            new Dictionary<long, RevealedEntity>();

        /*
        private Dictionary<long, RevealedEntity> EntitiesDetectedBy =
            new Dictionary<long, RevealedEntity>();
        private Dictionary<long, RevealedEntity> EntitiesBroadcastingTo =
            new Dictionary<long, RevealedEntity>();
        */

        #endregion
        #region Properties

        // The properties that we send to client are set instead of dynamically calculated
        // We could calculate these on the fly, but storing and updating them instead
        // allows us to let the client know exactly what the server sees when they're
        // sent in messages. It also allows us to delay updating them until needed.

        // ObserveableEntity
        public Vector3D Position { 
            get { return Entity.PositionComp.GetPosition(); } 
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

        // RevealedEntity
        public virtual bool IsConcealable { 
            get { return !IsRevealBlocked && !IsObserved; } 
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
            UpdateConcealability();
            stream.addBoolean(IsObserved);
            stream.addBoolean(IsRevealBlocked);
            stream.addLongList(EntitiesViewedBy.Keys.ToList());
        }

        #endregion
        #region Observed Marking

        public void MarkViewedBy(ObservingEntity e) {
            long id = e.EntityId;
            if (EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewedBy");
                return;
            }

            Log.Error("Adding " + id, "MarkViewedBy");
            EntitiesViewedBy.Add(id, e);
            UpdateObserveability();
        }

        public void UnmarkViewedBy(ObservingEntity e) {
            long id = e.EntityId;
            if (!EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewedBy");
                return;
            }

            Log.Error("Removing " + id, "UnmarkViewedBy");
            EntitiesViewedBy.Remove(id);
            UpdateObserveability();
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
        #region Update Attributes from Ingame data

        /// <summary>
        /// Should be called before concealing and before sending to clients
        /// </summary>
        protected virtual void UpdateConcealability() {
            UpdateRevealBlocked();
        }


        private void UpdateRevealBlocked() {
            IsRevealBlocked = false;

            Log.Trace("Begin UpdateRevealBlocked", "UpdateRevealBlocked");
            BoundingBoxD boxCopy = BoundingBox;
            List<IMyEntity> boundedEntities = MyAPIGateway.Entities.
                GetElementsInBox(ref boxCopy);

            Log.Trace("boundedEntities count " + boundedEntities.Count, "UpdateRevealBlocked");

            foreach (IMyEntity e in boundedEntities) {
                if (e.GetTopMostParent() != Entity) {
                    Log.Trace("Found an entity that's not a child", "UpdateRevealBlocked");
                    IsRevealBlocked = true;
                    return;
                }
            }

            Log.Trace("All entities in bounds are children.", "UpdateRevealBlocked");

            List<ConcealedEntity> concealedEntities = ServerConcealSession.Instance.
                Manager.Concealed.EntitiesInBox(boxCopy);

            Log.Trace("concealed boundedEntities count " + concealedEntities.Count, "UpdateRevealBlocked");

            if (concealedEntities.Count > 0) {
                Log.Trace("Found a concealed entity in the way", "UpdateRevealBlocked");
                IsRevealBlocked = true;
                return;
            }

            Log.Trace("All entities in concealed bounds are children.", "UpdateRevealBlocked");

        }

        private void UpdateObserveability() {
            IsObserved = (
                EntitiesViewedBy.Count > 0 //||
                //EntitiesDetectedBy.Count > 0 ||
                //EntitiesBroadcastingTo.Count > 0
                );
        }

        #endregion
        #region Reveal

        public virtual bool TryConceal() {
            UpdateConcealability();

            if (!IsConcealable) return false;

            Conceal();
            return true;
        }

        protected abstract void Conceal();

        #endregion

    }

}
