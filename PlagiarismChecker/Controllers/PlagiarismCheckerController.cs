using Microsoft.AspNetCore.Mvc;
using Nest;
using PlagiarismChecker.Interface;
using PlagiarismChecker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

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
        public async Task<IActionResult> GetPlagiarismPercentage([FromBody] Content content)
        {
            await _plagiarismService.EnsureIndex(_elasticClient);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await _plagiarismService.IndexText(_elasticClient, "Text1", content.s1);
            await _plagiarismService.IndexText(_elasticClient, "Text2", content.s2);
            var matchPercentage =await _plagiarismService.CheckPlagiarism(_elasticClient, "Text1", "Text2");
            stopwatch.Stop();
            TimeSpan timeTaken = stopwatch.Elapsed;
            return Ok($"Time taken: {timeTaken.TotalMilliseconds} milliseconds\nMatching Percentage: {matchPercentage}%");
        }
    }
}
