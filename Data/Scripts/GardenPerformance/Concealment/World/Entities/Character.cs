using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.ModAPI;

using SEGarden.Extensions;
using SEGarden.Logic;
using SEGarden.Logging;

using GP.Concealment.Sessions;

namespace GP.Concealment.World.Entities {

    class Character : ControllableEntity {

        /*
        private static ServerConcealSession Session {
            get { return ServerConcealSession.Instance; }
        }
        */

        #region Instance Fields

        IMyCharacter CharacterEntity;

        #endregion
        #region Instance Properties

        public override Dictionary<uint, Action> UpdateActions {
            get {
                return base.UpdateActions;
            }
        }

        #endregion
        #region Instance Event Helpers


        #endregion
        #region Constructor

        public Character(IMyEntity entity) : base(entity) {
            Log = new Logger("GP.Concealment.World.Entities.Character",
                Entity.EntityId.ToString());

            CharacterEntity = entity as IMyCharacter;

            Log.Trace("New Character " + Entity.EntityId + " " + DisplayName, "ctr");
        }

        #endregion
        #region Updates

        public override void Initialize() {
            base.Initialize();
            CharacterEntity.OnMovementStateChanged += MovementStateChanged;
            MarkControlled();
        }

        protected override void Update() {
            //Log.Trace("Character update " + DisplayName, "Update");
            base.Update();
        }

        public override void Terminate() {
            CharacterEntity.OnMovementStateChanged -= MovementStateChanged;
            MarkNotControlled();
            base.Terminate();
        }

        #endregion

        void MovementStateChanged (MyCharacterMovementEnum oldState, 
            MyCharacterMovementEnum newState) 
        {
            if (newState == MyCharacterMovementEnum.Died) {
                MarkNotControlled();
            }
        }



    }

}
