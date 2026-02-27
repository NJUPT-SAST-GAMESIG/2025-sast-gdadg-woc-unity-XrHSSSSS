using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXControl : MonoBehaviour
{
    [System.Serializable]
    private class VFXEventEntry
    {
        public string eventName;

        [Header("VFX")]
        public GameObject vfxPrefab;
        public Transform spawnPoint;
        public bool followSpawnPoint;
        [Min(0f)] public float autoDestroyDelay = 5f;  // 自动销毁时间

        [Header("SFX")]
        public AudioClip sfxClip;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        [Header("Voice")]
        public AudioClip voiceClip;
        [Range(0f, 1f)] public float voiceVolume = 1f;
    }

    [Header("VFX Event Map")]
    [SerializeField] private VFXEventEntry[] vfxEventEntries;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource voiceAudioSource;

    [SerializeField] private bool enableLog = false;

    private readonly Dictionary<string, VFXEventEntry> _vfxEventMap = new Dictionary<string, VFXEventEntry>();

    private void Awake()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }

        if (voiceAudioSource == null)
        {
            voiceAudioSource = sfxAudioSource;
        }

        RebuildEventMap();
    }

    public void PlayVFX(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            if (enableLog)
            {
                Debug.LogWarning("[VFXControl] Event name is empty.", this);
            }

            return;
        }

        if (!_vfxEventMap.TryGetValue(eventName, out VFXEventEntry entry))
        {
            if (enableLog)
            {
                Debug.LogWarning($"[VFXControl] Event not found: {eventName}", this);
            }

            return;
        }

        SpawnVFX(entry);
    }

    public void PlayVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null)
        {
            return;
        }

        Transform spawn = transform;
        GameObject instance = Instantiate(vfxPrefab, spawn.position, spawn.rotation);
        Destroy(instance, 5f);
    }

    [ContextMenu("Rebuild Event Map")]
    private void RebuildEventMap()
    {
        _vfxEventMap.Clear();

        if (vfxEventEntries == null)
        {
            return;
        }

        for (int i = 0; i < vfxEventEntries.Length; i++)
        {
            VFXEventEntry entry = vfxEventEntries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.eventName))
            {
                continue;
            }

            _vfxEventMap[entry.eventName] = entry;
        }
    }

    private void SpawnVFX(VFXEventEntry entry)
    {
        Transform spawn = entry.spawnPoint != null ? entry.spawnPoint : transform;

        if (entry.vfxPrefab != null)
        {
            GameObject instance = Instantiate(entry.vfxPrefab, spawn.position, spawn.rotation);

            if (entry.followSpawnPoint && spawn != null)
            {
                instance.transform.SetParent(spawn, true);
            }

            if (entry.autoDestroyDelay > 0f)
            {
                Destroy(instance, entry.autoDestroyDelay);
            }
        }
        else if (enableLog)
        {
            Debug.LogWarning($"[VFXControl] Prefab missing for event: {entry.eventName}", this);
        }

        PlayEventAudio(entry, spawn.position);

        if (enableLog)
        {
            Debug.Log($"[VFXControl] Play event: {entry.eventName}", this);
        }
    }

    private void PlayEventAudio(VFXEventEntry entry, Vector3 playPosition)
    {
        if (entry.sfxClip != null)
        {
            PlayClip(entry.sfxClip, entry.sfxVolume, sfxAudioSource, playPosition);
        }

        if (entry.voiceClip != null)
        {
            PlayClip(entry.voiceClip, entry.voiceVolume, voiceAudioSource, playPosition);
        }
    }

    private void PlayClip(AudioClip clip, float volume, AudioSource source, Vector3 playPosition)
    {
        if (clip == null)
        {
            return;
        }

        if (source != null)
        {
            source.PlayOneShot(clip, volume);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, playPosition, volume);
    }

}
