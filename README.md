# HoloZens
App for creating 3D [Specular Holograms](https://en.wikipedia.org/wiki/Specular_holography) 
on 2D surfaces.  Also known as abrasion holograms or [Chatoyant](https://en.wikipedia.org/wiki/Chatoyancy) 
holograms.  

[![Aza HoloZens youtube video](https://i.ytimg.com/vi/1uku3OEgDNg/hqdefault.jpg?sqp=-oaymwEcCNACELwBSFXyq4qpAw4IARUAAIhCGAFwAcABBg==&rs=AOn4CLDmtLtot2FTjpBS1rR6wLmUO8Q8xg)](https://www.youtube.com/watch?v=1uku3OEgDNg)

Brief overview video @ https://www.youtube.com/watch?v=1uku3OEgDNg.

This started as a fork of Mike Miller's https://github.com/mymikemiller/scratchhologram (see [changelist history](https://github.com/aaronse/hz-scratch-hologram/commits/master/)
for functional/perf edits I've made since).

Use this app to convert **Basic** 3D models (with very few faces/triangles) from .STL to 
.SVG image files that can be used in your CNC/Laser CAM software (e.g. EstlCam, Fusion 360, LightBurn, etc...) 
to generate .GCODE toolpaths executable by your machine.

Am using this app to lightly scratch and etch glossy acrylic, or metal surfaces using diamond drag bit held by a custom 
mount that's flexible in vertical Z axis, but rigid in XY...

https://www.printables.com/model/419594-v1e-lowrider-3-pen-drag-knife-drag-bit-rdz-engrave
[![Aza drag bit mount](https://media.printables.com/media/prints/419594/images/5418202_a390826c-3e65-4365-9d4a-ce38ffb65e9c_1b002f5e-2543-437f-aff7-a492db779c65/thumbs/inside/1280x960/png/holozens-thumbnail-5.webp)](https://www.printables.com/model/419594-v1e-lowrider-3-pen-drag-knife-drag-bit-rdz-engrave)

## History, Related work and Acknowledgments
Myself and other V1E forum members stumbled (~2023/2) onto [Steve Mould's Specular Hologram video](https://www.youtube.com/watch?v=sv-38lwV6vc),
we collectively had a [speculative discussion](https://forum.v1e.com/t/drag-knife-and-laser-etched-specular-holograms/36888) 
on how to get our CNC/Laser machines to achieve great results, that poor Steve was painstakingly doing by hand.

Some info and resources I stumbled across when researching into background, options and alternatives...

* Steve Mould’s [discord channel ](https://discord.com/channels/953683272754413668/1049781679377621014/1077586765386362950) has misc posts on Specular Holograms.
* Mike Miller's [scratchhologram C# code ](https://github.com/mymikemiller/scratchhologram) that 
* 1981 Article on Holographic Engraving [http://www.garfield.library.upenn.edu/essays/v5p348y1981-82.pdf ](http://www.garfield.library.upenn.edu/essays/v5p348y1981-82.pdf)
* 1995 Drawing Holograms by Hand (using compass), W. Beaty [SCIENCE HOBBYIST: Drawing holograms by hand ](http://amasci.com/amateur/hand1.html) and [Holography without Lasers: Hand-drawn Holograms [SCIENCE HOBBYIST] ](http://www.amasci.com/amateur/holo1.html)
* 2009-1-13 Abrasion Hologram Printer, Mike Miller, Daniel Wrachford, Spencer Kennedy, [Abrasion Hologram Printer - YouTube ](https://www.youtube.com/watch?v=JaGZ651U4j4)
* 2010-11-30 Specular Holography, Matthew Brand [https://arxiv.org/pdf/1101.0301.pdf ](https://arxiv.org/pdf/1101.0301.pdf)
* 2012/7/22 [Drawing lightfields: handdrawn approaches to abrasion holography, Tristan Duke, MIT Talk ](https://www.youtube.com/watch?v=NSxJ4uFWb5g)
* 2011 - 2014 Object and Scene datasets at [https://rgbd-dataset.cs.washington.edu/ ](https://rgbd-dataset.cs.washington.edu/)
* 2017-2018 Mike Miller shared scratch-hologram app, [C# Code on GitHub ](https://github.com/mymikemiller/scratchhologram)
    * Can load .x3d files, simulate and generate arcs on the view plane…  (HoloZens was forked from this).
    ![](https://us2.dh-cdn.net/uploads/db5587/original/3X/c/c/cce23ab90d071aa180e4fe093ecd3c97cba644f9.jpeg)
    * Example of the app running @ https://www.youtube.com/watch?v=JaGZ651U4j4
* 2020 CODE @ [GitHub - shchuko/ScratchedHologramFrom3D: Scratched hologram creation tool ](https://github.com/shchuko/ScratchedHologramFrom3D)
* 2020 CNC + Servo for Arcs [https://www.instructables.com/The-Scribe-bot-a-Machine-to-Create-Scratch-Hologra/ ](https://www.instructables.com/The-Scribe-bot-a-Machine-to-Create-Scratch-Hologra/)
* YouTube Playlist Hologram scratch abrasion videos @ [Holograms scratch abrasion - YouTube ](https://www.youtube.com/playlist?list=PL60100E8F3572CEB1)
  * Checkout hologram on Star Wars vinyl records.
* Very different from Red Laser Holograms [LitiHolo 3D Hologram Printer Video Intro - YouTube ](https://www.youtube.com/watch?v=tbLrAwaqdN8)
[/quote]


## Using HoloZens
Currently (2024/1/3) you need to clone/download this code, compile in Visual Studio and run on Windows.

Watch the YouTube video for quick overview, ping me if you want more detailed video/instructions showing 
how to use.  I'll try to put something together if there's enough interest.


## More Info
Consider checking out [https://forum.v1e.com/t/drag-knife-and-laser-etched-specular-holograms/36888](https://forum.v1e.com/t/drag-knife-and-laser-etched-specular-holograms/36888),
shared more info/context there, myself and others interested in this topic might be able to respond 
to questions there.


## Export GCODE 
NOPE.  Not happening, at least for now...  Initially, was planning to add Export GCODE feature, even wrote the code...  
But I don't want bugs/misuse to cause harm to User's machine, stock or $$$ bits, especially given cost 
of diamond drag bits and nice materials.

That said, here's some related info and inspiration for the bold...

[G2-G3 Arc/Circle Move, Marlin docs](https://marlinfw.org/docs/gcode/G002-G003.html)

// TODO:... int plungeRate = 3; plungeRate, Angle, Elipsis for distortion correction...
//int rapidXY = 4000;
//int zLift = 4;
//float toolDepth = 0.25f;

```c#
StringBuilder sbGcode = gcode.sbGcode;
sbGcode.AppendLine($"G0 X{x1} Y{y1} F{rapidXY}");   // Rapid to arc start
sbGcode.AppendLine($"G0 Z{toolDepth}");             // Z Plunge
sbGcode.AppendLine($"G2 X{x2} Y{y2} R{r}");         // Arc Move, Clockwise
sbGcode.AppendLine($"G0 Z{zLift}");                 // Z Lift
```


