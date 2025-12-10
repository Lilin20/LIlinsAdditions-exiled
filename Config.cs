using Exiled.API.Enums;
using Exiled.API.Interfaces;
using GockelsAIO_exiled.Items.GobbleGums;
using GockelsAIO_exiled.Items.SCPs;
using GockelsAIO_exiled.Items.Weapons.Grenade;
using GockelsAIO_exiled.Items.Weapons.LMGs;
using GockelsAIO_exiled.Items.Weapons.Pistols;
using GockelsAIO_exiled.Items.Weapons.Rifles;
using GockelsAIO_exiled.Items.Weapons.Shotguns;
using GockelsAIO_exiled.Items.Weapons.SMGs;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static GockelsAIO_exiled.SchematicSpawner;

namespace GockelsAIO_exiled
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool EnableCustomRoles { get; set; } = false;
        public bool EnableFortunaFizz {  get; set; } = true;
        public bool EnableMysteryBox { get; set; } = true;
        public bool EnableHiddenCoins { get; set; } = false;
        public bool EnableRandomGuardSpawn { get; set; } = false;
        public bool Enable914Teleport { get; set; } = false;
        public bool EnableCreditCardDrop { get; set; } = false;
        public bool EnableAntiSCPSuicide { get; set; } = false;
        public float BurstSoundVolume { get; set; } = 1;
        public string VendingMachineMusicPath { get; set; } = string.Empty;
        public string MysteryBoxMusicPath { get; set; } = string.Empty;
        public string BurstSoundPath { get; set; } = string.Empty;
        public string TrackingSoundPath { get; set; } = string.Empty;
        public float VendingMachineMusicVolume { get; set; } = 2;
        public float MysteryBoxMusicVolume { get; set; } = 1.5f;
        public float PickpocketCooldown { get; set; } = 15;
        public int StartingPoints { get; set; } = 400;
        public int PointsForKillingEnemy { get; set; } = 200;
        public int PointsOverTime { get; set; } = 100;
        public int PointsOverTimeDelay { get; set; } = 120;
        public int PointsForMysteryBox { get; set; } = 800;
        public int PointsForVendingMachine { get; set; } = 200;
        public int PointsForCoin { get; set; } = 1500;
        public string MysteryBoxMissingPointsText { get; set; } = "<color=red>You need 800 Points to open the box!</color>";
        public string VendingMachineMissingPointsText { get; set; } = "<color=red>You need 200 points!</color>";
        public List<Helper.MysteryBoxPoolConfi> MysteryBoxItemPool { get; set; } = new()
        {
            new() {Name = "Prototype LMG - Nano Rockets", Weight = 1},
            new() {Name = "Grenade Launcher", Weight = 2},
            new() {Name = "Behemoth 50.cal", Weight = 3},
            new() {Name = "Nullifier", Weight = 10},
            new() {Name = "Entity Swapper", Weight = 10},
            new() {Name = "Russian Roulette", Weight = 10},
            new() {Name = "HumeBreaker v2.1", Weight = 2},
            new() {Name = "RangeTec - .308 Lapua", Weight = 3},
            new() {Name = "Kerberos-12", Weight = 10},
            new() {Name = "MS9K - MedShot 9000", Weight = 6},
        };

        public Dictionary<RoomType, SpawnData> MysteryBoxSpawnPoints { get; set; } = new()
        {
            { RoomType.Lcz330, new SpawnData(new Vector3(-5.84f, 0.13f, 3.014f), new Vector3(0, -90, 0)) },
            { RoomType.LczGlassBox, new SpawnData(new Vector3(4.503f, 0.13f, 4.947f), new Vector3(0, -90, 0)) },
            { RoomType.LczAirlock, new SpawnData(new Vector3(0f, 0.13f, 1f), new Vector3(0, -90, 0)) },
            { RoomType.LczCafe, new SpawnData(new Vector3(-5.878f, 0.13f, 4.542f), new Vector3(0, -90, 0)) },
            { RoomType.LczClassDSpawn, new SpawnData(new Vector3(-24.711f, 0.13f, 0f), new Vector3(0, -180f, 0)) },
            { RoomType.LczCrossing, new SpawnData(new Vector3(2.343f, 0.13f, -2.31f), new Vector3(0, 45, 0)) },
            { RoomType.LczPlants, new SpawnData(new Vector3(0f, 0.13f, 1.474f), new Vector3(0, -90f, 0)) },
            { RoomType.LczStraight, new SpawnData(new Vector3(0f, 0.13f, -1.144f), new Vector3(0, 90f, 0)) },
            { RoomType.LczTCross, new SpawnData(new Vector3(1.158f, 0.13f, 0f), new Vector3(0, 0f, 0)) },
            { RoomType.Hcz127, new SpawnData(new Vector3(-3.769f, 0.13f, -5.085f), new Vector3(0, 135f, 0)) },
            { RoomType.HczIntersectionJunk, new SpawnData(new Vector3(-1.614f, 0.13f, 0f), new Vector3(0, -180f, 0)) },
            { RoomType.HczHid, new SpawnData(new Vector3(2.247f, 0.13f, -1.868f), new Vector3(0, 90f, 0)) },
            { RoomType.HczStraightPipeRoom, new SpawnData(new Vector3(-6.326f, 5.204f, 5.375f), new Vector3(0, 180f, 0)) },
            { RoomType.HczArmory, new SpawnData(new Vector3(2.065f, 0.13f, 5.214f), new Vector3(0, 0f, 0)) },
        };

        public Dictionary<RoomType, SpawnData> VendingMachineSpawnPoints { get; set; } = new()
        {
            { RoomType.LczGlassBox, new SpawnData(new Vector3(8.842f, 0.332f, -2.923f), new Vector3(0, 0, 0)) },
            { RoomType.LczCafe, new SpawnData(new Vector3(-4.339f, 0.332f, -4.614f), new Vector3(0, 90, 0)) },
            { RoomType.Lcz914, new SpawnData(new Vector3(0f, 0.332f, 6.879f), new Vector3(0, -90, 0)) },
            { RoomType.HczHid, new SpawnData(new Vector3(1.037f, 0.332f, 4.992f), new Vector3(0, -90f, 0)) },
            { RoomType.HczArmory, new SpawnData(new Vector3(2.11f, 0.332f, -5.15f), new Vector3(0, 0f, 0)) },
        };

        public Dictionary<RoomType, SpawnData> CoinSpawnPoints { get; set; } = new()
        {
            { RoomType.LczAirlock, new SpawnData(new Vector3(2.855f, 0.0178f, -4.25f), new Vector3(0, 0, 0)) },
            { RoomType.Lcz173, new SpawnData(new Vector3(12.099f, 11.479f, 3.962f), new Vector3(0, 0, 0)) },
            { RoomType.LczArmory, new SpawnData(new Vector3(0.2342f, 0.524f, 2.0502f), new Vector3(0, 0, 0)) },
            { RoomType.HczHid, new SpawnData(new Vector3(-6.05f, 4.4983f, -4.744f), new Vector3(0, 0, 0)) },
            { RoomType.HczNuke, new SpawnData(new Vector3(1.288f, -72.417f, -0.423f), new Vector3(0, 0, 0)) },
        };

        public List<Gasmask> gasmask { get; set; } = new()
        {
            new Gasmask()
        };
        public List<StickyGrenade> stickyGrenade { get; set; } = new()
        {
            new StickyGrenade()
        };
        public List<ExplosiveLMG> explosiveLMG { get; set; } = new()
        {
            new ExplosiveLMG()
        };
        public List<GrenadeLauncher> grenadeLauncher { get; set; } = new()
        {
            new GrenadeLauncher()
        };
        public List<Behemoth> behemoth { get; set; } = new()
        {
            new Behemoth()
        };
        public List<HackGun> hackGun { get; set; } = new()
        {
            new HackGun()
        };
        public List<PlaceSwap> placeSwap { get; set; } = new()
        {
            new PlaceSwap()
        };
        public List<RussianRoulette> russianRoulette { get; set; } = new()
        {
            new RussianRoulette()
        };
        public List<HumeBreaker> humeBreaker { get; set; } = new()
        {
            new HumeBreaker()
        };
        public List<Sniper> sniper { get; set; } = new()
        {
            new Sniper()
        };
        public List<BreachShotgun> breachShotgun { get; set; } = new()
        {
            new BreachShotgun()
        };
        public List<MedicGun> medicGun { get; set; } = new()
        {
            new MedicGun()
        };
        public List<AnywhereButHere> anywhereButHere { get; set; } = new()
        {
            new AnywhereButHere()
        };
        public List<ChemicalCocktail> americanDream { get; set; } = new()
        {
            new ChemicalCocktail()
        };
        public List<BetterBeSmall> betterBeSmall { get; set; } = new()
        {
            new BetterBeSmall()
        };
        public List<DeathCheat> deathCheat { get; set; } = new()
        {
            new DeathCheat()
        };
        public List<IDontWantToBeHere> dontWantToBeHere { get; set; } = new()
        {
            new IDontWantToBeHere()
        };
        public List<InPlainSight> inPlainSight { get; set; } = new()
        {
            new InPlainSight()
        };
        public List<Juggernaut> juggernaut { get; set; } = new()
        {
            new Juggernaut()
        };
        public List<LightHeaded> lightHeaded { get; set; } = new()
        {
            new LightHeaded()
        };
        public List<NeverSeen> neverSeen { get; set; } = new()
        {
            new NeverSeen()
        };
        public List<NowYouSeeMe> nowYouSeeMe { get; set; } = new()
        {
            new NowYouSeeMe()
        };
        public List<PeelAndRun> peelAndRun { get; set; } = new()
        {
            new PeelAndRun()
        };
        public List<RandomEffect> randomEffect { get; set; } = new()
        {
            new RandomEffect()
        };
        public List<ShadowStep> shadowStep { get; set; } = new()
        {
            new ShadowStep()
        };
        public List<ShrinkAndRun> shrinkAndRun { get; set; } = new()
        {
            new ShrinkAndRun()
        };
        public List<SilentStep> silentStep { get; set; } = new()
        {
            new SilentStep()
        };
        public List<Switcheroo> switcheroo { get; set; } = new()
        {
            new Switcheroo()
        };
        public List<WhereIsWaldo> whereIsWaldo { get; set; } = new()
        {
            new WhereIsWaldo()
        };
    }
}
