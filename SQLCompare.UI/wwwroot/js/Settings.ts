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
        Utility.OpenModalDialog(this.pageUrl, Utility.HttpMethod.Get, undefined, () => {
            $(".modal-dialog").css("max-width", "300px");
        });
    }

    /**
     * Save the settings
     * @param projectIsOpen Whether the project is open or not, in order to show the correct page
     */
    public static Save(projectIsOpen: boolean): void {
        const data: object = Utility.SerializeJSON($("#Settings"));
        Utility.AjaxCall(this.saveUrl, Utility.HttpMethod.Post, data, (): void => {
            // Load the new localization
            Localization.Load();
            // Recreate the menu with the new language
            Menu.CreateMenu();
            Menu.ToggleProjectRelatedMenuStatus(projectIsOpen);

            if (projectIsOpen) {
                Main.Open();
            } else {
                Utility.OpenWelcomePage();
            }
        });
    }
}
