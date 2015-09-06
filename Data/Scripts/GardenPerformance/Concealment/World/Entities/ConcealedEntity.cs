using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

using SEGarden.Extensions;
using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

using GP.Concealment.Sessions;
using GP.Concealment.World.Sectors;

namespace GP.Concealment.World.Entities {

    public abstract class ConcealedEntity : ConcealableEntity, AABBEntity {

        #region Static

        protected static RevealedSector Revealed {
            get { return ServerConcealSession.Instance.Manager.Revealed; }
        }

        #endregion
        #region Fields

        protected Logger Log;           

        // ObserveableEntity
        private Dictionary<long, RevealedEntity> EntitiesViewedBy =
            new Dictionary<long, RevealedEntity>();
        /*
        private Dictionary<long, RevealedEntity> EntitiesDetectedBy =
            new Dictionary<long, RevealedEntity>();
        private Dictionary<long, RevealedEntity> EntitiesBroadcastingTo =
            new Dictionary<long, RevealedEntity>();
        */ 

        #endregion
        #region Properties

        // All the XML-saved properties need to be manually set
        // The properties that we send to client are set instead of dynamically calculated too.
        // We could calculate these on the fly, but storing and updating them instead
        // allows us to let the client know exactly what the server sees when they're
        // sent in messages. It also allows us to delay updating them until needed.

        // ObserveableEntity
        public EntityType TypeOfEntity { get; set; }
        public long EntityId { get; set; }
        public String DisplayName { get; set;  }
        public Vector3D Position { get; set; }
        [XmlIgnore]
        public bool IsObserved { get; private set; }

        private bool ObservableEntityXMLSerializable {
            get { return TypeOfEntity != null && EntityId != null && 
                DisplayName != null & Position != null; }
        }

        // AABBEntity
        public BoundingBoxD BoundingBox { get; set; }
        public int TreeProxyID { get; set; }
        public Vector3D WorldTranslation { get; set; }
        public Vector3D LinearVelocity { get; set; }

        private bool AABBEntityyXMLSerializable {
            get {
                return BoundingBox != null && TreeProxyID != null &&
                WorldTranslation != null && LinearVelocity != null;
            }
        }

        // ConcealableEntity
        [XmlIgnore]
        public bool IsRevealBlocked { get; set; }


        // ConcealedEntity
        [XmlIgnore]
        public bool IsRevealable { 
            get { return IsRevealableAuto && IsRevealableManual; } 
        }
        [XmlIgnore]
        public bool IsRevealableAuto { get { return true; } }
        [XmlIgnore]
        public bool IsRevealableManual { get { return !IsRevealBlocked; } }


        [XmlIgnore]
        public virtual bool NeedsReveal { get { return IsObserved; } }

        [XmlIgnore]
        public virtual bool IsXMLSerializable {
            get {
                return ObservableEntityXMLSerializable && AABBEntityyXMLSerializable;
            }
        }

        #endregion
        #region Constructors

        // XML Deserialization
        public ConcealedEntity() {
            // EntityId is populated after instantiated from XML, so use an action
            Log = new Logger("GP.Concealment.World.Entities.ConcealedEntity", 
                (() => { return EntityId.ToString(); }));
        }

        // Byte Deserialization
        public ConcealedEntity(VRage.ByteStream stream) : this() {
            TypeOfEntity = (EntityType)stream.getUShort();
            EntityId = stream.getLong();
            Position = stream.getVector3D();
            // Clients don't need AABB details
            IsRevealBlocked = stream.getBoolean();
            IsObserved = stream.getBoolean();
            Log = new Logger("GP.Concealment.World.Entities.ConcealedEntity", 
                EntityId.ToString());
        }

        // Creation from ingame entity before it's removed
        public ConcealedEntity(IMyEntity entity) : this() {
            EntityId = entity.EntityId;
            DisplayName = entity.DisplayName;
            Position = entity.GetPosition();
            BoundingBox = entity.WorldAABB;
            WorldTranslation = entity.WorldMatrix.Translation;
            //TODO: stop entity if moving, we never update the below
            // and really we should never conceal a grid that's moving
            LinearVelocity = entity.Physics.LinearVelocity;
            Log = new Logger("GP.Concealment.World.Entities.ConcealedEntity",
                EntityId.ToString());
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public virtual void AddToByteStream(VRage.ByteStream stream) {
            UpdateRevealabilityManual();
            stream.addUShort((ushort)TypeOfEntity);
            stream.addLong(EntityId);
            stream.addVector3D(Position);
            // Clients don't need AABB details
            stream.addBoolean(IsRevealBlocked);
            stream.addBoolean(IsObserved);
        }

        #endregion
        #region Observed Marking

        public void MarkViewedBy(ObservingEntity e) {
            long id = e.EntityId;
            if (EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewedBy");
                return;
            }

            Log.Error("Viewed by " + id, "MarkViewedBy");
            EntitiesViewedBy.Add(id, e);
            UpdateObserveability();
        }

        public void UnmarkViewedBy(ObservingEntity e) {
            long id = e.EntityId;
            if (!EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewedBy");
                return;
            }

            Log.Error("No longer viewed by " + id, "UnmarkViewedBy");
            EntitiesViewedBy.Remove(id);
            UpdateObserveability();
        }

        /*
        public void MarkDetectedBy(ObservingEntity e) {
            long id = e.EntityId;
            if (EntitiesDetectedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkDetectedBy");
                return;
            }

            Log.Error("Adding " + id, "MarkDetectedBy");
            EntitiesDetectedBy.Add(id, e);
            UpdateObserveability();
        }

        public void UnmarkDetectedBy(ObservingEntity e) {
            long id = e.EntityId;
            if (!EntitiesDetectedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkDetectedBy");
                return;
            }

            Log.Error("Removing " + id, "UnmarkDetectedBy");
            EntitiesDetectedBy.Remove(id);
            UpdateObserveability();
        }

        public void MarkBroadcastingTo(ObservingEntity e) {
            long id = e.EntityId;
            if (EntitiesBroadcastingTo.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkBroadcastingTo");
                return;
            }

            Log.Error("Adding " + id, "MarkBroadcastingTo");
            EntitiesBroadcastingTo.Add(id, e);
            UpdateObserveability();
        }

        public void UnmarkBroadcastingTo(ObservingEntity e) {
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
        /// Should be called before revealing and before sending to clients
        /// </summary>
        public void UpdateRevealabilityManual(){
            UpdateRevealBlocked();
        }

        private void UpdateRevealabilityAuto() { }

        private void UpdateRevealBlocked() {
            BoundingBoxD boxCopy = BoundingBox;
            IsRevealBlocked = MyAPIGateway.Entities.
                GetElementsInBox(ref boxCopy).Count > 0;
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

        public bool TryReveal() {
            UpdateRevealabilityManual();

            if (!IsRevealable) {
                Log.Trace("Grid is not revealable", "TryReveal");
                return false;
            }

            return Reveal();
        }

        protected abstract bool Reveal();

        #endregion


    }

}
