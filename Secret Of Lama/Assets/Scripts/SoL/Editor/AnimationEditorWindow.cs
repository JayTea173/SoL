using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace SoL.Animation
{
    using Actors;
    using System.IO;

    [CustomEditor(typeof(AnimationCollection))]
    public class AnimationCollectionEditor : UnityEditor.Editor
    {
        AnimationCollection ac;

        private void OnEnable()
        {
            ac = (AnimationCollection)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                AnimationEditorWindow.Create(ac);
            }

            EditorGUILayout.Space(64f);
            base.OnInspectorGUI();
        }
    }

    public class AnimationEditorWindow : EditorWindow
    {
        protected AnimationCollection ac
        {
            get;
            set;
        }

        private int currentAnimationId;
        private int currentFrame;
        private BaseActor.Facing facing;
        private bool isPlaying = false;
        private GUIStyle spriteStyle = new GUIStyle();
        private int pixelSize = 4;
        private float t = 0f;

        private double lastEditorTime = 0f;

        private Vector2 animationMotionPosition;
        bool controlKey = false;
        bool allKey = false;

        bool showMotion = false;


        private string[] animationNames;

        private static string[] defaultAnimationNames = new string[] {
            "Idle",
            "Walk",
            "Attack",
            "Hurt",
            "Death",
            "Sprint",
            "Dodge",
            "Spell",
            "Misc Animation"
        };



        public static AnimationEditorWindow Create(AnimationCollection ac)
        {
            AnimationEditorWindow wnd = EditorWindow.GetWindow<AnimationEditorWindow>(true, "Animation Collection Editor", true);
            wnd.Init(ac);
            wnd.minSize = new Vector2(512f, 448);
            return wnd;

        }

        private void Update()
        {
            if (isPlaying)
            {
                double editorTime = EditorApplication.timeSinceStartup;
                float dt = (float)(editorTime - lastEditorTime);
                t += dt;
                lastEditorTime = editorTime;
                
                int frame = SpriteAnimationBehaviour.GetFrameIndex(ac[currentAnimationId], t);
                if (frame != currentFrame)
                {
                    currentFrame = frame;
                    if (currentFrame == 0)
                        animationMotionPosition = Vector2.zero;
                    Repaint();
                }

                if (showMotion)
                {
                    Vector2 move = SpriteAnimationBehaviour.Move(SpriteAnimationBehaviour.SampleMotion(ac[currentAnimationId], t), SpriteAnimationBehaviour.FacingToVector(facing)) * ac[currentAnimationId].frames[currentFrame].motion.motionMultiplier * dt;
                    animationMotionPosition += move;
                }
                
            }
            
        }

        private void OnGUI()
        {
            float labelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 64f;

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            currentAnimationId = EditorGUILayout.Popup("Animation", currentAnimationId, animationNames, GUILayout.Width(192f));

            if (EditorGUI.EndChangeCheck())
            {
                if (currentFrame > ac[currentAnimationId].frames.Count)
                    currentFrame = ac[currentAnimationId].frames.Count - 1;

            }

            if (GUILayout.Button("-", GUILayout.Width(20f)))
            {
                string n = ac[currentAnimationId] == null ? "null" : ac[currentAnimationId].name;

                if (EditorUtility.DisplayDialog("Delete Animation", "Are you sure you want to delete " + n + "? This will also delete the asset.", "Yes", "No"))
                {
                    List<SpriteAnimation> anims = new List<SpriteAnimation>(ac.animations);
                    string guid;
                    long localId;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(anims[currentAnimationId], out guid, out localId);
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
                    anims.RemoveAt(currentAnimationId);
                    ac.animations = anims.ToArray();
                    UpdateAnimations();

                }

            }
            if (GUILayout.Button("+", GUILayout.Width(20f)))
            {
                StringInputPopup.Display((s) =>
                {
                    Array.Resize(ref ac.animations, ac.Count + 1);

                    var a = SpriteAnimation.CreateInstance<SpriteAnimation>();
                    a.name = s;
                    a.frames = new List<SpriteFrame>();
                    ac[ac.Count - 1] = a;
                    string guid;
                    long localId;
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ac, out guid, out localId);
                    string path = AssetDatabase.GUIDToAssetPath(guid).Replace(".asset", "") + "/";
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path += "/" + a.name;
                    AssetDatabase.CreateAsset(ac[ac.Count - 1], path + ".asset");


                    UpdateAnimations();


                }, defaultAnimationNames[System.Math.Min(ac.Count, defaultAnimationNames.Length - 1)], "Enter new animation name.");
            }

            EditorGUI.BeginChangeCheck();
            int c = ac.Count > 0 ? (ac[currentAnimationId] == null ? 0 : ac[currentAnimationId].frames.Count) : 1;
            currentFrame = EditorGUILayout.IntSlider("Frame", currentFrame + 1, System.Math.Min(1, c), c) - 1;

            if (EditorGUI.EndChangeCheck())
            {

            }

            if (GUILayout.Button("-", GUILayout.Width(20f)))
            {
                if (EditorUtility.DisplayDialog("Delete Frame", "Are you sure you want to delete frame #" + (currentFrame + 1) + "?", "Yes", "No"))
                {
                    ac[currentAnimationId].frames.RemoveAt(currentFrame);
                }

            }
            if (GUILayout.Button("+", GUILayout.Width(20f)))
            {
                SpriteFrame f = new SpriteFrame();
                ac[currentAnimationId].frames.Add(f);
            }


            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(">", GUILayout.Width(20f)))
            {
                isPlaying = true;
            }
            if (GUILayout.Button("||", GUILayout.Width(20f)))
            {
                isPlaying = false;
            }
            if (GUILayout.Button("☐", GUILayout.Width(20f)))
            {
                isPlaying = false;
                t = 0f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(16f);

            pixelSize = EditorGUILayout.IntSlider("Pixel Scale", pixelSize, 1, 8, GUILayout.Width(256f));


            var anim = ac[currentAnimationId];

            if (anim != null)
            {
                if (c > currentFrame && currentFrame >= 0)
                {
                    var f = anim.frames[currentFrame];

                    if (f.durationMultiplier <= 0f)
                        f.durationMultiplier = 1f;
                    f.durationMultiplier = Mathf.Max(.5f, EditorGUILayout.FloatField("Duration", f.durationMultiplier, GUILayout.Width(128f)));

                    if (f != null)
                    {
                        switch (facing)
                        {
                            case BaseActor.Facing.Left:
                                ac[currentAnimationId].frames[currentFrame].left.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", f.left.sprite, typeof(Sprite), false);
                                break;
                            case BaseActor.Facing.Right:
                                ac[currentAnimationId].frames[currentFrame].right.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", f.right.sprite, typeof(Sprite), false);
                                break;
                            case BaseActor.Facing.Down:
                                ac[currentAnimationId].frames[currentFrame].down.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", f.down.sprite, typeof(Sprite), false);
                                break;
                            case BaseActor.Facing.Up:
                                ac[currentAnimationId].frames[currentFrame].up.sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", f.up.sprite, typeof(Sprite), false);
                                break;
                        }

                        Vector2 center = new Vector2(position.width / 2f, position.height / 2f);

                        var e = Event.current;
                        if (e.type == EventType.KeyDown)
                        {
                            switch (e.keyCode)
                            {
                                case KeyCode.LeftControl:
                                    controlKey = true;
                                    break;
                                case KeyCode.A:
                                    allKey = true;
                                    break;
                                case KeyCode.LeftArrow:
                                    currentFrame = (currentFrame - 1) % c;
                                    break;
                                case KeyCode.RightArrow:
                                    currentFrame = (currentFrame + 1) % c;
                                    break;
                                case KeyCode.DownArrow:
                                    currentAnimationId = (currentAnimationId - 1) % ac.Count;
                                    break;
                                case KeyCode.UpArrow:
                                    currentAnimationId = (currentAnimationId + 1) % ac.Count;
                                    break;


                            }
                        }
                        else if (e.type == EventType.KeyUp)
                        {
                            switch (e.keyCode)
                            {
                                case KeyCode.LeftControl:
                                    controlKey = false;
                                    break;
                                case KeyCode.A:
                                    allKey = false;
                                    break;
                            }
                        }
                        if (Event.current.commandName == "ObjectSelectorUpdated")
                        {
                            var pickedSprite = (Sprite)EditorGUIUtility.GetObjectPickerObject();
                            switch (facing)
                            {
                                case BaseActor.Facing.Left:
                                    f.left.sprite = pickedSprite;
                                    break;
                                case BaseActor.Facing.Right:
                                    f.right.sprite = pickedSprite;
                                    break;
                                case BaseActor.Facing.Down:
                                    f.down.sprite = pickedSprite;
                                    break;
                                case BaseActor.Facing.Up:
                                    f.up.sprite = pickedSprite;
                                    break;
                            }
                        }

                        if (f[facing] != null)
                        {
                            var spriteData = f[facing];
                            var sprite = spriteData.sprite;
                            if (sprite != null)
                            {
                                float w = (float)sprite.texture.width;
                                float h = (float)sprite.texture.height;
                                float x = center.x - sprite.textureRect.width / 2f * (float)pixelSize;
                                float y = center.y - sprite.textureRect.height / 2f * (float)pixelSize;
                                float spriteW = sprite.textureRect.width * (float)pixelSize;
                                float spriteH = sprite.textureRect.height * (float)pixelSize;
                                var mirr = anim.mirroring[(int)facing];
                                Vector2 mirrV = new Vector2(mirr.X ? -1f : 1f, mirr.Y ? -1f : 1f);

                                x += animationMotionPosition.x;
                                y += animationMotionPosition.y;
                                if (mirr.X)
                                    x += spriteW;
                                if (mirr.Y)
                                    y += spriteH;
                                Rect r = new Rect(x, y, spriteW * mirrV.x, spriteH * mirrV.y);

                                GUI.DrawTextureWithTexCoords(r, sprite.texture, new Rect(sprite.textureRect.x / w, sprite.textureRect.y / h, sprite.textureRect.width / w, sprite.textureRect.height / h));

                            }
                            else if (GUI.Button(new Rect(center.x - 128f, center.y - 128f, 256f, 256f), "Select a sprite"))
                            {
                                EditorGUIUtility.ShowObjectPicker<Sprite>(f[facing], false, string.Empty, 0);
                            }

                        }
                        GUIStyle passiveStyle = new GUIStyle("Button");
                        GUIStyle activeStyle = new GUIStyle(passiveStyle);
                        activeStyle.normal.textColor = activeStyle.hover.textColor = Color.green;
                        activeStyle.fontStyle = FontStyle.Bold;
                        Vector2 spriteAreaSize = new Vector2(256f + 0f, 256f + 0f);
                        if (GUI.Button(new Rect(center.x - spriteAreaSize.x / 2f - 20f, center.y - spriteAreaSize.y / 2f, 20f, spriteAreaSize.y), "<-", facing == BaseActor.Facing.Left ? activeStyle : passiveStyle))
                        {
                            if (controlKey)
                            {
                                if (allKey)
                                {
                                    for (int i = 0; i < c; i++)
                                        ac[currentAnimationId].frames[i].left.sprite = ac[currentAnimationId].frames[i][facing].sprite;
                                    Debug.Log("Copied active all animation frame sprites to target facing!");

                                }
                                else
                                {
                                    ac[currentAnimationId].frames[currentFrame].left.sprite = f[facing];
                                    Debug.Log("Copied active Sprite to target facing!");
                                }
                            }
                            else
                                facing = BaseActor.Facing.Left;
                        }
                        if (GUI.Button(new Rect(center.x + spriteAreaSize.x / 2f, center.y - spriteAreaSize.y / 2f, 20f, spriteAreaSize.y), "->", facing == BaseActor.Facing.Right ? activeStyle : passiveStyle))
                        {
                            if (controlKey)
                            {
                                if (allKey)
                                {
                                    for (int i = 0; i < c; i++)
                                        ac[currentAnimationId].frames[i].right.sprite = ac[currentAnimationId].frames[i][facing].sprite;
                                    Debug.Log("Copied active all animation frame sprites to target facing!");

                                }
                                else
                                {
                                    ac[currentAnimationId].frames[currentFrame].right.sprite = f[facing];
                                    Debug.Log("Copied active Sprite to target facing!");
                                }
                            }
                            facing = BaseActor.Facing.Right;
                        }

                        if (GUI.Button(new Rect(center.x - spriteAreaSize.x / 2f, center.y - spriteAreaSize.y / 2f - 20f, spriteAreaSize.x, 20f), "^", facing == BaseActor.Facing.Up ? activeStyle : passiveStyle))
                        {
                            if (controlKey)
                            {
                                if (allKey)
                                {
                                    for (int i = 0; i < c; i++)
                                        ac[currentAnimationId].frames[i].up.sprite = ac[currentAnimationId].frames[i][facing].sprite;
                                    Debug.Log("Copied active all animation frame sprites to target facing!");

                                }
                                else
                                {
                                    ac[currentAnimationId].frames[currentFrame].up.sprite = f[facing];
                                    Debug.Log("Copied active Sprite to target facing!");
                                }
                            }
                            facing = BaseActor.Facing.Up;
                        }
                        if (GUI.Button(new Rect(center.x - spriteAreaSize.x / 2f, center.y + spriteAreaSize.y / 2f, spriteAreaSize.x, 20f), "v", facing == BaseActor.Facing.Down ? activeStyle : passiveStyle))
                        {
                            if (controlKey)
                            {
                                if (allKey)
                                {
                                    for (int i = 0; i < c; i++)
                                        ac[currentAnimationId].frames[i].down.sprite = ac[currentAnimationId].frames[i][facing].sprite;
                                    Debug.Log("Copied active all animation frame sprites to target facing!");

                                }
                                else
                                {
                                    ac[currentAnimationId].frames[currentFrame].down.sprite = f[facing];
                                    Debug.Log("Copied active Sprite to target facing!");
                                }
                            }
                            facing = BaseActor.Facing.Down;
                        }

                    }
                    else
                        Debug.LogError("NULL");

                    EditorGUILayout.BeginVertical(GUILayout.Width(128f));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Mirror (X/Y)", GUILayout.Width(64f));
                    bool mirrorX = EditorGUILayout.Toggle(anim.mirroring[(int)facing].X);
                    bool mirrorY = EditorGUILayout.Toggle(anim.mirroring[(int)facing].Y);
                    anim.mirroring[(int)facing] = new Mirroring(mirrorX, mirrorY);

                    EditorGUILayout.EndHorizontal();

                    var duration = EditorGUILayout.FloatField("Duration", ac[currentAnimationId].frames[currentFrame].durationMultiplier);
                    if (controlKey)
                    {
                        for (int i = 0; i < c; i++)
                            ac[currentAnimationId].frames[i].durationMultiplier = duration;
                    }
                    else
                        ac[currentAnimationId].frames[currentFrame].durationMultiplier = duration;

                    var flags = (FrameFlags)EditorGUILayout.EnumFlagsField("Flags", ac[currentAnimationId].frames[currentFrame].flags);
                    if (controlKey)
                    {
                        for (int i = 0; i < c; i++)
                            ac[currentAnimationId].frames[i].flags = flags;
                    }
                    else
                        ac[currentAnimationId].frames[currentFrame].flags = flags;


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Motion", GUILayout.Width(48f));
                    showMotion = EditorGUILayout.Toggle("Show", showMotion);
                    EditorGUILayout.EndHorizontal();
                    var motionX = EditorGUILayout.CurveField("Forward", ac[currentAnimationId].frames[currentFrame].motion.motionX);
                    var motionY = EditorGUILayout.CurveField("Up", ac[currentAnimationId].frames[currentFrame].motion.motionY);

                    if (controlKey)
                    {
                        for (int i = 0; i < c; i++)
                            ac[currentAnimationId].frames[i].motion.motionX = motionX;
                    } else
                        ac[currentAnimationId].frames[currentFrame].motion.motionX = motionX;

                    if (controlKey)
                    {
                        for (int i = 0; i < c; i++)
                            ac[currentAnimationId].frames[i].motion.motionY = motionY;
                    }
                    else
                        ac[currentAnimationId].frames[currentFrame].motion.motionY = motionY;

                    float motionMultiplier = EditorGUILayout.FloatField("Multiplier", ac[currentAnimationId].frames[currentFrame].motion.motionMultiplier);
                    if (controlKey)
                    {
                        for (int i = 0; i < c; i++)
                            ac[currentAnimationId].frames[i].motion.motionMultiplier = motionMultiplier;
                    }
                    else
                        ac[currentAnimationId].frames[currentFrame].motion.motionMultiplier = motionMultiplier;


                    EditorGUILayout.EndVertical();


                }
                else
                    Debug.LogError("NO FRAMES");


            }

            EditorGUIUtility.labelWidth = labelWidth;

        }


        public void Init(AnimationCollection ac)
        {
            this.ac = ac;
            UpdateAnimations();
            lastEditorTime = EditorApplication.timeSinceStartup;
        }

        void UpdateAnimations()
        {
            int c = ac.Count;
            animationNames = new string[c];
            for (int i = 0; i < c; i++)
            {
                if (ac[i] == null)
                    animationNames[i] = "null";
                else
                    animationNames[i] = ac[i].name;
            }
        }
    }
}
