# DungeonGenerator
This code is a variation of the code developed by Rootbin Studio (https://www.youtube.com/@rootbindev) in the video "Unity Tutorial: ROGUELIKE Room / Dungeon Generation (Like the Binding of Isaac)" (https://www.youtube.com/watch?v=eK2SlZxNjiU)

## What's the difference?
The difference in this code is essentially the change in the way of controlling where rooms are placed. I preferred to use a dictionary approach, where the primary key is a vector2Int.

When the key exists, it is obviously not possible to place a room at that point, which in turn returns the "TryGenerateRoom" code as false (the return value is never used in this code, but is prepared for it).
    
## Development
Right now, Im usinfg *Unity 2023.3.b1 (beta)*, but I don't see a problem with it working perfectly in any version from 2022 onwards.

The objective of this code was to get rid of overlapping rooms, which I seem to have achieved successfully. I carried out several tests and was no longer able to make this "error".

A side effect (although I left it in the code) is that the maximum number of rooms (maxRoom) is always reached, which made the minRoom variable obsolete, apparently, in this code.

I'm sure that optimizing it is possible and desired, but I will do that over time. For now he is useful to me like that.

## Credits

Original Code: Rootbin Studio (https://www.youtube.com/@rootbindev)

Dictionary Variant: Bruno S Drago (https://github.com/bsdrago/)
