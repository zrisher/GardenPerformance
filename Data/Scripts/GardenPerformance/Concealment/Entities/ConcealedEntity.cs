using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GardenPerformance.Concealment.Common;

namespace GardenPerformance.Concealment.Entities {


    public abstract class ConcealedEntity {
        public long EntityId;
        public EntityType EntityType;
        public VRageMath.Vector3D Position;

        public abstract Revealability GetRevealability();
    }
}
