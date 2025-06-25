using Exiled.API.Features;
using UnityEngine;

namespace GockelsAIO_exiled
{
    public class AnimBox : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            Log.Info(collider.name + " entered!");

            if (!collider.CompareTag("Player") || !Player.TryGet(collider.transform.root.gameObject, out Player player)) return;

            Animator animator = GetComponentInParent<Animator>();
            if (animator != null)
            {
                //animator.SetTrigger("StartAnim");
                Log.Info("Animation triggered.");
            }
            else
            {
                Log.Warn("Animator not found on parent!");
            }
        }
    }
}
