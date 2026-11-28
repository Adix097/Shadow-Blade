using UnityEngine;

namespace ShadowBlade.Level
{
    // TEMPORARY player used only to test the level. Replace with the team's real player.
    // Controls: A/D or arrows = move, Space/W = jump (press twice for double jump).
    [RequireComponent(typeof(Rigidbody2D))]
    public class LevelTestPlayer : MonoBehaviour
    {
        public float speed = 8f, jumpVelocity = 15f;
        Rigidbody2D rb; BoxCollider2D box; int jumps;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>(); box = GetComponent<BoxCollider2D>();
            rb.freezeRotation = true; rb.gravityScale = 3f;
            rb.sharedMaterial = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        }

        bool Grounded()
        {
            Vector2 p = (Vector2)transform.position + new Vector2(0f, -0.05f);
            foreach (var h in Physics2D.OverlapBoxAll(p, new Vector2(box.size.x * 0.9f, 0.12f), 0f))
                if (h != null && !h.isTrigger && h.gameObject != gameObject) return true;
            return false;
        }

        void Update()
        {
            float h = LevelInput.Horizontal();
            Vector2 v = RbUtil.GetVel(rb); v.x = h * speed;
            if (Grounded() && v.y <= 0.01f) jumps = 0;
            if (LevelInput.JumpDown() && jumps < 2) { v.y = jumpVelocity; jumps++; }
            RbUtil.SetVel(rb, v);
            if (Mathf.Abs(h) > 0.01f) { var s = transform.localScale; s.x = Mathf.Abs(s.x) * Mathf.Sign(h); transform.localScale = s; }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<LevelHazard>() != null || other.GetComponent<LevelKillZone>() != null) LevelCheckpoint.Respawn(gameObject);
        }
    }
}
