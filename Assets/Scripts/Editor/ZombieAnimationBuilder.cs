using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Công cụ Editor tự động dựng Animation cho Zombie từ các sprite sheet đã cắt sẵn.
/// Kiểu ĐƠN GIẢN: chỉ lấy hàng "nhìn xuống" (top row) của mỗi sheet, lật trái/phải
/// bằng flipX trong EnemyChase.
///
/// Cách dùng: Menu Unity -> Tools > Zombie > Build Animations (Simple)
/// </summary>
public static class ZombieAnimationBuilder
{
    const string SpriteFolder = "Assets/Sprites/Game Assets/Zombies Free ver";
    const string OutputFolder = "Assets/Animations/Zombie";
    const string ControllerPath = "Assets/ZombieAnimator.controller";

    [MenuItem("Tools/Zombie/Build Animations (Simple)")]
    public static void Build()
    {
        // 1) Đảm bảo thư mục output tồn tại
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Zombie");

        // 2) Tạo các Animation Clip (chỉ dùng hàng nhìn-xuống)
        var idle = CreateClip("1Zombie-Idle.png", "Zombie_Idle", 10f, loop: true);
        var run = CreateClip("1Zombie-Run.png", "Zombie_Run", 12f, loop: true);
        var hurt = CreateClip("1Zombie-Hurt.png", "Zombie_Hurt", 12f, loop: false);
        var death = CreateClip("1Zombie-Death1.png", "Zombie_Death", 12f, loop: false);

        if (idle == null || run == null || hurt == null || death == null)
        {
            Debug.LogError("ZombieAnimationBuilder: Không tạo được clip. Kiểm tra lại đường dẫn sprite: " + SpriteFolder);
            return;
        }

        // 3) Nạp controller sẵn có (giữ nguyên GUID để prefab không bị đứt liên kết)
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        // Xoá sạch parameter + state cũ để chạy lại nhiều lần không bị trùng
        while (controller.parameters.Length > 0)
            controller.RemoveParameter(0);

        var sm = controller.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions.ToList())
            sm.RemoveAnyStateTransition(t);
        foreach (var cs in sm.states.ToList())
            sm.RemoveState(cs.state);

        // 4) Parameters
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        // 5) States
        var sIdle = sm.AddState("Idle");
        sIdle.motion = idle;
        var sRun = sm.AddState("Run");
        sRun.motion = run;
        var sHurt = sm.AddState("Hurt");
        sHurt.motion = hurt;
        var sDeath = sm.AddState("Death");
        sDeath.motion = death;
        sm.defaultState = sIdle;

        // 6) Transitions
        // Idle -> Run khi IsMoving = true
        var toRun = sIdle.AddTransition(sRun);
        toRun.hasExitTime = false; toRun.duration = 0f;
        toRun.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

        // Run -> Idle khi IsMoving = false
        var toIdle = sRun.AddTransition(sIdle);
        toIdle.hasExitTime = false; toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

        // AnyState -> Hurt khi có trigger Hurt
        var toHurt = sm.AddAnyStateTransition(sHurt);
        toHurt.hasExitTime = false; toHurt.duration = 0f;
        toHurt.canTransitionToSelf = false;
        toHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");

        // Hurt tự quay về Idle sau khi chạy xong
        var hurtBack = sHurt.AddTransition(sIdle);
        hurtBack.hasExitTime = true; hurtBack.exitTime = 0.9f; hurtBack.duration = 0f;

        // AnyState -> Death khi có trigger Die (Death không thoát -> giữ frame cuối)
        var toDeath = sm.AddAnyStateTransition(sDeath);
        toDeath.hasExitTime = false; toDeath.duration = 0f;
        toDeath.canTransitionToSelf = false;
        toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Die");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=lime>Zombie animations built xong!</color> Controller: " + ControllerPath);
    }

    /// <summary>Tạo 1 AnimationClip từ hàng trên cùng (nhìn xuống) của sprite sheet.</summary>
    static AnimationClip CreateClip(string pngName, string clipName, float fps, bool loop)
    {
        string spritePath = SpriteFolder + "/" + pngName;
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>().ToList();
        if (sprites.Count == 0)
        {
            Debug.LogWarning("Không tìm thấy sprite trong: " + spritePath + " (đã Slice chưa?)");
            return null;
        }

        var frames = GetTopRow(sprites);

        var clip = new AnimationClip { frameRate = fps };
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",              // SpriteRenderer nằm cùng object với Animator
            propertyName = "m_Sprite"
        };

        var keys = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string clipPath = OutputFolder + "/" + clipName + ".anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    /// <summary>
    /// Nhóm sprite theo hàng (dựa trên toạ độ y của rect), lấy hàng CAO NHẤT
    /// (nhìn xuống), rồi sắp xếp trái -> phải theo x = đúng thứ tự frame.
    /// </summary>
    static List<Sprite> GetTopRow(List<Sprite> sprites)
    {
        // Unity: y tính từ đáy ảnh -> hàng trên cùng có y lớn nhất
        float maxY = sprites.Max(s => s.rect.y);
        // Các frame trong cùng hàng lệch y không quá ~16px (bước hàng ~32px)
        var topRow = sprites.Where(s => Mathf.Abs(s.rect.y - maxY) <= 16f)
                            .OrderBy(s => s.rect.x)
                            .ToList();
        return topRow;
    }
}
