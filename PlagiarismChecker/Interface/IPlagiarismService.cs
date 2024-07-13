using Nest;

namespace PlagiarismChecker.Interface
{
    public interface IPlagiarismService
    {
        public void EnsureIndex(ElasticClient client);
        public void IndexText(ElasticClient client, string documentId, string filePath);
        public double CheckPlagiarism(ElasticClient client, string file1Id, string file2Id);
    }
}
