using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Menu
{
    //basically, this is a hierarchy of what menus override what
    // we can layer the levels and remove them as needed
    // is an int so that we can take advantage of arithmatic shift to create bitmasks
    [Flags]
    public enum MenuLevel : int
    {
        //gameplay, anything can override this
        GAMEPLAY = 1,
        //we want to lose control over the player while dialogue is going on
        DIALOGUE = 1 << 1,
        //we want to peek into our backpack if we're in either gameplay or dialogue
        BACKPACK = 1 << 2,
        //overrides all other dialogue stuff
        PAUSE = 1 << 3,
    }
}