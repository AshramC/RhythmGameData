using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 关卡数据 ScriptableObject - 重构后的纯数据类
/// 只负责数据存储和基本验证，不包含任何计算逻辑
/// </summary>
[CreateAssetMenu(fileName = "New Level", menuName = "Chart System/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("关卡名称")]
    public string levelName = "新关卡";
    
    [Tooltip("关卡描述")]
    [TextArea(3, 5)]
    public string description = "关卡描述";
    
    [Tooltip("难度等级 (1-5)")]
    [Range(1, 5)]
    public int difficulty = 1;
    
    [Tooltip("关卡创作者")]
    public string author = "";
    
    [Header("音乐设置")]
    [Tooltip("背景音乐文件")]
    public AudioClip backgroundMusic;
    
    [Tooltip("音乐音量 (0-1)")]
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;
    
    [Tooltip("音乐开始播放的偏移时间（秒）")]
    public float musicStartOffset = 0f;
    
    [Header("下落设置")]
    [Tooltip("音符固定下落时间（秒）- 设计期常量")]
    public float fixedDropTime = 2.0f;
    
    [Tooltip("判定线Y坐标位置")]
    public float judgementLineY = -4f;
    
    [Tooltip("音符生成Y坐标位置")]
    public float noteSpawnY = 6f;
    
    [Header("谱面段落")]
    [Tooltip("关卡包含的所有谱面段落")]
    public List<ChartSection> chartSections = new List<ChartSection>();
    
    [Header("调试设置")]
    [Tooltip("启用调试日志")]
    public bool enableDebugLog = false;
    
    /// <summary>
    /// 获取固定下落时间（只读属性）
    /// </summary>
    public float FixedDropTime => fixedDropTime;
    
    /// <summary>
    /// 验证关卡数据的基本完整性
    /// 注意：不包含复杂的计算验证，只验证数据结构
    /// </summary>
    /// <returns>验证结果和错误列表</returns>
    public (bool isValid, List<string> errors) ValidateBasicData()
    {
        var errors = new List<string>();
        
        // 验证基本信息
        if (string.IsNullOrEmpty(levelName))
            errors.Add("关卡名称不能为空");
            
        if (backgroundMusic == null)
            errors.Add("背景音乐不能为空");
            
        if (fixedDropTime <= 0)
            errors.Add("下落时间必须大于0");
            
        if (chartSections.Count == 0)
            errors.Add("至少需要一个谱面段落");
        
        // 验证段落数据结构（不进行复杂计算）
        for (int i = 0; i < chartSections.Count; i++)
        {
            var section = chartSections[i];
            if (section == null)
            {
                errors.Add($"段落 {i + 1} 为空");
                continue;
            }
            
            if (section.chartData == null)
            {
                errors.Add($"段落 {i + 1} 缺少谱面数据");
            }
            
            if (section.startTime < 0)
            {
                errors.Add($"段落 {i + 1} 开始时间不能为负数");
            }
        }
        
        // 检查段落时间重叠（简单检查）
        for (int i = 0; i < chartSections.Count; i++)
        {
            for (int j = i + 1; j < chartSections.Count; j++)
            {
                if (chartSections[i].startTime == chartSections[j].startTime)
                {
                    errors.Add($"段落 {i + 1} 和段落 {j + 1} 开始时间重复");
                }
            }
        }
        
        bool isValid = errors.Count == 0;
        return (isValid, errors);
    }
    
    /// <summary>
    /// 获取基本统计信息（不涉及复杂计算）
    /// </summary>
    /// <returns>基本统计信息</returns>
    public BasicLevelStatistics GetBasicStatistics()
    {
        int totalEvents = 0;
        int tapEvents = 0;
        int heavyEvents = 0;
        int holdEvents = 0;
        
        foreach (var section in chartSections)
        {
            if (section?.chartData?.events != null)
            {
                foreach (var evt in section.chartData.events)
                {
                    totalEvents++;
                    switch (evt.eventType)
                    {
                        case BeatEventType.Tap:
                            tapEvents++;
                            break;
                        case BeatEventType.Heavy:
                            heavyEvents++;
                            break;
                        case BeatEventType.Hold:
                            holdEvents++;
                            break;
                    }
                }
            }
        }
        
        return new BasicLevelStatistics
        {
            sectionsCount = chartSections.Count,
            totalEvents = totalEvents,
            tapEvents = tapEvents,
            heavyEvents = heavyEvents,
            holdEvents = holdEvents,
            estimatedDuration = backgroundMusic?.length ?? 0f
        };
    }
    
    // Unity编辑器验证
    void OnValidate()
    {
        // 确保参数在合理范围内
        fixedDropTime = Mathf.Clamp(fixedDropTime, 0.5f, 10f);
        difficulty = Mathf.Clamp(difficulty, 1, 5);
        musicVolume = Mathf.Clamp01(musicVolume);
        
        // 如果关卡名为空，使用文件名
        if (string.IsNullOrEmpty(levelName))
        {
            levelName = name;
        }
    }
}

/// <summary>
/// 基本关卡统计信息结构（不涉及复杂计算）
/// </summary>
[System.Serializable]
public struct BasicLevelStatistics
{
    public int sectionsCount;        // 段落数
    public int totalEvents;          // 总事件数
    public int tapEvents;            // 轻拍数
    public int heavyEvents;          // 重拍数
    public int holdEvents;           // 长按数
    public float estimatedDuration;  // 预估时长（基于音乐文件）
    
    public override string ToString()
    {
        return $"段落:{sectionsCount} | 事件:{totalEvents} | 预估时长:{estimatedDuration:F1}s";
    }
}