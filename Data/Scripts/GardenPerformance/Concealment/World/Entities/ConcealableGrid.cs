using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

namespace GP.Concealment.World.Entities {

    public interface ConcealableGrid : ConcealableEntity {

        #region Properties

        IMyCubeGrid Grid { get; }
        List<long> SpawnOwners { get; }
        List<long> BigOwners { get; }

        #endregion

    }

}
