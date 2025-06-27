using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 谱面段落数据资产 - 用于Timeline编辑器
/// 存储段落的基本参数和事件列表
/// </summary>
[CreateAssetMenu(fileName = "New Chart Section", menuName = "Chart System/Chart Section Asset")]
public class ChartSectionAsset : ScriptableObject
{
    [Header("段落基本信息")]
    public string sectionName = "New Section";
    
    [Header("谱面参数")]
    [Range(60, 300)]
    public int bpm = 120;
    
    [Range(1, 16)]
    public int measures = 8;
    
    [Range(4, 16)]
    public int beatsPerMeasure = 8;
    
    [Header("事件数据")]
    public List<BeatEvent> events = new List<BeatEvent>();
    
    [Header("复合事件数据")]
    public List<CompositeEvent> compositeEvents = new List<CompositeEvent>();
    
    [Header("显示设置")]
    public Color displayColor = Color.cyan;
    
    [Header("段落默认音效设置")]
    [Tooltip("段落默认音效包引用")]
    public SoundPack defaultSoundPack;
    
    [Tooltip("段落默认音效类型（从音效包中选择播放哪种音效）")]
    public BeatEventType defaultSoundType = BeatEventType.Tap;
    
    [Header("编辑器设置")]
    [Tooltip("编辑器中的音乐起始偏移时间（秒）")]
    public float editorMusicOffset = 0f;
    
    /// <summary>
    /// 计算段落持续时间
    /// </summary>
    public float GetDuration()
    {
        float beatDuration = 60f / bpm;
        return beatDuration * beatsPerMeasure * measures;
    }
    
    /// <summary>
    /// 获取总拍数
    /// </summary>
    public int GetTotalBeats()
    {
        return measures * beatsPerMeasure;
    }
    
    /// <summary>
    /// 添加事件到指定位置
    /// </summary>
    public void AddEvent(int measure, int beat, BeatEventType eventType)
    {
        // 检查位置是否有效
        if (measure < 0 || measure >= measures || beat < 0 || beat >= beatsPerMeasure)
        {
            Debug.LogWarning($"[ChartSectionAsset] 事件位置超出范围: M{measure + 1}B{beat + 1}");
            return;
        }
        
        // 检查是否已有事件在此位置
        var existing = events.Find(e => e.measure == measure && e.beat == beat);
        if (existing != null)
        {
            // 如果类型相同，删除事件；如果不同，替换类型
            if (existing.eventType == eventType)
            {
                events.Remove(existing);
                Debug.Log($"[ChartSectionAsset] 删除事件: M{measure + 1}B{beat + 1} {eventType}");
            }
            else
            {
                existing.eventType = eventType;
                Debug.Log($"[ChartSectionAsset] 替换事件: M{measure + 1}B{beat + 1} → {eventType}");
            }
        }
        else
        {
            // 添加新事件
            var newEvent = new BeatEvent(measure, beat, eventType);
            
            // 如果是SoundEffect事件，自动应用默认音效设置
            if (eventType == BeatEventType.SoundEffect)
            {
                ApplyDefaultSoundSettings(newEvent);
            }
            
            events.Add(newEvent);
            Debug.Log($"[ChartSectionAsset] 添加事件: M{measure + 1}B{beat + 1} {eventType}");
        }
        
        // 按位置排序
        events.Sort((a, b) => {
            int measureCompare = a.measure.CompareTo(b.measure);
            return measureCompare != 0 ? measureCompare : a.beat.CompareTo(b.beat);
        });
        
        // 标记为已修改
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 删除指定位置的事件
    /// </summary>
    public void RemoveEvent(int measure, int beat)
    {
        int removedCount = events.RemoveAll(e => e.measure == measure && e.beat == beat);
        if (removedCount > 0)
        {
            Debug.Log($"[ChartSectionAsset] 删除事件: M{measure + 1}B{beat + 1}");
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
    
    /// <summary>
    /// 获取指定位置的事件
    /// </summary>
    public BeatEvent GetEventAt(int measure, int beat)
    {
        return events.Find(e => e.measure == measure && e.beat == beat);
    }
    
    /// <summary>
    /// 获取指定位置的复合事件
    /// </summary>
    public CompositeEvent GetCompositeEventAt(int measure, int beat)
    {
        return compositeEvents?.Find(e => e.measure == measure && e.beat == beat);
    }
    
    /// <summary>
    /// 添加复合事件
    /// </summary>
    public void AddCompositeEvent(CompositeEvent compositeEvent)
    {
        if (compositeEvent == null)
        {
            Debug.LogWarning("[ChartSectionAsset] 尝试添加空的复合事件");
            return;
        }
        
        // 检查是否已存在相同位置的事件
        var existingEvent = GetEventAt(compositeEvent.measure, compositeEvent.beat);
        var existingComposite = GetCompositeEventAt(compositeEvent.measure, compositeEvent.beat);
        
        if (existingEvent != null || existingComposite != null)
        {
            Debug.LogWarning($"[ChartSectionAsset] 位置 {compositeEvent.GetPositionString()} 已存在事件");
            return;
        }
        
        // 确保复合事件列表存在
        if (compositeEvents == null)
        {
            compositeEvents = new List<CompositeEvent>();
        }
        
        compositeEvents.Add(compositeEvent);
        
        // 按位置排序
        compositeEvents.Sort((a, b) => {
            int measureCompare = a.measure.CompareTo(b.measure);
            return measureCompare != 0 ? measureCompare : a.beat.CompareTo(b.beat);
        });
        
        Debug.Log($"[ChartSectionAsset] 添加复合事件: {compositeEvent.GetPositionString()}");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 移除指定位置的复合事件
    /// </summary>
    public bool RemoveCompositeEventAt(int measure, int beat)
    {
        if (compositeEvents == null) return false;
        
        var compositeEvent = GetCompositeEventAt(measure, beat);
        if (compositeEvent != null)
        {
            compositeEvents.Remove(compositeEvent);
            Debug.Log($"[ChartSectionAsset] 移除复合事件: {compositeEvent.GetPositionString()}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 清空所有事件
    /// </summary>
    public void ClearAllEvents()
    {
        int eventCount = events.Count;
        int compositeEventCount = compositeEvents?.Count ?? 0;
        
        events.Clear();
        
        if (compositeEvents != null)
        {
            compositeEvents.Clear();
        }
        
        Debug.Log($"[ChartSectionAsset] 清空所有事件，共删除 {eventCount} 个单一事件和 {compositeEventCount} 个复合事件");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 应用默认音效设置到指定音效事件
    /// </summary>
    /// <param name="soundEvent">音效事件</param>
    private void ApplyDefaultSoundSettings(BeatEvent soundEvent)
    {
        if (soundEvent.eventType != BeatEventType.SoundEffect) return;
        
        if (defaultSoundPack != null)
        {
            soundEvent.soundPackId = defaultSoundPack.name; // 使用音效包名作为ID
            soundEvent.soundType = defaultSoundType;
        }
        else
        {
            // 如果没有设置默认音效包，使用默认值
            soundEvent.soundPackId = "default";
            soundEvent.soundType = defaultSoundType;
        }
    }
    
    /// <summary>
    /// 批量应用默认设置到所有音效事件
    /// </summary>
    public void ApplyDefaultSoundToAllEvents()
    {
        int updatedCount = 0;
        foreach (var evt in events)
        {
            if (evt.eventType == BeatEventType.SoundEffect)
            {
                ApplyDefaultSoundSettings(evt);
                updatedCount++;
            }
        }
        
        Debug.Log($"[ChartSectionAsset] 已将默认音效设置应用到 {updatedCount} 个音效事件");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 验证段落数据
    /// </summary>
    public (bool isValid, string errorMessage) ValidateSection()
    {
        if (string.IsNullOrEmpty(sectionName))
            return (false, "段落名称不能为空");
            
        if (bpm <= 0)
            return (false, "BPM必须大于0");
            
        if (measures <= 0)
            return (false, "小节数必须大于0");
            
        if (beatsPerMeasure <= 0)
            return (false, "每小节拍数必须大于0");
        
        // 验证事件是否在有效范围内
        foreach (var evt in events)
        {
            if (evt.measure < 0 || evt.measure >= measures)
                return (false, $"事件 {evt} 的小节超出范围");
                
            if (evt.beat < 0 || evt.beat >= beatsPerMeasure)
                return (false, $"事件 {evt} 的拍子超出范围");
        }
        
        // 验证复合事件
        if (compositeEvents != null)
        {
            foreach (var compositeEvent in compositeEvents)
            {
                var (isValidComposite, compositeErrors) = compositeEvent.Validate();
                if (!isValidComposite)
                {
                    return (false, $"复合事件 {compositeEvent.GetPositionString()}: {string.Join(", ", compositeErrors)}");
                }
                
                if (compositeEvent.measure < 0 || compositeEvent.measure >= measures)
                    return (false, $"复合事件 {compositeEvent.GetPositionString()} 的小节超出范围");
                    
                if (compositeEvent.beat < 0 || compositeEvent.beat >= beatsPerMeasure)
                    return (false, $"复合事件 {compositeEvent.GetPositionString()} 的拍子超出范围");
            }
        }
        
        return (true, "");
    }
    
    /// <summary>
    /// 获取段落统计信息
    /// </summary>
    public string GetStatistics()
    {
        var tapCount = events.FindAll(e => e.eventType == BeatEventType.Tap).Count;
        var heavyCount = events.FindAll(e => e.eventType == BeatEventType.Heavy).Count;
        var holdCount = events.FindAll(e => e.eventType == BeatEventType.Hold).Count;
        var soundCount = events.FindAll(e => e.eventType == BeatEventType.SoundEffect).Count;
        var animationCount = events.FindAll(e => e.eventType == BeatEventType.Animation).Count;
        
        var result = $"♪{bpm} {measures}×{beatsPerMeasure} ({events.Count}事件: {tapCount}●{heavyCount}◆{holdCount}━";
        
        if (soundCount > 0) result += $"{soundCount}♫";
        if (animationCount > 0) result += $"{animationCount}★";
        
        result += ")";
        
        // 添加复合事件统计
        int compositeEventCount = compositeEvents?.Count ?? 0;
        if (compositeEventCount > 0)
        {
            result += $" 复合:{compositeEventCount}";
        }
        
        return result;
    }
    
    /// <summary>
    /// 复制数据到ChartData
    /// </summary>
    public ChartData ToChartData()
    {
        var chartData = CreateInstance<ChartData>();
        chartData.chartName = sectionName;
        chartData.bpm = bpm;
        chartData.measures = measures;
        chartData.beatsPerMeasure = beatsPerMeasure;
        chartData.events = new List<BeatEvent>(events);
        
        // 复制复合事件
        if (compositeEvents != null && compositeEvents.Count > 0)
        {
            chartData.compositeEvents = new List<CompositeEvent>();
            foreach (var compositeEvent in compositeEvents)
            {
                var newCompositeEvent = new CompositeEvent(compositeEvent.measure, compositeEvent.beat);
                newCompositeEvent.eventId = compositeEvent.eventId;
                
                foreach (var subEvent in compositeEvent.subEvents)
                {
                    var newSubEvent = new BeatEvent
                    {
                        measure = subEvent.measure,
                        beat = subEvent.beat,
                        eventType = subEvent.eventType,
                        holdEndBeat = subEvent.holdEndBeat,
                        soundPackId = subEvent.soundPackId,
                        soundType = subEvent.soundType,
                        targetObjectId = subEvent.targetObjectId,
                        animationTrigger = subEvent.animationTrigger,
                        dotweenData = subEvent.dotweenData
                    };
                    newCompositeEvent.AddSubEvent(newSubEvent);
                }
                
                chartData.compositeEvents.Add(newCompositeEvent);
            }
        }
        
        return chartData;
    }
    
    /// <summary>
    /// 从ChartData加载数据
    /// </summary>
    public void LoadFromChartData(ChartData chartData)
    {
        if (chartData == null) return;
        
        sectionName = chartData.chartName;
        bpm = chartData.bpm;
        measures = chartData.measures;
        beatsPerMeasure = chartData.beatsPerMeasure;
        events = new List<BeatEvent>(chartData.events);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 设置音乐偏移
    /// </summary>
    public void SetMusicOffset(float offset)
    {
        editorMusicOffset = offset;
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 获取音乐偏移
    /// </summary>
    public float GetMusicOffset()
    {
        return editorMusicOffset;
    }
    
    /// <summary>
    /// 重置音乐偏移
    /// </summary>
    public void ResetMusicOffset()
    {
        editorMusicOffset = 0f;
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    // Unity编辑器验证
    void OnValidate()
    {
        // 确保参数在合理范围内
        bpm = Mathf.Clamp(bpm, 60, 300);
        measures = Mathf.Clamp(measures, 1, 16);
        beatsPerMeasure = Mathf.Clamp(beatsPerMeasure, 4, 16);
        
        // 移除超出范围的事件
        events.RemoveAll(evt => 
            evt.measure < 0 || evt.measure >= measures || 
            evt.beat < 0 || evt.beat >= beatsPerMeasure);
    }
}
