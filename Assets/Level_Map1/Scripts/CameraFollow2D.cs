using UnityEngine;

namespace ShadowBlade.Level
{
    public class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        public float smooth = 6f;
        public float minX = 15f, maxX = 225f;   // keeps the camera inside the level
        public float baseY = 5.5f;              // ground sits near the bottom of the screen
        public Vector2 offset = new Vector2(2f, 2.5f);

        void Start() { Snap(); }
        void LateUpdate()
        {
            if (target == null) return;
            Vector3 want = Wanted();
            transform.position = Vector3.Lerp(transform.position, want, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }
        public void Snap() { if (target != null) transform.position = Wanted(); }
        Vector3 Wanted()
        {
            float dir = Mathf.Sign(target.localScale.x == 0 ? 1 : target.localScale.x);
            float x = Mathf.Clamp(target.position.x + offset.x * dir, minX, maxX);
            float y = Mathf.Max(baseY, target.position.y + offset.y - 2.5f);
            return new Vector3(x, y, -10f);
        }
    }
}
