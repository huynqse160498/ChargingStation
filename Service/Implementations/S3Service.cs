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

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            var fileName = $"{folder}/{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = file.ContentType
                // ❌ Không dùng CannedACL nữa
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{fileName}";
        }


        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var key = ExtractKeyFromUrl(fileUrl);
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> RenameFileAsync(string oldFileUrl, string newFileName)
        {
            var oldKey = ExtractKeyFromUrl(oldFileUrl);
            var folder = oldKey.Contains('/') ? oldKey[..oldKey.LastIndexOf('/')] : "";
            var newKey = $"{folder}/{newFileName}";

            await _s3Client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                SourceKey = oldKey,
                DestinationBucket = _bucketName,
                DestinationKey = newKey
                // ❌ Không dùng CannedACL
            });

            await _s3Client.DeleteObjectAsync(_bucketName, oldKey);
            return GenerateFileUrl(newKey);
        }

        private string ExtractKeyFromUrl(string fileUrl)
        {
            var prefix = $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/";
            return fileUrl.Replace(prefix, "");
        }

        private string GenerateFileUrl(string key) =>
            $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
    }
}
