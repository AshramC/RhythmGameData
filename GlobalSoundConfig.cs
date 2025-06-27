using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 全局音效配置 ScriptableObject
/// 管理所有可用的音效包，提供中央化的音效管理
/// </summary>
[CreateAssetMenu(fileName = "GlobalSoundConfig", menuName = "Chart System/Global Sound Config")]
public class GlobalSoundConfig : ScriptableObject
{
    [System.Serializable]
    public class SoundPackEntry
    {
        [Header("音效包配置")]
        public string id = "";                  // 唯一ID，如 "default", "electronic", "classical"
        public string displayName = "";         // 显示名称，如 "默认音效", "电子音效", "古典音效"
        public SoundPack soundPack;            // 音效包引用
        
        [Header("状态信息")]
        public bool isActive = true;           // 是否激活（可用于临时禁用某些音效包）
        
        /// <summary>
        /// 验证条目是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && 
                   !string.IsNullOrEmpty(displayName) && 
                   soundPack != null && 
                   isActive;
        }
        
        /// <summary>
        /// 获取条目状态描述
        /// </summary>
        public string GetStatusDescription()
        {
            if (!isActive) return "已禁用";
            if (soundPack == null) return "音效包引用丢失";
            if (!soundPack.IsComplete()) return $"不完整 (缺少: {string.Join(", ", soundPack.GetMissingSounds())})";
            return "正常";
        }
    }
    
    [Header("可用音效包")]
    public List<SoundPackEntry> soundPacks = new List<SoundPackEntry>();
    
    [Header("默认设置")]
    public string defaultSoundPackId = "default";     // 默认音效包ID
    
    /// <summary>
    /// 根据ID获取音效包
    /// </summary>
    /// <param name="id">音效包ID</param>
    /// <returns>音效包实例</returns>
    /// <exception cref="System.Exception">找不到音效包时抛出异常</exception>
    public SoundPack GetSoundPack(string id)
    {
        var entry = soundPacks.Find(pack => pack.id == id && pack.isActive);
        
        if (entry == null)
        {
            throw new System.Exception($"找不到音效包ID: '{id}' 或该音效包已被禁用");
        }
        
        if (entry.soundPack == null)
        {
            throw new System.Exception($"音效包 '{id}' 的引用丢失");
        }
        
        Debug.Log("找到了音效包"+id);
        return entry.soundPack;
    }
    
    /// <summary>
    /// 安全获取音效包（带默认回退）
    /// </summary>
    /// <param name="id">音效包ID</param>
    /// <returns>音效包实例，失败时返回默认音效包</returns>
    public SoundPack GetSoundPackSafe(string id)
    {
        try
        {
            return GetSoundPack(id);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[GlobalSoundConfig] 获取音效包 '{id}' 失败: {e.Message}，尝试使用默认音效包");
            
            try
            {
                return GetSoundPack(defaultSoundPackId);
            }
            catch (System.Exception defaultError)
            {
                Debug.LogError($"[GlobalSoundConfig] 连默认音效包都获取失败: {defaultError.Message}");
                return null;
            }
        }
    }
    
    /// <summary>
    /// 获取所有可用音效包的ID和显示名称
    /// </summary>
    /// <returns>ID和显示名称的键值对</returns>
    public Dictionary<string, string> GetAvailableSoundPacks()
    {
        return soundPacks
            .Where(entry => entry.IsValid())
            .ToDictionary(entry => entry.id, entry => entry.displayName);
    }
    
    /// <summary>
    /// 添加新的音效包条目
    /// </summary>
    /// <param name="id">唯一ID</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="soundPack">音效包引用</param>
    /// <returns>是否添加成功</returns>
    public bool AddSoundPack(string id, string displayName, SoundPack soundPack)
    {
        // 检查ID是否已存在
        if (soundPacks.Any(entry => entry.id == id))
        {
            Debug.LogError($"[GlobalSoundConfig] 音效包ID '{id}' 已存在");
            return false;
        }
        
        var newEntry = new SoundPackEntry
        {
            id = id,
            displayName = displayName,
            soundPack = soundPack,
            isActive = true
        };
        
        soundPacks.Add(newEntry);
        Debug.Log($"[GlobalSoundConfig] 成功添加音效包: {displayName} (ID: {id})");
        return true;
    }
    
    /// <summary>
    /// 验证所有音效包
    /// </summary>
    public void ValidateAllSoundPacks()
    {
        Debug.Log("[GlobalSoundConfig] 开始验证所有音效包...");
        
        foreach (var entry in soundPacks)
        {
            string status = entry.GetStatusDescription();
            if (status == "正常")
            {
                Debug.Log($"[GlobalSoundConfig] ✓ {entry.displayName} ({entry.id}): {status}");
            }
            else
            {
                Debug.LogWarning($"[GlobalSoundConfig] ⚠ {entry.displayName} ({entry.id}): {status}");
            }
        }
        
        Debug.Log($"[GlobalSoundConfig] 验证完成，共 {soundPacks.Count} 个音效包");
    }
    
    // Unity编辑器验证
    void OnValidate()
    {
        // 确保默认音效包ID存在
        if (!string.IsNullOrEmpty(defaultSoundPackId))
        {
            bool defaultExists = soundPacks.Any(entry => entry.id == defaultSoundPackId);
            if (!defaultExists && soundPacks.Count > 0)
            {
                Debug.LogWarning($"[GlobalSoundConfig] 默认音效包ID '{defaultSoundPackId}' 不存在");
            }
        }
        
        // 检查重复ID
        var duplicateIds = soundPacks
            .GroupBy(entry => entry.id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);
            
        foreach (string duplicateId in duplicateIds)
        {
            Debug.LogError($"[GlobalSoundConfig] 发现重复的音效包ID: '{duplicateId}'");
        }
    }
}
