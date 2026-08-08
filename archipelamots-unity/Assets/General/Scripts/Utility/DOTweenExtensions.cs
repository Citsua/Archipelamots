using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[Serializable]
public struct TweenInfo
{
    private static readonly Ease[] ELASTIC_EASINGS = new Ease[] { Ease.InElastic, Ease.InOutElastic, Ease.OutElastic, Ease.InBack, Ease.OutBack, Ease.InOutBack };
    public float duration;
    public Ease ease;
    [ShowIf("@this.IsElastic()")] public float elasticity;

    private bool IsElastic()
    {
        return ELASTIC_EASINGS.Contains(this.ease);
    }
}

[Serializable]
public struct PunchTweenInfo
{
    public Vector3 target;
    public bool relativeToOriginalScale;
    public int vibrato;
    public float elasticity;
    public TweenInfo info;
}

[Serializable]
public struct ShakeTweenInfo
{
    public float strength;
    public int vibrato;
    public float randomness;
    public TweenInfo info;
}

[Serializable]
public struct JumpTweenInfo
{
    public float power;
    public int numJumps;
    public TweenInfo info;
}

public static class DOTweenExtensions
{
    public static Tweener DOTextInt(this Text text, int initialValue, int finalValue, float duration, Func<int, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextInt(this Text text, int initialValue, int finalValue, float duration)
    {
        return DOTextInt(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static Tweener DOTextFloat(this Text text, float initialValue, float finalValue, float duration, Func<float, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextFloat(this Text text, float initialValue, float finalValue, float duration)
    {
        return DOTextFloat(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static Tweener DOTextLong(this Text text, long initialValue, long finalValue, float duration, Func<long, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextLong(this Text text, long initialValue, long finalValue, float duration)
    {
        return DOTextLong(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static Tweener DOTextDouble(this Text text, double initialValue, double finalValue, float duration, Func<double, string> convertor)
    {
        return DOTween.To(
             () => initialValue,
             it => text.text = convertor(it),
             finalValue,
             duration
         );
    }

    public static Tweener DOTextDouble(this Text text, double initialValue, double finalValue, float duration)
    {
        return DOTextDouble(text, initialValue, finalValue, duration, it => it.ToString());
    }

    public static TweenerCore<float, float, FloatOptions> DOFontSizeMax(this TMP_Text text, float finalValue, float duration)
    {
        TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
             () => text.fontSizeMax,
             x => text.fontSizeMax = x,
             finalValue,
             duration
         );

        tweenerCore.SetTarget(text);
        return tweenerCore;
    }

    public static TweenerCore<float, float, FloatOptions> DORange(this Light light, float finalValue, float duration)
    {
        TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
             () => light.range,
             x => light.range = x,
             finalValue,
             duration
         );

        tweenerCore.SetTarget(light);
        return tweenerCore;
    }

    public static TweenerCore<float, float, FloatOptions> DOTimeScale(float finalValue, float duration)
    {
        TweenerCore<float, float, FloatOptions> tweenerCore = DOTween.To(
             () => Time.timeScale,
             x => Time.timeScale = x,
             finalValue,
             duration
         );

        tweenerCore.SetUpdate(true);
        return tweenerCore;
    }

    public static Tweener DOWidthMultiplier(this TrailRenderer trail, float finalValue, float duration)
    {
        return DOTween.To(
             () => trail.widthMultiplier,
             it => trail.widthMultiplier = it,
             finalValue,
             duration
         );
    }

    public static Tweener DOSmoothness(this Vignette vignette, float finalValue, float duration)
    {
        return DOTween.To(
             () => vignette.smoothness.value,
             it => vignette.smoothness.value = it,
             finalValue,
             duration
         );
    }

    public static Tweener DOMove(this CharacterController controller, Vector3 finalValue, float duration)
    {
        return DOTween.To(
             () => controller.transform.position,
             it => controller.Move(it - controller.transform.position),
             finalValue,
             duration
         );
    }

    public static int DOKill(this Vignette vignette, bool complete = false)
    {
        return DOTween.Kill(vignette, complete);
    }

    public static TweenerCore<Color, Color, ColorOptions> DOFade(this Material target, float endValue, float duration)
    {
        TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => target.color, delegate (Color x)
        {
            target.color = x;
        }, endValue, duration);
        tweenerCore.SetTarget(target);
        return tweenerCore;
    }

    public static Tweener DOFade(this DecalProjector decalProjector, float finalValue, float duration)
    {
        return DOTween.To(
             () => decalProjector.fadeFactor,
             it => decalProjector.fadeFactor = it,
             finalValue,
             duration
         );
    }

    public static Tweener DOMove(this Transform target, Vector3 to, TweenInfo info)
    {
        return target.DOMove(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOMoveX(this Transform target, float to, TweenInfo info)
    {
        return target.DOMoveX(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOMoveY(this Transform target, float to, TweenInfo info)
    {
        return target.DOMoveY(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOMoveZ(this Transform target, float to, TweenInfo info)
    {
        return target.DOMoveZ(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOLocalMove(this Transform target, Vector3 to, TweenInfo info)
    {
        return target.DOLocalMove(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOLocalMoveX(this Transform target, float to, TweenInfo info)
    {
        return target.DOLocalMoveX(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOLocalMoveY(this Transform target, float to, TweenInfo info)
    {
        return target.DOLocalMoveY(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOLocalMoveZ(this Transform target, float to, TweenInfo info)
    {
        return target.DOLocalMoveZ(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOScale(this Transform target, Vector3 to, TweenInfo info)
    {
        return target.DOScale(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOScale(this Transform target, float to, TweenInfo info)
    {
        return target.DOScale(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOScaleX(this Transform target, float to, TweenInfo info)
    {
        return target.DOScaleX(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOScaleY(this Transform target, float to, TweenInfo info)
    {
        return target.DOScaleY(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOScaleZ(this Transform target, float to, TweenInfo info)
    {
        return target.DOScaleZ(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOPunchScale(this Transform target, Vector3 punch, TweenInfo info, int vibrato = 10, float elasticity = 1f)
    {
        return target.DOPunchScale(punch, info.duration, vibrato, elasticity).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOPunchScale(this Transform target, PunchTweenInfo punchInfo)
    {
        Vector3 targetScale = punchInfo.target;
        if (punchInfo.relativeToOriginalScale)
            targetScale = Vector3.Scale(targetScale, target.localScale);
        return target.DOPunchScale(targetScale, punchInfo.info.duration, punchInfo.vibrato, punchInfo.elasticity).SetEase(punchInfo.info.ease, punchInfo.info.elasticity);
    }

    public static Tweener DORotate(this Transform target, Vector3 to, TweenInfo info)
    {
        return target.DORotate(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOLocalRotate(this Transform target, Vector3 to, TweenInfo info)
    {
        return target.DOLocalRotate(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DORotateQuaternion(this Transform target, Quaternion to, TweenInfo info)
    {
        return target.DORotateQuaternion(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOShakePosition(this Transform target, ShakeTweenInfo shakeInfo)
    {
        return target.DOShakePosition(shakeInfo.info.duration, shakeInfo.strength, shakeInfo.vibrato, shakeInfo.randomness).SetEase(shakeInfo.info.ease, shakeInfo.info.elasticity);
    }

    public static Tweener DOShakeRotation(this Transform target, ShakeTweenInfo shakeInfo)
    {
        return target.DOShakeRotation(shakeInfo.info.duration, shakeInfo.strength, shakeInfo.vibrato, shakeInfo.randomness).SetEase(shakeInfo.info.ease, shakeInfo.info.elasticity);
    }

    public static Tweener DOShakeScale(this Transform target, ShakeTweenInfo shakeInfo)
    {
        return target.DOShakeScale(shakeInfo.info.duration, shakeInfo.strength, shakeInfo.vibrato, shakeInfo.randomness).SetEase(shakeInfo.info.ease, shakeInfo.info.elasticity);
    }

    public static Sequence DOJump(this Transform target, Vector3 to, JumpTweenInfo jumpInfo)
    {
        return target.DOJump(to, jumpInfo.power, jumpInfo.numJumps, jumpInfo.info.duration).SetEase(jumpInfo.info.ease, jumpInfo.info.elasticity);
    }

    public static Tweener DOLocalRotateQuaternion(this Transform target, Quaternion to, TweenInfo info)
    {
        return target.DOLocalRotateQuaternion(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOSizeDelta(this RectTransform target, Vector2 to, TweenInfo info)
    {
        return target.DOSizeDelta(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOFade(this CanvasGroup target, float to, TweenInfo info)
    {
        return target.DOFade(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOFloat(this Material target, float to, string property, TweenInfo info)
    {
        return target.DOFloat(to, property, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOAnchorPosX(this RectTransform target, float to, TweenInfo info)
    {
        return target.DOAnchorPosX(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOAnchorPosY(this RectTransform target, float to, TweenInfo info)
    {
        return target.DOAnchorPosY(to, info.duration).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOOffsetLeft(this RectTransform target, float to, TweenInfo info)
    {
        return DOTween.To(
            () => target.offsetMin.x,
            it => target.offsetMin = new Vector2(it, target.offsetMin.y),
            to,
            info.duration
        ).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOOffsetBottom(this RectTransform target, float to, TweenInfo info)
    {
        return DOTween.To(
            () => target.offsetMin.y,
            it => target.offsetMin = new Vector2(target.offsetMin.x, it),
            to,
            info.duration
        ).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOOffsetRight(this RectTransform target, float to, TweenInfo info)
    {
        return DOTween.To(
            () => target.offsetMax.x,
            it => target.offsetMax = new Vector2(it, target.offsetMax.y),
            to,
            info.duration
        ).SetEase(info.ease, info.elasticity);
    }

    public static Tweener DOOffsetTop(this RectTransform target, float to, TweenInfo info)
    {
        return DOTween.To(
            () => target.offsetMax.y,
            it => target.offsetMax = new Vector2(target.offsetMax.x, it),
            to,
            info.duration
        ).SetEase(info.ease, info.elasticity);
    }

}