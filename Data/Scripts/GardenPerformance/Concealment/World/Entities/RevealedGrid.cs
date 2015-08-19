using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Sandbox.Common;
using Sandbox.Definitions;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.Components;
using Sandbox.Engine;
using Sandbox.Game;
using Sandbox.Game.Entities;

using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

using SEGarden.Extensions;
using SEGarden.Extensions.Objectbuilders;
using SEGarden.Logging;

namespace GP.Concealment.World.Entities {

    // Revealed entities cannot be concealed if they
    // Are controlled
    // Are "working" (refining, assembling, oxy creating, battery charging)
    // Are moving, 
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CubeGrid))]
    public class RevealedGrid : MyGameLogicComponent {

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false) {
            return new MyObjectBuilder_EntityBase();
        }

        #region Static

        private static Logger Log =
            new Logger("GP.Concealment.Records.Entities.ConcealableEntity");

        #endregion
        #region Instance

        private IMyCubeGrid Grid;
        private Dictionary<long, IMyControllableEntity> ControlsInUse;
        private Dictionary<long, IMyControllableEntity> ControlsNotInUse;

        public bool Controlled { get { return ControlsInUse.Count > 0; } }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder) {
            /*
            base.Init(objectBuilder);
            Grid = Container.Entity as IMyCubeGrid;

            Log.Trace("Loaded into new grid", "Init");

            // If this is not the server we don't need this class.
            // When we modify the grid on the server the changes should be
            // sent to all clients
            try {
                m_IsServer = Utility.isServer();
                Log.Trace("Is server: " + m_IsServer, "Init");
                if (!m_IsServer) {
                    // No cleverness allowed :[
                    Log.Trace("Disabled.  Not server.", "Init");
                    m_Logger = null;
                    m_Grid = null;
                    return;
                }
            }
            catch (NullReferenceException e) {
                log("Exception.  Multiplayer is not initialized.  Assuming server for time being: " + e,
                    "Init");
                // If we get an exception because Multiplayer was null (WHY KEEN???)
                // assume we are the server for a little while and check again later
                m_IsServer = true;
                m_CheckServerLater = true;
            }

            // We need to only turn on our rule checking after startup. Otherwise, if
            // a beacon is destroyed and then the server restarts, all but the first
            // 25 blocks will be deleted on startup.
            Grid.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;


            m_Grid.OnBlockAdded += blockAdded;
            m_Grid.OnBlockRemoved += blockRemoved;
            m_Grid.OnBlockOwnershipChanged += blockOwnerChanged;
            m_GridSubscribed = true;
        }

        public override void Close() {
            log("Grid closed", "Close");
            unServerize();


             * */

        }
        #endregion
        public override void UpdateBeforeSimulation100() {
            /*
			// Must be server, not be closing
			// Must not be transparent - aka a new grid not yet placed
			if (!m_IsServer || m_MarkedForClose || Grid.MarkedForClose || Grid.Transparent)
				return;

			// Do we need to verify that we are the server?
			if (m_CheckServerLater && m_IsServer) {
				try {
					log("Late server check", "UpdateBeforeSimulation100");
					m_IsServer = Utility.isServer();
					log("Is server: " + m_IsServer, "Init");
					m_CheckServerLater = false;

					if (!m_IsServer) {
						unServerize();
						return;
					}
				} catch (NullReferenceException e) {
					// Continue thinking we are server for the time being
					// This shouldn't happen (lol)
					log("Exception checking if server: " + e, "UpdateBeforeSimulation100");
				}
			}

			// = Main update logic
			try {
				// if cleanup previously marked this grid for deletion, do it and get us out of here
				if (m_DeleteNextUpdate) {
					m_DeleteNextUpdate = false;
					log("deleting all blocks and closing grid", "UpdateBeforeSimulation100");
					removeAllBlocks();
					m_MarkedForClose = true;
					return;
				}

				// clear flags not used in updates
				// when initing or merging, if any blocks are added they will flag m_CheckCleanup,
				// but blockAdded uses these flags to know it must allow any block through temporarily
				if (!m_BeyondFirst100) {
					m_BeyondFirst100 = true;
				}
				if (m_Merging) {
					m_Merging = false;
				}

				// If we're missing State data, try to get it
				if (!m_StateLoaded) {
					StateTracker = StateTracker.getInstance();

					if (StateTracker != null) {
						// Load state-dependent things
						m_StateLoaded = true;
					}
				}

				// check for existing cleanup, fix failed timers and let owner know
				// if cleanup is ongoing
				if (m_CleanupTimer != null) {
					m_CleanupTimer.updateTimeRemaining();
					notifyViolations();
				}

				// Update ownership
				if (m_CheckOwnerNextUpdate || m_CheckCleanupNextUpdate || m_CheckClassifierNextUpdate) {
					log("checking owner due to flag", "UpdateBeforeSimulation100");
					m_CheckOwnerNextUpdate = false;

					reevaluateOwnership();
				}

				// Update classification
				if (m_CheckClassifierNextUpdate) {
					log("checking classifier due to flag", "UpdateBeforeSimulation100");
					m_CheckClassifierNextUpdate = false;

					reevaluateClassification();
				}

				// Update cleanup state - violations & timers
				if (m_CheckCleanupNextUpdate) {
					log("checking cleanup state due to flag", "UpdateBeforeSimulation100");
					m_CheckCleanupNextUpdate = false;

					updateViolations();
					updateCleanupTimers();
				}

				// Do cleanup if needed
				if (m_CleanupTimer != null && m_CleanupTimer.TimerExpired) {
					log("timer expired, running cleanup", "UpdateBeforeSimulation100");

					doCleanupPhase();
					if (m_DeleteNextUpdate) return;
					m_CheckCleanupNextUpdate = true;
				}

			} catch (Exception e) {
				log("Exception occured: " + e, "UpdateBeforeSimulation100", Logger.severity.ERROR);
			}
             * */
		}


		#region SE Hooks - Block Added

		private void blockAdded(IMySlimBlock slimBlock) {
            /*

            //if (SEGarden.GardenGateway.RunningOn != SEGarden.Logic.Common.RunLocation.Server)
            //    return;

			//log(added.ToString() + " added to grid " + m_Grid.DisplayName, "blockAdded");

            IMyCubeBlock fatblock = slimBlock.FatBlock;
            if (fatblock == null) return;

            IMyControllableEntity control = fatblock as IMyControllableEntity;
            if (control == null) return;

            if (control.ControllerInfo.ControllingIdentityId != 0) {
                ControlsInUse.Add(fatblock.EntityId, control);
            } else {
                ControlsNotInUse.Add(fatblock.EntityId, control);
            }

            control.ControllerInfo.ControlAcquired += ControlAcquired;
            control.ControllerInfo.ControlReleased += ControlReleased;
             * */
		
		}

        private void blockRemoved(IMySlimBlock slimBlock) {

			//log(added.ToString() + " added to grid " + m_Grid.DisplayName, "blockAdded");

            IMyCubeBlock fatblock = slimBlock.FatBlock;
            if (fatblock == null) return;

            IMyControllableEntity control = fatblock as IMyControllableEntity;
            if (control == null) return;

            /* uuuuugggh we can;'t use controllerinfo
            if (control.ControllerInfo.ControllingIdentityId != 0) {
                if (!ControlsInUse.ContainsKey(fatblock.EntityId)) {
                    Log.Error("Removed unstored used controller", "blockRemoved");
                    return;
                }
                ControlsInUse.Remove(fatblock.EntityId);
            } else {
                if (!ControlsNotInUse.ContainsKey(fatblock.EntityId)) {
                    Log.Error("Removed unstored unused controller", "blockRemoved");
                    return;
                }
                ControlsNotInUse.Remove(fatblock.EntityId);
            }

            control.ControllerInfo.ControlAcquired -= ControlAcquired;
            control.ControllerInfo.ControlReleased -= ControlReleased;	
             * */
		}
        /*

        private void ControlAcquired(Sandbox.Game.World.MyEntityController controller) {
            IMyControllableEntity control = controller.ControlledEntity;
            long controlId = control.Entity.EntityId;

            if (!ControlsNotInUse.ContainsKey(controlId)) {
                Log.Error("Acquired control of unstored controller", "ControlAcquired");
            } else {
                ControlsNotInUse.Remove(controlId);
            }

            ControlsInUse.Add(controlId, control);
        }

        private void ControlReleased(Sandbox.Game.World.MyEntityController controller) {
            IMyControllableEntity control = controller.ControlledEntity;
            long controlId = control.Entity.EntityId;

            if (!ControlsInUse.ContainsKey(controlId)) {
                Log.Error("Released control of unstored controller", "ControlAcquired");
            } else {
                ControlsInUse.Remove(controlId);
            }

            ControlsNotInUse.Add(controlId, control);
        }
        */

        #endregion
    }
}
