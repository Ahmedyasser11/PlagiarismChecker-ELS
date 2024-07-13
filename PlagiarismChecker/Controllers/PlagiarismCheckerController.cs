using Microsoft.AspNetCore.Mvc;
using Nest;
using PlagiarismChecker.Interface;
using PlagiarismChecker.Models;

namespace PlagiarismChecker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlagiarismCheckerController : ControllerBase
    {


        private readonly ILogger<PlagiarismCheckerController> _logger;
        private readonly IPlagiarismService _plagiarismService;
        private readonly ConnectionSettings _connectionSettings;
        private readonly ElasticClient _elasticClient;

        public PlagiarismCheckerController(ILogger<PlagiarismCheckerController> logger, IPlagiarismService plagiarismService)
        {
            _logger = logger;
            _plagiarismService = plagiarismService;
            _connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"))
            .DefaultIndex("documents");
            _elasticClient = new ElasticClient(_connectionSettings);
        }

        [HttpPost("GetPlagiarismPercentage")]
        public  IActionResult GetPlagiarismPercentage(Content content)
        {
            _plagiarismService.EnsureIndex(_elasticClient);
            _plagiarismService.IndexText(_elasticClient, "Text1", content.s1);
            _plagiarismService.IndexText(_elasticClient, "Text2", content.s2);
            var matchPercentage = _plagiarismService.CheckPlagiarism(_elasticClient, "Text1", "Text2");
            return Ok($"Matching Percentage: {matchPercentage}%");
        }
    }
}
