using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Audionix.Models;

[Route("api/[controller]")]
[ApiController]
public class AudioController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly AppSettings _appSettings;
    private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
    {
        {".wav", "audio/wav"},
    };

    public AudioController(IWebHostEnvironment env, AppSettings appSettings)
    {
        _env = env;
        _appSettings = appSettings;
    }

    [HttpGet("{station}/{foldername}/{filename}")]
    public IActionResult Get(string station, string foldername, string filename)
    {
        Log.Debug("--- AudioController -- Get() - BEGINS - station: {station}, foldername: {foldername}, filename: {filename}", station, foldername, filename);

        filename = System.Net.WebUtility.UrlDecode(filename);

        Log.Information("--- AudioController - Get() -- UrlDecoded filename:  {filename}", filename);

        if (_appSettings.DataPath != null)
        {
            var path = Path.Combine(_appSettings.DataPath, "Stations", station, "Audio", foldername, filename);
            Log.Information("--- AudioController - Get() -- AudioController.Get: {path}", path);

            if (!System.IO.File.Exists(path))
            {
                Log.Error("++++++ AudioController -- Get() - File does not exist: {path}", path);
                return NotFound();
            }

            return PhysicalFile(path, GetContentType(path), Path.GetFileName(path));
        }
        else
        {
            Log.Error("++++++ AudioController -- Get() - AudioController.Get: DataPath is null");
            return NotFound();
        }
    }

    private string GetContentType(string path)
    {
        Log.Debug("--- AudioController -- GetContentType() -  path: {path}", path);
        var ext = Path.GetExtension(path).ToLowerInvariant();
        Log.Debug("--- AudioController -- GetContentType() -  GetExtension: {ext}", ext);
        return MimeTypes[ext];
    }
}
