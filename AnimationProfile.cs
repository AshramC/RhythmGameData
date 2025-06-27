using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 动画配置文件 - 定义判定时触发的动画效果
/// </summary>
[System.Serializable]
public class AnimationProfile
{
    [Header("基本设置")]
    [Tooltip("配置文件名称")]
    public string profileName = "Default Animation Profile";
    
    [Tooltip("是否启用此配置")]
    public bool enabled = true;
    
    [Header("UI动画设置")]
    [Tooltip("UI动画配置列表")]
    public List<UIAnimationConfig> uiAnimations = new List<UIAnimationConfig>();
    
    [Header("时序设置")]
    [Tooltip("动画开始前的延迟时间（秒）")]
    public float delayBeforeAnimation = 0f;
    
    [Tooltip("是否同时播放所有动画")]
    public bool playSimultaneously = true;
    
    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        if (!enabled) return false;
        if (uiAnimations == null || uiAnimations.Count == 0) return false;
        
        foreach (var anim in uiAnimations)
        {
            if (!anim.IsValid()) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取适用于指定事件类型的动画配置
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>适用的动画配置列表</returns>
    public List<UIAnimationConfig> GetApplicableAnimations(BeatEventType eventType)
    {
        if (uiAnimations == null) return new List<UIAnimationConfig>();
        
        var applicable = new List<UIAnimationConfig>();
        foreach (var config in uiAnimations)
        {
            if (config.IsApplicableForEventType(eventType))
            {
                applicable.Add(config);
            }
        }
        
        return applicable;
    }
    
    public override string ToString()
    {
        return $"AnimationProfile[{profileName}]: {(enabled ? "启用" : "禁用")}, 动画数:{uiAnimations?.Count ?? 0}";
    }
}

/// <summary>
/// UI动画配置 - 定义单个UI动画的参数
/// </summary>
[System.Serializable]
public class UIAnimationConfig
{
    [Header("动画设置")]
    [Tooltip("Animator触发器名称")]
    public string animationTrigger = "";
    
    [Tooltip("DOTween动画配置")]
    public DOTweenAnimationData dotweenData = DOTweenAnimationData.None;
    
    [Header("触发条件")]
    [Tooltip("指定哪些音符类型触发此动画（为空表示所有类型）")]
    public List<BeatEventType> triggerOnEventTypes = new List<BeatEventType>();
    
    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        // 至少需要设置Animator触发器或DOTween动画之一
        bool hasAnimatorTrigger = !string.IsNullOrEmpty(animationTrigger);
        bool hasDOTweenAnimation = dotweenData.IsValid();
        
        return hasAnimatorTrigger || hasDOTweenAnimation;
    }
    
    /// <summary>
    /// 检查是否适用于指定的事件类型
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>是否适用</returns>
    public bool IsApplicableForEventType(BeatEventType eventType)
    {
        // 如果没有指定触发条件，则适用于所有事件类型
        if (triggerOnEventTypes == null || triggerOnEventTypes.Count == 0)
        {
            return true;
        }
        
        // 检查是否包含指定的事件类型
        return triggerOnEventTypes.Contains(eventType);
    }
    
    public override string ToString()
    {
        string triggerTypes = triggerOnEventTypes?.Count > 0 ? 
            string.Join(", ", triggerOnEventTypes) : "全部";
        return $"UIAnimationConfig[触发器:{animationTrigger}, DOTween:{dotweenData.IsValid()}, 类型:{triggerTypes}]";
    }
}
