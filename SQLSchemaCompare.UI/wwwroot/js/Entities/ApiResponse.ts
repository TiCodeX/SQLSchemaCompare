/**
 * Represent an Api response
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
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
