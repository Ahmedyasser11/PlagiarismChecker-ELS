using Nest;
using PlagiarismChecker.Interface;
using PlagiarismChecker.Models;

namespace PlagiarismChecker.Service
{
    public class PlagiarismService : IPlagiarismService
    {
        public double CheckPlagiarism(ElasticClient client, string file1Id, string file2Id)
        {
            var termVectorsResponse1 = client.TermVectors<Document>(tv => tv
           .Index("documents")
           .Id(file1Id)
           .Fields("content")
           .Offsets(false)
           .Positions(false)
           .Payloads(false)
           .TermStatistics(false)
           .FieldStatistics(false)
       );

            var termVectorsResponse2 = client.TermVectors<Document>(tv => tv
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

            var terms1 = termVectorsResponse1.TermVectors["content"].Terms.Keys;
            var terms2 = termVectorsResponse2.TermVectors["content"].Terms.Keys;

            var commonTermsCount = terms1.Intersect(terms2).Count();
            var totalTermsCount = Math.Max(terms1.Count(), terms2.Count());

            double matchPercentage = totalTermsCount > 0 ? (double)commonTermsCount / totalTermsCount * 100 : 0.0;

            return matchPercentage;

        }

        public void EnsureIndex(ElasticClient client)
        {
            if (client.Indices.Exists("documents").Exists)
            {
                client.Indices.Delete("documents");
            }

            var createIndexResponse = client.Indices.Create("documents", c => c
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("custom_analyzer", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "stop", "shingle")
                            )
                        )
                        .TokenFilters(tf => tf
                            .Shingle("shingle", sf => sf
                                .MinShingleSize(2)
                                .MaxShingleSize(3)
                                .OutputUnigrams(false)
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
                Console.WriteLine($"Failed to create index: {createIndexResponse.ServerError?.Error?.Reason}");
            }
            else
            {
                Console.WriteLine("Index created successfully.");
            }
        }

        public void IndexText(ElasticClient client, string documentId, string filePath)
        {
            /*if (!File.Exists(filePath))
            {
                Console.WriteLine($"File '{filePath}' not found.");
                return;
            }*/

            //var content = File.ReadAllText(filePath);
            var content = filePath;
            var document = new Document { Id = documentId, Content = content };

            var indexResponse = client.Index(document, idx => idx.Id(documentId));

            if (!indexResponse.IsValid)
            {
                Console.WriteLine($"Failed to index document {filePath}: {indexResponse.ServerError?.Error?.Reason}");
            }
            else
            {
                Console.WriteLine($"Indexed document {filePath} successfully.");
            }
        }
    }
}
