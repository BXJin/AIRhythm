using System;
using UnityEngine;

namespace ChartModels
{
    [Serializable]
    public class NoteDto
    {
        public int t_ms;
    }

    [Serializable]
    public class AudioInfoDto
    {
        public string file;
        public int audio_offset_ms;
        public int duration_ms;
    }

    [Serializable]
    public class ChartDto
    {
        public string chart_version;
        public string song_id;
        public string difficulty;
        public AudioInfoDto audio;
        public NoteDto[] notes;
    }
}