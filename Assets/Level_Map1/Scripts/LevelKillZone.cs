using UnityEngine;

namespace ShadowBlade.Level
{
    // Falling below the map: send the player back to the last checkpoint.
    public class LevelKillZone : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) LevelCheckpoint.Respawn(other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject);
        }
    }
}
