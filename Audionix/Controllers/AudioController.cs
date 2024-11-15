using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Audionix.Shared;
using Audionix.Shared.Models;

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

            Log.Debug("Start checking if file exists");
            var fileExists = System.IO.File.Exists(path);
            Log.Debug("Finished checking if file exists");

            if (!fileExists)
            {
                Log.Error("File does not exist: {path}", path);
                return NotFound();
            }

            Log.Debug("Start reading file");
            var fileContent = System.IO.File.ReadAllText(path);
            Log.Debug("Finished reading file");

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
