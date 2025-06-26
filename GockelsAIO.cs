using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using GockelsAIO_exiled.Handlers;

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

            base.OnDisabled();
        }

        public void LoadAudioClips()
        {
            bool mysteryLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPDevelopmentEnvironment01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\mysterybox.ogg", "mysterybox");
            bool gobblegumLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPDevelopmentEnvironment01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\gobblegum.ogg", "gobblegum");
            bool bombsoundLoaded = AudioClipStorage.LoadClip("C:\\AMPDatastore\\Instances\\SCPDevelopmentEnvironment01\\scp-secret-laboratory\\996560\\AppData\\EXILED\\Audio\\bombsound.ogg", "bombsound");

            if (mysteryLoaded)
                Log.Info("Mysterybox Audio geladen.");
            else
                Log.Info("Mysterybox Audio konnte nicht geladen werden.");

            if (gobblegumLoaded)
                Log.Info("Gobblegum Audio geladen.");
            else
                Log.Info("Gobblegum Audio konnte nicht geladen werden.");
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
            Exiled.Events.Handlers.Player.ThrownProjectile += PlayerHandler.OnThrowingGrenade;
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
            Exiled.Events.Handlers.Player.ThrownProjectile -= PlayerHandler.OnThrowingGrenade;
        }
    }
}
