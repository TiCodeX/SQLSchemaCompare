/**
 * Contains utility methods related to the Project
 */
class Project {
    /**
     * Service URL for a Project page
     */
    public static readonly pageUrl: string = "/Project/ProjectPageModel";

    /**
     * Service URL for a new Project
     */
    private static readonly newUrl: string = `${Project.pageUrl}?handler=NewProject`;

    /**
     * Service URL for saving the Project
     */
    private static readonly saveUrl: string = `${Project.pageUrl}?handler=SaveProject`;

    /**
     * Service URL for loading a Project
     */
    private static readonly loadUrl: string = `${Project.pageUrl}?handler=LoadProject`;

    /**
     * Service URL for loading a Project
     */
    private static readonly closeUrl: string = `${Project.pageUrl}?handler=CloseProject`;

    /**
     * Open the new Project page
     */
    public static New(): void {
        Utility.OpenModalDialog(this.newUrl, "GET");
        Menu.ToggleProjectRelatedMenuStatus(true);
    }

    /**
     * Show the save dialog and save the Project
     */
    public static Save(): void {

        const filename: string = electron.remote.dialog.showSaveDialog(electron.remote.getCurrentWindow(),
            {
                title: "Save Project",
                buttonLabel: "Save Project",
                filters: [
                    {
                        name: "SQL Compare Project",
                        extensions: ["xml"],
                    },
                ],
            });

        if (Utility.IsNullOrWhitespace(filename)) {
            return;
        }

        const data: object = <object>JSON.parse(JSON.stringify(filename));

        Utility.AjaxCall(this.saveUrl, "POST", data, (): void => {
            alert("Saved successfully!");
        });
    }

    /**
     * Load the Project from the file, if not specified show the open file dialog
     * @param filename The Project file path
     */
    public static Load(filename?: string): void {
        let file: string = filename;
        if (file === undefined) {

            const filenames: Array<string> = electron.remote.dialog.showOpenDialog(electron.remote.getCurrentWindow(),
                {
                    title: "Load Project",
                    buttonLabel: "Load Project",
                    filters: [
                        {
                            name: "SQL Compare Project",
                            extensions: ["xml"],
                        },
                    ],
                    properties: ["openFile"],
                });

            if (!Boolean(filenames) || filenames.length < 1 || Utility.IsNullOrWhitespace(filenames[0])) {
                return;
            }

            file = filenames[0];
        }

        const data: object = <object>JSON.parse(JSON.stringify(file));

        Utility.OpenModalDialog(this.loadUrl, "POST", data);
        Menu.ToggleProjectRelatedMenuStatus(true);
    }

    /**
     * Close the project, prompt for save
     */
    public static Close(showWelcome: boolean = false): void {
        Utility.AjaxCall(this.closeUrl, "GET", undefined, () => {
            $("#mainDiv").empty();
            if (showWelcome) {
                Utility.OpenModalDialog("/WelcomePageModel", "GET");
            }
            Menu.ToggleProjectRelatedMenuStatus(false);
        });
    }
}
