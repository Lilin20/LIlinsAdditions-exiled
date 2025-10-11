using Exiled.API.Features;
using UnityEngine;

namespace GockelsAIO_exiled
{
    public class AnimBox : MonoBehaviour
    {
        public void OnTriggerEnter(Collider collider)
        {
            Log.Debug(collider.name + " entered!");

            if (!collider.CompareTag("Player") || !Player.TryGet(collider.transform.root.gameObject, out Player player)) return;

            Animator animator = GetComponentInParent<Animator>();
            if (animator != null)
            {
                //animator.SetTrigger("StartAnim");
                Log.Debug("Animation triggered.");
            }
            else
            {
                Log.Debug("Animator not found on parent!");
            }
        }
    }
}
