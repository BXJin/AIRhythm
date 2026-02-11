using System.Text;

namespace AIRhythmServerApi.Services;

public static class WavDurationReader
{
    public static int GetDurationMs(string wavPath)
    {
        using var fs = File.OpenRead(wavPath);
        using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

        // RIFF header
        var riff = new string(br.ReadChars(4));
        if (riff != "RIFF") throw new InvalidDataException("Not a RIFF file.");

        br.ReadUInt32(); // file size
        var wave = new string(br.ReadChars(4));
        if (wave != "WAVE") throw new InvalidDataException("Not a WAVE file.");

        int? byteRate = null;
        uint? dataSize = null;

        while (fs.Position + 8 <= fs.Length)
        {
            var chunkId = new string(br.ReadChars(4));
            uint chunkSize = br.ReadUInt32();

            long remaining = fs.Length - fs.Position;
            
            // ✅ 비정상 크기 처리: 남은 크기로 제한
            if (chunkSize > remaining)
            {
                // data 청크인 경우 실제 남은 크기를 사용
                if (chunkId == "data")
                {
                    dataSize = (uint)remaining;
                    break; // data 청크가 마지막이라고 가정
                }
                
                // 다른 청크는 스킵하고 종료
                break;
            }

            if (chunkId == "fmt ")
            {
                if (chunkSize < 16)
                    throw new InvalidDataException("Invalid fmt chunk (too small).");

                br.ReadUInt16(); // audioFormat
                br.ReadUInt16(); // channels
                br.ReadUInt32(); // sampleRate
                byteRate = (int)br.ReadUInt32();
                br.ReadUInt16(); // blockAlign
                br.ReadUInt16(); // bitsPerSample

                long extra = (long)chunkSize - 16;
                if (extra > 0)
                    fs.Seek(extra, SeekOrigin.Current);
            }
            else if (chunkId == "data")
            {
                dataSize = chunkSize;
                fs.Seek(Math.Min(chunkSize, remaining), SeekOrigin.Current); // ✅ 안전하게 Seek
            }
            else
            {
                fs.Seek(Math.Min(chunkSize, remaining), SeekOrigin.Current); // ✅ 안전하게 Seek
            }

            if ((chunkSize & 1) == 1 && fs.Position < fs.Length)
                fs.Seek(1, SeekOrigin.Current);

            if (byteRate.HasValue && dataSize.HasValue)
                break;
        }

        if (!byteRate.HasValue || byteRate.Value <= 0)
            throw new InvalidDataException("WAV byteRate not found.");

        if (!dataSize.HasValue)
            throw new InvalidDataException("WAV data chunk not found.");

        var seconds = (double)dataSize.Value / byteRate.Value;
        var ms = (int)Math.Round(seconds * 1000.0);

        return Math.Max(1, ms);
    }
}
