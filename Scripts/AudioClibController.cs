using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioClipChannel
{
    public List<AudioSource> audioSources;
    private int _currentSourceIndex = -1;
    public int currentSourceIndex
    {
        get
        {
            _currentSourceIndex++;
            if (_currentSourceIndex == audioSources.Count)
                _currentSourceIndex = 0;
            return _currentSourceIndex;
        }
        set { _currentSourceIndex = value; }
    }
    public AudioSource CurrentAudioSource
    { get { return audioSources[currentSourceIndex]; } }
}

public class AudioClipController : MonoBehaviour
{

    Dictionary<AudioClip, AudioClipChannel> audioClipChannels = new Dictionary<AudioClip, AudioClipChannel>();

    public void PlayClip(AudioClip clip, int nSources)
    {
        AudioClipChannel acc = GetOrCreateAudioClipChannel(clip, nSources);
        acc.CurrentAudioSource.Play();
    }

    public void PlayClipDelayed(AudioClip clip, float delay, int nSources)
    {
        AudioClipChannel acc = GetOrCreateAudioClipChannel(clip, nSources);
        acc.CurrentAudioSource.PlayDelayed(delay);
    }

    public void PlayClipScheduled(AudioClip clip, double playTime, int nSources)
    {
        AudioClipChannel acc = GetOrCreateAudioClipChannel(clip, nSources);
        acc.CurrentAudioSource.PlayScheduled(playTime);
    }

    protected AudioClipChannel GetOrCreateAudioClipChannel(AudioClip clip, int nSources)
    {
        AudioClipChannel acc = null;
        if (!audioClipChannels.ContainsKey(clip))
        {
            acc = audioClipChannels[clip] = new AudioClipChannel();
            acc.audioSources = GetNewAudioClipSources(clip, nSources);
        }
        else
            acc = audioClipChannels[clip];
        //Add sources, if current nSources is greater than existing number
        int nSourceDifference = nSources - acc.audioSources.Count;
        if (nSourceDifference > 0)
            acc.audioSources.AddRange(GetNewAudioClipSources(clip, nSourceDifference));
        return audioClipChannels[clip];
    }

    protected List<AudioSource> GetNewAudioClipSources(AudioClip clip, int nSources)
    {
        List<AudioSource> sources = new List<AudioSource>();
        for (int i = 0; i < nSources; i++)
        {
            AudioSource asrc = gameObject.AddComponent<AudioSource>();
            asrc.clip = clip;
            sources.Add(asrc);
        }
        return sources;
    }
}
