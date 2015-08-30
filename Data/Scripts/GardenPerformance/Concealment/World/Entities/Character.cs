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

    public class Character : ObservingEntity {

        #region Fields

        IMyCharacter CharacterEntity;

        #endregion
        #region Properties

        private bool Alive;

        public override String ComponentName { get { return "Character"; } }

        public override bool IsControlled { get { return Alive; } }

        public override EntityType TypeOfEntity {
            get { return EntityType.Character; }
        }

        #endregion
        #region Constructor

        public Character(IMyEntity entity) : base(entity) {
            Log.ClassName = "GP.Concealment.World.Entities.Character";
            Log.Trace("New Character " + Entity.EntityId + " " + DisplayName, "ctr");
            CharacterEntity = Entity as IMyCharacter;
        }

        public Character(VRage.ByteStream stream) : base(stream) {
            CharacterEntity = Entity as IMyCharacter;
        }


        #endregion
        #region Updates

        public override void Initialize() {
            base.Initialize();
            if (CharacterEntity != null)
                CharacterEntity.OnMovementStateChanged += MovementStateChanged;
            Alive = true;
        }

        protected override void Update() {
            //Log.Trace("Character update " + DisplayName, "Update");
            base.Update();
        }

        public override void Terminate() {
            if (CharacterEntity != null)
                CharacterEntity.OnMovementStateChanged -= MovementStateChanged;
            base.Terminate();
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
        }

        #endregion

        void MovementStateChanged (MyCharacterMovementEnum oldState, 
            MyCharacterMovementEnum newState) 
        {
            if (newState == MyCharacterMovementEnum.Died) {
                Alive = false;
            }
        }

        protected override void Conceal() {
            //throw new NotImplementedException();
        }

        public String ToString() {
            // TODO: implement
            return "Character to String - to implement";
        }

    }

}
