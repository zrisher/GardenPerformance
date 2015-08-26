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

    abstract class ControllableEntity : EntityComponent {

        #region All ControllableEntities Events

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
        #region Instance Fields

        protected Logger Log;
        public bool IsMoving = false;
        //public Vector3D LastRevealedNearbyPosition;

        #endregion
        #region Instance Properties

        public override Dictionary<uint, Action> UpdateActions {
            get {
                return new Dictionary<uint, Action> {
                    {60, Update} // TODO: Move to 300 when done testing
                };
            }
        }
        public bool IsControlled { get; private set; }

        #endregion
        #region Instance Event Helpers

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
        #region Constructor

        public ControllableEntity(IMyEntity entity) : base(entity) 
        {
            Log = new Logger("GP.Concealment.World.Entities.ControllableEntity",
                EntityId.ToString());
            Log.Trace("New Controllable Entity " + EntityId + " " + DisplayName, "ctr");
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
            }

            CheckPhysics();
        }

        public override void Terminate() {
            NotifyRemoved();
            base.Terminate();
        }

        #endregion
        #region Physics

        private void CheckPhysics() {
            //Log.Trace("Checking Physics of " + DisplayName, "CheckPhysics");
            if (Entity.IsMoving())
                IsMoving = true;
            else
                IsMoving = false;
        }

        #endregion

        protected void MarkControlled() {
            IsControlled = true;
            NotifyControlAcquired();
        }

        protected void MarkNotControlled() {
            IsControlled = false;
            NotifyControlControlReleased();
        }

    }

}
