/**
 * Contains utility methods related to the Settings page
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
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
        void DialogManager.OpenModalDialog(Localization.Get("MenuSettings"), this.pageUrl, "300px");
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

        void Utility.AjaxCall(this.saveUrl, HttpMethod.Post, data).then((): void => {
            // Load the new localization
            void Localization.Load().then((): void => {
                // Recreate the menu with the new language
                void MenuManager.CreateMenu().then((): void => {
                    MenuManager.ToggleProjectRelatedMenuStatus(projectIsOpen);
                    MenuManager.ToggleMainOpenRelatedMenuStatus(PageManager.GetOpenPage() === Page.Main);
                });
                // Close the modal and reopen the current page
                DialogManager.CloseModalDialog();
                void PageManager.RefreshOpenPages();
            });
        });
    }
}
