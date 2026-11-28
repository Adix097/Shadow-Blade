using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Shadow Blade: select one or more "*_frames" folders in the Project window,
// then use  Shadow Blade > Build Animations From Selected Frame Folders.
// Sets sprite import settings (PPU 16, point filter, bottom-centre pivot),
// creates one AnimationClip per animation and an AnimatorController per character.
public static class ShadowBladeAnimationBuilder
{
    static readonly Dictionary<string, float> Fps = new Dictionary<string, float> {
        {"idle",6},{"walk",10},{"run",14},{"jump",8},{"fall",8},{"wallslide",6},{"dash",16},
        {"slash1",14},{"slash2",14},{"slash3",14},{"attack",12},{"throw",14},{"slam",8},
        {"cast",8},{"block",6},{"shoot",10},{"hurt",8},{"death",8} };
    static readonly HashSet<string> Loops = new HashSet<string> { "idle","walk","run","fall","wallslide","block" };

    [MenuItem("Shadow Blade/Build Animations From Selected Frame Folders")]
    static void Build()
    {
        foreach (var obj in Selection.objects)
        {
            string folder = AssetDatabase.GetAssetPath(obj);
            if (!AssetDatabase.IsValidFolder(folder)) continue;
            string ch = Path.GetFileName(folder).Replace("_frames", "");
            string outDir = Path.GetDirectoryName(folder).Replace('\\', '/') + "/Animations/" + ch;
            Directory.CreateDirectory(outDir);
            var pngs = Directory.GetFiles(folder, "*.png").Select(p => p.Replace('\\', '/')).ToList();
            foreach (var path in pngs)
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = 16;
                imp.filterMode = FilterMode.Point;
                imp.mipmapEnabled = false;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                var ts = new TextureImporterSettings();
                imp.ReadTextureSettings(ts);
                ts.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                imp.SetTextureSettings(ts);
                imp.SaveAndReimport();
            }
            var groups = new Dictionary<string, List<(int i, string path)>>();
            foreach (var path in pngs)
            {
                string n = Path.GetFileNameWithoutExtension(path).Substring(ch.Length + 1);
                int us = n.LastIndexOf('_');
                string anim = n.Substring(0, us);
                int idx = int.Parse(n.Substring(us + 1));
                if (!groups.ContainsKey(anim)) groups[anim] = new List<(int, string)>();
                groups[anim].Add((idx, path));
            }
            string ctrlPath = outDir + "/" + ch + ".controller";
            AssetDatabase.DeleteAsset(ctrlPath);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var sm = ctrl.layers[0].stateMachine;
            foreach (var kv in groups.OrderBy(k => k.Key != "idle"))
            {
                float fps = Fps.ContainsKey(kv.Key) ? Fps[kv.Key] : 12f;
                var sprites = kv.Value.OrderBy(x => x.i).Select(x => AssetDatabase.LoadAssetAtPath<Sprite>(x.path)).ToArray();
                var clip = new AnimationClip { frameRate = fps };
                var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "sprite");
                var keys = sprites.Select((s, i) => new ObjectReferenceKeyframe { time = i / fps, value = s }).ToArray();
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
                var set = AnimationUtility.GetAnimationClipSettings(clip);
                set.loopTime = Loops.Contains(kv.Key);
                AnimationUtility.SetAnimationClipSettings(clip, set);
                string clipPath = outDir + "/" + ch + "_" + kv.Key + ".anim";
                AssetDatabase.DeleteAsset(clipPath);
                AssetDatabase.CreateAsset(clip, clipPath);
                var state = sm.AddState(kv.Key);
                state.motion = clip;
                if (kv.Key == "idle") sm.defaultState = state;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Shadow Blade: built animations for " + ch + " in " + outDir);
        }
        AssetDatabase.Refresh();
    }
}
