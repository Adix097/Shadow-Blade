using UnityEngine;
using UnityEngine.Events;

namespace ShadowBlade.Level
{
    // End of Map 1: when the player walks in, the ambush cinematic starts.
    // In the Inspector, add your friend's cinematic / scene-load function to "On Ambush".
    public class AmbushTrigger : MonoBehaviour
    {
        public UnityEvent onAmbush; bool done;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (done || !other.CompareTag("Player")) return;
            done = true; Debug.Log("AMBUSH TRIGGERED - start cinematic here"); onAmbush.Invoke();
        }
    }
}
