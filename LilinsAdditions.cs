using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Handlers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GockelsAIO_exiled
{
    public class LilinsAdditions : Plugin<Config>
    {
        public override string Name => "Lilin's Additions";
        public override string Author => "Lilin";
        public override Version Version => new Version(0, 10);
        public static LilinsAdditions Instance;
        public PlayerHandler PlayerHandler;
        public ServerHandler ServerHandler;
        public PMERHandler PMERHandler;
        public Harmony harmony = new Harmony("lilin.patches");

        public override void OnEnabled()
        {
            Instance = this;
            LoadAudioClips();

            PlayerHandler = new PlayerHandler();
            ServerHandler = new ServerHandler();
            PMERHandler = new PMERHandler();

            RegisterMERHandlers();
            RegisterPlayerHandlers();
            RegisterServerHandlers();
            WeaponSelector.WeightedCustomWeaponsWithConfig();

            CustomWeapon.RegisterItems();


            harmony.PatchAll();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            UnregisterMERHandlers();
            UnregisterPlayerHandlers();
            UnregisterServerHandlers();

            CustomWeapon.UnregisterItems();
            CustomAbility.UnregisterAbilities();

            PlayerHandler = null;
            ServerHandler = null;
            PMERHandler = null;

            Instance = null;

            harmony.UnpatchAll();        

            base.OnDisabled();
        }

        public void LoadAudioClips()
        {
            bool mysteryLoaded = AudioClipStorage.LoadClip(Instance.Config.MysteryBoxMusicPath, "mysterybox");
            bool gobblegumLoaded = AudioClipStorage.LoadClip(Instance.Config.VendingMachineMusicPath, "gobblegum");
            bool bombsoundLoaded = AudioClipStorage.LoadClip(Instance.Config.BurstSoundPath, "bombsound");
            bool trackingLoaded = AudioClipStorage.LoadClip(Instance.Config.TrackingSoundPath, "trackingsound");
        }

        public void RegisterMERHandlers()
        {
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += PMERHandler.OnButtonInteract; // Mystery Box Button Handler
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += PMERHandler.OnButtonInteractGobblegum;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += PMERHandler.OnButtonInteractCoin;
        }

        public void UnregisterMERHandlers()
        {
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= PMERHandler.OnButtonInteract; // Mystery Box Button Handler
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= PMERHandler.OnButtonInteractGobblegum;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= PMERHandler.OnButtonInteractCoin;
        }

        public void RegisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted += ServerHandler.OnStart; // Used for spawning Mystery Boxes.
            Exiled.Events.Handlers.Server.RoundStarted += ServerHandler.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded += ServerHandler.OnRoundEnd;
            Exiled.Events.Handlers.Map.Generated += ServerHandler.OnMapGeneration;
        }

        public void UnregisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandler.OnStart;
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandler.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerHandler.OnRoundEnd;
            Exiled.Events.Handlers.Map.Generated -= ServerHandler.OnMapGeneration;
        }

        public void RegisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned += PlayerHandler.SpawnSetPoints; // Handler to give players points on spawn
            Exiled.Events.Handlers.Player.Left += PlayerHandler.OnLeft; // Handler to remove the player from the points dict
            Exiled.Events.Handlers.Player.ChangingRole += PlayerHandler.OnChangingRolePoints;
            Exiled.Events.Handlers.Player.Dying += PlayerHandler.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting += PlayerHandler.OnSCPVoidJump;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer += PlayerHandler.OnPlayerIn914;
            Exiled.Events.Handlers.Player.Dying += PlayerHandler.DropCreditOnDeath;
            Exiled.Events.Handlers.Player.PickingUpItem += PlayerHandler.OnPickingUpCreditCard;
            //Exiled.Events.Handlers.Player.IntercomSpeaking += PlayerHandler.OnUsingIntercomWithCard;
            //Exiled.Events.Handlers.Scp914.UpgradingPickup += PlayerHandler.OnCraftingTrackingAccess;
        }

        public void UnregisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned -= PlayerHandler.SpawnSetPoints;
            Exiled.Events.Handlers.Player.Left -= PlayerHandler.OnLeft;
            Exiled.Events.Handlers.Player.ChangingRole -= PlayerHandler.OnChangingRolePoints;
            Exiled.Events.Handlers.Player.Dying -= PlayerHandler.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting -= PlayerHandler.OnSCPVoidJump;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer -= PlayerHandler.OnPlayerIn914;
            Exiled.Events.Handlers.Player.Dying -= PlayerHandler.DropCreditOnDeath;
            Exiled.Events.Handlers.Player.PickingUpItem -= PlayerHandler.OnPickingUpCreditCard;
            //Exiled.Events.Handlers.Player.IntercomSpeaking -= PlayerHandler.OnUsingIntercomWithCard;
            //Exiled.Events.Handlers.Scp914.UpgradingPickup -= PlayerHandler.OnCraftingTrackingAccess;
        }
    }
}
