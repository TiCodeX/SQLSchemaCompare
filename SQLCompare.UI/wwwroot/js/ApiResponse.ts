/**
 * Represent an Api response
 */
class ApiResponse<T> {

    /**
     * Indicates whether a response is success
     */
    public Success: boolean;

    /**
     * The error code
     */
    public ErrorCode: ApiResponse.EErrorCodes;

    /**
     * The error message
     */
    public ErrorMessage: string;

    /**
     * The error message
     */
    public Result: T;

}

namespace ApiResponse {
    /**
     * Defines possible error codes that can be returned by SQLCompare
     */
    export enum EErrorCodes {
        /**
         * Indicates a successful response
         */
        Success = 0,

        /**
         * The account has no subscription
         */
        ErrorNoSubscriptionAvailable = 3002,

        /**
         * The trial subscription is expired
         */
        ErrorTrialSubscriptionExpired = 3003,

        /**
         * The subscription is expired
         */
        ErrorSubscriptionExpired = 3004,

    }
}