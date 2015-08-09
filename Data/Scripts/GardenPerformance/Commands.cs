using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SEGarden.Chat.Commands;
using SEGarden.Notifications;

namespace GardenPerformance {

    static class Commands {


        static Command ConcealmentListCommand = new Command(
            "list",
            "list the concealment status of all grids",
            "The concealment status of all grids....",
            (List<String> inputs) => {
                return new WindowNotification() {
                    Text = "Revealed grid ''",
                    BigLabel = "Garden Performance",
                    SmallLabel = "Grid Concealment List"
                };
            }
        );

        static Command ConcealmentConcealCommand = new Command(
            "conceal",
            "conceal the Nth grid on the list",
            "Conceal a grid. This will store it locally and prepare it for....",
            (List<String> inputs) => {
                return new AlertNotification() {
                    Text = "Concealed grid ''",
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
            (List<String> inputs) => {
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
            "Grids are automatically concealed if they are: \n " +
            "  * Not controlled by a person or autopilot\n" +
            "  * Not within 35km of a controlled grid\n" +
            "  * Not refining or manufacturing\n" +
            "  * Not providing spawn points for players\n",
            0,
            new List<Node> {
                ConcealmentListCommand,
                ConcealmentConcealCommand,
                ConcealmentRevealCommand
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
