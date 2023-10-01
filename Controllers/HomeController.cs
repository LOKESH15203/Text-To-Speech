using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using text_to_speechStudio.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace text_to_speechStudio.Controllers;

public class HomeController : Controller
{

    private readonly ILogger<HomeController> _logger;
    public IConfiguration Configuration { get; }


    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        Configuration = configuration;
    }


    public IActionResult Index()
    {
        string text = "This is some text with <b>HTML tags</b>.";
        string newText = Regex.Replace(text, "<[^>]*>", "", RegexOptions.IgnoreCase);

        //return Json(new { text = newText });
        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    public IActionResult Text_To_Speech()
    {
        return View();
    }


    [HttpPost]
    public IActionResult Index(TextToSpeechModel model)
    {
        TextToSpeechResponse? result = null;
        string responseObject = string.Empty;
        try
        {
            JObject jsonObject = new JObject();
            JObject audioConfig = new JObject();
            audioConfig.Add("audioEncoding", "LINEAR16");
            audioConfig.Add("effectsProfileId", new JArray("small-bluetooth-speaker-class-device"));
            audioConfig.Add("pitch", model.pitch);          // #####################
            audioConfig.Add("speakingRate", model.speed);

            jsonObject.Add("audioConfig", audioConfig);
            JObject inputJson = new JObject();

            //inputJson.Add("text", Text.RemoveHtml().Replace("&nvsp", "").Replace("&nbsp;", "").Replace("&nbsp", ""));
            //inputJson.Add("text", Regex.Replace(ProcessTextWithPauses(model.text), "<[^>]*>", "", RegexOptions.IgnoreCase));
            inputJson.Add("ssml", ProcessTextWithPauses(model.text));

            jsonObject.Add("input", inputJson);

            JObject voiceJson = new JObject();
            voiceJson.Add("languageCode", "en-IN");
            voiceJson.Add("name", model.voiceName);

            jsonObject.Add("voice", voiceJson);
            var requestJson = jsonObject.ToString();
            var httpClient = new HttpClient();

            var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://texttospeech.googleapis.com/v1/text:synthesize"))
            {
                request.Content = requestContent;
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Add("X-goog-api-key", API-KEY);

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                httpClient.Timeout = TimeSpan.FromMinutes(30);
                using (var responsee = httpClient.SendAsync(request).Result)
                {
                    if (responsee.StatusCode == HttpStatusCode.OK)
                    {
                        responseObject = responsee.Content.ReadAsStringAsync().Result;
                        result = JsonConvert.DeserializeObject<TextToSpeechResponse>(responseObject);
                        byte[] bytes = Convert.FromBase64String(result.audioContent);
                        string guid = Guid.NewGuid().ToString();

                        System.IO.File.WriteAllBytes($"{Configuration.GetSection("AudioFolderPath")}{guid}.wav", bytes);
                        return Ok(result.audioContent);
                    }
                    else
                    {
                        responseObject = responsee.Content.ReadAsStringAsync().Result;

                        result = null;
                    }
                }
            }
        }

        catch (Exception ex)
        {
            return BadRequest("An error occurred: " + ex.Message);
        }

        if (result != null)
        {
            return Ok(result); // Return non-null value
        }
        else
        {
            return BadRequest("Error occurred"); // Return an appropriate response
        }

    }


    private string ProcessTextWithPauses(String st)
    {
        // Split the input text into segments based on "Pause: N" markers
        string[] segments = st.Split(new[] { " (Pause: " }, StringSplitOptions.None);

        // Initialize the processed text
        StringBuilder processedText = new StringBuilder();
        processedText.Append("<speak>");
        foreach (string segment in segments)
        {
            if (segment.Contains(")"))
            {
                // Extract the numeric part (N) from the marker
                string marker = segment.Substring(0, segment.IndexOf(")"));
                if (int.TryParse(marker, out int pauseDuration))
                {
                    // Add a pause of 'pauseDuration' seconds
                    //processedText.Append($"<break time=\"{pauseDuration * 1000}ms\"/>");
                    String str = segment.Replace($"{marker})", $" <break time=\"{pauseDuration * 1000}ms\"/> \n");
                    processedText.Append(str);
                    //Thread.Sleep(pauseDuration * 1000); // Pause for 'pauseDuration' seconds
                }
            }
            else
            {
                // Add the segment to the processed text
                processedText.Append(segment);
            }
        }

        processedText.Append("</speak>");
        return processedText.ToString();
    }

}
