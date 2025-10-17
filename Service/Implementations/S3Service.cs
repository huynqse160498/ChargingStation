using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Implementations
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration config)
        {
            var accessKey = config["AWS:AccessKey"];
            var secretKey = config["AWS:SecretKey"];
            var region = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
            _bucketName = config["AWS:BucketName"];

            _s3Client = new AmazonS3Client(accessKey, secretKey, region);
        }

        // ✅ Upload file
        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var fileName = $"{folder}/{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{fileName}";
        }

        // 🗑️ Delete file
        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var key = ExtractKeyFromUrl(fileUrl);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };
                await _s3Client.DeleteObjectAsync(deleteRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ✏️ Rename file (copy → delete → new URL)
        public async Task<string> RenameFileAsync(string oldFileUrl, string newFileName)
        {
            var oldKey = ExtractKeyFromUrl(oldFileUrl);
            var folder = oldKey.Contains('/') ? oldKey[..oldKey.LastIndexOf('/')] : "";
            var newKey = $"{folder}/{newFileName}";

            // Copy
            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                SourceKey = oldKey,
                DestinationBucket = _bucketName,
                DestinationKey = newKey,
                CannedACL = S3CannedACL.PublicRead
            };
            await _s3Client.CopyObjectAsync(copyRequest);

            // Delete old
            await _s3Client.DeleteObjectAsync(_bucketName, oldKey);

            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{newKey}";
        }

        // 🧠 Helper - tách key (đường dẫn nội bộ) từ URL public
        private string ExtractKeyFromUrl(string fileUrl)
        {
            var prefix = $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/";
            return fileUrl.Replace(prefix, "");
        }
    }
}
