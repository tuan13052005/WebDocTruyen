namespace WebDocTruyen.Infrastructure.Storage
{
    public interface ISupabaseStorageService
    {
        /// <summary>Upload file, trả về full public URL.</summary>
        Task<string> UploadAsync(Stream content, string path, string contentType);

        /// <summary>Xóa 1 file theo path tương đối trong bucket.</summary>
        Task DeleteAsync(string path);

        /// <summary>Xóa toàn bộ file trong 1 "thư mục" (prefix) — dùng khi xóa truyện/chapter.</summary>
        Task DeleteFolderAsync(string prefix);

        /// <summary>Trích path tương đối từ 1 public URL đã lưu trong DB.</summary>
        string? GetPathFromPublicUrl(string publicUrl);

        string GetContentType(string fileName);
    }
}