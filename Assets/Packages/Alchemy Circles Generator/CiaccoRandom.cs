using UnityEngine;
using System.Collections;

namespace AlchemyCirclesGenerator
{
    public class CiaccoRandom
    {
        private static int superSeed = 0;

        /// Sets the seed of the Random Generator.
        /// seed can only be positive and seed range is [0 -> 9999998] seed=9999999 should give the same as seed=0
        public void SetSeed(int seed)
        {
            superSeed = Mathf.Abs(seed) % 9999999 + 1;
            // init for randomness fairness
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
            superSeed = (superSeed * 125) % 2796203;
        }

        /// both included: getRand(0,1) will return 0s and 1s
        public int GetRand(int min, int max)
        {
            superSeed = (superSeed * 125) % 2796203;
            return superSeed % (max - min + 1) + min;
        }
    }
}
