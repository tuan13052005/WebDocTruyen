namespace WebDocTruyen.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // ── Thông tin lỗi mở rộng ─────────────────────────────────
        public int StatusCode { get; set; } = 500;
        public string Title { get; set; } = "Đã xảy ra lỗi";
        public string Message { get; set; } = "Đã xảy ra lỗi khi xử lý yêu cầu của bạn. Vui lòng thử lại sau.";

        // Đường dẫn gây lỗi (hữu ích khi log / hiển thị cho admin)
        public string? Path { get; set; }

        // Chỉ hiển thị chi tiết kỹ thuật (stack trace) khi ở môi trường Development
        public bool ShowDetails { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }

        // Map sẵn vài mã lỗi HTTP phổ biến → tiêu đề + thông điệp thân thiện
        public static ErrorViewModel FromStatusCode(int statusCode, string? requestId, string? path = null)
        {
            var (title, message) = statusCode switch
            {
                400 => ("Yêu cầu không hợp lệ", "Dữ liệu bạn gửi lên không đúng định dạng."),
                401 => ("Chưa đăng nhập", "Bạn cần đăng nhập để truy cập trang này."),
                403 => ("Không có quyền truy cập", "Bạn không có quyền thực hiện hành động này."),
                404 => ("Không tìm thấy trang", "Trang hoặc tài nguyên bạn tìm không tồn tại."),
                500 => ("Lỗi máy chủ", "Đã xảy ra lỗi không mong muốn ở phía máy chủ."),
                _ => ("Đã xảy ra lỗi", "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.")
            };

            return new ErrorViewModel
            {
                StatusCode = statusCode,
                Title = title,
                Message = message,
                RequestId = requestId,
                Path = path
            };
        }
    }
}