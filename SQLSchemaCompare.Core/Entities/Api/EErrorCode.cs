namespace TiCodeX.SQLSchemaCompare.Core.Entities.Api;

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

    /*******************************************
    *** Website Warnings (from 2000 to 2999) ***
    *******************************************/

    /********************************************
    *** SqlCompare Errors (from 3000 to 3999) ***
    ********************************************/

    /// <summary>
    /// Generic unexpected error
    /// </summary>
    ErrorUnexpected = 3005,

    /// <summary>
    /// Cannot load the project
    /// </summary>
    ErrorCannotLoadProject = 3006,

    /// <summary>
    /// Cannot save the project
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
