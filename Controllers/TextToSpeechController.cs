using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace text_to_speechStudio.Controllers
{
    public class TextToSpeechController : Controller
    {
        // GET: /<controller>/
        public IActionResult Text_To_Speech()
        {
            return View();
        }
    }
}

