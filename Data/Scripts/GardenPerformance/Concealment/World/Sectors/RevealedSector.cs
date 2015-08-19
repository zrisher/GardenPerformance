using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.ModAPI;

using SEGarden.Logging;

using GP.Concealment.World.Entities;

namespace GP.Concealment.World.Sectors {

    /// <summary>
    /// The revealed sector tracks ingame entities that we might conceal,
    /// as well as those that might affect concealment.
    /// It determines what to conceal and what to reveal based on current game state
    /// </summary>
    /// <remarks>
    /// Do view, detect, communicate checks on controlled entities
    /// Do collide detects on moving cubegrids - need higher level org than just "entities with physics"
    /// Do spawn check on all spawned players
    /// </remarks>
    class RevealedSector {

        /*
        private static Logger Log = 
            new Logger("GP.Concealment.Revealed.RevealedSector");

        #region Instance Fields

        public List<long> ActivePlayerIDs;

        // public List<Sandbox.ModAPI.Ingame.IMyShipController> Controllers;
        // public List<IMyControllableEntity> ControlledEntities

        public List<RevealedGrid> ControlledGrids;
        public List<RevealedGrid> UncontrolledGrids;

        #endregion
        #region Session Updates

        public void Initialize() {
            Log.Trace("Registering existing entities into sector", "Load");


            //HashSet<IMyEntity> allEntities = new HashSet<IMyEntity>();
            //MyAPIGateway.Entities.GetEntities(allEntities);
            //AddIngameEntities(allEntities);

            //MyAPIGateway.Entities.OnEntityAdd += AddIngameEntity;
            //MyAPIGateway.Entities.OnEntityRemove += RemoveIngameEntity;

        }

        public void Terminate() {

            //MyAPIGateway.Entities.OnEntityAdd -= AddIngameEntity;
            //MyAPIGateway.Entities.OnEntityRemove -= RemoveIngameEntity;

        }

        public void Update100() {
            RevealNeededEntities();
        }

        public void Update1000() {
            ConcealUnneededEntities();
        }

        #endregion
        #region Public Requests (chat commands)

        public List<ConcealableGrid> RevealedGridsList() {
            return RevealedGrids.Select((x) => x as ConcealableGrid).ToList();
        }

        #endregion
        #region Concealment Updates

        // process queue of reveal-causing registered entities
        // look for nearby things that need revealing
        // if they are concealed, queue their revealing
        // if they are reveal, ensure they're marked as needed
        private void RevealNeededEntities() {

            List<IMyCharacter> controlledCharacters;
            List<RevealedGrid> controlledGrids;
            ConcealedSector concealedSector;

            foreach (var character in controlledCharacters) {
                // visible check
                // visibility        public int REVEAL_VISIBILITY_KM = 35;
                //concealedSector.MarkConcealabilityNear(character.center, )

            }

            foreach (var grid in controlledGrids) {
                //        public int REVEAL_DETECTABILITY_KM = 50;
                // public int REVEAL_COMMUNICATION_KM = 50;
                // public int REVEAL_VISIBILITY_KM = 35;
                // public int REVEAL_COLLIDABLE_KM = 10;

                // Mark concealed
            }

            foreach (IMyControllableEntity controllable in ControlledEntities) {
                if (controllable.ControllerInfo) {
                    //
                }
            }

        }

        // process queue of concealable entities
        // if they're not needed and can be concealed, queue them for conceal
        private void ConcealUnneededEntities() {

            // List of entities to check

            // Check a couple 

            // check map of uncontrolled grids

        }

        #endregion
        #region Entity Add/Remove from game world

        private void AddIngameEntity(IMyEntity entity) {
            Log.Trace("Adding entity " + entity.EntityId + " of type " +
                entity.GetType(), "AddIngameEntity");

            if (entity.Transparent) {
                Log.Trace("It's Transparent, skipping", "AddIngameEntity");
                return;
            }

            if (entity is IMyCubeGrid) {
                Log.Trace("It's a CubeGrid.", "AddIngameEntity");

                IMyCubeGrid grid = entity as IMyCubeGrid;
                RevealedGrid revealed = new RevealedGrid();
                revealed.LoadFromCubeGrid(grid);

                grid.
                var revealedGrid = new Concealed.Entities.ConcealableGrid();
                revealedGrid.LoadFromCubeGrid(grid);
                RevealedGrids[grid.EntityId] = revealedGrid;
            }


            if (entity is IMyCharacter) {
                Log.Trace("It's a CubeGrid.", "AddIngameEntity");
                IMyCubeGrid grid = entity as IMyCubeGrid;
                var revealedGrid = new Concealed.Entities.ConcealableGrid();
                revealedGrid.LoadFromCubeGrid(grid);
                RevealedGrids[grid.EntityId] = revealedGrid;
            }

            Sandbox.Game.World.MyEntityController a;
            a.

            if (entity is IMyControllableEntity) {
                var controllable = entity as IMyControllableEntity;
                controllable.Entity.
                if (controllable.ControllerInfo != null) {
                    controllable.ControllerInfo.ControlAcquired += new Action<Sandbox.Game.World.MyEntityController>((x) => { x; });
                }

            }

        }

        private void AddIngameEntities(HashSet<IMyEntity> entities) {
            Log.Trace("Adding " + entities.Count + " entities", "AddIngameEntities");

            foreach (IMyEntity entity in entities) {
                AddIngameEntity(entity);
            }
        }

        private void RemoveIngameEntity(IMyEntity entity) {
            Log.Trace("Removing entity " + entity.EntityId + " of type " +
                entity.GetType(), "RemoveIngameEntity");

            if (entity.Transparent) {
                Log.Trace("It's Transparent, skipping", "AddIngameEntity");
                return;
            }

            // TODO: Store other types of entities
            if (entity is IMyCubeGrid) {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                Log.Trace("Removing CubeGrid " + grid.EntityId, "RemoveIngameEntity");

                if (!RevealedGrids.ContainsKey(entity.EntityId)) {
                    Log.Trace("Removed CubeGrid wasn't stored " + grid.EntityId, "RemoveIngameEntity");
                }
                else {
                    RevealedGrids.Remove(entity.EntityId);
                    Log.Trace("Removed " + entity.EntityId, "RemoveIngameEntity");
                }
            }
        }

        public void entityControlAquired(MyEntityController controller) {
            MyEntity parent = controller.ControlledEntity.Entity.GetTopMostParent();

            if (!ControlledEntities)

        }




        #endregion
               * */

    }

}
