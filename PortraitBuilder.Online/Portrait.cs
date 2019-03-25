using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Engine;
using PortraitBuilder.Model;

namespace PortraitBuilder.Online
{
    public static class PortraitFunction
    {
        [FunctionName("portrait")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dna = req.Query["dna"];
            string properties = req.Query["properties"];
            if (string.IsNullOrEmpty(dna) || string.IsNullOrEmpty(properties))
                return new BadRequestResult();

            string customProperties = req.Query["customProperties"];

            var portrait = new Portrait();
            portrait.Import(dna, properties + customProperties);

            // Reflect on dropdown
            //updateSelectedCharacteristicValues(portrait);

            //started = true;

            var portraitRenderer = new PortraitRenderer();
            User user = new User
            {
                GameDir = @"X:\Games\Steam\steamapps\common\Crusader Kings II",//readGameDir();
                ModDir = @"C:\Users\scorp\Documents\Paradox Interactive\Crusader Kings II",//readModDir(user.GameDir);
                DlcDir = "dlc/"
            };
            var loader = new Loader(user);
            var bmp = portraitRenderer.DrawPortrait(portrait, loader.ActiveContents, loader.ActivePortraitData.Sprites);

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            return new OkObjectResult(bmp);
        }
    }
}
