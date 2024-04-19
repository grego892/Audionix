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

    public AudioController(IWebHostEnvironment env, AppSettings appSettings)
    {
        _env = env;
        _appSettings = appSettings;
    }


    [HttpGet("{station}/{foldername}/{filename}")]
    public IActionResult Get(string station, string foldername, string filename)
    {
        filename = System.Net.WebUtility.UrlDecode(filename);

        Log.Information("--- AudioController - Get() -- UrlDecoded filename:  {filename}", filename);

        if (_appSettings.DataPath != null)
        {
            var path = Path.Combine(_appSettings.DataPath, "Stations", station, "Audio", foldername, filename);
            Log.Information("AudioController.Get: {path}", path);
            var memory = new MemoryStream();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }
        else
        {
            Log.Error("++++++ AudioController -- Get() - AudioController.Get: DataPath is null");
            return NotFound();
        }

    }

    private string GetContentType(string path)
    {
        var types = GetMimeTypes();
        var ext = Path.GetExtension(path).ToLowerInvariant();
        Log.Debug("GetExtension: {ext}", ext);
        return types[ext];
    }

    private Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
        {
            //{".mp3", "audio/mpeg"},
            {".wav", "audio/wav"},
            // Add more if needed
        };
    }
}
