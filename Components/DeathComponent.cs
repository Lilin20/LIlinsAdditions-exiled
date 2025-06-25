using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using UnityEngine;

namespace GockelsAIO_exiled
{
    public class DeathBox : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.CompareTag("Player")) return;

            Log.Info("uhm did it work?");

            if (Player.TryGet(collider.gameObject, out Player pl))
            {
                pl.Kill("yuh");
            }
        }
    }
}
