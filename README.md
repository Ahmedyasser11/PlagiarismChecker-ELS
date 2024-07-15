### Overview

This project is a simple plagiarism checker that uses ElasticSearch for text indexing and comparison. It compares two pieces of text and returns the percentage of matching content between them.

### Project Structure

- `Controllers/PlagiarismCheckerController.cs`: Handles API requests.
- `Interface/IPlagiarismService.cs`: Interface defining the plagiarism checking service.
- `Models/Content.cs`: Model representing the content to be checked for plagiarism.
- `Models/Document.cs`: Model representing the document stored in ElasticSearch.
- `Service/PlagiarismService.cs`: Implementation of the plagiarism checking service.

### Prerequisites
- **Docker**: Ensure Docker is installed on your machine. You can download it from [Docker's official site](https://www.docker.com/get-started).
- **Install .NET Core SDK**:
   - Download and install the .NET Core SDK from [Microsoft's official site](https://dotnet.microsoft.com/download).
### Setup Instructions

1. **Clone the Repository**:
   - Clone this repository to your local machine.

2. **Run Docker Compose**:
   - Ensure Docker is installed and running on your machine
   - Navigate to the project directory where the `docker-compose.yml` file is located.
   - open cmd in your directory and dont close it.
   - Run the following command to start the required services:
     ```sh
     docker-compose up
     ```

3. **Run the Application**:
   - Open new cmd in the Project directory 
   - In the project directory, run:
     ```sh
     dotnet run
     ```
   - The application will start and listen on `http://localhost:5280/swagger/index.html`.

### API Endpoints

- **POST /PlagiarismChecker/GetPlagiarismPercentage**
  - **Description**: Compares two pieces of text and returns the percentage of matching content.
  - **Request Body**:
    ```json
    {
      "s1": "First text content",
      "s2": "Second text content"
    }
    ```
  - **Response**:
    ```json
    "Matching Percentage: X%"
    ```

### Key Features

#### 1. ElasticSearch Integration
- **Text Indexing**: Text content is indexed in ElasticSearch, allowing for efficient search and retrieval.
- **Custom Analyzers**: Utilizes custom analyzers to tokenize and filter the text, making the comparison more accurate.
- **Term Vectors**: Uses ElasticSearch's term vectors API to analyze the content and calculate similarity based on term occurrences.

#### 2. API-Driven Design
- **RESTful API**: The application exposes a RESTful API endpoint to check plagiarism, making it easy to integrate with other applications or systems.
- **JSON Request and Response**: Communicates using JSON, a widely used data interchange format, ensuring compatibility and ease of use.

#### 3. Plagiarism Detection Logic
- **Content Comparison**: Compares two pieces of text by analyzing their term vectors.
- **Matching Percentage Calculation**: Calculates the percentage of common terms between the two texts, providing a simple yet effective measure of similarity.
- **Error Handling**: Handles errors gracefully, logging them and providing meaningful messages.

#### 4. Extensible and Modular Design
- **Service Interface**: The plagiarism checking logic is defined in an interface, making it easy to extend or replace the implementation.
- **Separation of Concerns**: The project is organized into controllers, services, and models, promoting clean architecture and maintainability.

#### 5. Configuration and Customization
- **Connection Settings**: ElasticSearch connection settings are configurable, allowing you to adjust the URI or index name as needed.
- **Analyzer Settings**: Custom analyzer settings can be adjusted to fine-tune the text processing according to your needs.

### Detailed Code Explanation

#### Controllers/PlagiarismCheckerController.cs

- **Constructor**:
  - Initializes the logger, plagiarism service, and ElasticSearch client with connection settings.
  - Configures the default index for ElasticSearch.

- **GetPlagiarismPercentage**:
  - Ensures the required index exists in ElasticSearch by calling `_plagiarismService.EnsureIndex(_elasticClient)`.
  - Indexes the two pieces of text provided in the request body using `_plagiarismService.IndexText(_elasticClient, "Text1", content.s1)` and `_plagiarismService.IndexText(_elasticClient, "Text2", content.s2)`.
  - Compares the indexed texts using `_plagiarismService.CheckPlagiarism(_elasticClient, "Text1", "Text2")`.
  - Returns the calculated matching percentage.

#### Interface/IPlagiarismService.cs

- **EnsureIndex**: Defines a method to ensure the existence of an ElasticSearch index.
- **IndexText**: Defines a method to index a text document in ElasticSearch.
- **CheckPlagiarism**: Defines a method to check plagiarism between two indexed documents.

#### Models/Content.cs

- **Content**: Represents the input model for the API, containing two strings (`s1` and `s2`) to be checked for plagiarism.

#### Models/Document.cs

- **Document**: Represents the document model stored in ElasticSearch with properties `Id` and `Content`.

#### Service/PlagiarismService.cs

- **EnsureIndex**:
  - Checks if the index exists and deletes it if it does, ensuring a clean setup.
  - Creates a new index with custom analyzer settings to tokenize and filter the text.
  - Logs the result of the index creation.

- **IndexText**:
  - Indexes the provided text content into ElasticSearch under a specified document ID.
  - Logs the result of the indexing operation.

- **CheckPlagiarism**:
  - Retrieves the term vectors for the two indexed documents.
  - Calculates the number of common terms between the two documents.
  - Computes the matching percentage based on the common terms and total terms.
  - Returns the matching percentage.

### Notes

- Make sure ElasticSearch is running on `http://localhost:9200` before starting the application.
- Adjust the ElasticSearch URI in `PlagiarismCheckerController.cs` if ElasticSearch is running on a different URI.

   
