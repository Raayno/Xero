// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.


#if UNITY_EDITOR
#endif

namespace AxonGenesis
{
    /// <summary>
    /// Classes that implement this interface may optionally define the static methods shown in the commments below.
    /// These static functions are not required and found using reflection.
    /// </summary>
    public interface ITimeflowBehaviorMenu
    {
        // The following show examples for static methods to define for this interface

        //Implement the following method in each class (only if GUIMenu is NOT defined)
        //public static string AddMenuItemName() { return "Category/Name of Behavior"; }

        // Implement the following method in your TimeflowBehavior class to define a custom menu using TimeflowContext
        //public static void AddMenuItem() { }

        // Define GUIMenu in any subclass that you wish to display menu options for in the Timeflow window (on right
        // mouse click). Since static methods cannot be virtual, this method must be defined individually for each
        // TimeflowBehavior derrived class.
        //public static void GUIMenu() { }
    }

}//AxonGenesis
