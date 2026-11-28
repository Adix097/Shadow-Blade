SHADOW BLADE - CHARACTER SPRITES (48x48, facing right, 11 characters)
Each <Name>_spritesheet.png is 6 columns x N rows (288 px wide); row order and frame counts are in <Name>_animations.json.
Animations: idle, walk, run, jump, fall, wallslide, dash, slash1-3 (combo), throw, attack, slam, cast, block, shoot, hurt, death
(Kenshin has the full player set; enemies/bosses have the subset they need.)

QUICK SETUP (recommended)
1. Copy the Characters folder to Assets/Art/Sprites/ and Unity/Editor/ShadowBladeAnimationBuilder.cs to Assets/Editor/.
2. Select one or more <Name>_frames folders in the Project window.
3. Menu: Shadow Blade > Build Animations From Selected Frame Folders.
   -> sets import settings and creates .anim clips + an Animator Controller per character (Assets/.../Animations/<Name>/).
4. Add a SpriteRenderer + Animator to your prefab and assign the controller. Draw transitions/parameters yourself.

MANUAL SETUP (sprite sheets)
Texture Type=Sprite (2D and UI), Sprite Mode=Multiple, Pixels Per Unit=16, Filter=Point, Compression=None.
Sprite Editor > Slice > Grid By Cell Size 48x48, Pivot=Bottom Center. Flip with SpriteRenderer.flipX to face left.
Death/hurt are non-looping; idle/walk/run/fall/wallslide/block loop.
