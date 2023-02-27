Assists in creating 3d scratch holograms (aka abrasion holograms or chatoyant holograms). They can 
be created on acrylic or metal sheets using only an adjustable compass as described on Bill 
Beaty's website: http://www.amasci.com/holo/

An example of the running application can be seen at: https://www.youtube.com/watch?v=JaGZ651U4j4


## Toolpath settings

Drag Knife guidance https://docs.v1e.com/tools/drag-knife/


## Export GCODE 
NOPE.  Not happening, at least for now...  Initially, was planning to add Export GCODE feature, but I don't want bugs/misuse to cause harm to User's machine, stock or $$$ bits, especially given cost of diamond drag bits.

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

## Export SVG



