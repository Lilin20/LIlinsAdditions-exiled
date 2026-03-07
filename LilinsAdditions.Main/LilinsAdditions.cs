using System;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using HarmonyLib;
using LilinsAdditions.Main.Features;
using LilinsAdditions.Main.Handlers;
using MEC;
using ProjectMER.Events.Handlers;
using Map = Exiled.Events.Handlers.Map;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace LilinsAdditions;

public class LilinsAdditions : Plugin<Config>
{
    // Harmony and Coroutines
    private Harmony _harmony;

    private CoroutineHandle _pointSystemCheckCoroutine;

    // Plugin Metadata
    public override string Name => "Lilin's Additions";
    public override string Author => "Lilin";
    public override Version Version => new(1, 0, 5);

    // Singleton Instance
    public static LilinsAdditions Instance { get; private set; }

    // Handlers
    public PlayerHandler PlayerHandler { get; private set; }
    public ServerHandler ServerHandler { get; private set; }
    public PMERHandler PMERHandler { get; private set; }

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
        PMERHandler = new PMERHandler();
        _harmony = new Harmony("lilins.patches");
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
        Schematic.ButtonInteracted += PMERHandler.OnButtonInteract;
        Schematic.ButtonInteracted += PMERHandler.OnButtonInteractGobblegum;
        Schematic.ButtonInteracted += PMERHandler.OnButtonInteractCoin;
    }

    private void UnregisterMERHandlers()
    {
        Schematic.ButtonInteracted -= PMERHandler.OnButtonInteract;
        Schematic.ButtonInteracted -= PMERHandler.OnButtonInteractGobblegum;
        Schematic.ButtonInteracted -= PMERHandler.OnButtonInteractCoin;
    }

    private void RegisterServerHandlers()
    {
        Server.RoundStarted += ServerHandler.OnStart;
        Server.RoundStarted += ServerHandler.OnSpawningGuards;
        Server.RoundEnded += ServerHandler.OnRoundEnd;
        Map.Generated += ServerHandler.OnMapGeneration;
    }

    private void UnregisterServerHandlers()
    {
        Server.RoundStarted -= ServerHandler.OnStart;
        Server.RoundStarted -= ServerHandler.OnSpawningGuards;
        Server.RoundEnded -= ServerHandler.OnRoundEnd;
        Map.Generated -= ServerHandler.OnMapGeneration;
    }

    private void RegisterPlayerHandlers()
    {
        Player.Spawned += PlayerHandler.SpawnSetPoints;
        Player.Left += PlayerHandler.OnLeft;
        Player.Dying += PlayerHandler.OnKillGivePoints;
        Player.Hurting += PlayerHandler.OnSCPVoidJump;
        Exiled.Events.Handlers.Scp914.UpgradingPlayer += PlayerHandler.OnPlayerIn914;
        Player.Dying += PlayerHandler.DropCreditOnDeath;
        Player.PickingUpItem += PlayerHandler.OnPickingUpCreditCard;
    }

    private void UnregisterPlayerHandlers()
    {
        Player.Spawned -= PlayerHandler.SpawnSetPoints;
        Player.Left -= PlayerHandler.OnLeft;
        Player.Dying -= PlayerHandler.OnKillGivePoints;
        Player.Hurting -= PlayerHandler.OnSCPVoidJump;
        Exiled.Events.Handlers.Scp914.UpgradingPlayer -= PlayerHandler.OnPlayerIn914;
        Player.Dying -= PlayerHandler.DropCreditOnDeath;
        Player.PickingUpItem -= PlayerHandler.OnPickingUpCreditCard;
    }

    #endregion
}