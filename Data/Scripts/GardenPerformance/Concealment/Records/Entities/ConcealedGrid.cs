using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GardenPerformance.Concealment.Common;

namespace GardenPerformance.Concealment.Records.Entities {

    public class ConcealedGrid : ConcealedEntity {
        public class SpawnPoint {
            public long OwnerId;
        }

        public List<SpawnPoint> SpawnPoints;

        public override Revealability GetRevealability() {
            // TODO: check for concealment conditionals
            return Revealability.Revealable;
        }
    }
}
