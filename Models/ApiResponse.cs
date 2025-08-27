namespace OrderManagementAPI.Models
{
    public class ApiResponse<T>
    {
        public Status Status { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorCode { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "İşlem başarılı.")
        {
            return new ApiResponse<T>
            {
                Status = Status.Success,
                ResultMessage = message,
                Data = data
            };
        }

        public static ApiResponse<T> Failed(string message, string errorCode = null)
        {
            return new ApiResponse<T>
            {
                Status = Status.Failed,
                ResultMessage = message,
                ErrorCode = errorCode
            };
        }
    }
}