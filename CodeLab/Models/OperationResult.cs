namespace CodeLab.Models
{
    /// <summary>
    /// The outcome of a BLL operation, with a message the page can show to the user.
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static OperationResult Ok(string message = "")
        {
            return new OperationResult { Success = true, Message = message };
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult { Success = false, Message = message };
        }
    }

    /// <summary>
    /// An OperationResult that also carries data back to the caller, e.g. a list to bind to a grid.
    /// </summary>
    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }

        public static OperationResult<T> Ok(T data, string message = "")
        {
            return new OperationResult<T> { Success = true, Message = message, Data = data };
        }

        public new static OperationResult<T> Fail(string message)
        {
            return new OperationResult<T> { Success = false, Message = message };
        }
    }
}
