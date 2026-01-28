using AIRhythmServerApi.Stores;
using Microsoft.AspNetCore.Mvc;

namespace YourServer.Controllers;

[ApiController]
[Route("api/charts")]
public sealed class ChartsController : ControllerBase
{
    private readonly IChartStore _charts;

    public ChartsController(IChartStore charts)
    {
        _charts = charts;
    }

    [HttpGet("{chartId}/chart")]
    public IActionResult GetChart([FromRoute] string chartId)
    {
        if (!_charts.TryGet(chartId, out var chart))
            return NotFound(new { message = "chart not found" });

        if (!System.IO.File.Exists(chart.ChartPath))
            return NotFound(new { message = "chart file missing" });

        return PhysicalFile(chart.ChartPath, "application/json; charset=utf-8");
    }

    [HttpGet("{chartId}/audio")]
    public IActionResult GetAudio([FromRoute] string chartId)
    {
        if (!_charts.TryGet(chartId, out var chart))
            return NotFound(new { message = "chart not found" });

        if (!System.IO.File.Exists(chart.AudioPath))
            return NotFound(new { message = "audio file missing" });

        // WAV 고정
        return PhysicalFile(chart.AudioPath, "audio/wav", fileDownloadName: $"{chartId}.wav" ,enableRangeProcessing: true);
    }
}
