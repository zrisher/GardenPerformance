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

    /// <summary>
    /// Controllable Entities can be moving and controlled
    /// </summary>
    /// <remarks>
    /// Check moving every update because there are no events for movement
    /// Physics has OnWorldPositionChanged, but it's just an action
    /// We might remove all these events and just put it in the sector directly eventually
    /// </remarks>
    public abstract class ControllableEntity : RevealedEntity {

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
        /*
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
        */
        #endregion
        #region Fields
        #endregion
        #region Properties

        public override Dictionary<uint, Action> UpdateActions {
            get {
                Dictionary<uint, Action> actions = base.UpdateActions;
                actions.Add(60, Update); // TODO: Move to 300 when done testing
                return actions;
            }
        }

        public override bool IsConcealable {
            get {
                return base.IsConcealable && !IsControlled;
            }
        }

        public bool IsMoving { get; private set; }
        public bool RecentlyMoved { get; private set; }
        public DateTime RecentlyMovedEnds { get; private set; }
        public virtual bool IsControlled { get { return IsMoving || RecentlyMoved;  } }

        #endregion
        #region Constructors

        // Creation from ingame entity
        public ControllableEntity(IMyEntity entity) : base(entity) 
        {
            //Log.Trace("Running ControllableEntity ctr", "ctr");
            Log.ClassName = "GP.Concealment.World.Entities.ControllableEntity";
            Log.Trace("New Controllable Entity " + EntityId + " " + DisplayName, "ctr");
            //Log.Trace("Finished ControllableEntity ctr", "ctr");
        }

        // Byte Deserialization
        public ControllableEntity(VRage.ByteStream stream) : base(stream) {
            Log.ClassName = "GP.Concealment.World.Entities.ControllableEntity";
            IsMoving = stream.getBoolean();
            RecentlyMoved = stream.getBoolean();
            RecentlyMovedEnds = stream.getDateTime();
            Log.Trace("New Controllable Entity " + EntityId + " " + DisplayName, "ctr");
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
            stream.addBoolean(IsMoving);
            stream.addBoolean(RecentlyMoved);
            stream.addDateTime(RecentlyMovedEnds);
        }

        #endregion
        #region Updates

        public override void Initialize() {
            base.Initialize();
            NotifyAdded();
        }

        protected virtual void Update() {
            //Log.Trace("ControllableEntity update " + DisplayName, "Update");
            UpdateMoving();

            if (IsMoving) {
                NotifyMoved();
            }

        }

        public override void Terminate() {
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
            //Log.Trace("ControllableEntity " + DisplayName + " moved ", "NotifyMoved");
            if (ControllableEntityMovement != null) ControllableEntityMovement(this);
        }

        private void NotifyRemoved() {
            Log.Trace("ControllableEntity " + DisplayName + " removed.", "NotifyRemoved");
            if (ControllableEntityRemoval != null) ControllableEntityRemoval(this);
        }
        /*
        private void NotifyControlAcquired() {
            Log.Trace("ControllableEntity " + DisplayName + " controlled.", "NotifyRemoved");
            if (ControlAcquisition != null) ControlAcquisition(this);
        }

        private void NotifyControlControlReleased() {
            Log.Trace("ControllableEntity " + DisplayName + " released.", "NotifyRemoved");
            if (ControlRelease != null) ControlRelease(this);
        }
        */
        #endregion
        #region Movement

        private void UpdateMoving() {
            //Log.Trace("Checking Physics of " + DisplayName, "CheckPhysics");


            if (Entity.IsMoving()) {
                // Moving

                // Mark moving if not marked
                if (!IsMoving) {
                    IsMoving = true;
                    ControlAcquired();
                }


            }
            else {
                //Not moving

                // Update recently moved
                if (RecentlyMoved) {
                    if (DateTime.UtcNow > RecentlyMovedEnds) {
                        RecentlyMoved = false;
                        ControlReleased();
                    }
                }

                // Mark stopped if not marked
                if (IsMoving) {
                    IsMoving = false;
                    RecentlyMoved = true;
                    RecentlyMovedEnds = DateTime.UtcNow.AddSeconds(
                        Settings.Instance.ControlledMovingGraceTimeSeconds);
                }
            }
        }

        #endregion
        #region Control 

        protected virtual void ControlAcquired() { }

        protected virtual void ControlReleased() { }

        /*
        protected virtual void MarkControlled() {
            IsControlled = true;
            //NotifyControlAcquired();
        }

        protected virtual void MarkNotControlled() {
            IsControlled = false;
            //NotifyControlControlReleased();
        }
        */
        #endregion

        protected override void UpdateConcealability() {
            base.UpdateConcealability();
        }
    }

}
