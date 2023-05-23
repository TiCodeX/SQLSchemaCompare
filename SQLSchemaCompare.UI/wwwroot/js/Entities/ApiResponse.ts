/**
 * Represent an Api response
 */
class ApiResponse<T> {
    /**
     * Indicates whether a response is success
     */
    public Success?: boolean;

    /**
     * The error code
     */
    public ErrorCode?: ApiErrorCode;

    /**
     * The error message
     */
    public ErrorMessage?: string;

    /**
     * The error message
     */
    public Result?: T;
}
