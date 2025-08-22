namespace LogScrub.Gui.Common
{
    /// <summary>
    /// Represents the result of an operation that may succeed or fail
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// Indicates whether the operation failed
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        public string? Error { get; protected set; }

        /// <summary>
        /// Exception that caused the failure, if any
        /// </summary>
        public Exception? Exception { get; protected set; }

        protected Result(bool isSuccess, string? error, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static Result Success() => new(true, null);

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        public static Result Failure(string error) => new(false, error);

        /// <summary>
        /// Creates a failed result with an exception
        /// </summary>
        public static Result Failure(Exception exception) => new(false, exception.Message, exception);

        /// <summary>
        /// Creates a failed result with error message and exception
        /// </summary>
        public static Result Failure(string error, Exception exception) => new(false, error, exception);
    }

    /// <summary>
    /// Represents the result of an operation that returns a value
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// The value returned by the operation (only valid if IsSuccess is true)
        /// </summary>
        public T? Value { get; private set; }

        private Result(bool isSuccess, T? value, string? error, Exception? exception = null) 
            : base(isSuccess, error, exception)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a successful result with a value
        /// </summary>
        public static Result<T> Success(T value) => new(true, value, null);

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        public new static Result<T> Failure(string error) => new(false, default, error);

        /// <summary>
        /// Creates a failed result with an exception
        /// </summary>
        public new static Result<T> Failure(Exception exception) => new(false, default, exception.Message, exception);

        /// <summary>
        /// Creates a failed result with error message and exception
        /// </summary>
        public new static Result<T> Failure(string error, Exception exception) => new(false, default, error, exception);

        /// <summary>
        /// Implicitly converts a value to a successful Result
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);
    }
}