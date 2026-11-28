using UnityEngine;

namespace ShadowBlade.Level
{
    public class LevelPickup : MonoBehaviour
    {
        public enum Kind { Health, Ammo }
        public Kind kind = Kind.Health; public int amount = 25;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            // Hook for the team: add a method named OnLevelPickup(LevelPickup p) on the player and it will be called.
            other.SendMessage("OnLevelPickup", this, SendMessageOptions.DontRequireReceiver);
            Debug.Log("Picked up " + kind + " +" + amount);
            Destroy(gameObject);
        }
    }
}
