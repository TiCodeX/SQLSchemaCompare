/**
 * Contains utility methods related to the Project
 */
class Project {
    /**
     * Service URL for a Project page
     */
    private static readonly pageUrl: string = "/Project/ProjectPageModel";

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
     * Service URL for retrieving the list of databases
     */
    private static readonly loadDatabaseListUrl: string = `${Project.pageUrl}?handler=LoadDatabaseList`;

    /**
     * Service URL for starting the comparation
     */
    private static readonly compareUrl: string = `${Project.pageUrl}?handler=Compare`;

    /**
     * Current opened project file
     */
    private static filename: string;

    /**
     * Open the Project page
     */
    public static Open(): void {
        Utility.OpenModalDialog(this.pageUrl, Utility.HttpMethod.Get);
    }

    /**
     * Open the new Project page
     */
    public static New(): void {
        Utility.OpenModalDialog(this.newUrl, Utility.HttpMethod.Get);
        Menu.ToggleProjectRelatedMenuStatus(true);
    }

    /**
     * Save the Project
     * @param showDialog Whether to show the save dialog
     */
    public static Save(showDialog: boolean = false): void {
        let filename: string = this.filename;
        if (filename === undefined || showDialog) {
            filename = electron.remote.dialog.showSaveDialog(electron.remote.getCurrentWindow(),
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
        }
        if (Utility.IsNullOrWhitespace(filename)) {
            return;
        }

        const data: object = <object>JSON.parse(JSON.stringify(filename));

        Utility.AjaxCall(this.saveUrl, Utility.HttpMethod.Post, data, (): void => {
            this.filename = filename;
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

        Utility.OpenModalDialog(this.loadUrl, Utility.HttpMethod.Post, data);

        this.filename = filename;
        Menu.ToggleProjectRelatedMenuStatus(true);
    }

    /**
     * Close the project, prompt for save
     */
    public static Close(showWelcome: boolean = false): void {
        Utility.AjaxCall(this.closeUrl, Utility.HttpMethod.Get, undefined, () => {
            this.filename = undefined;
            $("#mainDiv").empty();
            if (showWelcome) {
                Utility.OpenModalDialog("/WelcomePageModel", Utility.HttpMethod.Get);
            }
            Menu.ToggleProjectRelatedMenuStatus(false);
        });
    }

    /**
     * Load the database values of the select
     * @param selectId The id of the select
     * @param dataDivId The id of the div with the data to serialize
     */
    public static LoadDatabaseSelectValues(selectId: string, dataDivId: string): void {
        Utility.LoadSelectValues($(`select[name=${selectId}]`), Project.loadDatabaseListUrl, Utility.HttpMethod.Post, $(`#${dataDivId}`));
    }

    /**
     * Perform the comparison
     */
    public static Compare(): void {

        const data: object = Utility.SerializeJSON($("#tabDataSources"));

        Utility.AjaxCall(this.compareUrl, Utility.HttpMethod.Post, data, (): void => {
            // TODO: move the polling functionality in Utility
            const pollingTime: number = 200;
            const polling: VoidFunction = (): void => {
                setTimeout(() => {
                    if ($("#stopPolling").length > 0) {
                        Utility.AjaxCall(Main.pageUrl, Utility.HttpMethod.Get, undefined, (result: string): void => {
                            Utility.CloseModalDialog();
                            $("#mainDiv").html(result);
                        });
                    } else {
                        Utility.OpenModalDialog("/TaskStatusPageModel", Utility.HttpMethod.Get, undefined);
                        polling();
                    }
                }, pollingTime);
            };
            polling();
        });
    }
}
