using UnityEngine;

namespace ShadowBlade.Level
{
    // Lantern checkpoint. Stand next to it and it lights up.
    public class LevelCheckpoint : MonoBehaviour
    {
        public static Vector3 Last; public static bool HasPoint;
        public static Vector3 DefaultSpawn = new Vector3(3f, 1.2f, 0f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { HasPoint = false; }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            Last = new Vector3(transform.position.x, transform.position.y + 0.3f, 0f); HasPoint = true;
            var sr = GetComponent<SpriteRenderer>(); if (sr != null) sr.color = new Color(1.6f, 1.4f, 1f, 1f);
        }

        public static void Respawn(GameObject player)
        {
            player.transform.position = HasPoint ? Last : DefaultSpawn;
            var rb = player.GetComponent<Rigidbody2D>(); if (rb != null) RbUtil.SetVel(rb, Vector2.zero);
        }
    }
}
