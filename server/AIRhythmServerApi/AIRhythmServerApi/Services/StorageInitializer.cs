using Microsoft.Extensions.Options;

namespace AIRhythmServerApi.Services
{
    public sealed class StorageOptions
    {
        public string RootPath { get; set; } = "storage";
        public string AudioDir { get; set; } = "audio";
        public string ChartsDir { get; set; } = "charts";
    }

    public class StorageInitializer
    {
        private readonly StorageOptions _opt;

        public StorageInitializer(IOptions<StorageOptions> opt)
        {
            _opt = opt.Value;
        }

        public (string Root, string Audio, string Charts) EnsureDirectories()
        {
            var root = Path.GetFullPath(_opt.RootPath);
            var audio = Path.Combine(root, _opt.AudioDir);
            var charts = Path.Combine(root, _opt.ChartsDir);

            Directory.CreateDirectory(root);
            Directory.CreateDirectory(audio);
            Directory.CreateDirectory(charts);

            return (root, audio, charts);
        }
    }
}
