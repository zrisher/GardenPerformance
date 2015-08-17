using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Chat.Commands;
using SEGarden.Notifications;
using SEGarden.Logging;

using GP.Concealment.Records.Entities;
using GP.Concealment.Messaging.Messages.Requests;


namespace GP.Concealment {

    static class Commands {

        private static Logger Log = new Logger("GardenPerformance.Commands");

        static Command ConcealedCommand = new Command(
            "concealed",
            "list all concealed grids",
            "list all concealed grids....",
            (List<String> args) => {
                Log.Trace("Requesting Concealed Grids", "ConcealedCommand");

                ConcealedGridsRequest request = new ConcealedGridsRequest();
                request.SendToServer();

                return new EmptyNotification();
            }
        );

        static Command RevealedCommand = new Command(
            "revealed",
            "list all revealed grids",
            "list all revealed grids....",
            (List<String> args) => {
                Log.Trace("Requesting Revealed Grids", "RevealedCommand");

                RevealedGridsRequest request = new RevealedGridsRequest();
                request.SendToServer();

                return new EmptyNotification();
            }
        );

        static Command ConcealCommand = new Command(
            "conceal",
            "conceal the Nth grid on the list",
            "Conceal the Nth grid on the list. This will store it locally and " + 
            "prepare it for....",
            (List<String> args) => {
                int n = Int32.Parse(args[0]);

                List<ConcealableGrid> revealedGrids = Session.Client.RevealedGrids;
                
                if (revealedGrids == null) return new ChatNotification() {
                    Text = "No list of revealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > revealedGrids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                ConcealableGrid grid = revealedGrids[n - 1];
                long entityId = grid.EntityId;

                Log.Trace("Requesting Conceal Grid " + entityId, "ConcealCommand");
                ConcealRequest request = new ConcealRequest { EntityId = entityId };
                request.SendToServer();

                return null;

            },
            new List<String> { "N" },
            0
        );

        static Command RevealCommand = new Command(
            "reveal",
            "reveal the Nth grid on the list",
            "Reveal a grid....",
            (List<String> args) => {
                int n = Int32.Parse(args[0]);

                List<ConcealableGrid> grids = Session.Client.ConcealedGrids;

                if (grids == null) return new ChatNotification() {
                    Text = "No list of concealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > grids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                ConcealableGrid grid = grids[n - 1];
                long entityId = grid.EntityId;

                Log.Trace("Requesting Reveal Grid " + entityId, "RevealCommand");
                RevealRequest request = new RevealRequest { EntityId = entityId };
                request.SendToServer();

                return null;
            },
            new List<String> { "N" },
            0
        );

        static Command SaveCommand = new Command(
            "save",
            "save the current conceal state",
            "Blah blah blah longtext about saving",
            (List<String> args) => {              
                //Session.Server.SaveSector();
                return new ChatNotification() { Text = "Saving..." };
            }
        );

        static Command LoadCommand = new Command(
            "load",
            "load the current conceal state",
            "Blah blah blah longtext about loading",
            (List<String> args) => {
                //Session.Server.LoadSector();
                return new ChatNotification() { Text = "Loading..." };
            }
        );

        static Tree ConcealmentTree = new Tree(
            "concealment",
            "manage entity concealment",
            "Grids are automatically concealed if they are: \n" +
            "  * Not controlled by a person or autopilot\n" +
            "  * Not within 35km of a controlled grid\n" +
            "  * Not moving\n" +
            "  * Not in an asteroid\n" +
            "  * Not refining or manufacturing\n" +
            "  * Not providing spawn points for players",
            0,
            new List<Node> {
                ConcealedCommand,
                RevealCommand,
                RevealedCommand,
                ConcealCommand,
                SaveCommand,
                LoadCommand
            }
        );

        public static Tree FullTree = new Tree(
            "gp",
            "all GardenPerformance commands",
            "GardenPerformance offers numerous commands to assist in use and " +
            "configuration.",
            0,
            new List<Node> { ConcealmentTree }
        );

    }

}
