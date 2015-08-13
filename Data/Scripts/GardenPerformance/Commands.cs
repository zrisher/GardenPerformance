using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;

using SEGarden.Chat.Commands;
using SEGarden.Notifications;
using SEGarden.Logging;

using GardenPerformance.Concealment.Entities;

namespace GardenPerformance {

    static class Commands {

        private static Logger Log = new Logger("GardenPerformance.Commands");
        private static List<IMyCubeGrid> RevealedGrids;

        static Command ConcealmentConcealedCommand = new Command(
            "concealed",
            "list all concealed grids",
            "list all concealed grids....",
            (List<String> args) => {
                Log.Trace("Running command", "ConcealmentConcealedCommand");
                Concealer.RequestConcealedGrids();
                return new EmptyNotification();
            }
        );

        static Command ConcealmentRevealedCommand = new Command(
            "revealed",
            "list all revealed grids",
            "list all revealed grids....",
            (List<String> args) => {
                Concealer.RequestRevealedGrids();
                return new EmptyNotification();
            }
        );

        static Command ConcealmentConcealCommand = new Command(
            "conceal",
            "conceal the Nth grid on the list",
            "Conceal the Nth grid on the list. This will store it locally and " + 
            "prepare it for....",
            (List<String> args) => {
                int n = Int32.Parse(args[0]);

                if (RevealedGrids == null) return new ChatNotification() {
                    Text = "No list of revealed grids available.",
                    Sender = "GP"
                };

                if (n < 1 || n > RevealedGrids.Count) return new ChatNotification() {
                    Text = "Incorrect index for list",
                    Sender = "GP"
                };

                IMyCubeGrid grid = RevealedGrids[n - 1];

                Concealer.RequestConceal(grid.EntityId);

                return new AlertNotification() {
                    Text = "Concealing grid " + n + " - " + grid.EntityId,
                    DisplaySeconds = 5,
                    Color = Sandbox.Common.MyFontEnum.White,
                };
            },
            new List<String> { "N" },
            0
        );

        static Command ConcealmentRevealCommand = new Command(
            "reveal",
            "reveal the Nth grid on the list",
            "Reveal a grid....",
            (List<String> args) => {
                return new AlertNotification() {
                    Text = "Revealed grid ''",
                    DisplaySeconds = 5,
                    Color = Sandbox.Common.MyFontEnum.White,
                };
            },
            new List<String> { "N" },
            0
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
                ConcealmentConcealedCommand,
                ConcealmentRevealCommand,
                ConcealmentRevealedCommand,
                ConcealmentConcealCommand,
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
