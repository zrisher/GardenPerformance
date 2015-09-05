using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRage.ModAPI;
using VRageMath;

using SEGarden.Extensions;
using SEGarden.Extensions.VRageMath;
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

        private bool UpdateObservingNextUpdate = true;
        private Vector3D LastObservingPosition;
        private DateTime LastObservingTime;

        #endregion
        #region Properties

        protected double ViewDistance {
            get { return Settings.Instance.RevealVisibilityMeters; }
        }

        //private BoundingSphereD viewingSphere { get; set; }
        //protected virtual double DetectDistance { get { return 0; } }
        //protected virtual double CommunicateDistance { get { return 0; } }

        private double DistanceSinceLastObservingCheck {
            get {
                if (!PreviouslyObserving)
                    return double.PositiveInfinity;
                else
                    return (Position - LastObservingPosition).AbsMax();
            }
        }

        private bool PreviouslyObserving { 
            get { return LastObservingTime != DateTime.MinValue; } 
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

            List<long> entitiesViewing = stream.getLongList();
            foreach (long id in entitiesViewing) {
                EntitiesViewing.Add(id, null);
            }

            LastObservingTime = stream.getDateTime();
            LastObservingPosition = stream.getVector3D();
            //Log.Trace("Deserialized LastObservingTime of " + LastObservingTime, "stream ctr");
            //Log.Trace("Deserialized LastObservingPosition of " + LastObservingPosition, "stream ctr");
            //Log.Trace("Deserialized ViewDistance of " + ViewDistance, "stream ctr");
        }

        // Creation from ingame entity
        public ObservingEntity(IMyEntity entity) : base(entity) {
            Log.Trace("Set view distance to " + ViewDistance, "ctr");
        }

        #endregion
        #region Control Event Handlers

        protected override void ControlAcquired() {
            UpdateObservingNextUpdate = true;
        }

        protected override void ControlReleased() {
            UpdateObservingNextUpdate = true;
        }

        #endregion
        #region Updates

        /*
        public override void Initialize() {
            base.Initialize();
        }
        */

        protected override void UpdateControl() {
            base.UpdateControl();
            CheckMovedFarEnoughToReobserve();
            UpdateObservingIfNeeded();
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

            //Log.Trace("Adding observing entity to byte stream", "AddToByteStream");

            stream.addLongList(EntitiesViewing.Keys.ToList());
            stream.addDateTime(LastObservingTime);
            stream.addVector3D(LastObservingPosition);

            //Log.Trace("Serialized LastObservingTime of " + LastObservingTime, "AddToByteStream");
            //Log.Trace("Serialized LastObservingPosition of " + LastObservingPosition, "AddToByteStream");
            //Log.Trace("Serialized ViewDistance of " + ViewDistance, "AddToByteStream");
        }

        #endregion
        #region Update Observing

        private void UpdateObservingIfNeeded() {
            if (UpdateObservingNextUpdate) {
                UpdateObserving();
                UpdateObservingNextUpdate = false;
            }
        }

        private void CheckMovedFarEnoughToReobserve() {
            if (IsMoving && IsControlled && DistanceSinceLastObservingCheck >
                Settings.Instance.ControlledMovementGraceDistanceMeters) 
            {
                Log.Trace("Moved far enough: " + DistanceSinceLastObservingCheck + 
                    " / " + Settings.Instance.ControlledMovementGraceDistanceMeters, 
                    "CheckMovedFarEnoughToReobserve");

                UpdateObservingNextUpdate = true;
            }
        }

        protected void UpdateObserving() {
            Log.Trace("Update Observing", "UpdateObserving");
            ClearObserving();
            Observe();
        }

        protected void ClearObserving() {
            //UnmarkDetectingAll();
            UnmarkViewingAll();
            //UnmarkReceivingAll();
        }

        #endregion
        #region Observe
        protected void Observe() {
            if (!IsControlled) return;

            //Log.Trace("Observing", "Observe");

            var viewingSphere = new BoundingSphereD(Position, ViewDistance);

            //Log.Trace("Viewing shpere Center: " + viewingSphere.Center, "Observe");
            //Log.Trace("Viewing shpere Radius: " + viewingSphere.Radius, "Observe");

            List<ObservableEntity> viewableEntities = Sector.ObservableInSphere(viewingSphere);
            viewableEntities.Concat(Concealed.EntitiesInSphere(viewingSphere));

            //Log.Trace("Viewable entity count: " + viewableEntities.Count, "Observe");

            foreach (ObservableEntity e in viewableEntities) {
                MarkViewing(e);
            }

            // do other ranges, can use largest between detection and view,
            // but communication depends on broadcast radius of others so would have to check that too

            LastObservingTime = DateTime.UtcNow;
            LastObservingPosition = Position;
        }

        private void MarkViewing(ObservableEntity e) {
            long id = e.EntityId;
            if (EntitiesViewing.ContainsKey(id)) {
                Log.Error("Already added " + id, "MarkViewing");
                return;
            }

            Log.Trace("Marking " + id + " as viewed by me", "MarkViewing");
            EntitiesViewing.Add(id, e);
            e.MarkViewedBy(this);
        }

        private void UnmarkViewing(ObservableEntity e) {
            long id = e.EntityId;
            if (!EntitiesViewing.ContainsKey(id)) {
                Log.Error("Not stored " + id, "UnmarkViewing");
                return;
            }

            Log.Trace("Removing view mark on " + id + " from me", "UnmarkViewing");
            EntitiesViewing.Remove(id);
            e.UnmarkViewedBy(this);
        }

        private void UnmarkViewingAll() {
            //Log.Trace("Unmarking all viewed entities", "UnmarkViewingAll");
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
        #region Describe

        public String ObservationDetails() {
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

            // Control
            if (IsControlled) {
                result += "  Controlled:\n";

                if (IsMoving) {
                    result += "    Moving at " + 
                        System.Math.Truncate(LinearVelocity.Length()) + " m/s";
                }
                else if (RecentlyMoved) {
                    result += "    Recently moved until " + RecentlyMovedEnds;
                }

                result += "\n";
            }
            else {
                result += "  Not Controlled. (Shouldn't be viewing anything.)\n";
            }

            // Last check details
            if (!PreviouslyObserving) {
                result += "  Hasn't yet had an observe check.\n";
            }
            //else {
                result += "  Last View Check at pos: " + LastObservingPosition.ToRoundedString() + "\n";
                result += "  Distance from last view check: " + DistanceSinceLastObservingCheck + "\n";
                result += "  Last View Check at time: " + LastObservingTime.ToLocalTime() + "\n";
                result += "  View radius: " + ViewDistance + "\n";
                result += "  Viewed entities: \n";
            //}



            // TODO: fetch the entities ingame on client side so we can have their details
            foreach (long id in EntitiesViewing.Keys) {
                result += "    " + id + "\n";
            }

            return result;
        }

        #endregion

    }

}
