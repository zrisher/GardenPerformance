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

    // Revealed entities cannot be concealed if they
    // Are controlled
    // Are "working" (refining, assembling, oxy creating, battery charging)
    // Are moving, 
    // Unfortunately ControllerInfo isn't whitelisted, so we just guess if 
    // it's controlled by whether it's moving. Character entities report presence separately. 
    //public bool Controlled { get { return ControlsInUse.Count > 0; } }
    class RevealedGrid : ControllableEntity {

        /*
        private static ServerConcealSession Session {
            get { return ServerConcealSession.Instance; }
        }
        */

        #region Instance Fields

        private IMyCubeGrid Grid;
        public EntityConcealability Concealability { get; private set; }

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

        public RevealedGrid(IMyEntity entity) : base(entity) {
            Log = new Logger("GP.Concealment.World.Entities.CubeGrid",
                Entity.EntityId.ToString());

            Log.Trace("New CubeGrid " + Entity.EntityId + " " + DisplayName, "ctr");
        }

        #endregion
        #region Updates

        public override void Initialize() {
            base.Initialize();
        }

        protected override void Update() {
            //Log.Trace("Character update " + DisplayName, "Update");
            base.Update();
        }

        public override void Terminate() {
            base.Terminate();
            
        }

        #endregion

        /*
        // Checking assembler and refinery for production
        if (block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Assembler)
            || block.FatBlock.BlockDefinition.TypeId == typeof(MyObjectBuilder_Refinery)) {
            Log.Trace("Block is refinery/assembler", "CanConceal");
            InGame.IMyProductionBlock productionBlock = block.FatBlock as InGame.IMyProductionBlock;
            if (productionBlock.Enabled && productionBlock.IsProducing) {
                Log.Trace("Entity is working", "CanConceal");
                concealability |= ConcealableEntity.EntityConcealability.Working;
                Log.Trace("Concealability as int: " + (int)concealability, "CanConceal");
            }
        }
        */

    }

}
