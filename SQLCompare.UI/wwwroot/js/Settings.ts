/**
 * Contains utility methods related to the Settings page
 */
class Settings {
    /**
     * Service URL for the Settings page
     */
    private static readonly pageUrl: string = "/SettingsPageModel";

    /**
     * Service URL for saving the settings
     */
    private static readonly saveUrl: string = `${Settings.pageUrl}?handler=Save`;

    /**
     * Open the Settings page
     */
    public static Open(): void {
        Utility.OpenModalDialog(this.pageUrl, "GET");
    }

    /**
     * Save the settings
     */
    public static Save(): void {
        const data: object = Utility.SerializeJSON($("#Settings"));
        Utility.AjaxCall(this.saveUrl, "POST", data, (): void => {
            // Load the new localization
            Localization.Load();
            // Recreate the menu with the new language
            Menu.CreateMenu();

            Utility.OpenModalDialog("/WelcomePageModel", "GET");
        });
    }
}
