namespace SV20T1020136.BusinessLayers
{
    /// <summary>
    /// Khởi tạo và lưu trữ các thông tin cấu hình của BusinessLayer
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Chuỗi thông số kết nối với CSDL
        /// </summary>
        public static string ConnectionString { get; private set; } = "";

        /// <summary>
        /// Hàm khởi tạo cấu hình cho BusinessLayer
        /// Hàm này phải gọi trước khi chạy ứng dụng
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            Configuration.ConnectionString = connectionString;
        }
    }
}

//1. Lớp static là gì, có đặc điềm như thế nào
//2. Hàm, thuộc tính static khác với hàm/thuộc tính không phải static ở điểm nào