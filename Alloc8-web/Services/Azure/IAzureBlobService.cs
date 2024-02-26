namespace Alloc8_web.Services.Azure
{
    public interface IAzureBlobService
    {
        public Task<string> UploadFileAsync(IFormFile file, string containerName);
    }
}
