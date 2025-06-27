using UnityEngine;

/// <summary>
/// 音效包 ScriptableObject
/// 存储不同类型事件对应的音效文件
/// </summary>
[CreateAssetMenu(fileName = "New Sound Pack", menuName = "Chart System/Sound Pack")]
public class SoundPack : ScriptableObject
{
    [Header("音效包信息")]
    public string packName = "新音效包";
    [TextArea(2, 4)]
    public string description = "音效包描述";
    
    [Header("事件音效")]
    public AudioClip tapSound;      // 轻拍音效
    public AudioClip heavySound;    // 重拍音效
    public AudioClip holdSound;     // 长按音效
    
    [Header("音效设置")]
    [Range(0f, 1f)]
    public float defaultVolume = 0.8f;      // 默认音量
    
    /// <summary>
    /// 根据事件类型获取对应音效
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>对应的音效，如果没有则返回null</returns>
    public AudioClip GetSoundForEvent(BeatEventType eventType)
    {
        return eventType switch
        {
            BeatEventType.Tap => tapSound,
            BeatEventType.Heavy => heavySound,
            BeatEventType.Hold => holdSound,
            _ => null
        };
    }
    
    /// <summary>
    /// 检查音效包是否完整
    /// </summary>
    /// <returns>是否所有音效都已配置</returns>
    public bool IsComplete()
    {
        return tapSound != null && heavySound != null && holdSound != null;
    }
    
    /// <summary>
    /// 获取缺失的音效类型
    /// </summary>
    /// <returns>缺失的音效类型列表</returns>
    public string[] GetMissingSounds()
    {
        var missing = new System.Collections.Generic.List<string>();
        
        if (tapSound == null) missing.Add("轻拍音效");
        if (heavySound == null) missing.Add("重拍音效");
        if (holdSound == null) missing.Add("长按音效");
        
        return missing.ToArray();
    }
    
    /// <summary>
    /// 验证音效包状态
    /// </summary>
    public void ValidateSoundPack()
    {
        if (IsComplete())
        {
            Debug.Log($"[SoundPack] '{packName}' 音效包验证通过");
        }
        else
        {
            string[] missing = GetMissingSounds();
            Debug.LogWarning($"[SoundPack] '{packName}' 缺少音效: {string.Join(", ", missing)}");
        }
    }
    
    // Unity编辑器验证
    void OnValidate()
    {
        // 确保音量在合理范围内
        defaultVolume = Mathf.Clamp01(defaultVolume);
        
        // 如果包名为空，使用文件名
        if (string.IsNullOrEmpty(packName))
        {
            packName = name;
        }
    }
}
