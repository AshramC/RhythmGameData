using UnityEngine;

/// <summary>
/// 音符状态枚举
/// </summary>
public enum NoteState
{
    Waiting,     // 等待击中
    Hit,         // 已击中 
    Missed,      // 已错过
    Cleanup      // 已清理
}

/// <summary>
/// 音符信息结构
/// 简化版本：只存储计算好的时间结果和状态信息
/// </summary>
[System.Serializable]
public class NoteInfo
{
    [Header("计算好的时间信息")]
    [Tooltip("音符的精确判定时间（一次性计算）")]
    public double judgementTime;
    
    [Tooltip("音符的生成时间（一次性计算）")]
    public double spawnTime;
    
    [Header("事件信息")]
    [Tooltip("音符类型")]
    public BeatEventType eventType;
    
    [Tooltip("原始节拍事件数据")]
    public BeatEvent beatEvent;
    
    [Tooltip("所属的谱面段落")]
    public ChartSection chartSection;
    
    [Header("Hold事件信息")]
    [Tooltip("是否为Hold音符")]
    public bool isHoldNote = false;
    
    [Tooltip("Hold结束时间（仅Hold音符有效）")]
    public double holdEndTime = 0.0;
    
    [Tooltip("Hold持续时间（仅Hold音符有效）")]
    public float holdDuration = 0f;
    
    [Header("运行时状态")]
    [Tooltip("是否已经被处理过（避免重复判定）")]
    public bool isProcessed = false;
    
    [Tooltip("音符当前状态")]
    public NoteState noteState = NoteState.Waiting;
    
    /// <summary>
    /// 构造函数 - 存储计算好的时间
    /// </summary>
    /// <param name="judgementTime">判定时间</param>
    /// <param name="spawnTime">生成时间</param>
    /// <param name="beatEvent">节拍事件</param>
    /// <param name="section">所属段落</param>
    public NoteInfo(double judgementTime, double spawnTime, BeatEvent beatEvent, ChartSection section)
    {
        this.judgementTime = judgementTime;
        this.spawnTime = spawnTime;
        this.beatEvent = beatEvent;
        this.chartSection = section;
        this.eventType = beatEvent.eventType;
        this.isProcessed = false;
        this.noteState = NoteState.Waiting;
    }
    
    /// <summary>
    /// 默认构造函数（用于序列化）
    /// </summary>
    public NoteInfo()
    {
        // 用于序列化
    }
    
    /// <summary>
    /// 获取与输入时间的偏差
    /// </summary>
    /// <param name="inputTime">输入时间</param>
    /// <returns>时间偏差（秒）</returns>
    public float GetTimingError(double inputTime)
    {
        return (float)(inputTime - judgementTime);
    }
    
    /// <summary>
    /// 检查此音符是否适合与给定输入时间进行判定
    /// </summary>
    /// <param name="inputTime">输入时间</param>
    /// <param name="maxWindow">最大判定窗口（秒）</param>
    /// <returns>是否在判定窗口内</returns>
    public bool IsInJudgementWindow(double inputTime, float maxWindow = 0.2f)
    {
        return Mathf.Abs(GetTimingError(inputTime)) <= maxWindow;
    }
    
    /// <summary>
    /// 检查音符是否可以被判定（未处理且在等待状态）
    /// </summary>
    /// <returns>是否可判定</returns>
    public bool CanBeJudged()
    {
        return !isProcessed && noteState == NoteState.Waiting;
    }
    
    /// <summary>
    /// 标记为已击中
    /// </summary>
    public void MarkAsHit()
    {
        isProcessed = true;
        noteState = NoteState.Hit;
    }
    
    /// <summary>
    /// 标记为已错过
    /// </summary>
    public void MarkAsMissed()
    {
        isProcessed = true;
        noteState = NoteState.Missed;
    }
    
    /// <summary>
    /// 标记为已清理
    /// </summary>
    public void MarkAsCleanup()
    {
        noteState = NoteState.Cleanup;
    }
    
    /// <summary>
    /// 重置处理状态（用于重新开始关卡）
    /// </summary>
    public void ResetProcessedState()
    {
        isProcessed = false;
        noteState = NoteState.Waiting;
    }
    
    /// <summary>
    /// 获取音符在段落中的相对时间
    /// </summary>
    /// <returns>相对时间</returns>
    public float GetRelativeTimeInSection()
    {
        return (float)(judgementTime - chartSection.startTime);
    }
    
    /// <summary>
    /// 获取音符的小节和拍子信息
    /// </summary>
    /// <returns>格式化的位置字符串</returns>
    public string GetBeatPosition()
    {
        return $"小节{beatEvent.measure + 1}拍{beatEvent.beat + 1}";
    }
    
    /// <summary>
    /// 获取调试信息
    /// </summary>
    /// <returns>调试字符串</returns>
    public string GetDebugInfo()
    {
        return $"NoteInfo[{eventType}]: 判定时间={judgementTime:F3}, 生成时间={spawnTime:F3}, " +
               $"位置={GetBeatPosition()}, 段落={chartSection.sectionName}";
    }
    
    #region === 音效事件支持 ===
    
    /// <summary>
    /// 判断是否为音效事件
    /// </summary>
    public bool IsSoundEvent => eventType == BeatEventType.SoundEffect;
    
    /// <summary>
    /// 判断是否为判定事件
    /// </summary>
    public bool IsJudgementEvent => eventType != BeatEventType.SoundEffect && eventType != BeatEventType.Animation;
    
    /// <summary>
    /// 获取音效包ID（仅音效事件有效）
    /// </summary>
    public string GetSoundPackId() => beatEvent?.soundPackId ?? "default";
    
    /// <summary>
    /// 获取音效类型（仅音效事件有效）
    /// </summary>
    public BeatEventType GetSoundType() => beatEvent?.soundType ?? BeatEventType.Tap;
    
    #endregion
    
    #region === 动画事件支持 ===
    
    /// <summary>
    /// 判断是否为动画事件
    /// </summary>
    public bool IsAnimationEvent => eventType == BeatEventType.Animation;
    
    /// <summary>
    /// 获取目标对象ID（仅动画事件有效）
    /// </summary>
    public string GetTargetObjectId() => beatEvent?.targetObjectId ?? "";
    
    /// <summary>
    /// 获取动画触发器名称（仅动画事件有效）
    /// </summary>
    public string GetAnimationTrigger() => beatEvent?.animationTrigger ?? "";
    
    /// <summary>
    /// 获取DOTween动画配置（仅动画事件有效）
    /// </summary>
    public DOTweenAnimationData GetDOTweenData() => beatEvent?.dotweenData ?? DOTweenAnimationData.None;
    
    #endregion
    
    /// <summary>
    /// 比较方法，用于排序
    /// </summary>
    /// <param name="other">另一个音符信息</param>
    /// <returns>比较结果</returns>
    public int CompareTo(NoteInfo other)
    {
        if (other == null) return 1;
        return judgementTime.CompareTo(other.judgementTime);
    }
    
    public override string ToString()
    {
        return $"Note[{eventType}]@{judgementTime:F2}s ({GetBeatPosition()})";
    }
    
    /// <summary>
    /// 创建音符信息的深拷贝
    /// </summary>
    /// <returns>新的音符信息实例</returns>
    public NoteInfo Clone()
    {
        return new NoteInfo
        {
            judgementTime = this.judgementTime,
            spawnTime = this.spawnTime,
            eventType = this.eventType,
            beatEvent = this.beatEvent, // BeatEvent是struct，自动深拷贝
            chartSection = this.chartSection, // 引用复制
            isProcessed = this.isProcessed,
            noteState = this.noteState
        };
    }
}
