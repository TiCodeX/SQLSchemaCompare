/**
 * HTTP Method for the ajax call
 */
enum HttpMethod {
    /**
     * HTTP Method GET
     */
    Get = 0,
    /**
     * HTTP Method POST
     */
    Post = 1,
}

/**
 * Defines possible error codes that can be returned by SQLSchemaCompare
 */
enum ApiErrorCode {
    /**
     * Indicates a successful response
     */
    Success = 0,

    /**
     * Unexpected error
     */
    ErrorUnexpected = 1020,

    /**
     * The application need to be updated
     */
    ErrorApplicationUpdateNeeded = 1021,

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

    /**
     * The project need to be saved
     */
    ErrorProjectNeedToBeSaved = 3008,
}

/**
 * Defines the compare directions
 */
enum CompareDirection {
    /**
     * Represent the source database
     */
    Source = 0,

    /**
     * Represent the target database
     */
    Target = 1,
}

/**
 * Dialog buttons
 */
enum DialogButton {
    /**
     * Yes
     */
    Yes = 0,
    /**
     * No
     */
    No = 1,
    /**
     * Cancel
     */
    Cancel = 2,
}

/**
 * The editor type
 */
enum EditorType {
    /**
     * Normal editor
     */
    Normal = 0,
    /**
     * Diff editor
     */
    Diff = 1,
}

/**
 * Pages handled by PageManager
 */
enum Page {
    /**
     * The main page
     */
    Main = 0,
    /**
     * The welcome page
     */
    Welcome = 1,
    /**
     * The project page
     */
    Project = 2,
    /**
     * The task status page
     */
    TaskStatus = 3,
}

/**
 * The database type
 */
enum DatabaseType {
    /**
     * Microsoft SQL Server
     */
    MicrosoftSql = 0,

    /**
     * MySQL
     */
    MySql = 1,

    /**
     * PostgreSQL
     */
    PostgreSql = 2,

    /**
     * MariaDB
     */
    MariaDb = 3,
}

/**
 * The settings copy direction
 */
enum SettingsCopyDirection {
    /**
     * Left
     */
    Left = 0,

    /**
     * Right
     */
    Right = 1,

    /**
     * Exchange
     */
    Exchange = 2,
}
