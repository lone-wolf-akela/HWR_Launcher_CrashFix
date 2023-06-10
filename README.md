# HWR_Launcher_CrashFix

Start from around 2023 June 8th afternoon, the game launcher of *Homeworld Remastered Collection* steam version started to crash.

The crash happens as it tries to connect to https://account.services.gearboxsoftware.com/verify/hickory/pc/steam/int/ for multiplayer account authorization, but gets `404 Not Found` as response. The launcher does not handle this error correctly, and crashes when trying to parse this errored response.

This repo contains code decompiled from Gearbox's launcher, with this bug fixed.

To be specifically, the only change I've made is to change line 254 @ `Spark\CSparkInterface.cs` to set the request status to `Failed` instead of `Done`.

## How do I use this to fix my launcher?

There is a `Release` link at the right side of the github page. Go there, download the `Launcher-LoneWolfFix.zip` file. Unzip it and copy the files to your `Steam\steamapps\common\Homeworld\HWLauncher` folder, override the files there.

**Note**: this patch makes the launcher not crash when it cannot get correct response from Gearbox's account authorization server. However, unless Gearbox fixes their server at their side, this patch won't enable you to play multiplayer game through Gearbox's server. LAN multiplayer is not affected by this, though.
