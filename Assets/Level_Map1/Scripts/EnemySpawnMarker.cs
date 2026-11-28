using UnityEngine;

namespace ShadowBlade.Level
{
    // Placeholder where an enemy should stand. Drag the enemy prefab into "Enemy Prefab" and it spawns on Play.
    public class EnemySpawnMarker : MonoBehaviour
    {
        public enum Kind { Soldier, Spearman, Archer, Ambusher }
        public Kind kind = Kind.Soldier; public GameObject enemyPrefab; public bool faceLeft = true;

        void Start()
        {
            if (enemyPrefab == null) return;
            var e = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            if (faceLeft) { var s = e.transform.localScale; s.x = -Mathf.Abs(s.x); e.transform.localScale = s; }
        }
        void OnDrawGizmos()
        {
            Gizmos.color = kind == Kind.Archer ? Color.yellow : Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 1.5f, new Vector3(1.5f, 3f, 0f));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3.2f, kind.ToString());
#endif
        }
    }
}
