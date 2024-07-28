using Nest;

namespace PlagiarismChecker.Interface
{
    public interface IPlagiarismService
    {
        Task  EnsureIndex(ElasticClient client);
        Task IndexText(ElasticClient client, string documentId, string filePath);
        Task<double> CheckPlagiarism(ElasticClient client, string file1Id, string file2Id);
    }
}
