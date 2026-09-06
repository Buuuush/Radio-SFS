// ---------- DESCRIPTION --------- \\
/*                                  *\

1. Load the mod
2. Load the saved radio URL
3. Create the radio player
4. Close the radio properly when the game closes

Help :
https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/classes
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/

Useful links :
https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/classes
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/

*\                                   */
// -------- END DESCRIPTION -------- \\

using System;
using System.Collections.Generic;
using HarmonyLib;
using ModLoader;

namespace RadioSFS
{
    public class Main : Mod
{
    public static Main Instance { get; private set; }
    public Main()
    {
        Instance = this;
    }

    public override string ModNameID => "radio.sfs";
    public override string DisplayName => "Radio-SFS";
    public override string Author => "Gogogadgetozebra";
    public override string MinimumGameVersionNecessary => "1.6.00.16";
    public override string ModVersion => "1.0.0";
    public override string Description => "A mod that lets you listen to the radio ingame";
    public override string IconLink => null;
    public override Action LoadKeybindings => null;

    public override Dictionary<string, string> Dependencies => new Dictionary<string, string>();

    public override void Early_Load()
    {
        new Harmony(Instance.ModNameID).PatchAll();
    }

    public override void Load()
    {

    }

}
}
