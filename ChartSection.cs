using UnityEngine;

/// <summary>
/// 谱面段落数据结构 - 重构后的纯数据类
/// 移除了依赖LevelData计算的方法，计算逻辑迁移到LevelManager
/// </summary>
[System.Serializable]
public class ChartSection
{
    [Header("时间设置")]
    [Tooltip("此谱面段落在歌曲中的起始时间（秒）")]
    public float startTime = 0f;
    
    [Header("谱面数据")]
    [Tooltip("此段落使用的谱面数据")]
    public ChartData chartData;
    
    [Header("段落信息")]
    [Tooltip("段落名称（用于编辑器显示）")]
    public string sectionName = "新段落";
    
    [Tooltip("段落描述")]
    [TextArea(2, 3)]
    public string description = "";
    
    /// <summary>
    /// 检查指定时间是否可能在此段落范围内（简单检查，不进行复杂计算）
    /// 注意：精确的范围检查应该由LevelManager进行
    /// </summary>
    /// <param name="time">检查的时间点</param>
    /// <returns>是否可能在范围内</returns>
    public bool MightContainTime(float time)
    {
        // 只做简单的开始时间检查，结束时间由LevelManager计算
        return time >= startTime;
    }
    
    /// <summary>
    /// 验证段落数据的基本完整性（不包含复杂计算）
    /// </summary>
    /// <returns>验证结果和错误信息</returns>
    public (bool isValid, string errorMessage) ValidateBasicData()
    {
        if (startTime < 0)
            return (false, "起始时间不能为负数");
            
        if (chartData == null)
            return (false, "谱面数据不能为空");
            
        // 基本的谱面数据验证（不进行时间计算）
        var (chartValid, chartErrors) = chartData.ValidateChart();
        if (!chartValid)
            return (false, $"谱面数据无效: {string.Join(", ", chartErrors)}");
            
        return (true, "");
    }
    
    /// <summary>
    /// 创建段落的深拷贝
    /// </summary>
    /// <returns>新的段落实例</returns>
    public ChartSection Clone()
    {
        var clone = new ChartSection
        {
            startTime = this.startTime,
            chartData = this.chartData, // 引用复制
            sectionName = this.sectionName,
            description = this.description
        };
        return clone;
    }
    
    /// <summary>
    /// 获取段落的基本信息字符串
    /// </summary>
    public override string ToString()
    {
        string chartInfo = chartData != null ? $"BPM:{chartData.bpm}" : "无谱面";
        return $"ChartSection[{sectionName}]: {startTime:F1}s开始, {chartInfo}";
    }
}