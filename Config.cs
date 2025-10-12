using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;
using GockelsAIO_exiled.Items.GobbleGums;
using GockelsAIO_exiled.Items.Keycards;
using GockelsAIO_exiled.Items.SCPs;
using GockelsAIO_exiled.Items.Weapons.Grenade;
using GockelsAIO_exiled.Items.Weapons.LMGs;
using GockelsAIO_exiled.Items.Weapons.Pistols;
using GockelsAIO_exiled.Items.Weapons.Rifles;
using GockelsAIO_exiled.Items.Weapons.Shotguns;
using GockelsAIO_exiled.Items.Weapons.SMGs;
using GockelsAIO_exiled.Roles.ClassD;
using GockelsAIO_exiled.Roles.NTF;

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
        public bool EnableCeilingTrap { get; set; } = false;
        public bool EnableRandomGuardSpawn { get; set; } = false;
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
        public List<Lockpicker> lockpicker { get; set; } = new()
        {
            new Lockpicker()
        };
        public List<LuckyMan> luckyMan { get; set; } = new()
        {
            new LuckyMan()
        };
        public List<Thief> thief { get; set; } = new()
        {
            new Thief()
        };
        public List<RiotOperator> riotOperator { get; set; } = new()
        {
            new RiotOperator()
        };
        public List<KamikazeZombie> kamikazeZombie { get; set; } = new()
        {
            new KamikazeZombie()
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
