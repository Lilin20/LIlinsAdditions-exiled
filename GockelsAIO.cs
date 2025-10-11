using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using GockelsAIO_exiled.Handlers;
using GockelsAIO_exiled.Helper;
using HarmonyLib;

namespace GockelsAIO_exiled
{
    public class GockelsAIO : Plugin<Config>
    {
        public override string Name => "Gockels AIO";
        public override string Author => "Gockel";
        public static GockelsAIO Instance;
        public PlayerHandler PlayerHandler;
        public ServerHandler ServerHandler;
        public PMERHandler PMERHandler;
        public CustomRoleHandler CustomRoleHandler;
        public Harmony harmony = new Harmony("gockel.patch");

        public override void OnEnabled()
        {
            PlayerHandler = new PlayerHandler();
            ServerHandler = new ServerHandler();
            PMERHandler = new PMERHandler();
            CustomRoleHandler = new CustomRoleHandler();

            Instance = this;
            
            RegisterMERHandlers();
            RegisterPlayerHandlers();
            RegisterServerHandlers();
            RegisterCustomRoles();

            CustomWeapon.RegisterItems();

            LoadAudioClips();

            harmony.PatchAll();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            UnregisterMERHandlers();
            UnregisterPlayerHandlers();
            UnregisterServerHandlers();
            UnregisterCustomRoles();

            CustomWeapon.UnregisterItems();

            PlayerHandler = null;
            ServerHandler = null;
            PMERHandler = null;
            CustomRoleHandler = null;

            Instance = null;

            harmony.UnpatchAll();

            base.OnDisabled();
        }

        public void LoadAudioClips()
        {
            //C:\AMPDatastore\Instances\SCPSecretLaboratory01\scp-secret-laboratory\996560\AppData\EXILED\Audio\
            bool mysteryLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPSecretLaboratory01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\mysterybox.ogg", "mysterybox");
            bool gobblegumLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPSecretLaboratory01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\gobblegum.ogg", "gobblegum");
            bool bombsoundLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPSecretLaboratory01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\bombsound.ogg", "bombsound");
            bool trackingLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPSecretLaboratory01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\trackingsound.ogg", "trackingsound");
        }

        public void RegisterCustomRoles()
        {
            CustomRoleHandler.RegisterRoles();
        }

        public void UnregisterCustomRoles()
        {
            CustomRoleHandler.UnregisterRoles();
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
        }

        public void UnregisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandler.OnStart;
            Exiled.Events.Handlers.Server.RoundStarted -= ServerHandler.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerHandler.OnRoundEnd;
        }

        public void RegisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned += PlayerHandler.SpawnSetPoints; // Handler to give players points on spawn
            Exiled.Events.Handlers.Player.Left += PlayerHandler.OnLeft; // Handler to remove the player from the points dict
            Exiled.Events.Handlers.Player.ChangingRole += PlayerHandler.OnChangingRolePoints;
            Exiled.Events.Handlers.Player.ChangingItem += PlayerHandler.OnChangingItem;
            Exiled.Events.Handlers.Player.Dying += PlayerHandler.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting += PlayerHandler.OnSCPVoidJump;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer += PlayerHandler.OnPlayerIn914;
            Exiled.Events.Handlers.Player.IntercomSpeaking += PlayerHandler.OnUsingIntercomWithCard;
            Exiled.Events.Handlers.Scp914.UpgradingPickup += PlayerHandler.OnCraftingTrackingAccess;
        }

        public void UnregisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned -= PlayerHandler.SpawnSetPoints;
            Exiled.Events.Handlers.Player.Left -= PlayerHandler.OnLeft;
            Exiled.Events.Handlers.Player.ChangingRole -= PlayerHandler.OnChangingRolePoints;
            Exiled.Events.Handlers.Player.ChangingItem -= PlayerHandler.OnChangingItem;
            Exiled.Events.Handlers.Player.Dying -= PlayerHandler.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting -= PlayerHandler.OnSCPVoidJump;
            Exiled.Events.Handlers.Scp914.UpgradingPlayer -= PlayerHandler.OnPlayerIn914;
            Exiled.Events.Handlers.Player.IntercomSpeaking -= PlayerHandler.OnUsingIntercomWithCard;
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= PlayerHandler.OnCraftingTrackingAccess;
        }
    }
}
