using Nest;
using PlagiarismChecker.Interface;
using PlagiarismChecker.Models;
using Microsoft.Extensions.Logging;

namespace PlagiarismChecker.Service
{
    public class PlagiarismService : IPlagiarismService
    {
        private readonly ILogger<PlagiarismService> _logger;

        public PlagiarismService(ILogger<PlagiarismService> logger)
        {
            _logger = logger;
        }

        public async Task <double> CheckPlagiarism(ElasticClient client, string file1Id, string file2Id)
        {
            var termVectorsResponse1 = await client.TermVectorsAsync<Document>(tv => tv
                .Index("documents")
                .Id(file1Id)
                .Fields("content")
                .Offsets(false)
                .Positions(false)
                .Payloads(false)
                .TermStatistics(false)
                .FieldStatistics(false)
            );

            var termVectorsResponse2 = await client.TermVectorsAsync<Document>(tv => tv
                .Index("documents")
                .Id(file2Id)
                .Fields("content")
                .Offsets(false)
                .Positions(false)
                .Payloads(false)
                .TermStatistics(false)
                .FieldStatistics(false)
            );

            if (!termVectorsResponse1.IsValid)
            {
                Console.WriteLine($"Failed to get term vectors for document {file1Id}: {termVectorsResponse1.ServerError?.Error?.Reason}");
                return 0.0;
            }
            if (!termVectorsResponse2.IsValid)
            {
                Console.WriteLine($"Failed to get term vectors for document {file2Id}: {termVectorsResponse2.ServerError?.Error?.Reason}");
                return 0.0;
            }

            var terms1 = termVectorsResponse1.TermVectors["content"].Terms;
            var terms2 = termVectorsResponse2.TermVectors["content"].Terms;
            foreach (var term in terms1.Keys)
            {
                Console.WriteLine("term1" + term);
            }
            foreach (var term in terms2.Keys)
            {
                Console.WriteLine("term2"+term);
            }
            var commonTerms = terms1.Keys.Intersect(terms2.Keys).ToList();
            foreach(var term in commonTerms)
            {
                Console.WriteLine(term);
            }
            var dotProduct = commonTerms.Sum(term => terms1[term].TermFrequency * terms2[term].TermFrequency);
            var magnitude1 = Math.Sqrt(terms1.Values.Sum(tv => Math.Pow(tv.TermFrequency, 2)));
            var magnitude2 = Math.Sqrt(terms2.Values.Sum(tv => Math.Pow(tv.TermFrequency, 2)));

            double matchPercentage = Math.Round((dotProduct / (magnitude1 * magnitude2)) * 100, 1);

            return matchPercentage;
            /*var terms1 = termVectorsResponse1.TermVectors["content"].Terms.Keys;
            var terms2 = termVectorsResponse2.TermVectors["content"].Terms.Keys;
            foreach (var term in terms1)
            {
                Console.WriteLine(term);
            }

            Console.WriteLine("Document 2 terms:");
            foreach (var term in terms2)
            {
                Console.WriteLine(term);
            }
            var commonTermsCount = terms1.Intersect(terms2).Count();
            var comn = terms1.Intersect(terms2);
            foreach (var term in comn)
            {
                Console.WriteLine($"the common {term}");
            }

            var totalTermsCount = Math.Max(terms1.Count(), terms2.Count());

            double matchPercentage = totalTermsCount > 0 ? (double)commonTermsCount / totalTermsCount * 100 : 0.0;

            return matchPercentage;*/

        }
        public async Task EnsureIndex(ElasticClient client)
        {
            var indexExistsResponse = await client.Indices.ExistsAsync("documents");
            if (indexExistsResponse.Exists)
            {
                var deleteIndexResponse = await client.Indices.DeleteAsync("documents");
                if (!deleteIndexResponse.IsValid)
                {
                    _logger.LogError($"Failed to delete existing index: {deleteIndexResponse.ServerError?.Error?.Reason}");
                    return;
                }
            }

            var createIndexResponse = await client.Indices.CreateAsync("documents", c => c
            .Settings(s => s
               .Analysis(a => a
                   .Analyzers(an => an
                       .Custom("custom_analyzer", ca => ca
                           .Tokenizer("standard")
                           .Filters("lowercase", "remove_punctuation", "stop", "english_stemmer", "shingle", "remove_underscore", "normalize_whitespace", "trim_whitespace")
                       )
                   )
                   .TokenFilters(tf => tf
                       .Stemmer("english_stemmer", st => st
                                .Language("porter")
                        )
                       .PatternReplace("remove_punctuation", pr => pr
                                .Pattern("[\\p{P}\\p{S}]")
                                .Replacement("")
                        )
                       .PatternReplace("remove_underscore", pr => pr
                           .Pattern("_+")
                           .Replacement("")
                       )
                       .PatternReplace("normalize_whitespace", pr => pr
                           .Pattern("\\s{2,}")
                           .Replacement(" ")
                       )
                       .PatternReplace("trim_whitespace", pr => pr
                           .Pattern("^\\s+|\\s+$")
                           .Replacement("")
                       )
                       .Shingle("shingle", sf => sf
                           .MinShingleSize(2)
                           .MaxShingleSize(3)
                           .OutputUnigrams(true)
                           .OutputUnigramsIfNoShingles(true)
                       )
                   )
               )
             )
            .Map<Document>(m => m
               .Properties(p => p
                   .Text(t => t
                       .Name(n => n.Content)
                       .Analyzer("custom_analyzer")
                   )
               )
             )
             );

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError($"Failed to create index: {createIndexResponse.ServerError?.Error?.Reason}");
            }
            else
            {
                _logger.LogInformation("Index created successfully.");
            }
        }

        public async Task IndexText(ElasticClient client, string documentId, string content)
        {
            /*if (!File.Exists(filePath))
            {
                Console.WriteLine($"File '{filePath}' not found.");
                return;
            }*/

            //var content = File.ReadAllText(filePath);
            var document = new Document { Id = documentId, Content = content };

            var indexResponse = await client.IndexAsync(document, idx => idx.Id(documentId));

            if (!indexResponse.IsValid)
            {
                _logger.LogError($"Failed to index document {documentId}: {indexResponse.ServerError?.Error?.Reason}");
            }
            else
            {
                _logger.LogInformation($"Indexed document {documentId} successfully.");
            }
        }
    }
}
