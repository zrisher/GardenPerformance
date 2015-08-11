using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance {
    class GardenPerformance {




        public struct BuilderHolder1 {
        }

        public class HoldMe {

        }

        public struct BuilderHolder2 {
            public HoldMe ThingToHold;
        }

        /*
        [XmlType("BuilderHolder3")]
        [ProtoContract]
        public class BuilderHolder3 {
            [ProtoMember, DefaultValue(null)]
            public MyObjectBuilder_CubeGrid Builder;

            public BuilderHolder3() {
                Builder = new MyObjectBuilder_CubeGrid();
            }
        }
        */

        //[XmlType("GardenConquestSettings")]
        public struct Settings {
            public long EntityId;
            public VRageMath.Vector3D Position; // = new VRageMath.Vector3D();
            //public MyObjectBuilder_CubeGrid Builder; // = new MyObjectBuilder_CubeGrid();
            public long OwningFleet;
            public bool HasSpawnPoint;
        }


        /*
          List<int> testIntList = new List<int>() { 1, 2, 3 };
          SEGarden.Files.Manager.writeLine(
              MyAPIGateway.Utilities.
                  SerializeToXML<List<int>>(testIntList),
              "ConcealSaveFileTestInt.txt"
          );

          List<SETTINGS> testStructList = new List<SETTINGS>() { new SETTINGS(), new SETTINGS() };
          SEGarden.Files.Manager.writeLine(
              MyAPIGateway.Utilities.
                  SerializeToXML<List<SETTINGS>>(testStructList),
              "ConcealSaveFileTestStruct.txt"
          );
           * */

        //Logger.Trace("Saving empty list", "Save");
        //concealedList = new List<ConcealedGrid>() { new ConcealedGrid() };


        

          /*
            SEGarden.Files.Manager.writeLine(
                MyAPIGateway.Utilities.SerializeToXML<ConcealedGrid>(concealedList[0]),
                "ConcealSaveFileGridTest.txt");
            */


        /*
        SEGarden.Files.Manager.writeLine(
            MyAPIGateway.Utilities.
                SerializeToXML<List<Settings>>(new List<Settings>() { new Settings() {
                }}),
            "ListSettingsTest.txt"
        );



        VRage.ObjectBuilders.MyObjectBuilderSerializer.SerializeXML(
"SerializeTestVRage-Builder.txt", false, new MyObjectBuilder_Sector(),
typeof(MyObjectBuilder_Sector));




        SEGarden.Files.Manager.writeLine(
            MyAPIGateway.Utilities.
                SerializeToXML<List<BuilderHolder1>>(new List<BuilderHolder1>() { new BuilderHolder1() {
                }}),
            "SerializeTest-ListBuilderHolders1.txt"
        );


        SEGarden.Files.Manager.writeLine(
            MyAPIGateway.Utilities.
                SerializeToXML<List<BuilderHolder2>>(new List<BuilderHolder2>() { new BuilderHolder2() {
                            }}),
            "SerializeTest-ListBuilderHolders2.txt"
        );

        SEGarden.Files.Manager.writeLine(
            MyAPIGateway.Utilities.
                SerializeToXML<MyObjectBuilder_CubeGrid>(new MyObjectBuilder_CubeGrid() { }),
            "SerializeTest-Builder.txt"
        );

        SEGarden.Files.Manager.writeLine(
            MyAPIGateway.Utilities.
                SerializeToXML<List<MyObjectBuilder_CubeGrid>>(new List<MyObjectBuilder_CubeGrid>() { new MyObjectBuilder_CubeGrid() {
                            }}),
            "SerializeTest-ListBuilders.txt"
        );

        SEGarden.Files.Manager.writeLine(
            MyAPIGateway.Utilities.
                SerializeToXML<List<BuilderHolder3>>(new List<BuilderHolder3>() { new BuilderHolder3() {
                }}),
            "SerializeTest-ListBuilderHolders3.txt"
        );
                */


        /*
public interface ConcealedEntity {
    public long EntityId;
    public VRageMath.Vector3D Position;
    public MyObjectBuilder_CubeGrid Builder;
}
         * 
         * 

        public struct ConcealedGrid { //: ConcealedEntity {
            public long EntityId;
            public VRageMath.Vector3D Position; // = new VRageMath.Vector3D();
            //public MyObjectBuilder_CubeGrid Builder; // = new MyObjectBuilder_CubeGrid();
            public long OwningFleet;
            public bool HasSpawnPoint;
        }

        public struct ConcealedEntityInfo { //: ConcealedEntity {
            public long EntityId;
            public VRageMath.Vector3D Position; // = new VRageMath.Vector3D();
            //public MyObjectBuilder_CubeGrid Builder; // = new MyObjectBuilder_CubeGrid();
            public long OwningFleet;
            public bool HasSpawnPoint;
        }
         * 
         *                             /*
            foreach (KeyValuePair<long, ConcealedGrid> pair in concealedGrids) {


                if (pair.Value.Builder == null) {
                    Logger.Error("Builder null for " + pair.Key, "Save");
                    continue;
                }

                if (pair.Value.Position == null) {
                    Logger.Error("Position null for " + pair.Key, "Save");
                    continue;
                }
  
                Logger.Trace("Saving to list " + pair.Key, "Save");
                Logger.Trace("EntityId " + pair.Value.EntityId, "Save");
                Logger.Trace("OwningFleet " + pair.Value.OwningFleet, "Save");
                Logger.Trace("HasSpawnPoint " + pair.Value.HasSpawnPoint, "Save");
                Logger.Trace("Builder entityId " + pair.Value.Builder.EntityId, "Save");
                Logger.Trace("Position " + pair.Value.Position.ToString(), "Save");

                concealedList.Add(pair.Value);
            }
                         * */

            /*
            String test;
            int i = 1;
            Logger.Trace("concealedList null? " + (concealedList == null), "Save");
            Logger.Trace("concealedList.count " + (concealedList.Count), "Save");

            foreach (ConcealedGrid grid in concealedList) {
                Logger.Trace("Try serialize Concealed Grid " + i, "Save");
                //Logger.Trace("Trying setting first", "Save");
                //test = MyAPIGateway.Utilities.SerializeToXML<ConcealedGrid>(grid);
                SEGarden.Files.Manager.writeLine(MyAPIGateway.Utilities.
                    SerializeToXML<ConcealedGrid>(grid),
                "ConcealSaveFileGridTest-" + i + ".txt");

                Logger.Trace("Saved to " + "ConcealSaveFileGridTest-" + i, "Save");
                i++;
            }
             * 
             *             SEGarden.Files.Manager.writeLine(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<ConcealedGrid>>(new List<ConcealedGrid>() { new ConcealedGrid() {
                        Builder = new MyObjectBuilder_CubeGrid(),
                    }}),
                "ConcealSaveFileForThisWorld.txt"
            );


            SEGarden.Files.Manager.writeLine(
                MyAPIGateway.Utilities.
                    SerializeToXML<List<MyObjectBuilder_CubeGrid>>(concealedBuildersInternal),
                "ConcealSaveFileForThisWorld.txt"
            );


         * 
 * */


    }
}
