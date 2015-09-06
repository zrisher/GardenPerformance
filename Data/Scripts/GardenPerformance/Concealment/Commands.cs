using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Chat.Commands;
using SEGarden.Notifications;
using SEGarden.Logging;

using GP.Concealment.World.Entities;
using GP.Concealment.Messages.Requests;

using GP.Concealment.Sessions;


namespace GP.Concealment {

    static class Commands {

        private static Logger Log = new Logger("GardenPerformance.Commands");

        private static ClientConcealSession Session { 
            get { return ClientConcealSession.Instance; } 
        }

        static Command ObserversCommand = new Command(
            "observers",
            "list all observing entities",
            "Observing entities are....",
            (List<String> args) => {
                Log.Trace("Requesting Observing Entities", "ObserversCommand");

                ObservingEntitiesRequest request = new ObservingEntitiesRequest();
                request.SendToServer();

                return new EmptyNotification();
            }
        );

        static Command SettingsListCommand = new Command(
            "list",
            "list settings",
            "Settings are....",
            (List<String> args) => {
                Log.Trace("Requesting Observing Entities", "ObserversCommand");

                Notification notice = new WindowNotification() {
                    Text = Session.Settings.Describe(),
                    BigLabel = "Garden Performance",
                    SmallLabel = "Settings"
                };

                return notice;
            }
        );

        static Command SettingsSetCommand = new Command(
            "set",
            "set setting N to X",
            "Details....",
            (List<String> args) => {
                byte n = Byte.Parse(args[0]);
                ushort x = UInt16.Parse(args[1]);

                Settings settings = Session.Settings;

                if (settings == null) return new ChatNotification() {
                    Text = "No list of settings available.",
                    Sender = "GP"
                };


                if (n < 1 || n > settings.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                // TODO - have a setting identifier enum
                Log.Trace("Requesting change setting " + n + " to " + x, "ConcealCommand");

                ChangeSettingRequest request = new ChangeSettingRequest {
                    Index = n,
                    Value = x
                };
                request.SendToServer();

                return null;

            },
            new List<String> { "N", "X" },
            0
        );


        static Tree SettingsTree = new Tree(
            "settings",
            "setting management",
            "setting management...",
            0,
            new List<Node> { SettingsListCommand, SettingsSetCommand }
        );

        static Command ConcealedListCommand = new Command(
            "list",
            "list all concealed grids",
            "list all concealed grids....",
            (List<String> args) => {
                Log.Trace("Requesting Concealed Grids", "ConcealedCommand");

                ConcealedGridsRequest request = new ConcealedGridsRequest();
                request.SendToServer();

                return new EmptyNotification();
            }
        );

        static Command ConcealedDetailCommand = new Command(
            "detail",
            "show detail for grid N",
            "show detail for grid N....",
            (List<String> args) => {
                ushort n = UInt16.Parse(args[0]);

                List<ConcealedGrid> concealedGrids = Session.ConcealedGrids;

                if (concealedGrids == null) return new ChatNotification() {
                    Text = "No list of revealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > concealedGrids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                ConcealedGrid grid = concealedGrids[n - 1];

                String text = grid.ConcealmentDetails() + 
                    "\nNote: Run \"/gp c concealed list\" to refresh this info.";

                return new WindowNotification() {
                    Text = text,
                    BigLabel = "Garden Performance",
                    SmallLabel = "Revealed Grid Detail"
                };
            },
            new List<String> { "N"},
            0
        );

        static Command ConcealedRevealCommand = new Command(
            "reveal",
            "reveal the Nth grid on the list",
            "Reveal a grid....",
            (List<String> args) => {
                int n = Int32.Parse(args[0]);

                List<ConcealedGrid> grids = Session.ConcealedGrids;

                if (grids == null) return new ChatNotification() {
                    Text = "No list of concealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > grids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                ConcealedGrid grid = grids[n - 1];
                long entityId = grid.EntityId;

                Log.Trace("Requesting Reveal Grid " + entityId, "RevealCommand");
                RevealRequest request = new RevealRequest { EntityId = entityId };
                request.SendToServer();

                return null;
            },
            new List<String> { "N" },
            0
        );

        static Tree ConcealedTree = new Tree(
            "concealed",
            "concealed grid management",
            "concealed grid management...",
            0,
            new List<Node> { 
                ConcealedListCommand, 
                ConcealedDetailCommand, 
                ConcealedRevealCommand 
            }
        );

        static Command RevealedListCommand = new Command(
            "list",
            "list all revealed grids",
            "list all revealed grids....",
            (List<String> args) => {
                Log.Trace("Requesting Revealed Grids", "RevealedCommand");

                RevealedGridsRequest request = new RevealedGridsRequest();
                request.SendToServer();

                return new EmptyNotification();
            }
        );

        static Command RevealedDetailCommand = new Command(
            "detail",
            "show detail for grid N",
            "show detail for grid N....",
            (List<String> args) => {
                ushort n = UInt16.Parse(args[0]);

                List<RevealedGrid> revealedGrids = Session.RevealedGrids;

                if (revealedGrids == null) return new ChatNotification() {
                    Text = "No list of revealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > revealedGrids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                RevealedGrid grid = revealedGrids[n - 1];

                String text = grid.ConcealDetails() +
                    "\nNote: Run \"/gp c revealed list\" to refresh this info.";

                return new WindowNotification() {
                    Text = text,
                    BigLabel = "Garden Performance",
                    SmallLabel = "Revealed Grid Detail"
                };
            },
            new List<String> { "N" },
            0
        );

        static Command RevealedConcealCommand = new Command(
            "conceal",
            "conceal the Nth grid on the list",
            "Conceal the Nth grid on the list. This will store it locally and " + 
            "prepare it for....",
            (List<String> args) => {
                int n = Int32.Parse(args[0]);

                List<RevealedGrid> revealedGrids = Session.RevealedGrids;
                
                if (revealedGrids == null) return new ChatNotification() {
                    Text = "No list of revealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > revealedGrids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                RevealedGrid grid = revealedGrids[n - 1];
                long entityId = grid.EntityId;

                Log.Trace("Requesting Conceal Grid " + entityId, "ConcealCommand");
                ConcealRequest request = new ConcealRequest { EntityId = entityId };
                request.SendToServer();

                return null;

            },
            new List<String> { "N" },
            0
        );

        static Tree RevealedTree = new Tree(
            "revealed",
            "revealed grid management",
            "revealed grid management...",
            0,
            new List<Node> { 
                RevealedListCommand, 
                RevealedDetailCommand, 
                RevealedConcealCommand 
            }
        );

        static Tree ConcealmentTree = new Tree(
            "c",
            "manage entity concealment",
            BuildConcealmentInfo(),
            0,
            new List<Node> {
                ConcealedTree,
                RevealedTree,
                ObserversCommand,
                SettingsTree,
            },
            "concealment"
        );

        public static Tree FullTree = new Tree(
            "gp",
            "all GardenPerformance commands",
            " --- GardenPerformance  - v " + ModInfo.Version + " --- \n" + 
            "Offers numerous commands to assist in use and " +
            "configuration.",
            0,
            new List<Node> { ConcealmentTree }
        );

        private static String BuildConcealmentInfo() {
            return
                "All grids are automatically concealed unless they: \n" +
                " * Are Controlled:\n" +
                "    * Piloted by a player OR\n" +
                "    * Moving or moved within the last " + 
                Settings.Instance.ControlledMovingGraceTimeSeconds + " seconds.\n" +
                " * Are within " + Settings.Instance.RevealVisibilityMeters + 
                "m of a controlled grid or player.\n" +
                " * Have medbays or cryochambers for a current player\n" +
                " * Are Producing:\n" +
                "    * refining\n"+
                "    * assembling\n"+
                "    * generating oxygen\n"+
                "    * charging batteries\n"+
                " * Were revealed within the past " + 
                Settings.Instance.RevealedMinAgeSeconds + " seconds\n" +
                ((Settings.Instance.ConcealNearAsteroids) ? "" : 
                " * Are within the bounding box of an asteroid.");
        }

    }

}
