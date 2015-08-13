using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRage.ModAPI;

using GardenPerformance.Concealment.Common;

namespace GardenPerformance.Concealment.Entities {

    public abstract class RevealedEntity {

        protected IMyEntity IngameEntity;
        public long EntityId;

        public RevealedEntity(IMyEntity entity) {
            IngameEntity = entity;
            EntityId = entity.EntityId;
        }

        public abstract Concealability GetConcealability();

    }

}
