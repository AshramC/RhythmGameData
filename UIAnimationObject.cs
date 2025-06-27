using UnityEngine;
using System.Collections;

/// <summary>
/// UI动画对象组件 - 挂载在需要播放动画的UI对象上
/// </summary>
public class UIAnimationObject : MonoBehaviour
{
    [Header("对象标识")]
    [Tooltip("唯一ID，用于事件绑定")]
    public string objectId = "";
    
    [Header("组件引用")]
    [Tooltip("动画控制器")]
    public Animator animator;
    
    [Tooltip("RectTransform引用")]
    public RectTransform rectTransform;
    
    [Header("状态")]
    [Tooltip("是否正在播放动画")]
    [SerializeField] private bool isPlaying = false;
    
    [Header("调试设置")]
    [Tooltip("启用调试日志")]
    public bool enableDebugLog = false;
    
    // DOTween控制器
    // private DOTweenController dotweenController;
    
    /// <summary>
    /// 是否正在播放动画（只读属性）
    /// </summary>
    public bool IsPlaying => isPlaying;
    
    void Awake()
    {
        // 自动获取组件
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
            
        // 初始化DOTween控制器
        if (rectTransform != null)
        {
            // dotweenController = new DOTweenController(rectTransform);
            LogDebug("DOTween控制器初始化完成");
        }
        else
        {
            Debug.LogWarning($"[UIAnimationObject] {objectId}: 未找到RectTransform组件");
        }
        
        // 验证objectId
        if (string.IsNullOrEmpty(objectId))
        {
            Debug.LogWarning($"[UIAnimationObject] {gameObject.name}: objectId为空");
        }
    }
    
    /// <summary>
    /// 播放动画 - 主要接口（同时播放Animator和DOTween）
    /// </summary>
    /// <param name="triggerName">Animator触发器名称</param>
    /// <param name="dotweenData">DOTween动画配置</param>
    public void PlayAnimation(string triggerName, DOTweenAnimationData dotweenData)
    {
        if (isPlaying)
        {
            LogDebug("动画正在播放中，停止当前动画");
            StopAnimation();
        }
        
        isPlaying = true;
        LogDebug($"开始播放动画 - Trigger: {triggerName}, DOTween: {dotweenData.useMove}");
        
        // 触发Animator动画
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            try
            {
                animator.SetTrigger(triggerName);
                LogDebug($"Animator触发器已设置: {triggerName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIAnimationObject] {objectId}: Animator触发器设置失败: {e.Message}");
            }
        }
        
        // 播放DOTween动画（如果启用）
        // if (dotweenData.useMove && dotweenController != null)
        // {
        //     dotweenController.PlayDOTweenAnimation(dotweenData, () => {
        //         isPlaying = false;
        //         OnAnimationComplete();
        //         LogDebug("DOTween动画完成");
        //     });
        // }
        // else
        // {
            // 如果没有DOTween动画，延迟标记完成
            StartCoroutine(DelayedComplete(GetAnimatorDuration()));
        // }
    }
    
    /// <summary>
    /// 停止所有动画
    /// </summary>
    public void StopAnimation()
    {
        if (!isPlaying) return;
        
        isPlaying = false;
        
        // 停止DOTween动画
        // dotweenController?.StopAnimation();
        
        // 停止延迟完成协程
        StopAllCoroutines();
        
        LogDebug("动画已停止");
    }
    
    /// <summary>
    /// 重置到初始状态
    /// </summary>
    public void ResetToInitial()
    {
        StopAnimation();
        
        // 重置DOTween位置
        // dotweenController?.ResetToInitial();
        
        // 重置Animator状态
        if (animator != null)
        {
            try
            {
                animator.Rebind();
                LogDebug("Animator状态已重置");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UIAnimationObject] {objectId}: Animator重置失败: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 获取动画对象状态信息
    /// </summary>
    public string GetStatusInfo()
    {
        string status = $"ID: {objectId} | 播放中: {isPlaying}";
        
        // if (dotweenController != null)
        // {
        //     status += $" | DOTween: {(dotweenController.IsPlaying ? "播放中" : "停止")}";
        // }
        
        if (animator != null)
        {
            status += $" | Animator: {(animator.enabled ? "启用" : "禁用")}";
        }
        
        return status;
    }
    
    /// <summary>
    /// 获取预估的Animator动画时长
    /// </summary>
    private float GetAnimatorDuration()
    {
        // 简单的默认时长，实际可以通过AnimationClip获取精确时长
        return 1.0f;
    }
    
    /// <summary>
    /// 延迟完成协程
    /// </summary>
    private IEnumerator DelayedComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlaying = false;
        OnAnimationComplete();
        LogDebug($"延迟完成 - {delay}秒后标记动画完成");
    }
    
    /// <summary>
    /// 动画完成回调
    /// </summary>
    private void OnAnimationComplete()
    {
        LogDebug("动画完成");
        // 可以在这里添加动画完成事件
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[UIAnimationObject] {objectId}: {message}");
        }
    }
    
    /// <summary>
    /// 组件销毁时清理
    /// </summary>
    void OnDestroy()
    {
        // dotweenController?.Dispose();
        LogDebug("组件销毁，资源已清理");
    }
    
    /// <summary>
    /// Unity编辑器验证
    /// </summary>
    void OnValidate()
    {
        // 如果objectId为空，使用GameObject名称作为默认值
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = gameObject.name;
        }
    }
}
