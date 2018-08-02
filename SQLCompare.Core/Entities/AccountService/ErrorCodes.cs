namespace SQLCompare.Core.Entities.AccountService
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

        /*********************************
        *** Errors (from 1000 to 1999) ***
        *********************************/

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

        /***********************************
        *** Warnings (from 2000 to 2999) ***
        ***********************************/
    }
}
