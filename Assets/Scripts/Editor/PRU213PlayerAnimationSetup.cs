using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class PRU213PlayerAnimationSetup : EditorWindow
{
    [MenuItem("Tools/PRU213 Setup/🎭 Setup Player Knight Animations", priority = 14)]
    public static void SetupPlayerKnightAnimations()
    {
        Debug.Log("=== PRU213 Setup: Creating and Configuring Player Knight Animations ===");

        // 1. Create Animations directory if missing
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        // 2. Load Sprites from Art/Sprites/GameAssets/KnightAnimation/
        string knightPath = "Assets/Art/Sprites/GameAssets/KnightAnimation";
        Sprite[] idleSprites = new Sprite[4];
        Sprite[] runSprites = new Sprite[4];
        Sprite hitSprite = null;

        for (int i = 0; i < 4; i++)
        {
            idleSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{knightPath}/knight_m_idle_anim_f{i}.png");
            runSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{knightPath}/knight_m_run_anim_f{i}.png");
        }
        hitSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{knightPath}/knight_m_hit_anim_f0.png");

        if (idleSprites[0] == null || runSprites[0] == null || hitSprite == null)
        {
            Debug.LogError($"❌ Knight sprites could not be loaded at {knightPath}! Please verify paths.");
            return;
        }

        // 3. Create Animation Clips
        AnimationClip idleClip = CreateSpriteAnimationClip("PlayerIdle", idleSprites, 6, true);
        AnimationClip runClip = CreateSpriteAnimationClip("PlayerRun", runSprites, 10, true);
        AnimationClip hitClip = CreateSpriteAnimationClip("PlayerHit", new Sprite[] { hitSprite }, 5, false);

        // 4. Create Animator Controller
        string controllerPath = "Assets/Animations/PlayerAnimator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        // Create BlendTree in Locomotion State
        BlendTree blendTree;
        AnimatorState locomotionState = controller.CreateBlendTreeInController("Locomotion", out blendTree, 0);
        blendTree.blendType = BlendTreeType.Simple1D;
        blendTree.blendParameter = "Speed";
        blendTree.useAutomaticThresholds = false;

        // Link clips to thresholds (0f for Idle, 0.1f or higher for Run)
        blendTree.AddChild(idleClip, 0f);
        blendTree.AddChild(runClip, 0.1f);

        AnimatorState hitState = stateMachine.AddState("Hit");
        hitState.motion = hitClip;

        // Any State -> Hit
        var anyToHit = stateMachine.AddAnyStateTransition(hitState);
        anyToHit.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        anyToHit.hasExitTime = false;

        // Hit -> Locomotion
        var hitToLoco = hitState.AddTransition(locomotionState);
        hitToLoco.hasExitTime = true;
        hitToLoco.exitTime = 1.0f; // Exits at 100% of hit duration

        Debug.Log("  ✓ AnimatorController state machine with 1D BlendTree configured successfully");

        // 5. Apply to Player in Scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // Set Default Sprite to Hiệp sĩ mới
            SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = idleSprites[0];
                sr.color = Color.white; // Reset coloring
            }

            Animator animator = playerObj.GetComponent<Animator>();
            if (animator == null)
            {
                animator = playerObj.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = controller;

            // Ensure Player Prefab is updated
            if (PrefabUtility.IsPartOfPrefabInstance(playerObj))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(playerObj);
                PrefabUtility.ApplyPrefabInstance(playerObj, InteractionMode.UserAction);
            }

            Debug.Log("  ✓ Animator component and Knight sprite linked to Player GameObject in scene!");
        }
        else
        {
            Debug.LogWarning("⚠️ No Player found in active scene to apply Animator component.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("🎭 Knight Animation", "Đã tạo các Clip hoạt ảnh Hiệp sĩ mới và Animator Controller thành công!", "Duyệt");
    }

    private static AnimationClip CreateSpriteAnimationClip(string name, Sprite[] sprites, float frameRate, bool loop)
    {
        string path = $"Assets/Animations/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.frameRate = frameRate;

        // Setup loop settings
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        // Build keyframes
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
        float timeDelta = 1f / frameRate;

        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * timeDelta,
                value = sprites[i]
            };
        }

        // Add dummy final frame
        keyframes[sprites.Length] = new ObjectReferenceKeyframe
        {
            time = sprites.Length * timeDelta,
            value = sprites[sprites.Length - 1]
        };

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        EditorUtility.SetDirty(clip);

        return clip;
    }
}
