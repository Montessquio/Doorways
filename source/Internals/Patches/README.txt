!!!!! BEWARE !!!!!

This file contains LOTS of glue logic, and all of it
is very sensitive. If you've forked this, then edit
at your own risk. If you're working on the main repo,
then I WILL git blame you if things break.

----- README -----

If you want to affect the game from your own Mod
that uses Doorways as a dependency, then this isn't
what you want to be looking at. Instead, please refer
to `Doorways.UIExtensions` or `Doorways.CoreExtensions`
for ways to manipulate the game UI and game Engine,
respectively.

The one exception is if you want to run a function
from your mod when a certain event happens in Core.
This can be done by subscribing to one of the events
in `Doorways.Patches.SecretHistories.CoreEvents`.

----- Info -----

The `sh.monty.doorways.HarmonyPatchLayers` namespace
contains all the harmony patches performed by this
mod. No Harmony code of any kind is  permitted outside 
this namespace except for the call to 
`harmony.PatchAll(assembly);` performed in `doorways.cs`.

The idiomatic way of defining patch classes is to
set all patch functions as `private` and restrict access
to them with some public API functions which check
invariants and perform runtime assertions.

Please, *please* use `Logger.Span` to log your messages.
Your future self will thank you, as will future me.
