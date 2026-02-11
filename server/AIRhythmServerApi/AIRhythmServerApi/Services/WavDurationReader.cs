using System.Text;

namespace AIRhythmServerApi.Services;

public static class WavDurationReader
{
    // RIFF(WAVE) + fmt + data 기반 duration 계산
    // byteRate와 dataSize로 duration_ms = dataSize / byteRate
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

            // 깨진 파일/비정상 chunkSize 방어: 남은 길이보다 크면 중단
            long remaining = fs.Length - fs.Position;
            if (chunkSize > remaining)
            {
                throw new InvalidDataException($"Invalid WAV chunk size: {chunkId} size={chunkSize} remaining={remaining}");
            }

            if (chunkId == "fmt ")
            {
                // fmt chunk 최소 16바イト는 있어야 함
                if (chunkSize < 16)
                    throw new InvalidDataException("Invalid fmt chunk (too small).");

                br.ReadUInt16(); // audioFormat
                br.ReadUInt16(); // channels
                br.ReadUInt32(); // sampleRate
                byteRate = (int)br.ReadUInt32();
                br.ReadUInt16(); // blockAlign
                br.ReadUInt16(); // bitsPerSample

                // fmt chunk가 16보다 길면 남은 바이트는 Seek로 스킵
                long extra = (long)chunkSize - 16;
                if (extra > 0)
                    fs.Seek(extra, SeekOrigin.Current);
            }
            else if (chunkId == "data")
            {
                dataSize = chunkSize;

                // ❗ 핵심 패치: data 청크를 메모리에 읽지 말고 파일 포인터만 이동
                fs.Seek(chunkSize, SeekOrigin.Current);
            }
            else
            {
                // 다른 청크도 메모리 읽기 대신 Seek로 스킵
                fs.Seek(chunkSize, SeekOrigin.Current);
            }

            // chunk는 짝수 정렬 패딩이 있을 수 있음
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
