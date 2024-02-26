using Azure.Storage.Blobs;

namespace Alloc8_web.Services.Azure
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public AzureBlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }
        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("Invalid file");
                }

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                if (!containerClient.Exists())
                {
                    containerClient.Create();
                }

                var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var blobClient = containerClient.GetBlobClient(blobName);

                using (var stream = file.OpenReadStream())
                {
                    var resp = await blobClient.UploadAsync(stream, true);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                throw new InvalidOperationException("Error uploading file to Azure Blob Storage", ex);
            }
        }
    }
}
