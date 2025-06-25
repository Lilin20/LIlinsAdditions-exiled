using Exiled.CustomRoles.API;
using GockelsAIO_exiled.Roles.ClassD;
using GockelsAIO_exiled.Roles.NTF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GockelsAIO_exiled.Handlers
{
    public class CustomRoleHandler
    {
        public static void RegisterRoles()
        {
            new RiotOperator().Register();
            new KamikazeZombie().Register();
            new Lockpicker().Register();
        }

        public static void UnregisterRoles()
        {
            new RiotOperator().Unregister();
            new KamikazeZombie().Unregister();
            new Lockpicker().Unregister();
        }
    }
}
