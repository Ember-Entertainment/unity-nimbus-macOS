# How To
If you are just looking to get the Nimbus up and running, you should be able to put the NimbusCustom.cs file anywhere in your project's Assets folder and it should work from there, barring any namespace collisions, etc. For more information on how we got here, read on!

# Adding Nimbus support for macOS in Unity
This file solves the issue of the Steelseries Nimbus+ gamepad not being supported by Unity's new input system for MacOS. It should also illustrate how to set up any gamepad that isn't supported so that it will work as expected. It's not exhaustive; this is our first look at these systems, but it should hopefully be enough to get you going if you're stuck on this.

The issue presents itself as the Nimbus+ not showing up as a gamepad (in Gamepad.current or Gamepad.all, etc). We get around that by creating a custom class that inherits from Gamepad and registers itself with Unity, telling Unity how to read it. This is done via a device state struct that inherits from IInputStateTypeInfo.

To determine offsets and bits used in the device state struct, we use Unity's Input Debugger window (Window->Analysis->Input Debugger). Note that, once the custom device file is in your project, it will change what you see in this window for that device. Depending on what is in the device state struct, it can even hide information, so it was useful while setting everything up to occasionally comment the file out to see what Unity's defaults for the device were.

Everything in the Input Debugger is useful for getting information, but a lot of what you initially see just reflects what you are defining in the device state struct. What we ended up using the most was the raw memory view. As you do things with the device (press buttons, move joysticks, etc), events show up in the Events section. You can double click on these events to get a snapshot of the information sent with the event. In the snapshot window, if you click Display Raw Memory, you can see the exact memory layout for that event. This can be used to determine the offset and bit values that you will need to plug in to the InputControl attributes.

You may need to play around with formats to get correct results. One issue we ran across was that Unity's Custom Device sample used an unsigned byte (BYTE) for the joystick axes. Using this for the Nimbus gave weird results where the axis would count from 0 to -1 on one side and then switch to 1 in the middle, then count back down to 0 on the other side. To fix this, we needed to switch the signed byte (SBYT) format.

# Thanks
Our engineer https://github.com/sunkicked did the research and implementation to make this fix happen!
