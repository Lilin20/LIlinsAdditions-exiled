using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GockelsAIO_exiled.Helper
{
    public class HelperScripts
    {
        public static float GetLeftXPosition(float _aspectRatio)
        {
            return (622.27444f * Mathf.Pow(_aspectRatio, 3f)) + (-2869.08991f * Mathf.Pow(_aspectRatio, 2f)) + (3827.03102f * _aspectRatio) - 1580.21554f;
        }
    }
}
