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
        DialogManager.OpenModalDialog(Localization.Get("MenuSettings"), this.pageUrl, "300px").catch(() => {
            // Do nothing
        });
    }

    /**
     * Save the settings
     * @param projectIsOpen Whether the project is open or not, in order to show the correct page
     */
    public static Save(projectIsOpen: boolean): void {
        const data = Utility.SerializeJSON($("#Settings"));
        if (data === undefined) {
            return;
        }

        Utility.AjaxCall(this.saveUrl, HttpMethod.Post, data).then(async () => {
            // Load the new localization
            await Localization.Load().then(async () => {
                // Recreate the menu with the new language
                await MenuManager.CreateMenu().then((): void => {
                    MenuManager.ToggleProjectRelatedMenuStatus(projectIsOpen);
                    MenuManager.ToggleMainOpenRelatedMenuStatus(PageManager.GetOpenPage() === Page.Main);
                });
                // Close the modal and reopen the current page
                DialogManager.CloseModalDialog();
                await PageManager.RefreshOpenPages();
            });
        }).catch(() => {
            // Do nothing
        });
    }
}
