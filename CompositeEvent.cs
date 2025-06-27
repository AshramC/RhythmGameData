using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 复合事件类 - 支持在同一时间点包含多个不同类型的事件
/// </summary>
[System.Serializable]
public class CompositeEvent
{
    [Header("位置信息")]
    [Tooltip("小节索引（从0开始）")]
    public int measure;
    
    [Tooltip("拍子索引（从0开始）")]
    public int beat;
    
    [Header("事件标识")]
    [Tooltip("事件唯一标识符")]
    public string eventId;
    
    [Header("子事件列表")]
    [Tooltip("包含的所有子事件")]
    public List<BeatEvent> subEvents = new List<BeatEvent>();
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public CompositeEvent()
    {
        eventId = System.Guid.NewGuid().ToString();
        subEvents = new List<BeatEvent>();
    }
    
    /// <summary>
    /// 指定位置的构造函数
    /// </summary>
    /// <param name="measure">小节索引</param>
    /// <param name="beat">拍子索引</param>
    public CompositeEvent(int measure, int beat)
    {
        this.measure = measure;
        this.beat = beat;
        this.eventId = System.Guid.NewGuid().ToString();
        this.subEvents = new List<BeatEvent>();
    }
    
    /// <summary>
    /// 添加子事件
    /// </summary>
    /// <param name="beatEvent">要添加的节拍事件</param>
    public void AddSubEvent(BeatEvent beatEvent)
    {
        if (beatEvent == null)
        {
            Debug.LogWarning("[CompositeEvent] 尝试添加空的BeatEvent");
            return;
        }
        
        // 确保子事件的位置与复合事件一致
        beatEvent.measure = this.measure;
        beatEvent.beat = this.beat;
        
        subEvents.Add(beatEvent);
        Debug.Log($"[CompositeEvent] 添加子事件: {beatEvent.eventType} 到 {GetPositionString()}");
    }
    
    /// <summary>
    /// 移除指定索引的子事件
    /// </summary>
    /// <param name="index">子事件索引</param>
    public void RemoveSubEvent(int index)
    {
        if (index < 0 || index >= subEvents.Count)
        {
            Debug.LogWarning($"[CompositeEvent] 子事件索引超出范围: {index}");
            return;
        }
        
        var removedEvent = subEvents[index];
        subEvents.RemoveAt(index);
        Debug.Log($"[CompositeEvent] 移除子事件: {removedEvent.eventType} 从 {GetPositionString()}");
    }
    
    /// <summary>
    /// 将复合事件展开为多个NoteInfo对象
    /// </summary>
    /// <param name="sectionStartTime">段落开始时间</param>
    /// <param name="section">所属段落</param>
    /// <returns>展开后的音符信息列表</returns>
    public List<NoteInfo> ExpandToNoteInfos(double sectionStartTime, ChartSection section)
    {
        var notes = new List<NoteInfo>();
        
        if (section?.chartData == null)
        {
            Debug.LogWarning("[CompositeEvent] 段落或谱面数据为空，无法展开");
            return notes;
        }
        
        foreach (var subEvent in subEvents)
        {
            try
            {
                // 计算子事件的时间
                float eventTimeInSection = CalculateEventTime(subEvent, section.chartData);
                double judgementTime = sectionStartTime + eventTimeInSection;
                
                // 获取FixedDropTime，需要从LevelData中获取
                double spawnTime = judgementTime - GetFixedDropTime(section);
                
                var noteInfo = new NoteInfo(judgementTime, spawnTime, subEvent, section);
                
                // 处理Hold事件的特殊情况
                if (subEvent.eventType == BeatEventType.Hold && subEvent.holdEndBeat > subEvent.beat)
                {
                    var endBeatEvent = new BeatEvent(subEvent.measure, subEvent.holdEndBeat, BeatEventType.Tap);
                    float endEventTime = CalculateEventTime(endBeatEvent, section.chartData);
                    double holdEndTime = sectionStartTime + endEventTime;
                    float holdDuration = (float)(holdEndTime - judgementTime);
                    
                    noteInfo.isHoldNote = true;
                    noteInfo.holdEndTime = holdEndTime;
                    noteInfo.holdDuration = holdDuration;
                }
                
                notes.Add(noteInfo);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CompositeEvent] 展开子事件失败: {subEvent.eventType} - {e.Message}");
            }
        }
        
        Debug.Log($"[CompositeEvent] 展开复合事件 {GetPositionString()}: {subEvents.Count} 个子事件 -> {notes.Count} 个音符");
        return notes;
    }
    
    /// <summary>
    /// 计算事件时间
    /// </summary>
    /// <param name="beatEvent">节拍事件</param>
    /// <param name="chartData">谱面数据</param>
    /// <returns>事件时间（秒）</returns>
    private float CalculateEventTime(BeatEvent beatEvent, ChartData chartData)
    {
        if (chartData.bpm <= 0 || chartData.beatsPerMeasure <= 0) return 0f;
        
        float baseBeatDuration = 60f / chartData.bpm;
        float measureDuration = baseBeatDuration * chartData.beatsPerMeasure;
        
        return beatEvent.measure * measureDuration + beatEvent.beat * baseBeatDuration;
    }
    
    /// <summary>
    /// 获取固定下落时间
    /// </summary>
    /// <param name="section">段落</param>
    /// <returns>固定下落时间</returns>
    private double GetFixedDropTime(ChartSection section)
    {
        // 尝试从段落的父级LevelData获取FixedDropTime
        // 这里使用默认值，实际实现时可能需要更复杂的逻辑
        return 2.0; // 默认2秒下落时间
    }
    
    /// <summary>
    /// 检查复合事件是否为空
    /// </summary>
    /// <returns>是否为空</returns>
    public bool IsEmpty()
    {
        return subEvents == null || subEvents.Count == 0;
    }
    
    /// <summary>
    /// 获取位置字符串
    /// </summary>
    /// <returns>位置描述</returns>
    public string GetPositionString()
    {
        return $"小节{measure + 1}拍{beat + 1}";
    }
    
    /// <summary>
    /// 获取事件类型摘要
    /// </summary>
    /// <returns>事件类型摘要字符串</returns>
    public string GetEventTypesSummary()
    {
        if (IsEmpty()) return "空";
        
        var types = subEvents.Select(e => e.eventType.ToString()).Distinct();
        return string.Join(", ", types);
    }
    
    /// <summary>
    /// 验证复合事件的有效性
    /// </summary>
    /// <returns>验证结果和错误信息</returns>
    public (bool isValid, List<string> errors) Validate()
    {
        var errors = new List<string>();
        
        // 检查事件ID
        if (string.IsNullOrEmpty(eventId))
        {
            errors.Add("事件ID不能为空");
        }
        
        // 检查位置
        if (measure < 0)
        {
            errors.Add("小节索引不能为负数");
        }
        
        if (beat < 0)
        {
            errors.Add("拍子索引不能为负数");
        }
        
        // 检查子事件
        if (IsEmpty())
        {
            errors.Add("复合事件不能为空");
        }
        else
        {
            for (int i = 0; i < subEvents.Count; i++)
            {
                var subEvent = subEvents[i];
                
                // 检查子事件位置是否与复合事件一致
                if (subEvent.measure != measure || subEvent.beat != beat)
                {
                    errors.Add($"子事件{i}的位置与复合事件不一致");
                }
            }
        }
        
        bool isValid = errors.Count == 0;
        return (isValid, errors);
    }
    
    /// <summary>
    /// 获取调试信息
    /// </summary>
    /// <returns>调试字符串</returns>
    public string GetDebugInfo()
    {
        return $"CompositeEvent[{eventId}]: {GetPositionString()} - {GetEventTypesSummary()} ({subEvents.Count} 个子事件)";
    }
    
    public override string ToString()
    {
        return $"复合事件@{GetPositionString()}: {GetEventTypesSummary()}";
    }
}
