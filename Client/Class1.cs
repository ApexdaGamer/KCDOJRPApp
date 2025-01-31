using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.FiveM;
using System.Reflection;
using static CitizenFX.FiveM.Native.Natives;
using CitizenFX.FiveM.GUI;

namespace KCDOJRPApp.Client
{
    public class Class1 : BaseScript
    {
        public Class1()
        {
            Debug.WriteLine("hi, this is the client thing"); // does it work? Yes, it does. sweet
            // lets register that one event handler for that one command or two
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);

            DisableControlAction(0, 243, false); // STOP BEING ABLE TO TYPE IN CHEATS!!! I DONT LIKE THAT!!!
            Tick += OnTick; // instead do this so you can see people's names
        }

        async Coroutine OnTick() // ticker go ticking
        {
            if (IsDisabledControlPressed(0, 243)) // NO WAY IS ` PRESSED
            {
                foreach (Player x in PlayerList.Players) // go through the players.
                {
                    int id = x.Handle;
                    if (id != -1)
                    {
                        Vector3 pos = x.Character.Position; // i see you

                        if (pos.DistanceToSquared(Player.Local.Character.Position) <= 50) // infinite range? not on my watch
                        {
                            float sx = 0f, sy = 0f;
                            var onScreen = World3dToScreen2d(pos.X, pos.Y, pos.Z, ref sx, ref sy); // yea i hate conversion its annoying

                            var spos = new Vector2(sx * Screen.Width, sy * Screen.Height);

                            var text = new Text($"[{GetPlayerServerId(id)}] {GetPlayerName(id)}", spos, 0.215f); // heres some text
                            text.Enabled = onScreen;
                            text.Draw(); // remember that text? it's on the screen now. because i said so.
                        }
                    }
                }
            }
        }

        // helper functions
        private void SendError(string msg) // mario says you stink cuz u made it error
        {
            Events.TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "[ERROR]", msg }
            });
        }

        private void SendSuccess(string msg) // mario approves
        {
            Events.TriggerEvent("chat:addMessage", new
            {
                color = new[] { 0, 255, 0 },
                args = new[] { "[SUCCESS]", msg }
            });
        }

        // event handler needs a function from somewhere
        private void OnClientResourceStart(string resource)
        {
            if (resource != GetCurrentResourceName()) return; // make sure its only for this script. that'd be a hell of a lot of commands if not

            RegisterCommand("spawnvehicle", new Action<int, object[], string>(async (source, args, raw) =>
            {
                Debug.WriteLine("yea guys lets spawn a car. i mean, it was " + source + "'s idea after all.");

                var model = (args.Length > 0 ? args[0].ToString() : "adder"); // short and sweet check for args.

                var hash = GetHashKey(model); // check if it exists...... or something
                if (!IsModelInCdimage(hash) || !IsModelAVehicle(hash))
                {
                    SendError($"Vehicle model {model} doesn't exist!"); // yea stinky. you should know better.
                    return;
                }

                var curVehicle = Game.PlayerPed.CurrentVehicle;

                if (Game.PlayerPed.IsInAir || (curVehicle != null && curVehicle.IsInAir)) { SendError("You cannot summon a vehicle while in air!"); return; }
                else if (Game.PlayerPed.IsInWater || (curVehicle != null && curVehicle.IsInWater)) { SendError("You cannot summon a vehicle while in water!"); return; } // silly checks for silly people

                if (curVehicle != null) curVehicle.Delete();

                var vehicle = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading); // yum, vehicles

                if (vehicle.PlaceOnGround()) SendError("Vehicle may not be fully on ground!"); // no way im so dumb ....

                Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver); // drive into a lamp post or something solid to draw attention and to damage the car

                SendSuccess($"Successfully summoned a {model}."); // yippie
            }), false); // what does restricted even mean

            // time to add another command that does weapon shite
            RegisterCommand("setweapon", new Action<int, object[], string>(async (source, args, raw) =>
            {
                Debug.WriteLine($"WATCH OUT!!! {source} IS SCREWING WITH /setweapon !!!!");

                var model = (args.Length > 0 ? args[0].ToString() : "WEAPON_PISTOL"); // short and sweet check for args. gotta know if we have the right..,,,, HE HAS A GUN!!!!

                var hash = GetHashKey(model);
                if (!IsWeaponValid(hash)) // does it exist and is it valid
                {
                    SendError($"Weapon model {model} doesn't exist!"); // stinky person
                    return;
                }

                var ammo = args.Length > 1 ? int.Parse(args[1].ToString()) : 25; // ammo check !

                var chr = Game.PlayerPed;

                if (chr.Weapons.HasWeapon((WeaponHash)hash))
                {
                    chr.Weapons.Remove(chr.Weapons[(WeaponHash)hash]);
                    SendSuccess($"Successfully removed a {model}.");
                }
                else
                {
                    var wep = chr.Weapons.Give((WeaponHash)hash, ammo, true, true); // yippie give em all guns
                    Debug.WriteLine($"cur: {chr.Weapons.Current.DisplayName}, spawned: {wep.DisplayName}, can select?: {chr.Weapons.Select(wep)}"); // make sure its working right
                    SendSuccess($"Successfully summoned a {model}.");
                }
            }), false); // i still don't know what restricted does
        }
    }
}