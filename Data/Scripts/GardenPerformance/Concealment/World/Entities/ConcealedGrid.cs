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
using SEGarden.Extensions.VRageMath;
using SEGarden.Extensions.Objectbuilders;
using SEGarden.Logging;
using SEGarden.Math;

using VRageMath;

using GP.Concealment.Sessions;

namespace GP.Concealment.World.Entities {

    public class ConcealedGrid : ConcealedEntity, ConcealableGrid {

        #region Static

        #endregion
        #region Fields

        #endregion
        #region Properties

        // ConcealableGrid
        [XmlIgnore]
        public IMyCubeGrid Grid { get; private set; }
        public List<long> SpawnOwners { get; set; }
        public List<long> BigOwners { get; set; }
        private bool ConcealableGridXMLSerializable {
            get { return SpawnOwners != null && BigOwners != null; }
        }

        [XmlIgnore]
        public override bool NeedsReveal {
            get { return base.NeedsReveal || NeedsRevealForSpawn; }
        }

        [XmlIgnore]
        public bool NeedsRevealForSpawn { get; private set; }

        // wish we could save these with it directly, but we can't
        [XmlIgnore]
        public MyObjectBuilder_CubeGrid Builder { get; set;}

        [XmlIgnore]
        public override bool IsXMLSerializable {
            get {
                return base.IsXMLSerializable && ConcealableGridXMLSerializable &&
                Builder != null;
            }
        }

        #endregion
        #region Constructors

        // XML Deserialization
        public ConcealedGrid() : base() {
            TypeOfEntity = EntityType.Grid;
            Log.ClassName = "GP.Concealment.World.Entities.ConcealedGrid";
        }

        // Byte Deserialization
        public ConcealedGrid(VRage.ByteStream stream) : base(stream) {
            SpawnOwners = stream.getLongList();
            BigOwners = stream.getLongList();
        }

        // Creation from ingame entity before it's removed
        public ConcealedGrid(ConcealableGrid grid) : base(grid.Grid as IMyEntity) {
            Grid = grid.Grid;
            BigOwners = grid.BigOwners;
            SpawnOwners = grid.SpawnOwners;

            if (Grid == null) {
                Log.Error("Stored cubegrid reference is null", "ctr");
            } else {
                Builder = Grid.GetObjectBuilder() as MyObjectBuilder_CubeGrid;
            }
        }

        #endregion
        #region Serialization

        // Byte Serialization
        public override void AddToByteStream(VRage.ByteStream stream) {
            base.AddToByteStream(stream);
            stream.addLongList(SpawnOwners);
            stream.addLongList(BigOwners);
        }

        #endregion
        #region Update Attributes from Ingame data

        private void UpdateNeededForSpawn() {
            NeedsRevealForSpawn = false;

            // If we want to only use Working blocks, need hooks
            foreach (long owner in SpawnOwners) {
                if (Revealed.SpawnOwnerNeeded(owner)) {
                    NeedsRevealForSpawn = true;
                    return;
                }
            }
        }


        public void MarkSpawnUpdateNeeded() {
            UpdateNeededForSpawn();
        }

        #endregion

        protected override bool Reveal() {
            Log.Trace("Revealing grid " + EntityId, "Reveal");

            if (Builder == null) {
                Log.Error("No stored builder, aborting.", "Reveal");
                return false;
            }

            if (!ServerConcealSession.Instance.Manager.Concealed.CanRemoveGrid(this)) {
                Log.Error("Couldn't find in Conceal session to remove, aborting.", 
                    "Reveal");
                return false;
            }

            // Reallocate ID if necessary
            if (MyAPIGateway.Entities.EntityExists(EntityId)) {
                EntityId = 0;
                Builder.EntityId = 0;
                Log.Trace("Reallocating entityId", "Reveal");
            }

            // === Add it back to game world
            MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(Builder);
            Log.Trace("Added entity into game from builder. " + EntityId, "Reveal");

            // === Update lists
            ServerConcealSession.Instance.Manager.Concealed.RemoveGrid(this);

            Log.Trace("End reveal " + EntityId, "revealEntity");
            return true;
        }

        public String ConcealmentDetails() {
            String result = "";

            // Ids
            result += "\"" + DisplayName + "\" - " + EntityId + "\n";

            // Owners
            // TODO: show owner names instead of playerIds
            result += "  Owners: TODO\n";
            /*
            if (BigOwners != null) {
                result += "  Owners: " + String.Join(", ", BigOwners) + "\n";
            }
            else {
                Log.Error("Grid had null BigOwners", "ReceiveRevealedGridsResponse");
            }
             * */

            // Position
            result += "  Position: " + Position.ToRoundedString() + "\n";

            result += "  Revealable? " + IsRevealable + "\n";
            result += "  Reveal Blocked? " + IsRevealBlocked + "\n";
            return result;
        }

    }

}
