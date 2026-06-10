namespace WUIAM.DTOs
{
    public class ApiResponse<T>
        where T : class
    
    {
        public string Message { get; set; }
        public bool Status { get; set; } // true = success, false = error/failure
        public T? Data { get; set; } // Optional payload
        public object? Error { get; set; } // Optional error details

        public ApiResponse(string message, bool status, T? data = default,int? code=default, object? error = null)
        {
            Message = message;
            Status = status;
            Data = data;
            Error = error;
        }

        // Helper static methods
        public static ApiResponse<T> Success(string message, T data)
            => new(message, true, data);

        public static ApiResponse<T> Failure(string message, object? error = null)
            => new(message, false, default,default, error);
    }

}
