using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;
using VRage.ModAPI;

using GardenPerformance.Concealment.Common;


namespace GardenPerformance.Concealment.Records.Entities {

    public class RevealedGrid : RevealedEntity {

        private IMyCubeGrid Grid;

        public RevealedGrid(IMyCubeGrid entity) : base (entity as IMyEntity) {
            Grid = entity;
        }

        public override Concealability GetConcealability() {
            // TODO: check for concealment conditionals
            return Concealability.Concealable;
        }

    }

}
