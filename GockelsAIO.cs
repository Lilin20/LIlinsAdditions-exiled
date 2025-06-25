using System;
using System.Text;
using Exiled.API.Features;
using Exiled.CustomItems;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using GockelsAIO_exiled.Features;
using GockelsAIO_exiled.Roles.ClassD;
using GockelsAIO_exiled.Roles.NTF;
using HarmonyLib;

namespace GockelsAIO_exiled
{
    public class GockelsAIO : Plugin<Config>
    {
        public override string Name => "Gockels AIO";
        public override string Author => "Gockel";
        public static GockelsAIO Instance;
        public EventHandlers EventHandlers;

        public override void OnEnabled()
        {
            EventHandlers = new EventHandlers();
            Instance = this;
            
            RegisterMERHandlers();
            RegisterPlayerHandlers();
            RegisterServerHandlers();
            RegisterCustomRoles();

            Exiled.Events.Handlers.Map.ExplodingGrenade += EventHandlers.DEBUGGrenadeThrow;

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

            Exiled.Events.Handlers.Map.ExplodingGrenade -= EventHandlers.DEBUGGrenadeThrow;

            CustomWeapon.UnregisterItems();

            EventHandlers = null;
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
            new RiotOperator().Register();
            new KamikazeZombie().Register();
            new Lockpicker().Register();
        }

        public void UnregisterCustomRoles()
        {
            new RiotOperator().Unregister();
            new KamikazeZombie().Unregister();
            new Lockpicker().Unregister();
        }

        public void RegisterMERHandlers()
        {
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += EventHandlers.OnButtonInteract; // Mystery Box Button Handler
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += EventHandlers.OnButtonInteractGobblegum;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted += EventHandlers.OnButtonInteractCoin;
            //ProjectMER.Events.Handlers.Schematic.SchematicSpawned += EventHandlers.OnSchematicSpawn; NUR DEBUG
        }

        public void UnregisterMERHandlers()
        {
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= EventHandlers.OnButtonInteract; // Mystery Box Button Handler
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= EventHandlers.OnButtonInteractGobblegum;
            ProjectMER.Events.Handlers.Schematic.ButtonInteracted -= EventHandlers.OnButtonInteractCoin;
            //ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= EventHandlers.OnSchematicSpawn; NUR DEBUG
        }

        public void RegisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnStart; // Used for spawning Mystery Boxes.
            Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded += EventHandlers.OnRoundEnd;
        }

        public void UnregisterServerHandlers()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnStart;
            Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.OnSpawningGuards;
            Exiled.Events.Handlers.Server.RoundEnded -= EventHandlers.OnRoundEnd;
        }

        public void RegisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned += EventHandlers.SpawnSetPoints; // Handler to give players points on spawn
            Exiled.Events.Handlers.Player.Left += EventHandlers.OnLeft; // Handler to remove the player from the points dict
            Exiled.Events.Handlers.Player.PickingUpItem += EventHandlers.OnPickingUp; // Test point giver
            Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnChangingRolePoints;
            Exiled.Events.Handlers.Player.ChangingItem += EventHandlers.OnChangingItem;
            Exiled.Events.Handlers.Player.Dying += EventHandlers.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting += EventHandlers.OnSCPVoidJump;
            //Exiled.Events.Handlers.Player.Dying += EventHandlers.OnPlayerDied;
        }

        public void UnregisterPlayerHandlers()
        {
            Exiled.Events.Handlers.Player.Spawned -= EventHandlers.SpawnSetPoints;
            Exiled.Events.Handlers.Player.Left -= EventHandlers.OnLeft;
            Exiled.Events.Handlers.Player.PickingUpItem -= EventHandlers.OnPickingUp;
            Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnChangingRolePoints;
            Exiled.Events.Handlers.Player.ChangingItem -= EventHandlers.OnChangingItem;
            Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnKillGivePoints;
            Exiled.Events.Handlers.Player.Hurting -= EventHandlers.OnSCPVoidJump;
            //Exiled.Events.Handlers.Player.Dying -= EventHandlers.OnPlayerDied;
        }

        public static string GetContent(Player player)
        {
            if (!EventHandlers.PlayerPoints.ContainsKey(player))
                return $"💰: -";

            return $"💰: {EventHandlers.GetPoints(player)}";
        }

        public static string GetContentBAK(Player player)
        {
            //Player player = Player.Get(hub);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("💰: ");
            stringBuilder.Append(1000);

            return stringBuilder.ToString();
        }
    }
}
