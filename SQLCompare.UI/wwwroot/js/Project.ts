/**
 * Contains utility methods related to the Project
 */
class Project {
    /**
     * Service URL for saving the Project
     */
    private static readonly saveUrl: string = "/Project/CompareProject?handler=SaveProject";

    /**
     * Service URL for loading a Project
     */
    private static readonly loadUrl: string = "/Project/CompareProject?handler=LoadProject";

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
     * Show the open file dialog and load the Project
     */
    public static Load(): void {

        const filename: Array<string> = electron.remote.dialog.showOpenDialog(electron.remote.getCurrentWindow(),
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

        if (!Boolean(filename) || filename.length < 1 || Utility.IsNullOrWhitespace(filename[0])) {
            return;
        }

        const data: object = <object>JSON.parse(JSON.stringify(filename[0]));

        Utility.OpenModalDialog(this.loadUrl, "POST", data);
    }
}
