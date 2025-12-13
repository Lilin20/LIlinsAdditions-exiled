using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using GockelsAIO_exiled.Handlers;
using HarmonyLib;
using MEC;
using System;
using GockelsAIO_exiled.Items;

namespace GockelsAIO_exiled
{
    public class LilinsAdditions : Plugin<Config>
    {
        // Plugin Metadata
        public override string Name => "Lilin's Additions";
        public override string Author => "Lilin";
        public override Version Version => new Version(0, 10);
        
        // Singleton Instance
        public static LilinsAdditions Instance { get; private set; }
        
        // Handlers
        public PlayerHandler PlayerHandler { get; private set; }
        public ServerHandler ServerHandler { get; private set; }
        public PMERHandler PMERHandler { get; private set; }
        
        // Harmony and Coroutines
        private Harmony _harmony;
        private CoroutineHandle _pointSystemCheckCoroutine;

        public override void OnEnabled()
        {
            Instance = this;
            
            InitializeComponents();
            LoadResources();
            RegisterEventHandlers();
            StartBackgroundTasks();
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            StopBackgroundTasks();
            UnregisterEventHandlers();
            CleanupComponents();

            Instance = null;

            base.OnDisabled();
        }

        #region Initialization

        private void InitializeComponents()
        {
            PlayerHandler = new PlayerHandler();
            ServerHandler = new ServerHandler();
            PMERHandler =  new PMERHandler();
            _harmony = new Harmony("lilin.patches");
        }

        private void LoadResources()
        {
            LoadAudioClips();
            WeaponSelector.WeightedCustomWeaponsWithConfig();
            CustomWeapon.RegisterItems();
        }
        
        private static void LoadAudioClips()
        {
            AudioClipStorage.LoadClip(Instance.Config.MysteryBoxMusicPath, "mysterybox");
            AudioClipStorage.LoadClip(Instance.Config.VendingMachineMusicPath, "gobblegum");
            AudioClipStorage.LoadClip(Instance.Config.BurstSoundPath, "bombsound");
            AudioClipStorage.LoadClip(Instance.Config.TrackingSoundPath, "trackingsound");
        }

        private void StartBackgroundTasks()
        {
            _pointSystemCheckCoroutine = Timing.RunCoroutine(ServerHandler.PeriodicPointSystemCheck());
            _harmony.PatchAll();
        }
        
        #endregion
        
        #region Cleanup

        private void StopBackgroundTasks()
        {
            Timing.KillCoroutines(_pointSystemCheckCoroutine);
            _harmony.UnpatchAll();
        }
        
        private void CleanupComponents()
        {
            CustomWeapon.UnregisterItems();
            CustomAbility.UnregisterAbilities();

            PlayerHandler = null;
            ServerHandler = null;
            PMERHandler = null;
        }
        
        #endregion
        
        #region Event Registration

        private void RegisterEventHandlers()
        {
            RegisterMERHandlers();
            RegisterServerHandlers();
            RegisterPlayerHandlers();
        }

        private void UnregisterEventHandlers()
        {
            UnregisterMERHandlers();
            UnregisterServerHandlers();
            UnregisterPlayerHandlers();
        }
        
        private void RegisterMERHandlers()
        {
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += PMERHandler.OnButtonInteract;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += PMERHandler.OnButtonInteractGobblegum;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += PMERHandler.OnButtonInteractCoin;
        }

        private void UnregisterMERHandlers()
        {
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= PMERHandler.OnButtonInteract;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= PMERHandler.OnButtonInteractGobblegum;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= PMERHandler.OnButtonInteractCoin;
        }

        private void RegisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted += ServerHandler.OnStart;
            Exiled.Events.Handlers.Server.RoundStarted += ServerHandler.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded += ServerHandler.OnRoundEnd;
            Exiled.Events.Handlers.Map.Generated += ServerHandler.OnMapGeneration;
        }

        private void UnregisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandler.OnStart;
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandler.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerHandler.OnRoundEnd;
            Exiled.Events.Handlers.Map.Generated -= ServerHandler.OnMapGeneration;
        }

        private void RegisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned += PlayerHandler.SpawnSetPoints;
            Exiled.Events.Handlers.Player.Left += PlayerHandler.OnLeft;
            Exiled.Events.Handlers.Player.Dying += PlayerHandler.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting += PlayerHandler.OnSCPVoidJump;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer += PlayerHandler.OnPlayerIn914;
            Exiled.Events.Handlers.Player.Dying += PlayerHandler.DropCreditOnDeath;
            Exiled.Events.Handlers.Player.PickingUpItem += PlayerHandler.OnPickingUpCreditCard;
        }

        private void UnregisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned -= PlayerHandler.SpawnSetPoints;
            Exiled.Events.Handlers.Player.Left -= PlayerHandler.OnLeft;
            Exiled.Events.Handlers.Player.Dying -= PlayerHandler.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting -= PlayerHandler.OnSCPVoidJump;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer -= PlayerHandler.OnPlayerIn914;
            Exiled.Events.Handlers.Player.Dying -= PlayerHandler.DropCreditOnDeath;
            Exiled.Events.Handlers.Player.PickingUpItem -= PlayerHandler.OnPickingUpCreditCard;
        }
        
        #endregion
    }
}
