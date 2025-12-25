# 2D Shape Editor for Unity.

The 2D Shape Editor is a versatile tool to create complex 3D meshes out of 2D shapes.

With this program you can easily create a design cross-section in a two-dimensional view and then expand it to a 3D mesh.

![Default 2D Shape Editor Window](https://github.com/Henry00IS/ShapeEditor/wiki/images/home/multiple-windows.png)

This tool has been integrated directly into the Unity editor to ensure a seamless and fast workflow. When you need a special design, such as curved pipes, an underground channel or a baroque arch, this is the tool for the job.

The controls and user interface were inspired by the popular modeling program Blender. Many of the shortcuts and tricks also work in this program. Special tools have been provided to allow a non-destructive workflow with generators that can be easily modified.

YouTube [2D Shape Editor for Unity - Getting Started](https://www.youtube.com/watch?v=yq0keiF3o8o)

## Constructive Solid Geometry:

The shape editor can generate boolean brushes for constructive solid geometry. These brushes let you add and subtract arbitrary shapes to quickly create walls and rooms or cut out doors and windows.

It automatically detects and supports [RealtimeCSG](https://github.com/LogicalError/realtime-CSG-for-unity) and [Chisel Editor](https://github.com/RadicalCSG/Chisel.Prototype) by using convex decomposition to create multiple convex brushes that accurately represent the concave shape.

## TrenchBroom Level Editor:

The shape editor can copy 3D extrusions into the clipboard to be pasted into TrenchBroom. This allows for accurate curved staircases and other shapes that would otherwise be a chore to create.

![Illustrating a curved staircase from Unity into TrenchBroom](https://github.com/Henry00IS/ShapeEditor/wiki/images/home/trenchbroom.png)

## Installation Instructions:

Add the following line to your Unity Package Manager:

![Unity Package Manager](https://user-images.githubusercontent.com/7905726/84954483-c82ba100-b0f5-11ea-9cd0-1cdc24ef2660.png)

`https://github.com/Henry00IS/ShapeEditor.git`

## Support:

Feel free to [join my Discord server](https://discord.gg/sKEvrBwHtq) and let's talk about it.

[![Join my Discord server](https://dcbadge.limes.pink/api/server/sKEvrBwHtq)](https://discord.gg/sKEvrBwHtq)

If you found this package useful, please consider making a donation or supporting me on Patreon. Your donations are a tremendous encouragement for the continued development and support of this package. 😁

[![Support me on Patreon](https://raw.githubusercontent.com/wiki/Henry00IS/DynamicLighting/images/badges/patreon.svg)](https://patreon.com/henrydejongh) [![Support me on Ko-fi](https://raw.githubusercontent.com/wiki/Henry00IS/DynamicLighting/images/badges/kofi.svg)](https://ko-fi.com/henry00) [![paypal](https://raw.githubusercontent.com/wiki/Henry00IS/DynamicLighting/images/badges/paypal.svg)](https://paypal.me/henrydejongh)
