# Held Human

## Mod Overview

Held Human is a RimWorld mod that allows human pawns to be captured using the holding platform from the Anomaly DLC.

Anomaly's holding platform is originally designed to work with specific entities. This mod extends the system so that human pawns can also be processed by holding platform-related mechanics by modifying vanilla job flows and various pawn properties through Harmony patches.

Rather than replacing the existing system with a separate custom system, this mod is implemented by preserving the vanilla/DLC behavior flow as much as possible and extending only the required parts.

## Supported Versions and Requirements

### Supported Versions

* RimWorld 1.5 (support discontinued)
* RimWorld 1.6

### Required Mods and DLC

* [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
* [RimWorld - Anomaly](https://store.steampowered.com/app/2380740/RimWorld__Anomaly)

## Installation

Subscribe on Steam Workshop, or place the files from this repository into RimWorld's `Mods` folder.

## Compatibility and Notes

This mod applies multiple Harmony patches to vanilla code so that captured human pawns can also use position, status, holding platform-related handling, and other relevant pawn behavior. Because of this, performance analysis tools may show this mod's patches as taking up a high percentage. However, this is caused by extending existing vanilla logic, and no notable negative impact on actual gameplay performance has been found so far. If no specific issue occurs, the mod should be safe to use as-is.

Jobs targeting captured human pawns are implemented as separate jobs. As a result, pawn-targeting jobs added by other mods may not be directly compatible with captured humans. For example, jobs related to water supply, hygiene, bathing, thirst handling, and similar features from [Dubs Bad Hygiene](https://steamcommunity.com/sharedfiles/filedetails/?id=836308268) do not apply to captured humans.

## For Developers

Held Human provides an extension API that allows other mods to control the escape interval, bioferrite production amount, and research points of captured humans.

### Implementing a Hooker

By inheriting from the `Hooker` class in the `HeldHuman.Hook` namespace, a class can be hooked automatically without any additional class registration.

For example, to adjust bioferrite production, implement a class that inherits from `BioferriteDensityHooker`.

```csharp
public abstract class BioferriteDensityHooker : Hooker
{
    public abstract void Modify(Pawn pawn, ref float value);

    public virtual void AddStatDraw(Pawn pawn, ref StringBuilder stringBuilder) { }
}
```

The `Modify` function is used to adjust the actual value.

The `AddStatDraw` function is used to add extra text to the in-game stat description UI. If no additional description is needed, it does not need to be implemented.

### StatFactor-Based Control

Some values can be controlled through XML `statFactors` using the built-in `Hooker`.

For example, the escape interval can be adjusted as follows.

```xml
<statFactors>
    <EscapeIntervalFactor>10</EscapeIntervalFactor>
</statFactors>
```

This allows values to be adjusted using only XML Defs without writing separate C# code.

## License

This project is licensed under the [MIT License](LICENSE).

The source code of this mod may be freely used, modified, and distributed, provided that the original copyright notice and license text are included when distributing it.

See the [LICENSE](LICENSE) file for details.

## Links

* [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3231873043)
* [GitHub](https://github.com/WooLyung/HeldHuman)