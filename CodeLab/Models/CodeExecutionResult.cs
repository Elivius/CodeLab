namespace CodeLab.Models
{
    public sealed class CodeExecutionResult
    {
        // True when the execution service returns a valid response.
        // This does not mean the learner passed the challenge.
        public bool RequestSucceeded { get; set; }

        // Output returned by JDoodle.
        // It may also contain compilation or runtime error messages.
        public string Output { get; set; } = string.Empty;

        // A user-facing message for connection, quota,
        // authentication, or other service failures.
        public string ErrorMessage { get; set; } = string.Empty;

        // JDoodle's response status, when available.
        public int? StatusCode { get; set; }

        public string CpuTime { get; set; } = string.Empty;

        public string Memory { get; set; } = string.Empty;
    }
}