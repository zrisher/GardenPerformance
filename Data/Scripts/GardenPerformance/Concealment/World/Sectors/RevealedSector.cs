using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

using SEGarden.Logging;
using SEGarden.Math;

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
    public class RevealedSector {

        private static Logger Log = 
            new Logger("GP.Concealment.World.Sectors.RevealedSector");

        #region Instance Fields

        // These cause us to reveal things
        private List<ulong> ActiveSteamIDs = new List<ulong>();

        // Populate this with everyone online as well as all players in their faction,
        // because spawn is shared
        public List<long> ActivePlayersAndAllies = new List<long>();

        private Dictionary<long, ControllableEntity> ControlledEntities =
            new Dictionary<long, ControllableEntity>();

        private Dictionary<long, ObservableEntity> ObservableEntities =
            new Dictionary<long, ObservableEntity>();

        // These can be concealed or marked to remain revealed
        private Dictionary<long, RevealedGrid> Grids =
            new Dictionary<long, RevealedGrid>();

        private AABBTree GridTree = new AABBTree();

        #endregion
        #region Public Field Access Helpers

        public List<RevealedGrid> RevealedGridsList() {
            Log.Trace("Returning concealed grids list of count " + Grids.Values.Count, "RevealedGridsList");
            return Grids.Values.ToList();
        }

        public List<ObservingEntity> ObservingEntitiesList() {
            // TODO - implement
            //Log.Trace("Returning ObservingEntities list of count " + Grids.Values.Count, "RevealedGridsList");
            //return Grids.Values.ToList();
            return new List<ObservingEntity>();
        }

        #endregion
        #region Private Field Access Helpers

        private void RememberControlledEntity(ControllableEntity e) {
            long id = e.EntityId;
            if (ControlledEntities.ContainsKey(id)) {
                Log.Error("Already added " + id, "RememberControlledEntity");
                return;
            }

            Log.Trace("Adding " + id, "RememberControlledEntity");
            ControlledEntities.Add(id, e);
        }

        private void ForgetControlledEntity(ControllableEntity e) {
            long id = e.EntityId;
            if (!ControlledEntities.ContainsKey(id)) {
                Log.Error("Not stored " + id, "ForgetControlledEntity");
                return;
            }

            Log.Trace("Removing " + id, "ForgetControlledEntity");
            ControlledEntities.Remove(id);
        }

        private void RememberGrid(RevealedGrid e) {
            long id = e.EntityId;
            if (Grids.ContainsKey(id)) {
                Log.Error("Already added " + id, "RememberGrid");
                return;
            }

            Log.Trace("Adding " + id, "RememberGrid");
            Grids.Add(id, e);
            GridTree.Add(e);
        }

        private void ForgetGrid(RevealedGrid e) {
            long id = e.EntityId;
            if (!Grids.ContainsKey(id)) {
                Log.Error("Not stored " + id, "ForgetGrid");
                return;
            }

            Log.Trace("Removing " + id, "ForgetGrid");
            Grids.Remove(id);
            GridTree.Remove(e);
        }

        private void RememberPlayerId(ulong id) {
            if (ActiveSteamIDs.Contains(id)) {
                Log.Error("Already added steam id: " + id, "RememberPlayerId");
                return;
            }

            Log.Trace("Adding steam id " + id, "RememberPlayerId");
            ActiveSteamIDs.Add(id);
        }

        private void ForgetPlayerId(ulong id) {
            if (!ActiveSteamIDs.Contains(id)) {
                Log.Error("Steam id not stored: " + id, "RememberPlayerId");
                return;
            }

            Log.Trace("Removing steam id " + id, "RememberPlayerId");
            ActiveSteamIDs.Remove(id);
        }

        #endregion
        #region Entity Updates

        public void ControllableEntityAdded(ControllableEntity e) {
            Log.Trace("Controllable Entity Added", "ControllableEntityAdded");

            if (e.IsControlled) RememberControlledEntity(e);
            RevealedGrid grid = e as RevealedGrid;
            if (grid != null) RememberGrid(grid);
        }

        public void ControllableEntityMoved(ControllableEntity e) {
            //Log.Trace("Controllable Entity Moved", "ControllableEntityAdded");
        }

        public void ControllableEntityRemoved(ControllableEntity e) {
            Log.Trace("Controllable Entity Added", "ControllableEntityAdded");

            if (e.IsControlled) ForgetControlledEntity(e);
            RevealedGrid grid = e as RevealedGrid;
            if (grid != null) ForgetGrid(grid);
        }

        public void ControllableEntityControlled(ControllableEntity e) {
            Log.Trace("Controllable Entity Added", "ControllableEntityAdded");
            RememberControlledEntity(e);
        }

        public void ControllableEntityReleased(ControllableEntity e) {
            Log.Trace("Controllable Entity Added", "ControllableEntityAdded");
            ForgetControlledEntity(e);
        }

        public void PlayerLoggedIn(ulong steamId) {
            RememberPlayerId(steamId);
        }

        public void PlayerLoggedOut(ulong steamId) {
            ForgetPlayerId(steamId);
        }

        #endregion
        #region Marking

        public List<ObservableEntity> ObservableInSphere(BoundingSphereD bounds) {
            var results = new List<ObservableEntity>();
            GridTree.GetAllEntitiesInSphere<ObservableEntity>(ref bounds, results);
            return results;
        }

        /*
        public List<ObservableEntity> GridsInBox(BoundingBoxD bounds) {
            var results = new List<ObservableEntity>();
            GridTree.GetAllEntitiesInBox<RevealedGrid>(ref bounds, results);
            return results;
        }

        public List<ObservableEntity> GridsInSphere(Vector3D center, double Radius) {
            BoundingSphereD sphere2 = new BoundingSphereD()
            var results = new List<ObservableEntity>();
            GridTree.GetAllEntitiesInSphere<RevealedGrid>(ref sphere, results);
            return results;
        }

        public List<ObservableEntity> GridsInSphere(BoundingSphereD sphere) {
            var results = new List<ObservableEntity>();
            GridTree.GetAllEntitiesInSphere<RevealedGrid>(ref sphere, results);
            return results;
        }
         * */

        #endregion
        #region Session Updates

        public void Conceal() {
            // look through all grids for those that are revealable
            // send their entity Ids to concealed sector for conceal
            // or maybe actually do the hard part here? A few at a time?
            // it's really working with revealed entities until they are concealed,
            // so maybe it makes sense to manage that queue here and let the
            // concealed work handle the reveal queue and concealed grids that need
            // to be revealed

            // Grid should handle concealability based on marked near controlled
            // + asteroid, working, internal controlled (moving/has pilots)
        }

        #endregion

    }

}
