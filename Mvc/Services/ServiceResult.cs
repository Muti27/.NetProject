namespace Mvc.Services
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T? data) => new() { Success = true, Data = data };
        public static ServiceResult<T> Failed(string message) => new() { Success = false, Message = message };
    }
}
