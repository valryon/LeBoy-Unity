# LeBoy - Unity frontend

A quick and experimental Unity frontend for [mrhelmut's LeBoy (GB emulator)](https://github.com/mrhelmut/LeBoy).

## Versions

- Unity 5.5.0f3

## Usage

**This is a recreational project and it shouldn't be used in production or in a commercial setup.**

1. Place a ROM in StreamingAssets
2. Update the LeBoyScript.rom field and fill it with the ROM you want to load (set the full file name, like "Mario.gb")
3. Launch
4. Enjoy!

## Know issues

- Clock is too fast in builds.
- Insupported .NET 4.5 features like "AggressiveInlining" compiler attribute
- And everything related to LeBoy, obviously, like sound
