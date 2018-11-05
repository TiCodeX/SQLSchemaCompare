namespace SQLCompare.Core.Entities.Api
{
    /// <summary>
    /// Defines all the possible error codes that can be returned in the account service response
    /// </summary>
    public enum EErrorCode
    {
        /// <summary>
        /// Indicates a successful response
        /// </summary>
        Success = 0,

        /*****************************************
        *** Website Errors (from 1000 to 1999) ***
        *****************************************/

        /// <summary>
        /// The email has not been specified
        /// </summary>
        ErrorEmailNotSpecified = 1000,

        /// <summary>
        /// The specified password is too short
        /// </summary>
        ErrorPasswordTooShort = 1001,

        /// <summary>
        /// The account already exists
        /// </summary>
        ErrorAccountAlreadyExists = 1002,

        /// <summary>
        /// The specified account does not exists
        /// </summary>
        ErrorAccountDoesNotExist = 1003,

        /// <summary>
        /// The specified password is wrong
        /// </summary>
        ErrorWrongPassword = 1004,

        /// <summary>
        /// The specified account is locked
        /// </summary>
        ErrorAccountLocked = 1005,

        /// <summary>
        /// The specified e-mail is not valid
        /// </summary>
        ErrorInvalidEmail = 1006,

        /// <summary>
        /// The password has not been specified
        /// </summary>
        ErrorPasswordNotSpecified = 1007,

        /// <summary>
        /// The request does not specify the product
        /// </summary>
        ErrorProductIsMissing = 1008,

        /// <summary>
        /// The specified user name or password is not valid
        /// </summary>
        ErrorInvalidUsernameOrPassword = 1009,

        /// <summary>
        /// The specified e-mail has not been verified yet
        /// </summary>
        ErrorEmailNotVerified = 1010,

        /// <summary>
        /// The session has expired
        /// </summary>
        ErrorSessionExpired = 1011,

        /// <summary>
        /// The account id is invalid
        /// </summary>
        ErrorInvalidAccountId = 1012,

        /// <summary>
        /// The stripe account already exists with the specified email address
        /// </summary>
        ErrorStripeCustomerAlreadyExists = 1013,

        /// <summary>
        /// The stripe account does not exist with the specified email address
        /// </summary>
        ErrorStripeCustomerDoesNotExists = 1014,

        /// <summary>
        /// The language code has not been specified
        /// </summary>
        ErrorNoLanguageSpecified = 1015,

        /// <summary>
        /// The application version is too old and needs to be updated
        /// </summary>
        ErrorApplicationUpdateNeeded = 1021,

        /*******************************************
        *** Website Warnings (from 2000 to 2999) ***
        *******************************************/

        /********************************************
        *** SqlCompare Errors (from 3000 to 3999) ***
        ********************************************/

        /// <summary>
        /// The redirect url for the login process is null
        /// </summary>
        ErrorRedirectUrlIsNull = 3000,

        /// <summary>
        /// The session token received by the login process is null or empty
        /// </summary>
        ErrorSessionTokenIsNullOrEmpty = 3001,

        /// <summary>
        /// The account has no subscription
        /// </summary>
        ErrorNoSubscriptionAvailable = 3002,

        /// <summary>
        /// The trial subscription is expired
        /// </summary>
        ErrorTrialSubscriptionExpired = 3003,

        /// <summary>
        /// The subscription is expired
        /// </summary>
        ErrorSubscriptionExpired = 3004,

        /// <summary>
        /// Generic unexpected error
        /// </summary>
        ErrorUnexpected = 3005,

        /// <summary>
        /// Cannot load the SQLCompare project
        /// </summary>
        ErrorCannotLoadProject = 3006,

        /// <summary>
        /// Cannot save the SQLCompare project
        /// </summary>
        ErrorCannotSaveProject = 3007,

        /// <summary>
        /// The project needs to be saved
        /// </summary>
        ErrorProjectNeedToBeSaved = 3008,

        /**********************************************
        *** SqlCompare Warnings (from 4000 to 4999) ***
        **********************************************/
    }
}
