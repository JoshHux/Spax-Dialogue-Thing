using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
namespace Menu
{
    public class EnumHelper
    {
        //call this to check if the specifies level is the current most outer layer
        //IMPORTANT: assumes that the layer bit is already in checkAgainst
        public static bool IsMostOuterLayer(MenuLevel layer, MenuLevel checkAgainst)
        {
            //if the layer bit is the outer most bit, then it has the largest value
            // but if there is a bit with a larger value, then removing the layer bit
            // results in a value that is still larger than the layer bit, otherwise
            // it should always be smaller

            //but if the bit isn't there, that causes issues, let's fix that
            //check if bit is there
            var bitExists = layer & checkAgainst;
            //if it doesn't exist, the checkAgainst value won't change, otherwise, it will
            var val = (int)(bitExists ^ checkAgainst);

            //Debug.Log(bitExists);
            //Debug.Log(val);
            //Debug.Log((int)checkAgainst);
            //Debug.Log(val < (int)layer && bitExists > 0);

            return val < (int)layer && bitExists > 0;
        }
    }
}