using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VRage.ModAPI;
using VRageMath;

using SEGarden.Logging;
using SEGarden.Logic;
using SEGarden.Math;

namespace GP.Concealment.World.Entities {

    public interface ObservableEntity {

        #region Fields

        /* You should have these to implement the below, but interfaces can only
         * define public things
        private Dictionary<long, RevealedEntity> EntitiesViewedBy;
        private Dictionary<long, RevealedEntity> EntitiesDetectedBy;
        private Dictionary<long, RevealedEntity> EntitiesBroadcastingTo;
        */

        #endregion
        #region Properties

        EntityType TypeOfEntity { get; }
        long EntityId { get; }
        String DisplayName { get; }
        Vector3D Position { get; }
        bool IsObserved { get; }
        //double BroadcastRange { get; }

        #endregion
        #region Observed Marking

        void MarkViewedBy(ObservingEntity e);

        void UnmarkViewedBy(ObservingEntity e);
        /*

        void MarkDetectedBy(ObservingEntity e);

        void UnmarkDetectedBy(ObservingEntity e);

        void MarkBroadcastingTo(ObservingEntity e);

        void UnmarkBroadcastingTo(ObservingEntity e);
        
        */ 

        #endregion

    }

}
