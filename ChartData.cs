using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 谱面数据 ScriptableObject
/// 简化版本：只存储配置数据和事件列表，不包含时间计算逻辑
/// </summary>
[CreateAssetMenu(fileName = "New Chart", menuName = "Chart System/Chart Data")]
public class ChartData : ScriptableObject
{
    [Header("谱面基本参数")]
    public int bpm = 120;                           // 每分钟节拍数
    public int measures = 8;                        // 小节数
    public int beatsPerMeasure = 8;                 // 每小节拍数
    
    [Header("音效配置")]
    public string soundPackId = "default";          // 音效包ID
    
    [Header("谱面信息")]
    public string chartName = "新谱面";
    [TextArea(2, 4)]
    public string description = "谱面描述";
    public int difficulty = 1;                      // 难度等级 1-5
    
    [Header("事件数据")]
    public List<BeatEvent> events = new List<BeatEvent>();
    
    [Header("复合事件数据")]
    public List<CompositeEvent> compositeEvents = new List<CompositeEvent>();
    
    [Header("编辑器状态")]
    public bool isGridGenerated = false;            // 是否已生成网格（锁定拍数参数）
    
    public BeatEvent GetEventAt(int measure, int beat)
    {
        return events.Find(e => e.measure == measure && e.beat == beat);
    }
    
    /// <summary>
    /// 获取指定位置的复合事件
    /// </summary>
    /// <param name="measure">小节索引</param>
    /// <param name="beat">拍子索引</param>
    /// <returns>复合事件，未找到返回null</returns>
    public CompositeEvent GetCompositeEventAt(int measure, int beat)
    {
        return compositeEvents.Find(e => e.measure == measure && e.beat == beat);
    }
    
    /// <summary>
    /// 添加复合事件
    /// </summary>
    /// <param name="compositeEvent">要添加的复合事件</param>
    public void AddCompositeEvent(CompositeEvent compositeEvent)
    {
        if (compositeEvent == null)
        {
            Debug.LogWarning("[ChartData] 尝试添加空的复合事件");
            return;
        }
        
        // 检查是否已存在相同位置的复合事件
        var existing = GetCompositeEventAt(compositeEvent.measure, compositeEvent.beat);
        if (existing != null)
        {
            Debug.LogWarning($"[ChartData] 位置 {compositeEvent.GetPositionString()} 已存在复合事件");
            return;
        }
        
        compositeEvents.Add(compositeEvent);
        Debug.Log($"[ChartData] 添加复合事件: {compositeEvent.GetPositionString()}");
    }
    
    /// <summary>
    /// 移除指定位置的复合事件
    /// </summary>
    /// <param name="measure">小节索引</param>
    /// <param name="beat">拍子索引</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveCompositeEventAt(int measure, int beat)
    {
        var compositeEvent = GetCompositeEventAt(measure, beat);
        if (compositeEvent != null)
        {
            compositeEvents.Remove(compositeEvent);
            Debug.Log($"[ChartData] 移除复合事件: {compositeEvent.GetPositionString()}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 根据ID移除复合事件
    /// </summary>
    /// <param name="eventId">事件ID</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveCompositeEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId)) return false;
        
        var compositeEvent = compositeEvents.Find(e => e.eventId == eventId);
        if (compositeEvent != null)
        {
            compositeEvents.Remove(compositeEvent);
            Debug.Log($"[ChartData] 移除复合事件: {compositeEvent.GetPositionString()}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取指定位置的所有事件（包括单一事件和复合事件的子事件）
    /// </summary>
    /// <param name="measure">小节索引</param>
    /// <param name="beat">拍子索引</param>
    /// <returns>所有事件列表</returns>
    public List<BeatEvent> GetAllEventsAt(int measure, int beat)
    {
        var result = new List<BeatEvent>();
        
        // 添加单一事件
        var singleEvent = GetEventAt(measure, beat);
        if (singleEvent != null)
        {
            result.Add(singleEvent);
        }
        
        // 添加复合事件的子事件
        var compositeEvent = GetCompositeEventAt(measure, beat);
        if (compositeEvent != null && !compositeEvent.IsEmpty())
        {
            result.AddRange(compositeEvent.subEvents);
        }
        
        return result;
    }
    
    /// <summary>
    /// 获取音效包
    /// </summary>
    /// <returns>音效包实例</returns>
    public SoundPack GetSoundPack()
    {
        var globalConfig = Resources.Load<GlobalSoundConfig>("GlobalSoundConfig");
        if (globalConfig == null)
        {
            throw new System.Exception("找不到 GlobalSoundConfig，请确保在 Resources 文件夹中创建了该文件");
        }
        
        return globalConfig.GetSoundPack(soundPackId);
    }
    
    /// <summary>
    /// 安全获取音效包（带回退）
    /// </summary>
    /// <returns>音效包实例</returns>
    public SoundPack GetSoundPackSafe()
    {
        try
        {
            return GetSoundPack();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ChartData] 获取音效包失败: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 验证谱面数据
    /// </summary>
    /// <returns>验证结果和错误信息</returns>
    public (bool isValid, List<string> errors) ValidateChart()
    {
        var errors = new List<string>();
        
        // 验证基本参数
        if (bpm <= 0) errors.Add("BPM 必须大于 0");
        if (measures <= 0) errors.Add("小节数必须大于 0");
        if (beatsPerMeasure <= 0) errors.Add("每小节拍数必须大于 0");
        
        // 验证音效包
        try
        {
            var soundPack = GetSoundPack();
            if (!soundPack.IsComplete())
            {
                errors.Add($"音效包不完整: {string.Join(", ", soundPack.GetMissingSounds())}");
            }
        }
        catch (System.Exception e)
        {
            errors.Add($"音效包错误: {e.Message}");
        }
        
        // 验证事件数据
        foreach (var evt in events)
        {
            if (evt.measure < 0 || evt.measure >= measures)
            {
                errors.Add($"事件 {evt} 的小节索引超出范围");
            }
            
            if (evt.beat < 0 || evt.beat >= beatsPerMeasure)
            {
                errors.Add($"事件 {evt} 的拍子索引超出范围");
            }
        }
        
        // 验证复合事件数据
        if (compositeEvents != null)
        {
            foreach (var compositeEvent in compositeEvents)
            {
                var (isValidComposite, compositeErrors) = compositeEvent.Validate();
                if (!isValidComposite)
                {
                    errors.AddRange(compositeErrors.Select(error => $"复合事件 {compositeEvent.GetPositionString()}: {error}"));
                }
                
                // 检查复合事件的位置范围
                if (compositeEvent.measure < 0 || compositeEvent.measure >= measures)
                {
                    errors.Add($"复合事件 {compositeEvent.GetPositionString()} 的小节索引超出范围");
                }
                
                if (compositeEvent.beat < 0 || compositeEvent.beat >= beatsPerMeasure)
                {
                    errors.Add($"复合事件 {compositeEvent.GetPositionString()} 的拍子索引超出范围");
                }
            }
        }
        
        // 检查重复事件(单一事件之间)
        var duplicates = events
            .GroupBy(evt => new { evt.measure, evt.beat })
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
            
        foreach (var duplicate in duplicates)
        {
            errors.Add($"重复事件: 小节{duplicate.measure + 1}拍{duplicate.beat + 1}");
        }
        
        // 检查复合事件与单一事件的冲突
        if (compositeEvents != null)
        {
            foreach (var compositeEvent in compositeEvents)
            {
                var conflictingSingleEvent = events.Find(evt => evt.measure == compositeEvent.measure && evt.beat == compositeEvent.beat);
                if (conflictingSingleEvent != null)
                {
                    errors.Add($"复合事件 {compositeEvent.GetPositionString()} 与单一事件冲突");
                }
            }
        }
        
        // 检查复合事件之间的重复
        if (compositeEvents != null && compositeEvents.Count > 1)
        {
            var duplicateComposites = compositeEvents
                .GroupBy(evt => new { evt.measure, evt.beat })
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
                
            foreach (var duplicate in duplicateComposites)
            {
                errors.Add($"重复的复合事件: 小节{duplicate.measure + 1}拍{duplicate.beat + 1}");
            }
        }
        
        bool isValid = errors.Count == 0;
        if (isValid)
        {
            Debug.Log($"[ChartData] 谱面验证通过: {chartName}");
        }
        else
        {
            Debug.LogWarning($"[ChartData] 谱面验证失败: {chartName}\n错误: {string.Join("\n", errors)}");
        }
        
        return (isValid, errors);
    }
    
    /// <summary>
    /// 获取统计信息
    /// </summary>
    public void PrintStatistics()
    {
        var tapCount = events.Count(e => e.eventType == BeatEventType.Tap);
        var heavyCount = events.Count(e => e.eventType == BeatEventType.Heavy);
        var holdCount = events.Count(e => e.eventType == BeatEventType.Hold);
        var soundCount = events.Count(e => e.eventType == BeatEventType.SoundEffect);
        var animationCount = events.Count(e => e.eventType == BeatEventType.Animation);
        
        Debug.Log($"[ChartData] 谱面统计 - {chartName}:");
        Debug.Log($"  小节数: {measures}, 每小节拍数: {beatsPerMeasure}, BPM: {bpm}");
        Debug.Log($"  单一事件数: {events.Count} (轻拍:{tapCount}, 重拍:{heavyCount}, 长按:{holdCount}, 音效:{soundCount}, 动画:{animationCount})");
        
        // 复合事件统计
        int compositeEventCount = compositeEvents?.Count ?? 0;
        if (compositeEventCount > 0)
        {
            int totalSubEvents = compositeEvents.Sum(ce => ce.subEvents?.Count ?? 0);
            Debug.Log($"  复合事件数: {compositeEventCount} (包含 {totalSubEvents} 个子事件)");
            
            // 复合事件中的子事件统计
            var compositeTapCount = 0;
            var compositeHeavyCount = 0;
            var compositeHoldCount = 0;
            var compositeSoundCount = 0;
            var compositeAnimationCount = 0;
            
            foreach (var compositeEvent in compositeEvents)
            {
                if (compositeEvent.subEvents != null)
                {
                    compositeTapCount += compositeEvent.subEvents.Count(e => e.eventType == BeatEventType.Tap);
                    compositeHeavyCount += compositeEvent.subEvents.Count(e => e.eventType == BeatEventType.Heavy);
                    compositeHoldCount += compositeEvent.subEvents.Count(e => e.eventType == BeatEventType.Hold);
                    compositeSoundCount += compositeEvent.subEvents.Count(e => e.eventType == BeatEventType.SoundEffect);
                    compositeAnimationCount += compositeEvent.subEvents.Count(e => e.eventType == BeatEventType.Animation);
                }
            }
            
            Debug.Log($"  复合事件分布: 轻拍:{compositeTapCount}, 重拍:{compositeHeavyCount}, 长按:{compositeHoldCount}, 音效:{compositeSoundCount}, 动画:{compositeAnimationCount}");
        }
        
        Debug.Log($"  难度: {difficulty}/5");
    }
    
    /// <summary>
    /// 修改小节数（编辑器用）
    /// </summary>
    /// <param name="newMeasures">新的小节数</param>
    public void ChangeMeasures(int newMeasures)
    {
        if (newMeasures <= 0)
        {
            Debug.LogError("[ChartData] 小节数必须大于0");
            return;
        }
        
        int oldMeasures = measures;
        measures = newMeasures;
        
        // 如果缩小了小节数，需要删除超出范围的事件
        if (newMeasures < oldMeasures)
        {
            int removedCount = events.RemoveAll(evt => evt.measure >= newMeasures);
            if (removedCount > 0)
            {
                Debug.Log($"[ChartData] 缩小小节数，删除了 {removedCount} 个超出范围的事件");
            }
        }
        
        Debug.Log($"[ChartData] 小节数从 {oldMeasures} 修改为 {newMeasures}");
    }
    
    /// <summary>
    /// 复制谱面数据到当前对象
    /// </summary>
    /// <param name="sourceChart">源谱面</param>
    public void CopyFromChart(ChartData sourceChart)
    {
        if (sourceChart == null)
        {
            Debug.LogError("[ChartData] 源谱面为空");
            return;
        }
        
        // 复制基本参数
        bpm = sourceChart.bpm;
        measures = sourceChart.measures;
        beatsPerMeasure = sourceChart.beatsPerMeasure;
        chartName = sourceChart.chartName;
        description = sourceChart.description;
        difficulty = sourceChart.difficulty;
        soundPackId = sourceChart.soundPackId;
        isGridGenerated = sourceChart.isGridGenerated;
        
        // 复制事件列表
        events.Clear();
        foreach (var evt in sourceChart.events)
        {
            var newEvent = new BeatEvent
            {
                measure = evt.measure,
                beat = evt.beat,
                eventType = evt.eventType,
                holdEndBeat = evt.holdEndBeat
            };
            events.Add(newEvent);
        }
        
        // 复制复合事件列表
        compositeEvents.Clear();
        if (sourceChart.compositeEvents != null)
        {
            foreach (var compositeEvt in sourceChart.compositeEvents)
            {
                var newCompositeEvent = new CompositeEvent(compositeEvt.measure, compositeEvt.beat);
                newCompositeEvent.eventId = System.Guid.NewGuid().ToString(); // 生成新的ID
                
                foreach (var subEvt in compositeEvt.subEvents)
                {
                    var newSubEvent = new BeatEvent
                    {
                        measure = subEvt.measure,
                        beat = subEvt.beat,
                        eventType = subEvt.eventType,
                        holdEndBeat = subEvt.holdEndBeat,
                        soundPackId = subEvt.soundPackId,
                        soundType = subEvt.soundType,
                        targetObjectId = subEvt.targetObjectId,
                        animationTrigger = subEvt.animationTrigger,
                        dotweenData = subEvt.dotweenData
                    };
                    newCompositeEvent.AddSubEvent(newSubEvent);
                }
                
                compositeEvents.Add(newCompositeEvent);
            }
        }
        
        Debug.Log($"[ChartData] 已从 {sourceChart.chartName} 复制谱面数据");
    }
    
    // Unity编辑器验证
    void OnValidate()
    {
        // 确保参数在合理范围内
        bpm = Mathf.Clamp(bpm, 60, 300);
        measures = Mathf.Clamp(measures, 1, 32);
        beatsPerMeasure = Mathf.Clamp(beatsPerMeasure, 4, 16);
        difficulty = Mathf.Clamp(difficulty, 1, 5);
        
        // 确保复合事件列表不为空
        if (compositeEvents == null)
        {
            compositeEvents = new List<CompositeEvent>();
        }
        
        // 如果谱面名为空，使用文件名
        if (string.IsNullOrEmpty(chartName))
        {
            chartName = name;
        }
    }
}
