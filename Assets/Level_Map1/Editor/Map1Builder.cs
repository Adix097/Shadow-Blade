#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using ShadowBlade.Level;

namespace ShadowBlade.LevelEditor
{
    // Menu: Shadow Blade > Build Map 1 Scene   (builds the whole level + a test player in a new scene)
    public static class Map1Builder
    {
        const string Root = "Assets/Level_Map1";
        const int W = 240;                       // level width in tiles (8 screens)
        static Material spriteMat;
        static Tilemap ground, hazards, decor;
        static Tile grass, dirt, stone, wood, spikes, lantern, fire, pole;
        static readonly HashSet<int> pit = new HashSet<int>();

        [MenuItem("Shadow Blade/Build Map 1 Scene")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder(Root + "/Art"))
            {
                EditorUtility.DisplayDialog("Shadow Blade", "Folder Assets/Level_Map1 not found. Copy the Level_Map1 folder into your Assets folder first.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Build Map 1", "This creates Assets/Scenes/Map1.unity (it overwrites an existing Map1). Continue?", "Build", "Cancel")) return;
            try { DoBuild(); }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog("Shadow Blade", "Build failed: " + e.Message + "\n\nIf textures were still importing, wait a few seconds and run it again.", "OK");
            }
        }

        static void DoBuild()
        {
            AssetDatabase.Refresh();
            spriteMat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            EnsureFolder(Root, "Tiles"); EnsureFolder("Assets", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            grass = MakeTile("grass", Tile.ColliderType.Grid); dirt = MakeTile("dirt", Tile.ColliderType.Grid);
            stone = MakeTile("stone", Tile.ColliderType.Grid); wood = MakeTile("wood", Tile.ColliderType.Grid);
            spikes = MakeTile("spikes", Tile.ColliderType.Grid); fire = MakeTile("fire", Tile.ColliderType.Grid);
            lantern = MakeTile("lantern", Tile.ColliderType.None); pole = MakeTile("pole", Tile.ColliderType.None);

            var gridGo = new GameObject("Grid"); gridGo.AddComponent<Grid>();
            ground = NewMap(gridGo.transform, "Ground", 0);
            hazards = NewMap(gridGo.transform, "Hazards", 1);
            decor = NewMap(gridGo.transform, "Decor", -5);
            SetupGroundCollider(); SetupHazardCollider();

            BuildTerrain();
            BuildBackgrounds();
            var player = BuildPlayer();
            BuildCamera(player.transform);
            BuildObjects();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Map1.unity");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Shadow Blade", "Map 1 built and saved as Assets/Scenes/Map1.unity.\n\nPress Play. Move: A/D, jump: Space (twice = double jump).", "OK");
        }

        // ---------------- helpers ----------------
        static void EnsureFolder(string parent, string name) { if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name); }

        static Sprite Spr(string rel)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/" + rel);
            if (s == null) throw new System.Exception("Missing sprite " + rel);
            return s;
        }

        static Tile MakeTile(string name, Tile.ColliderType col)
        {
            string path = Root + "/Tiles/" + name + ".asset";
            var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (t == null) { t = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(t, path); }
            t.sprite = Spr("Tiles/tile_" + name + ".png"); t.colliderType = col;
            EditorUtility.SetDirty(t); return t;
        }

        static Tilemap NewMap(Transform parent, string name, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var tm = go.AddComponent<Tilemap>(); var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order; r.sharedMaterial = spriteMat; return tm;
        }

        static void SetupGroundCollider()
        {
            var go = ground.gameObject;
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            var cc = go.AddComponent<CompositeCollider2D>(); cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
#if UNITY_2022_2_OR_NEWER
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
#else
            tc.usedByComposite = true;
#endif
        }

        static void SetupHazardCollider()
        {
            var tc = hazards.gameObject.AddComponent<TilemapCollider2D>(); tc.isTrigger = true;
            hazards.gameObject.AddComponent<LevelHazard>();
        }

        static void Put(Tilemap tm, int x, int y, TileBase t) { tm.SetTile(new Vector3Int(x, y, 0), t); }
        static void Block(Tilemap tm, int x, int y, int w, int h, TileBase t) { for (int i = 0; i < w; i++) for (int j = 0; j < h; j++) Put(tm, x + i, y + j, t); }

        // ---------------- terrain ----------------
        static void BuildTerrain()
        {
            pit.Clear();
            for (int x = 40; x <= 42; x++) pit.Add(x);       // small tutorial pit
            for (int x = 50; x <= 91; x++) pit.Add(x);       // rooftop pit (jump platform to platform)

            for (int x = 0; x < W; x++)
            {
                if (pit.Contains(x))
                {
                    for (int y = -8; y <= -3; y++) Put(ground, x, y, dirt);
                    Put(ground, x, -2, stone);
                    Put(hazards, x, -1, spikes);
                }
                else
                {
                    for (int y = -8; y <= -1; y++) Put(ground, x, y, dirt);
                    Put(ground, x, 0, grass);
                }
            }

            // Section 1: village gate (0-45)  tutorial: platform, steps, small pit at 40-42
            Block(ground, 20, 2, 4, 1, wood);
            Block(ground, 30, 1, 2, 1, stone);
            Block(ground, 33, 1, 2, 2, stone);

            // Section 2: rooftops (46-95)  hop between wooden roofs over spikes
            int[,] roofs = { { 51, 1, 4 }, { 58, 2, 4 }, { 65, 3, 4 }, { 72, 4, 4 }, { 79, 3, 4 }, { 86, 2, 4 } };
            for (int i = 0; i < roofs.GetLength(0); i++) Block(ground, roofs[i, 0], roofs[i, 1], roofs[i, 2], 1, wood);
            Block(ground, 68, 7, 3, 1, wood);                 // bonus roof (double jump) with a pickup

            // Section 3: market street (96-150)  stalls, fire, spikes, enemies
            foreach (int sx in new[] { 105, 120, 135 })
            {
                Block(ground, sx, 2, 5, 1, wood);
                Put(decor, sx, 1, pole); Put(decor, sx + 4, 1, pole);
            }
            Put(hazards, 114, 1, fire); Put(hazards, 115, 1, fire); Put(hazards, 128, 1, fire);
            for (int x = 140; x <= 142; x++) Put(hazards, x, 1, spikes);

            // Section 4: walls (151-200)  stairs up, long wall-walk with a gap, stairs down
            Block(ground, 156, 1, 1, 1, stone); Block(ground, 157, 1, 1, 2, stone);
            Block(ground, 158, 1, 1, 3, stone); Block(ground, 159, 1, 1, 4, stone);
            Block(ground, 160, 1, 12, 4, stone);
            for (int x = 172; x <= 174; x++) Put(hazards, x, 1, spikes);
            Block(ground, 175, 1, 16, 4, stone);
            Block(ground, 191, 1, 1, 3, stone); Block(ground, 192, 1, 1, 2, stone); Block(ground, 193, 1, 1, 1, stone);
            Put(decor, 165, 5, pole); Put(decor, 180, 5, pole);

            // Section 5: mountain pass (201-239)  climb, fire, ambush plateau, cliff at the end
            Block(ground, 205, 1, 4, 1, stone); Block(ground, 209, 1, 4, 2, stone); Block(ground, 213, 1, 4, 3, stone);
            Block(ground, 217, 1, 21, 3, stone);
            Put(hazards, 222, 4, fire); Put(hazards, 223, 4, fire);
            Block(ground, 238, 1, 2, 12, stone);

            // background fence posts
            var rnd = new System.Random(7);
            for (int x = 4; x < 236; x += rnd.Next(6, 13))
            {
                if (pit.Contains(x)) continue;
                int h = rnd.Next(3, 7);
                for (int y = 1; y <= h; y++) Put(decor, x, y, pole);
            }
        }

        // ---------------- backgrounds ----------------
        static void BuildBackgrounds()
        {
            var parent = new GameObject("Backgrounds").transform;
            string[] names = { "0_sky", "1_far", "2_fog_a", "3_mid", "4_fog_b", "5_foreground" };
            float[] factor = { 0.98f, 0.85f, 0.7f, 0.55f, 0.4f, 1.12f };
            int[] order = { -100, -90, -80, -70, -60, 50 };
            for (int i = 0; i < names.Length; i++)
            {
                var go = new GameObject("BG_" + names[i]); go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(15f, 5.5f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Spr("Backgrounds/bg_map1_" + names[i] + ".png");
                sr.drawMode = SpriteDrawMode.Tiled; sr.tileMode = SpriteTileMode.Continuous;
                sr.size = new Vector2(90f, 16.875f);
                sr.sortingOrder = order[i]; sr.sharedMaterial = spriteMat;
                var p = go.AddComponent<ParallaxLayer>(); p.factor = factor[i]; p.length = 30f;
            }
        }

        // ---------------- player + camera ----------------
        static GameObject BuildPlayer()
        {
            var go = new GameObject("TestPlayer"); go.tag = "Player";
            go.transform.position = LevelCheckpoint.DefaultSpawn;
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("Characters/samurai_idle_0.png"); sr.sortingOrder = 10; sr.sharedMaterial = spriteMat;
            var rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 3f; rb.freezeRotation = true;
            var box = go.AddComponent<BoxCollider2D>(); box.size = new Vector2(1.2f, 3f); box.offset = new Vector2(0f, 1.5f);
            go.AddComponent<LevelTestPlayer>();
            return go;
        }

        static void BuildCamera(Transform target)
        {
            var go = new GameObject("Main Camera"); go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 8.4375f; // 270px / 16 / 2
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.02f, 0.04f, 0.06f);
            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(15f, 5.5f, -10f);
            var f = go.AddComponent<CameraFollow2D>(); f.target = target; f.minX = 15f; f.maxX = W - 15f;
        }

        // ---------------- checkpoints, pickups, enemies, triggers ----------------
        static void BuildObjects()
        {
            var cps = new GameObject("Checkpoints").transform;
            float[,] cp = { { 44, 1 }, { 95, 1 }, { 150, 1 }, { 198, 1 }, { 226, 4 } };
            for (int i = 0; i < cp.GetLength(0); i++)
            {
                var go = new GameObject("Checkpoint_" + (i + 1)); go.transform.SetParent(cps, false);
                go.transform.position = new Vector3(cp[i, 0] + 0.5f, cp[i, 1] + 0.5f, 0f);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("Tiles/tile_lantern.png"); sr.sortingOrder = 3; sr.sharedMaterial = spriteMat;
                var bc = go.AddComponent<BoxCollider2D>(); bc.isTrigger = true; bc.size = new Vector2(2f, 4f); bc.offset = new Vector2(0f, 1f);
                go.AddComponent<LevelCheckpoint>();
            }

            var pickups = new GameObject("Pickups").transform;
            float[,] pk = { { 34f, 3.8f }, { 68.5f, 8.6f }, { 112.5f, 2f }, { 176.5f, 5.8f }, { 219.5f, 4.8f } };
            for (int i = 0; i < pk.GetLength(0); i++)
            {
                var go = new GameObject("Pickup_Health_" + (i + 1)); go.transform.SetParent(pickups, false);
                go.transform.position = new Vector3(pk[i, 0], pk[i, 1], 0f);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Spr("Items/item_rice_ball.png"); sr.sortingOrder = 4; sr.sharedMaterial = spriteMat;
                var cc = go.AddComponent<CircleCollider2D>(); cc.isTrigger = true; cc.radius = 0.7f;
                go.AddComponent<LevelPickup>();
            }

            var en = new GameObject("EnemySpawns").transform;
            object[,] es = {
                { EnemySpawnMarker.Kind.Soldier, 108, 1 }, { EnemySpawnMarker.Kind.Spearman, 118, 1 }, { EnemySpawnMarker.Kind.Soldier, 126, 1 },
                { EnemySpawnMarker.Kind.Spearman, 132, 1 }, { EnemySpawnMarker.Kind.Soldier, 146, 1 },
                { EnemySpawnMarker.Kind.Archer, 165, 5 }, { EnemySpawnMarker.Kind.Archer, 177, 5 }, { EnemySpawnMarker.Kind.Archer, 186, 5 },
                { EnemySpawnMarker.Kind.Ambusher, 233, 4 }, { EnemySpawnMarker.Kind.Ambusher, 235, 4 }, { EnemySpawnMarker.Kind.Ambusher, 236, 4 } };
            for (int i = 0; i < es.GetLength(0); i++)
            {
                var kind = (EnemySpawnMarker.Kind)es[i, 0];
                var go = new GameObject("EnemySpawn_" + kind + "_" + (i + 1)); go.transform.SetParent(en, false);
                go.transform.position = new Vector3((int)es[i, 1] + 0.5f, (int)es[i, 2], 0f);
                var m = go.AddComponent<EnemySpawnMarker>(); m.kind = kind;
            }

            var amb = new GameObject("AmbushTrigger"); amb.transform.position = new Vector3(231f, 9f, 0f);
            var ab = amb.AddComponent<BoxCollider2D>(); ab.isTrigger = true; ab.size = new Vector2(4f, 10f);
            amb.AddComponent<AmbushTrigger>();

            var kz = new GameObject("KillZone"); kz.transform.position = new Vector3(W / 2f, -12f, 0f);
            var kb = kz.AddComponent<BoxCollider2D>(); kb.isTrigger = true; kb.size = new Vector2(W + 60f, 4f);
            kz.AddComponent<LevelKillZone>();

            var walls = new GameObject("Boundaries");
            var l = new GameObject("LeftWall"); l.transform.SetParent(walls.transform, false); l.transform.position = new Vector3(-0.5f, 10f, 0f);
            l.AddComponent<BoxCollider2D>().size = new Vector2(1f, 60f);
            var r = new GameObject("RightWall"); r.transform.SetParent(walls.transform, false); r.transform.position = new Vector3(W + 0.5f, 10f, 0f);
            r.AddComponent<BoxCollider2D>().size = new Vector2(1f, 60f);
        }
    }
}
#endif
