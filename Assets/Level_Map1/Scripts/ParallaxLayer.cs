using UnityEngine;

namespace ShadowBlade.Level
{
    // Endless horizontal parallax. factor: 1 = glued to camera (far sky), 0 = moves with the world.
    public class ParallaxLayer : MonoBehaviour
    {
        public float factor = 0.5f;
        public float length = 30f;   // width of ONE background image in world units (480px / 16 = 30)
        Transform cam; float startX;

        void Start() { cam = Camera.main != null ? Camera.main.transform : null; startX = transform.position.x; }

        void LateUpdate()
        {
            if (cam == null) { if (Camera.main == null) return; cam = Camera.main.transform; }
            float temp = cam.position.x * (1f - factor);
            float dist = cam.position.x * factor;
            if (temp > startX + length) startX += length;
            else if (temp < startX - length) startX -= length;
            transform.position = new Vector3(startX + dist, cam.position.y, transform.position.z);
        }
    }
}
