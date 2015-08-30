using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRage.ModAPI;
using VRageMath;

using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

using GP.Concealment.Sessions;

namespace GP.Concealment.World.Entities {

    public abstract class ObservingEntity : ControllableEntity {

        #region Fields

        private Dictionary<long, ObservableEntity> EntitiesViewing =
            new Dictionary<long, ObservableEntity>();
        /*
        private Dictionary<long, ObservableEntity> EntitiesDetecting =
            new Dictionary<long, ObservableEntity>();
        private Dictionary<long, ObservableEntity> EntitiesReceivingFrom =
            new Dictionary<long, ObservableEntity>();
        */

        //private BoundingSphere DetectingSphere;
        //private BoundingSphere CommunicatingSphere;

        private bool RefreshObservingNextUpdate = true;
        private Vector3D LastObservingPosition;


        #endregion
        #region Properties

        protected double ViewDistance {
            get { return Settings.Instance.RevealVisibilityMeters;  }
        }

        private BoundingSphereD ViewingSphere { get; set; }

        //protected virtual double DetectDistance { get { return 0; } }

        //protected virtual double CommunicateDistance { get { return 0; } }

        private double DistanceSinceLastObservingCheck {
            get {
                if (LastObservingPosition == null)
                    return double.PositiveInfinity;
                else
                    return (Position - LastObservingPosition).AbsMax();
            }
        }

        /*
        private double GreatestObservingDistance {
            get {
                return new double[3]{ 
                    ViewDistance, DetectDistance, CommunicateDistance
                }.Max();
            }
        }
        */

        #endregion
        #region Constructors

        // Byte Deserialization
        public ObservingEntity(VRage.ByteStream stream) : base(stream) {
        }

        // Creation from ingame entity
        public ObservingEntity(IMyEntity entity) : base(entity) {
            UpdateSpheres();
        }

        #endregion
        #region Updates

        /*
        public override void Initialize() {
            base.Initialize();
        }
        */

        protected virtual void Update() {
            base.Update();

            if (IsMoving && IsControlled && 
                DistanceSinceLastObservingCheck > 
                Settings.Instance.ControlledMovementGraceDistanceMeters) 
            {
                RefreshObservingNextUpdate = true;
            }

            if (RefreshObservingNextUpdate) {
                RefreshObserving();
                RefreshObservingNextUpdate = false;
            }
        }

        public override void Terminate() {
            ClearObserving();
            base.Terminate();
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
        }

        #endregion
        #region Observing Marking High level

        protected void RefreshObserving() {
            ClearObserving();
            MarkObserving();
        }

        protected void ClearObserving() {
            //UnmarkDetectingAll();
            UnmarkViewingAll();
            //UnmarkReceivingAll();
        }

        // TODO: This
        protected void MarkObserving() {
            if (!IsControlled) return;

            foreach (ObservableEntity e in Sector.ObservableInSphere(ViewingSphere)) {
                MarkViewing(e);
            }

            // do other ranges, can use largest between detection and view,
            // but communication depends on broadcast radius of others so would have to check that too


            LastObservingPosition = Position;
        }

        #endregion
        #region Observing Marking

        private void MarkViewing(ObservableEntity e) {
            long id = e.EntityId;
            if (EntitiesViewing.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewing");
                return;
            }

            Log.Error("Adding " + id, "MarkViewing");
            EntitiesViewing.Add(id, e);
            e.MarkViewedBy(this);
        }

        private void UnmarkViewing(ObservableEntity e) {
            long id = e.EntityId;
            if (!EntitiesViewing.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewing");
                return;
            }

            Log.Error("Removing " + id, "UnmarkViewing");
            EntitiesViewing.Remove(id);
            e.UnmarkViewedBy(this);
        }

        private void UnmarkViewingAll() {
            Log.Trace("Unmarking all viewed entities", "UnmarkViewingAll");
            foreach (ObservableEntity e in EntitiesViewing.Values) {
                e.UnmarkViewedBy(this);
            }
            EntitiesViewing.Clear();
        }

        /*
        private void MarkDetecting(ObservableEntity e) {
            long id = e.EntityId;
            if (EntitiesDetecting.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkDetecting");
                return;
            }

            Log.Error("Adding " + id, "MarkDetecting");
            EntitiesDetecting.Add(id, e);
            e.MarkDetectedBy(this);
        }

        private void UnmarkDetecting(ObservableEntity e) {
            long id = e.EntityId;
            if (!EntitiesDetecting.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkDetecting");
                return;
            }

            Log.Error("Removing " + id, "UnmarkDetecting");
            EntitiesDetecting.Remove(id);
            e.UnmarkDetectedBy(this);
        }

        private void UnmarkDetectingAll() {
            Log.Trace("Unmarking all Detected entities", "UnmarkDetectingAll");
            foreach (ObservableEntity e in EntitiesDetecting.Values) {
                e.UnmarkDetectedBy(this);
            }
            EntitiesDetecting.Clear();
        }

        private void MarkReceivingFrom(ObservableEntity e) {
            long id = e.EntityId;
            if (EntitiesReceivingFrom.ContainsKey(id)) {
                Log.Error("Already added " + id, "ReceivingFrom");
                return;
            }

            Log.Error("Adding " + id, "ReceivingFrom");
            EntitiesReceivingFrom.Add(id, e);
            e.MarkBroadcastingTo(this);
        }

        private void UnmarkReceivingFrom(ObservableEntity e) {
            long id = e.EntityId;
            if (!EntitiesReceivingFrom.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkReceivingFrom");
                return;
            }

            Log.Error("Removing " + id, "UnmarkReceivingFrom");
            EntitiesReceivingFrom.Remove(id);
            e.UnmarkBroadcastingTo(this);
        }

        private void UnmarkReceivingAll() {
            Log.Trace("Unmarking all comm receiving entities", "UnmarkReceivingAll");
            foreach (ObservableEntity e in EntitiesReceivingFrom.Values) {
                e.UnmarkBroadcastingTo(this);
            }
            EntitiesReceivingFrom.Clear();
        }
        */

        #endregion
        #region Control 

        protected override void ControlAcquired() {
            RefreshObservingNextUpdate = true;
        }

        protected override void ControlReleased() {
            RefreshObservingNextUpdate = true;
        }

        #endregion

        private void UpdateSpheres() {
            ViewingSphere = new BoundingSphereD(Position, ViewDistance);
        }


        public String ToString() {
            // TODO: implement
            return "Observing Entity to String - to implement";
        }

    }

}
