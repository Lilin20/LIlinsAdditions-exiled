using Exiled.CustomRoles.API;
using Exiled.Loader;
using GockelsAIO_exiled.Roles.ClassD;
using GockelsAIO_exiled.Roles.NTF;
using System.Linq;

namespace GockelsAIO_exiled.Handlers
{
    public class CustomRoleHandler
    {
        public static void RegisterRoles()
        {
            new RiotOperator().Register();

            new KamikazeZombie().Register();

            if (!Loader.Plugins.Any(plugin => plugin.Prefix == "VVUP.CR"))
            {
                new Lockpicker().Register();
            }

            new LuckyMan().Register();

            new Thief().Register();
        }

        public static void UnregisterRoles()
        {
            new RiotOperator().Unregister();

            new KamikazeZombie().Unregister();

            if (!Loader.Plugins.Any(plugin => plugin.Prefix == "VVUP.CR"))
            {
                new Lockpicker().Unregister();
            }

            new LuckyMan().Unregister();

            new Thief().Unregister();
        }
    }
}
