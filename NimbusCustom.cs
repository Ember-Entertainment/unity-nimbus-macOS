// This was only an issue on osx & in the editor, so leaving it out of other builds for now
#if UNITY_STANDALONE_OSX || UNITY_EDITOR

using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

// This file was cobbled together from a few sources:
// • Unity's Custom Device sample for the new input system. Note that I created a separate
//   project for this, then copied the source from there to keep Unity from polluting ours.
//    • Found at: go to Window->Package Manager->Input System, then click on the Import into Project button for the Custom Device sample
//    • This sample wasn't doing exactly what we're looking for, so I removed a lot of comments/code that didn't exactly
//      fit the Nimbus. However, I highly recommend looking at the original file too, since some of that
//      information is still valuable if you're looking to implement some other device
//    • I did leave some comments from it intact where they still pertained to our case.
// • https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/HID.html#overriding-the-hid-fallback
//    • This shows a layout that could be used for a Dualshock 4, which gives some insight into different InputControl
//      parameters. It also shows an explicit layout, which is a little different from what we do here, where we specify
//      offsets per control.
// • Unity's Input Debugger (Window->Analysis->Input Debugger)
//    • Useful for showing how memory is mapped and what the names are for a Gamepad that need to be overridden
//    • To view raw memory, do something on the device that generates input events (hit a button, move a stick, etc)
//      and then double click an event. Then, in the window that pops up, hit the Display Raw Memory button in the upper left.
//      This was very useful, since the other displays don't show everything that is happening, only what's been defined so far.


// This is where most of the magic happens. We're mapping memory sent in from the device to input controls. You can find
// documenation of the input control attribute here:
// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Layouts.InputControlAttribute.html
//
// There are two parameters that designate our location in memory, offset and bit, where offset is in bytes.
//
// Note that for the name parameter, we are specifically overriding the names that are used for gamepad state info.
//

//
// From the Custon Device sample:
// --
// The input system stores a chunk of memory for each device. What that
// memory looks like we can determine ourselves. The easiest way is to just describe
// it as a struct.
//
// Each chunk of memory is tagged with a "format" identifier in the form
// of a "FourCC" (a 32-bit code comprised of four characters). Using
// IInputStateTypeInfo we allow the system to get to the FourCC specific
// to our struct.
// --
public struct CustomDeviceState : IInputStateTypeInfo
{
    // seems good? I think this can pretty much be whatever you want.
    public FourCC format => new FourCC('H', 'I', 'D');

    //
    // For the Nimbus, there are two sticks, with two bytes apiece, one for each axis. Note that some devices will have an
    // unsigned byte per axis that centers at 127 (this is what Unity goes with for their sample). However, the Nimbus has a
    // signed byte that centers at 0, so we use the SBYT format below. defaultState defaults to 0, so no need to specify that
    // here (it would be 127 in the unsigned case).

    // -- left stick --
    [InputControl(name = "leftStick", offset = 0, format = "VC2B", layout = "Stick", displayName = "Left Stick")]
    [InputControl(name = "leftStick/x", format = "SBYT", offset = 0)]
    public byte leftStickX;
    [InputControl(name = "leftStick/y", format = "SBYT", offset = 1)]
    [InputControl(name = "leftStick/up", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=0,clampMax=1")]
    [InputControl(name = "leftStick/down", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=-1,clampMax=0,invert")]
    [InputControl(name = "leftStick/left", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=-1,clampMax=0,invert")]
    [InputControl(name = "leftStick/right", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=0,clampMax=1")]
    public byte leftStickY;

    // -- right stick --
    [InputControl(name = "rightStick", offset = 2, format = "VC2B", layout = "Stick", displayName = "Right Stick")]
    [InputControl(name = "rightStick/x", format = "SBYT", offset = 0)]
    public byte rightStickX;
    [InputControl(name = "rightStick/y", format = "SBYT", offset = 1)]
    [InputControl(name = "rightStick/up", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=0,clampMax=1")]
    [InputControl(name = "rightStick/down", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=-1,clampMax=0,invert")]
    [InputControl(name = "rightStick/left", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=-1,clampMax=0,invert")]
    [InputControl(name = "rightStick/right", parameters = "normalize,normalizeMin=-1,normalizeMax=1,normalizeZero=0,clamp=2,clampMin=0,clampMax=1")]
    public byte rightStickY;

    //
    // The nimbus also has two analog controls: the left and right trigger. Each is represented with a byte.

    // -- left trigger --
    [InputControl(name = "leftTrigger", offset = 4, format = "BYTE")]
    public byte leftTrigger;

    // -- right trigger --
    [InputControl(name = "rightTrigger", offset = 5, format = "BYTE")]
    public byte rightTrigger;

    //
    // And finally a set of buttons in two bytes, with each button representad as a single bit.

    // -- buttons 1 --
    [InputControl(name = "dpad", format = "BIT", layout = "Dpad", offset = 6, sizeInBits = 4)]
    [InputControl(name = "dpad/up", layout = "Button", bit = 0)]
    [InputControl(name = "dpad/right", layout = "Button", bit = 1)]
    [InputControl(name = "dpad/down", layout = "Button", bit = 2)]
    [InputControl(name = "dpad/left", layout = "Button", bit = 3)]
    [InputControl(name = "buttonSouth", layout = "Button", offset = 6, bit = 4, displayName = "Button South")]
    [InputControl(name = "buttonEast", layout = "Button", offset = 6, bit = 5, displayName = "Button East")]
    [InputControl(name = "buttonWest", layout = "Button", offset = 6, bit = 6, displayName = "Button West")]
    [InputControl(name = "buttonNorth", layout = "Button", offset = 6, bit = 7, displayName = "Button North")]
    public ushort buttons1;

    // -- buttons 2 --
    [InputControl(name = "leftShoulder", format = "BIT", layout = "Button", offset = 7, bit = 0)]
    [InputControl(name = "rightShoulder", format = "BIT", layout = "Button", offset = 7, bit = 1)]
    [InputControl(name = "leftStickPress", format = "BIT", layout = "Button", offset = 7, bit = 2)]
    [InputControl(name = "rightStickPress", format = "BIT", layout = "Button", offset = 7, bit = 3)]
    [InputControl(name = "systemButton", format = "BIT", layout = "Button", offset = 7, bit = 4, displayName = "System")]
    [InputControl(name = "select", format = "BIT", layout = "Button", offset = 7, bit = 5, displayName = "Select")]
    [InputControl(name = "start", format = "BIT", layout = "Button", offset = 7, bit = 6, displayName = "Start")]
    public ushort buttons2;

}

// Two things of note here (from the sample):
//
// --
// For one, we want to ensure that the call to InputSystem.RegisterLayout happens as
// part of startup. Doing so ensures that the layout is known to the input system and
// thus appears in the control picker. So we use [InitializeOnLoad] and [RuntimeInitializeOnLoadMethod]
// here to ensure initialization in both the editor and the player.
//
// Also, we use the [InputControlLayout] attribute here. This attribute is optional on
// types that are used as layouts in the input system. In our case, we have to use it
// to tell the input system about the state struct we are using to define the memory
// layout we are using and the controls tied to it.
// --
#if UNITY_EDITOR
[InitializeOnLoad] // Call static class constructor in editor.
#endif
[InputControlLayout(stateType = typeof(CustomDeviceState))]
public class NimbusCustomGamepad : Gamepad, IInputUpdateCallbackReceiver
{
    // [InitializeOnLoad] will ensure this gets called on every domain (re)load
    // in the editor.
    #if UNITY_EDITOR
    static NimbusCustomGamepad()
    {
        // Trigger our RegisterLayout code in the editor.
        Initialize();
    }

    #endif

    // In the player, [RuntimeInitializeOnLoadMethod] will make sure our
    // initialization code gets called during startup.
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        // From the sample:
        // --
        // Register our device with the input system. We also register
        // a "device matcher" here. These are used when a device is discovered
        // by the input system. Each device is described by an InputDeviceDescription
        // and an InputDeviceMatcher can be used to match specific properties of such
        // a description. See the documentation of InputDeviceMatcher for more
        // details.
        // --
        //
        // For the Nimbus, the interface and product seemed to be enough, since the manufacturer showed
        // up as Unknown for me. You can get these from the Input Debugger window.
        InputSystem.RegisterLayout<NimbusCustomGamepad>(
            matches: new InputDeviceMatcher()
                .WithInterface("HID")
                .WithProduct("Nimbus+"));
    }

    // FinishSetup is where our device setup is finalized. Here we can look up
    // the controls that have been created.
    protected override void FinishSetup()
    {
        base.FinishSetup();
    }

    public void OnUpdate() {}
}

#endif //UNITY_STANDALONE_OSX || UNITY_EDITOR
