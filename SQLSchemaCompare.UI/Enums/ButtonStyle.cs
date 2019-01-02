namespace TiCodeX.SQLSchemaCompare.UI.Enums
{
    /// <summary>
    /// List of possible button styles
    /// </summary>
    public enum ButtonStyle
    {
        /// <summary>
        /// Adds basic styling to the button
        /// </summary>
        Basic,

        /// <summary>
        /// Provides extra visual weight and identifies the primary action in a set of buttons
        /// </summary>
        Primary,

        /// <summary>
        /// Provides extra visual weight and identifies the secondary action in a set of buttons
        /// </summary>
        Secondary,

        /// <summary>
        /// Indicates a successful or positive action
        /// </summary>
        Success,

        /// <summary>
        /// Indicates a dangerous or potentially negative action
        /// </summary>
        Danger,

        /// <summary>
        /// Indicates caution should be taken with this action
        /// </summary>
        Warning,

        /// <summary>
        /// Contextual button for informational alert messages
        /// </summary>
        Info,

        /// <summary>
        /// Provides a light color button
        /// </summary>
        Light,

        /// <summary>
        /// Provides a dark color button
        /// </summary>
        Dark,

        /// <summary>
        /// Makes a button look like a link (will still have button behavior)
        /// </summary>
        Link,

        /// <summary>
        /// Custom highlight theme color
        /// </summary>
        TcxHighlight
    }
}
