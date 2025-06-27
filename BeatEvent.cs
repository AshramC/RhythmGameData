using UnityEngine;

/// <summary>
/// 节拍事件类型枚举
/// </summary>
public enum BeatEventType
{
    None = 0,         // 无事件
    Tap = 1,          // 轻拍 (判定事件)
    Heavy = 2,        // 重拍 (判定事件)
    Hold = 3,         // 长按 (判定事件)
    SoundEffect = 4,  // 音效播放 (非判定事件)
    Animation = 5     // 动画事件 (非判定事件)
}

/// <summary>
/// 节拍事件数据结构
/// 简化版本：只存储位置和类型信息，不包含时间计算逻辑
/// </summary>
[System.Serializable]
public class BeatEvent
{
    [Header("事件位置")]
    public int measure;         // 小节索引（从0开始）
    public int beat;           // 拍子索引（从0开始）
    
    [Header("事件类型")]
    public BeatEventType eventType = BeatEventType.Tap;
    
    [Header("长按专用")]
    public int holdEndBeat = 0;         // Hold结束拍子（同小节内，仅Hold类型有效）
    
    [Header("音效事件专用")]
    [Tooltip("音效包ID（从 GlobalSoundConfig 获取）")]
    public string soundPackId = "default";
    
    [Tooltip("音效类型（使用现有的 BeatEventType 作为音效类型）")]
    public BeatEventType soundType = BeatEventType.Tap;
    
    [Header("动画事件专用")]
    [Tooltip("目标UI对象的唯一ID")]
    public string targetObjectId = "";
    
    [Tooltip("动画触发器名称")]
    public string animationTrigger = "";
    
    [Tooltip("DOTween动画配置")]
    public DOTweenAnimationData dotweenData = DOTweenAnimationData.None;
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public BeatEvent()
    {
        measure = 0;
        beat = 0;
        eventType = BeatEventType.Tap;
        holdEndBeat = 0;
    }
    
    /// <summary>
    /// 普通事件构造函数
    /// </summary>
    public BeatEvent(int measure, int beat, BeatEventType eventType)
    {
        this.measure = measure;
        this.beat = beat;
        this.eventType = eventType;
        this.holdEndBeat = 0;
    }
    
    /// <summary>
    /// Hold事件构造函数（同小节内）
    /// </summary>
    public BeatEvent(int measure, int startBeat, int endBeat)
    {
        this.measure = measure;
        this.beat = startBeat;
        this.eventType = BeatEventType.Hold;
        this.holdEndBeat = endBeat;
    }
    
    /// <summary>
    /// SoundEffect事件构造函数
    /// </summary>
    public BeatEvent(int measure, int beat, string soundPackId, BeatEventType soundType)
    {
        this.measure = measure;
        this.beat = beat;
        this.eventType = BeatEventType.SoundEffect;
        this.holdEndBeat = 0;
        this.soundPackId = soundPackId;
        this.soundType = soundType;
    }
    
    /// <summary>
    /// Animation事件构造函数
    /// </summary>
    public BeatEvent(int measure, int beat, string targetObjectId, string animationTrigger, DOTweenAnimationData dotweenData)
    {
        this.measure = measure;
        this.beat = beat;
        this.eventType = BeatEventType.Animation;
        this.holdEndBeat = 0;
        this.targetObjectId = targetObjectId;
        this.animationTrigger = animationTrigger;
        this.dotweenData = dotweenData;
    }
    
    /// <summary>
    /// 检查两个事件是否在同一位置
    /// </summary>
    public bool IsSamePosition(BeatEvent other)
    {
        return measure == other.measure && beat == other.beat;
    }
    
    /// <summary>
    /// 获取事件的显示字符串
    /// </summary>
    public override string ToString()
    {
        string typeChar = eventType switch
        {
            BeatEventType.Tap => "●",
            BeatEventType.Heavy => "◆", 
            BeatEventType.Hold => "━",
            BeatEventType.SoundEffect => "♫",
            BeatEventType.Animation => "★",
            _ => "○"
        };
        
        if (eventType == BeatEventType.Hold && holdEndBeat > beat)
        {
            return $"小节{measure + 1}拍{beat + 1}~拍{holdEndBeat + 1}: {typeChar} {eventType}";
        }
        else if (eventType == BeatEventType.SoundEffect)
        {
            return $"小节{measure + 1}拍{beat + 1}: {typeChar} {soundPackId}({soundType})";
        }
        else if (eventType == BeatEventType.Animation)
        {
            return $"小节{measure + 1}拍{beat + 1}: {typeChar} {targetObjectId}({animationTrigger})";
        }
        else
        {
            return $"小节{measure + 1}拍{beat + 1}: {typeChar} {eventType}";
        }
    }

    #region === 编辑器专用方法（仅用于ChartDataEditor） ===
    
    /// <summary>
    /// 验证Hold事件的有效性（同小节内）- 编辑器专用
    /// </summary>
    /// <param name="maxMeasures">最大小节数</param>
    /// <param name="beatsPerMeasure">每小节拍数</param>
    /// <returns>验证结果和错误信息</returns>
    public (bool isValid, string errorMessage) ValidateHoldEvent(int maxMeasures, int beatsPerMeasure)
    {
        if (eventType != BeatEventType.Hold)
            return (false, "不是Hold事件");
            
        if (measure < 0 || measure >= maxMeasures)
            return (false, "小节索引超出范围");
            
        if (beat < 0 || beat >= beatsPerMeasure)
            return (false, "拍子索引超出范围");
            
        if (holdEndBeat <= beat)
            return (false, "Hold结束拍子必须大于开始拍子");
            
        if (holdEndBeat >= beatsPerMeasure)
            return (false, "Hold结束拍子超出小节范围");
            
        return (true, "");
    }
    
    /// <summary>
    /// 检查位置是否在Hold范围内（同小节内）- 编辑器专用
    /// </summary>
    /// <param name="measure">检查的小节</param>
    /// <param name="beat">检查的拍子</param>
    /// <returns>是否在范围内</returns>
    public bool IsPositionInHoldRange(int measure, int beat)
    {
        if (eventType != BeatEventType.Hold) return false;
        if (this.measure != measure) return false;
        
        return beat >= this.beat && beat <= holdEndBeat;
    }
    
    /// <summary>
    /// 检查两个事件是否有冲突（同小节内）- 编辑器专用
    /// </summary>
    /// <param name="otherEvent">其他事件</param>
    /// <returns>是否有冲突</returns>
    public bool HasConflictWith(BeatEvent otherEvent)
    {
        if (eventType != BeatEventType.Hold) return false;
        if (measure != otherEvent.measure) return false;
        
        // 检查其他事件是否在Hold范围内
        return IsPositionInHoldRange(otherEvent.measure, otherEvent.beat);
    }
    
    #endregion
}