using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

using SEGarden.Extensions;
using SEGarden.Logic;
using SEGarden.Logging;

using GP.Concealment.Sessions;

namespace GP.Concealment.World.Entities {

    public abstract class ControllableEntity : ConcealableEntity {

        #region Static ControllableEntities Events

        private static Action<ControllableEntity> ControllableEntityAddition;
        public static event Action<ControllableEntity> ControllableEntityAdded {
            add { ControllableEntityAddition += value; }
            remove { ControllableEntityAddition -= value; }
        }

        private static Action<ControllableEntity> ControllableEntityMovement;
        public static event Action<ControllableEntity> ControllableEntityMoved {
            add { ControllableEntityMovement += value; }
            remove { ControllableEntityMovement -= value; }
        }

        private static Action<ControllableEntity> ControllableEntityRemoval;
        public static event Action<ControllableEntity> ControllableEntityRemoved {
            add { ControllableEntityRemoval += value; }
            remove { ControllableEntityRemoval -= value; }
        }

        private static Action<ControllableEntity> ControlAcquisition;
        public static event Action<ControllableEntity> ControlAcquired {
            add { ControlAcquisition += value; }
            remove { ControlAcquisition -= value; }
        }

        private static Action<ControllableEntity> ControlRelease;
        public static event Action<ControllableEntity> ControlReleased {
            add { ControlRelease += value; }
            remove { ControlRelease -= value; }
        }

        #endregion
        #region Fields

        // TODO: cache radar and comm blocks
        public bool IsMoving = false;
        public bool HasRadar = false;
        public bool HasComms = false;

        private bool RefreshRevealMarkersNextUpdate = true;

        protected Logger Log;

        private Vector3D LastNearbyRevealPosition;

        private Dictionary<long, ConcealableEntity> EntitiesViewed =
            new Dictionary<long, ConcealableEntity>();
        private Dictionary<long, ConcealableEntity> EntitiesDetected =
            new Dictionary<long, ConcealableEntity>();
        private Dictionary<long, ConcealableEntity> EntitiesReceivingFrom =
            new Dictionary<long, ConcealableEntity>();

        #endregion
        #region Properties

        public override Dictionary<uint, Action> UpdateActions {
            get {
                return new Dictionary<uint, Action> {
                    {60, Update} // TODO: Move to 300 when done testing
                };
            }
        }

        public bool IsControlled { get; private set; }

        public uint ViewDistance {
            get {
                // TODO: actually use view distance from session
                return Settings.Instance.RevealVisibilityMeters; 
            }
        }

        public uint DetectDistance {
            get {
                // TODO: Actually use greatest set radar distance from blocks
                if (HasRadar) return Settings.Instance.RevealVisibilityMeters;
                else return 0;
            }
        }

        public uint CommunicateDistance {
            get {
                // TODO: Actually use greatest set comm distance from blocks
                if (HasComms) return Settings.Instance.RevealDetectabilityMeters;
                else return 0;
            }
        }

        public double DistanceSinceLastRevealMarking {
            get {
                if (LastNearbyRevealPosition == null) 
                    return double.PositiveInfinity;
                else 
                    return (Position - LastNearbyRevealPosition).AbsMax();
            }
        }

        #endregion
        #region Constructors

        public ControllableEntity(IMyEntity entity) : base(entity) 
        {
            Log = new Logger("GP.Concealment.World.Entities.ControllableEntity",
                EntityId.ToString());
            Log.Trace("New Controllable Entity " + EntityId + " " + DisplayName, "ctr");
        }

        public ControllableEntity(VRage.ByteStream stream) : base(stream) {
            Log = new Logger("GP.Concealment.World.Entities.ControllableEntity",
                EntityId.ToString());
            Log.Trace("New Controllable Entity " + EntityId + " " + DisplayName, "ctr");
        }

        #endregion
        #region Serialization

        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
        }

        #endregion
        #region Updates

        public override void Initialize() {
            NotifyAdded();
            // Unfortunately movement is not actually implemented with actions yet
            // Physics has OnWorldPositionChanged, but it's called directly instead
            // of attached to an event.
            //Entity.OnPhysicsChanged += PhysicsChanged;
            base.Initialize();
        }

        protected virtual void Update() {
            //Log.Trace("ControllableEntity update " + DisplayName, "Update");

            if (IsMoving) {
                NotifyMoved();

                if (IsControlled & DistanceSinceLastRevealMarking > 
                    Settings.Instance.ControlledRevealCacheMeters) 
                {
                    RefreshRevealMarkersNextUpdate = true;
                }
            }

            // TODO: controlled = moving or has character pilots for grid, always for character
            if (RefreshRevealMarkersNextUpdate) {
                RefreshRevealMarkers();
                RefreshRevealMarkersNextUpdate = false;
            }

            RefreshMoving();
        }

        public override void Terminate() {
            RemoveCachedMarkers();
            NotifyRemoved();
            base.Terminate();
        }

        #endregion
        #region ControllableEntities Event Helpers

        private void NotifyAdded() {
            Log.Trace("ControllableEntity " + DisplayName + " added.", "NotifyAdded");
            if (ControllableEntityAddition != null) ControllableEntityAddition(this);
        }

        private void NotifyMoved() {
            Log.Trace("ControllableEntity " + DisplayName + " moved ", "NotifyMoved");
            if (ControllableEntityMovement != null) ControllableEntityMovement(this);
        }

        private void NotifyRemoved() {
            Log.Trace("ControllableEntity " + DisplayName + " removed.", "NotifyRemoved");
            if (ControllableEntityRemoval != null) ControllableEntityRemoval(this);
        }

        private void NotifyControlAcquired() {
            Log.Trace("ControllableEntity " + DisplayName + " controlled.", "NotifyRemoved");
            if (ControlAcquisition != null) ControlAcquisition(this);
        }

        private void NotifyControlControlReleased() {
            Log.Trace("ControllableEntity " + DisplayName + " released.", "NotifyRemoved");
            if (ControlRelease != null) ControlRelease(this);
        }

        #endregion
        #region Movement

        private void RefreshMoving() {
            //Log.Trace("Checking Physics of " + DisplayName, "CheckPhysics");
            if (Entity.IsMoving())
                IsMoving = true;
            else
                IsMoving = false;
        }

        #endregion
        #region Control 

        protected void MarkControlled() {
            IsControlled = true;
            RefreshRevealMarkersNextUpdate = true;
            NotifyControlAcquired();
        }

        protected void MarkNotControlled() {
            IsControlled = false;
            RefreshRevealMarkersNextUpdate = true;
            NotifyControlControlReleased();
        }

        #endregion
        #region Reveal Marking

        private void RefreshRevealMarkers() {
            RemoveCachedMarkers();
            MarkNearbyEntitiesForReveal();
        }

        private void RemoveCachedMarkers() {
            UnmarkDetectingAll();
            UnmarkViewingAll();
            UnmarkReceivingAll();
        }

        // TODO: This
        private void MarkNearbyEntitiesForReveal() {
            if (!IsControlled) return;

            // the inradius function should return nonthing if zero;

            // foreach revealedgrid in revealedgridsinradius(viewrange)
            //   grid.markviewedby(this);

            // foreach revealedgrid in revealedgridsinradius(detectionrange)
            //   grid.markviewedby(this);

            // foreach revealedgrid in revealedgridsinradius(commsrange)
            //   grid.markviewedby(this);

            LastNearbyRevealPosition = Position;
        }

        #endregion
        #region Reveal Marker Access Helpers

        private void MarkViewing(ConcealableEntity e) {
            long id = e.EntityId;
            if (EntitiesViewed.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewing");
                return;
            }

            Log.Error("Adding " + id, "MarkViewing");
            EntitiesViewed.Add(id, e);
            e.MarkViewedBy(this);
        }

        private void UnmarkViewing(ConcealableEntity e) {
            long id = e.EntityId;
            if (!EntitiesViewed.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewing");
                return;
            }

            Log.Error("Removing " + id, "UnmarkViewing");
            EntitiesViewed.Remove(id);
            e.UnmarkViewedBy(this);
        }

        private void UnmarkViewingAll() {
            Log.Trace("Unmarking all viewed entities", "UnmarkViewingAll");
            foreach(ConcealableEntity e in EntitiesViewed.Values) {
                e.UnmarkViewedBy(this);
            }
            EntitiesViewed.Clear();
        }

        private void MarkDetecting(ConcealableEntity e) {
            long id = e.EntityId;
            if (EntitiesDetected.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkDetecting");
                return;
            }

            Log.Error("Adding " + id, "MarkDetecting");
            EntitiesDetected.Add(id, e);
            e.MarkDetectedBy(this);
        }

        private void UnmarkDetecting(ConcealableEntity e) {
            long id = e.EntityId;
            if (!EntitiesDetected.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkDetecting");
                return;
            }

            Log.Error("Removing " + id, "UnmarkDetecting");
            EntitiesDetected.Remove(id);
            e.UnmarkDetectedBy(this);
        }

        private void UnmarkDetectingAll() {
            Log.Trace("Unmarking all Detected entities", "UnmarkDetectingAll");
            foreach (ConcealableEntity e in EntitiesDetected.Values) {
                e.UnmarkDetectedBy(this);
            }
            EntitiesDetected.Clear();
        }

        private void MarkReceivingFrom(ConcealableEntity e) {
            long id = e.EntityId;
            if (EntitiesReceivingFrom.ContainsKey(id)) {
                Log.Error("Already added " + id, "ReceivingFrom");
                return;
            }

            Log.Error("Adding " + id, "ReceivingFrom");
            EntitiesReceivingFrom.Add(id, e);
            e.MarkBroadcastingTo(this);
        }

        private void UnmarkReceivingFrom(ConcealableEntity e) {
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
            foreach (ConcealableEntity e in EntitiesReceivingFrom.Values) {
                e.UnmarkBroadcastingTo(this);
            }
            EntitiesReceivingFrom.Clear();
        }

        #endregion

    }

}
