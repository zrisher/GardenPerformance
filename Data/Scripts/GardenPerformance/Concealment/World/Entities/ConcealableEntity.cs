using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRage.ModAPI;
using VRageMath;

using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

namespace GP.Concealment.World.Entities {

    public abstract class ConcealableEntity : EntityComponent, AABBEntity {

        #region Static

        protected static readonly Logger Log = 
            new Logger("GP.Concealment.World.Entities.ConcealableEntity");

        #endregion
        #region Fields

        public EntityType TypeOfEntity;

        private Dictionary<long, ConcealableEntity> EntitiesViewedBy =
            new Dictionary<long, ConcealableEntity>();
        private Dictionary<long, ConcealableEntity> EntitiesDetectedBy =
            new Dictionary<long, ConcealableEntity>();
        private Dictionary<long, ConcealableEntity> EntitiesBroadcastingTo =
            new Dictionary<long, ConcealableEntity>();

        #endregion
        #region Properties

        public Vector3D Position { 
            get { return Entity.PositionComp.GetPosition(); } 
        }
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

        #endregion
        #region Constructors

        public ConcealableEntity(IMyEntity entity) : base(entity) { }

        public ConcealableEntity(VRage.ByteStream stream) : base(stream) { }

        #endregion
        #region Serialization

        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
        }

        #endregion
        #region Reveal Marking

        public void MarkViewedBy(ConcealableEntity e) {
            long id = e.EntityId;
            if (EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewedBy");
                return;
            }

            Log.Error("Adding " + id, "MarkViewedBy");
            EntitiesViewedBy.Add(id, e);

            //  concealability |= ConcealableEntity.EntityConcealability.NearControlled;
        }

        public void UnmarkViewedBy(ConcealableEntity e) {
            long id = e.EntityId;
            if (!EntitiesViewedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewedBy");
                return;
            }

            Log.Error("Removing " + id, "UnmarkViewedBy");
            EntitiesViewedBy.Remove(id);
        }

        public void MarkDetectedBy(ConcealableEntity e) {
            long id = e.EntityId;
            if (EntitiesDetectedBy.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkDetectedBy");
                return;
            }

            Log.Error("Adding " + id, "MarkDetectedBy");
            EntitiesDetectedBy.Add(id, e);
        }

        public void UnmarkDetectedBy(ConcealableEntity e) {
            long id = e.EntityId;
            if (!EntitiesDetectedBy.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkDetectedBy");
                return;
            }

            Log.Error("Removing " + id, "UnmarkDetectedBy");
            EntitiesDetectedBy.Remove(id);
        }

        public void MarkBroadcastingTo(ConcealableEntity e) {
            long id = e.EntityId;
            if (EntitiesBroadcastingTo.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkBroadcastingTo");
                return;
            }

            Log.Error("Adding " + id, "MarkBroadcastingTo");
            EntitiesBroadcastingTo.Add(id, e);
        }

        public void UnmarkBroadcastingTo(ConcealableEntity e) {
            long id = e.EntityId;
            if (!EntitiesBroadcastingTo.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkBroadcastingTo");
                return;
            }

            Log.Error("Removing " + id, "UnmarkBroadcastingTo");
            EntitiesBroadcastingTo.Remove(id);
        }

        #endregion

    }

}
