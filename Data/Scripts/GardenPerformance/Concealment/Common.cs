using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GardenPerformance.Concealment.Common {

    public enum EntityType { 
        Unknown,
        Asteroid,
        Character,
        FloatingObject,
        Grid, 
        Planet, 
    }

    [FlagsAttribute]
    public enum Concealability : short {
        Concealable = 0,
        Controlled = 1,
        NearControlled = 2,
        Moving = 4,
        Working = 8, // refining or manufacturing
        NearAsteroid = 16,
        NeededForSpawn = 32,
    };

    [FlagsAttribute]
    public enum Revealability : short {
        Revealable = 0,
        Blocked = 1,
    };

}
