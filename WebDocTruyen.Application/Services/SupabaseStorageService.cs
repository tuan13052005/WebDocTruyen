using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace WebDocTruyen.Infrastructure.Storage
{
    public class SupabaseStorageService : ISupabaseStorageService
    {
        private readonly HttpClient _http;
        private readonly string _url;
        private readonly string _key;
        private readonly string _bucket;
        private readonly string _publicPrefix;

        public SupabaseStorageService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            _url = config["Supabase:Url"]!.TrimEnd('/');
            _key = config["Supabase:ServiceRoleKey"]!;
            _bucket = config["Supabase:Bucket"]!;
            _publicPrefix = $"{_url}/storage/v1/object/public/{_bucket}/";
        }

        public async Task<string> UploadAsync(Stream content, string path, string contentType)
        {
            var uploadUrl = $"{_url}/storage/v1/object/{_bucket}/{path}";

            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            request.Headers.Add("x-upsert", "true");

            using var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content = streamContent;

            var resp = await _http.SendAsync(request);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                throw new Exception($"Upload Supabase thất bại ({resp.StatusCode}): {err}");
            }

            return _publicPrefix + path;
        }

        public async Task DeleteAsync(string path)
        {
            var deleteUrl = $"{_url}/storage/v1/object/{_bucket}/{path}";
            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            await _http.SendAsync(request); // best-effort, không throw nếu file không tồn tại
        }

        // Supabase không có API xóa "folder" trực tiếp → liệt kê rồi xóa từng file
        public async Task DeleteFolderAsync(string prefix)
        {
            var listUrl = $"{_url}/storage/v1/object/list/{_bucket}";
            using var listRequest = new HttpRequestMessage(HttpMethod.Post, listUrl);
            listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            listRequest.Content = new StringContent(
                JsonSerializer.Serialize(new { prefix, limit = 1000 }),
                System.Text.Encoding.UTF8, "application/json");

            var listResp = await _http.SendAsync(listRequest);
            if (!listResp.IsSuccessStatusCode) return;

            var json = await listResp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var paths = new List<string>();
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString();
                if (name != null) paths.Add($"{prefix}/{name}");
            }
            if (paths.Count == 0) return;

            var deleteUrl = $"{_url}/storage/v1/object/{_bucket}";
            using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _key);
            deleteRequest.Content = new StringContent(
                JsonSerializer.Serialize(new { prefixes = paths }),
                System.Text.Encoding.UTF8, "application/json");
            await _http.SendAsync(deleteRequest);
        }

        public string? GetPathFromPublicUrl(string publicUrl)
        {
            if (string.IsNullOrEmpty(publicUrl) || !publicUrl.StartsWith(_publicPrefix))
                return null;
            return publicUrl.Substring(_publicPrefix.Length);
        }

        public string GetContentType(string fileName) =>
            Path.GetExtension(fileName).ToLower() switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };
    }
}