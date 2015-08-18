using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.Common.ObjectBuilders;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

using SEGarden.Extensions;
using SEGarden.Extensions.Objectbuilders;
using SEGarden.Logging;

namespace GP.Concealment.Records.Entities {

    public class TestEntity : ConcealableEntity {

        #region Static

        private static Logger Log =
            new Logger("GP.Concealment.Records.Entities.ConcealableEntity");

        #endregion
        #region Instance


        public List<long> SpawnOwners = new List<long>();
        public String DisplayName = "";
        public List<long> BigOwners = new List<long>();

        
        // Giving this a default value of new MyObjectBuilder_CubeGrid() does NOT work
        // The new object will have null fields, which makes it unsaveable.
        //public MyObjectBuilder_CubeGrid Builder;
        //public MyObjectBuilder_EntityBase Builder;
        /*
        [XmlIgnore]
        public IMyCubeGrid IngameGrid;
        */

        public TestEntity() {
            Type = EntityType.Grid;
        }

        /*
        public void LoadFromCubeGrid(IMyCubeGrid grid) {
            base.LoadFromEntity(grid as IMyEntity);

            IngameGrid = grid;

            //IngameGrid = grid;
            DisplayName = grid.DisplayName;
            // ToDo: get all owners instead of big (for targeting)
            BigOwners = grid.BigOwners;
            // ToDo: get real spawn owners (for spawning)
            SpawnOwners = grid.BigOwners;
            //IMyEntity entity = grid as IMyEntity;
            Builder = grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;


            if (Builder == null) {
                Log.Error("Got null builder for entity " + EntityId +
                    ". We will be unable to save it now.", "LoadFromCubeGrid");
            }
            else {
                //Log.Error("Filling builder nulls for " + EntityId, "LoadFromCubeGrid");
                //Builder.FillNullsWithDefaults();
            }
        }
        */

        #endregion

    }
}
