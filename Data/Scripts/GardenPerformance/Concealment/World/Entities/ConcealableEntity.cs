using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;
using Sandbox.Common.ObjectBuilders;

using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

namespace GP.Concealment.World.Entities {

    public interface ConcealableEntity : ObservableEntity {

        #region Properties

        bool IsRevealBlocked { get; }

        #endregion

    }

}
