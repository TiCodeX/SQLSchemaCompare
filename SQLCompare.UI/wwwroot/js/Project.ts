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
     * Service URL for editing a Project
     */
    private static readonly editUrl: string = `${Project.pageUrl}?handler=EditProject`;

    /**
     * Service URL for loading a Project
     */
    private static readonly closeUrl: string = `${Project.pageUrl}?handler=CloseProject`;

    /**
     * Service URL for retrieving the list of databases
     */
    private static readonly loadDatabaseListUrl: string = `${Project.pageUrl}?handler=LoadDatabaseList`;

    /**
     * Service URL for starting the comparison
     */
    private static readonly startCompareUrl: string = `${Project.pageUrl}?handler=StartCompare`;

    /**
     * Current opened project file
     */
    private static filename: string;

    /**
     * Check if the project is open
     * @returns Whether the project is open
     */
    public static IsProjectPageOpen(): boolean {
        return Utility.IsModalDialogOpen() && $("#ProjectPage").is(":visible");
    }

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
        Utility.OpenModalDialog(this.newUrl, Utility.HttpMethod.Get, undefined, (): void => {
            Menu.ToggleProjectRelatedMenuStatus(true);
        });
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

        const saveCall: () => void = (): void => {
            const data: object = <object>JSON.parse(JSON.stringify(filename));

            Utility.AjaxCall(this.saveUrl,
                Utility.HttpMethod.Post,
                data,
                (): void => {
                    this.filename = filename;
                    alert("Saved successfully!");
                });
        };

        if (this.IsProjectPageOpen()) {
            this.Edit(saveCall);
        } else {
            saveCall();
        }
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

            if (!Array.isArray(filenames) || filenames.length < 1 || Utility.IsNullOrWhitespace(filenames[0])) {
                return;
            }

            file = filenames[0];
        }

        const data: object = <object>JSON.parse(JSON.stringify(file));

        Utility.OpenModalDialog(this.loadUrl, Utility.HttpMethod.Post, data, (): void => {
            this.filename = filename;
            Menu.ToggleProjectRelatedMenuStatus(true);
        });
    }

    /**
     * Serialize the project values in the UI and send them to the service
     * @param successCallback - The callback function in case of success
     */
    public static Edit(successCallback: JQuery.Ajax.SuccessCallback<object>): void {
        const data: object = Utility.SerializeJSON($("#ProjectPage"));

        Utility.AjaxCall(this.editUrl, Utility.HttpMethod.Post, data, successCallback);
    }

    /**
     * Close the project, prompt for save
     */
    public static Close(showWelcome: boolean = false): void {
        Utility.AjaxCall(this.closeUrl, Utility.HttpMethod.Get, undefined, () => {
            this.filename = undefined;
            $("#mainDiv").empty();
            if (showWelcome) {
                Utility.OpenWelcomePage();
            }
            Menu.ToggleProjectRelatedMenuStatus(false);
        });
    }

    /**
     * Load the database values of the select
     * @param button The button jQuery element that triggered the load
     * @param selectId The id of the select
     * @param dataDivId The id of the div with the data to serialize
     */
    public static LoadDatabaseSelectValues(button: JQuery, selectId: string, dataDivId: string): void {
        Utility.LoadSelectValues(button, $(`select[name=${selectId}]`), Project.loadDatabaseListUrl, Utility.HttpMethod.Post, $(`#${dataDivId}`));
    }

    /**
     * Save the project and perform the comparison
     */
    public static EditAndCompare(): void {
        this.Edit((): void => {
            Project.Compare();
        });
    }

    /**
     * Perform the comparison
     */
    public static Compare(): void {
        Utility.AjaxCall(this.startCompareUrl, Utility.HttpMethod.Get, undefined, (): void => {
            // TODO: move the polling functionality in Utility
            const pollingTime: number = 200;
            const polling: VoidFunction = (): void => {
                setTimeout(() => {
                    if ($("#stopPolling").length > 0) {
                        // Open the main page only if there aren't failed tasks
                        if ($("#taskFailed").length === 0) {
                            Main.Open();
                        }
                    } else {
                        Utility.OpenModalDialog("/TaskStatusPageModel", Utility.HttpMethod.Get);
                        polling();
                    }
                }, pollingTime);
            };
            polling();
        });
    }
}
