using UnityEngine;
using DG.Tweening;

/// <summary>
/// DOTween动画配置数据
/// </summary>
[System.Serializable]
public struct DOTweenAnimationData
{
    [Header("位移动画")]
    [Tooltip("是否使用位移动画")]
    public bool useMove;
    
    [Tooltip("目标位置（相对于初始位置的偏移）")]
    public Vector2 targetPosition;
    
    [Tooltip("位移动画时长（秒）")]
    public float moveDuration;
    
    [Tooltip("位移动画缓动类型")]
    public Ease moveEase;
    
    /// <summary>
    /// 默认空配置
    /// </summary>
    public static DOTweenAnimationData None => new DOTweenAnimationData
    {
        useMove = false,
        targetPosition = Vector2.zero,
        moveDuration = 0.5f,
        moveEase = Ease.OutQuad
    };
    
    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public bool IsValid()
    {
        return moveDuration > 0f;
    }
    
    /// <summary>
    /// 获取配置信息字符串
    /// </summary>
    public override string ToString()
    {
        if (!useMove)
            return "DOTween: 禁用";
            
        return $"DOTween: 移动到{targetPosition}, 时长{moveDuration:F2}s, 缓动{moveEase}";
    }
    
    /// <summary>
    /// 相等比较操作符
    /// </summary>
    public static bool operator ==(DOTweenAnimationData left, DOTweenAnimationData right)
    {
        return left.useMove == right.useMove &&
               left.targetPosition == right.targetPosition &&
               Mathf.Approximately(left.moveDuration, right.moveDuration) &&
               left.moveEase == right.moveEase;
    }
    
    /// <summary>
    /// 不等比较操作符
    /// </summary>
    public static bool operator !=(DOTweenAnimationData left, DOTweenAnimationData right)
    {
        return !(left == right);
    }
    
    /// <summary>
    /// 重写Equals方法
    /// </summary>
    public override bool Equals(object obj)
    {
        if (obj is DOTweenAnimationData other)
        {
            return this == other;
        }
        return false;
    }
    
    /// <summary>
    /// 重写GetHashCode方法
    /// </summary>
    public override int GetHashCode()
    {
        return System.HashCode.Combine(useMove, targetPosition, moveDuration, moveEase);
    }
    
    /// <summary>
    /// 创建简单移动动画配置
    /// </summary>
    public static DOTweenAnimationData CreateSimpleMove(Vector2 targetPos, float duration = 0.5f)
    {
        return new DOTweenAnimationData
        {
            useMove = true,
            targetPosition = targetPos,
            moveDuration = duration,
            moveEase = Ease.OutQuad
        };
    }
    
    /// <summary>
    /// 创建弹跳效果动画配置
    /// </summary>
    public static DOTweenAnimationData CreateBounceMove(Vector2 targetPos, float duration = 0.8f)
    {
        return new DOTweenAnimationData
        {
            useMove = true,
            targetPosition = targetPos,
            moveDuration = duration,
            moveEase = Ease.OutBounce
        };
    }
}
