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
            LilinsAdditions.Instance.Config.riotOperator.Register();

            LilinsAdditions.Instance.Config.kamikazeZombie.Register();

            if (!Loader.Plugins.Any(plugin => plugin.Prefix == "VVUP.CR"))
            {
                //LilinsAdditions.Instance.Config.lockpicker.Register();
            }

            LilinsAdditions.Instance.Config.luckyMan.Register();

            LilinsAdditions.Instance.Config.thief.Register();
        }

        public static void UnregisterRoles()
        {
            LilinsAdditions.Instance.Config.riotOperator.Unregister();

            LilinsAdditions.Instance.Config.kamikazeZombie.Unregister();

            if (!Loader.Plugins.Any(plugin => plugin.Prefix == "VVUP.CR"))
            {
                //LilinsAdditions.Instance.Config.lockpicker.Unregister();
            }

            LilinsAdditions.Instance.Config.luckyMan.Unregister();

            LilinsAdditions.Instance.Config.thief.Unregister();
        }
    }
}
