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
     * Service URL for making the Project dirty
     */
    private static readonly dirtyUrl: string = `${Project.pageUrl}?handler=SetProjectDirtyState`;

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
     * Defines if the current project is dirty
     */
    private static isDirty: boolean = false;

    /**
     * Set the current project to dirty
     */
    public static SetDirtyState(): void {
        if (!this.isDirty) {
            this.isDirty = true;

            Utility.AjaxCall(this.dirtyUrl, Utility.HttpMethod.Post, undefined, () => {
                Menu.ToggleProjectRelatedMenuStatus(true);
            });
        }
    }

    /**
     * Check if the project is open
     * @returns Whether the project is open
     */
    public static IsProjectPageOpen(): boolean {
        return PageManager.GetOpenPage() === PageManager.Page.Project;
    }

    /**
     * Open the Project page
     */
    public static Open(): void {
        PageManager.LoadPage(PageManager.Page.Project, false);
    }

    /**
     * Open the new Project page
     */
    public static New(ignoreDirty: boolean): void {
        const data: object = <object>JSON.parse(JSON.stringify(ignoreDirty));

        Utility.AjaxCall(this.newUrl, Utility.HttpMethod.Post, data, (response: ApiResponse<string>): void => {
            if (response.Success) {
                this.isDirty = false;
                this.filename = undefined;
                PageManager.LoadPage(PageManager.Page.Project, true, (): void => {
                    Menu.ToggleProjectRelatedMenuStatus(true);
                });
            }
            else {
                if (response.ErrorCode === ApiResponse.EErrorCodes.ErrorProjectNeedToBeSaved) {
                    DialogManager.OpenSaveQuestionDialog((answer: number, checked: boolean): void => {
                        switch (answer) {
                            case DialogManager.SaveDialogAnswers.Yes:
                                this.Save(false, (): void => { this.New(false); });
                                break;
                            case DialogManager.SaveDialogAnswers.No:
                                this.New(true);
                                break;
                            default:
                        }
                    });
                }
                else {
                    DialogManager.ShowError(Localization.Get("TitleError"), response.ErrorMessage);
                }
            }
        });
    }

    /**
     * Save the Project
     * @param showDialog Whether to show the save dialog
     */
    public static Save(showDialog: boolean = false, saveCallback?: () => void): void {
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

            Utility.AjaxCall(this.saveUrl, Utility.HttpMethod.Post, data, (response: ApiResponse<object>): void => {
                if (response.Success) {
                    this.filename = filename;
                    this.isDirty = false;
                    Menu.ToggleProjectRelatedMenuStatus(true);
                    DialogManager.ShowInformation(Localization.Get("TitleSaveProject"), Localization.Get("MessageProjectSavedSuccessfully"));
                    if (saveCallback !== undefined) {
                        saveCallback();
                    }
                }
                else {
                    DialogManager.ShowError(Localization.Get("TitleError"), response.ErrorMessage);
                }
            });
        };

        if (this.IsProjectPageOpen()) {
            this.Edit().then((): void => {
                saveCall();
            });
        }
        else {
            saveCall();
        }
    }

    /**
     * Load the Project from the file, if not specified show the open file dialog
     * @param filename The Project file path
     */
    public static Load(ignoreDirty: boolean, filename?: string): void {
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

        Utility.AjaxCall(this.loadUrl, Utility.HttpMethod.Post, { IgnoreDirty: ignoreDirty, Filename: file }, (response: ApiResponse<object>): void => {
            if (response.Success) {
                this.isDirty = false;
                this.filename = file;
                PageManager.LoadPage(PageManager.Page.Project, true, (): void => {
                    Menu.ToggleProjectRelatedMenuStatus(true);
                });
            }
            else {
                if (response.ErrorCode === ApiResponse.EErrorCodes.ErrorProjectNeedToBeSaved) {
                    DialogManager.OpenSaveQuestionDialog((answer: number, checked: boolean): void => {
                        switch (answer) {
                            case DialogManager.SaveDialogAnswers.Yes:
                                this.Save(false, (): void => { this.Load(false, file); });
                                break;
                            case DialogManager.SaveDialogAnswers.No:
                                this.Load(true, file);
                                break;
                            default:
                        }
                    });
                }
                else {
                    DialogManager.ShowError(Localization.Get("TitleError"), response.ErrorMessage);
                }
            }
        });
    }

    /**
     * Serialize the project values in the UI and send them to the service
     * @param successCallback - The callback function in case of success
     */
    public static async Edit<T>(): Promise<ApiResponse<T>> {
        const data: object = Utility.SerializeJSON($("#ProjectPage"));

        return new Promise<ApiResponse<T>>((resolve: PromiseResolve<ApiResponse<T>>): void => {
            Utility.AjaxCall(this.editUrl, Utility.HttpMethod.Post, data, (response: ApiResponse<T>): void => {
                resolve(response);
            });
        });
    }

    /**
     * Close the project, prompt for save
     */
    public static Close(ignoreDirty: boolean): void {
        const data: object = <object>JSON.parse(JSON.stringify(ignoreDirty));
        Utility.AjaxCall(this.closeUrl, Utility.HttpMethod.Post, data, (response: ApiResponse<string>) => {
            if (response.Success) {
                this.isDirty = false;
                this.filename = undefined;
                PageManager.LoadPage(PageManager.Page.Project, true, (): void => {
                    PageManager.ClosePage();
                    Menu.ToggleProjectRelatedMenuStatus(false);
                });
            }
            else {
                if (response.ErrorCode === ApiResponse.EErrorCodes.ErrorProjectNeedToBeSaved) {
                    DialogManager.OpenSaveQuestionDialog((answer: number, checked: boolean): void => {
                        switch (answer) {
                            case DialogManager.SaveDialogAnswers.Yes:
                                this.Save(false, (): void => { this.Close(false); });
                                break;
                            case DialogManager.SaveDialogAnswers.No:
                                this.Close(true);
                                break;
                            default:
                        }
                    });
                }
                else {
                    DialogManager.ShowError(Localization.Get("TitleError"), response.ErrorMessage);
                }
            }
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
        this.Edit().then((): void => {
            Project.Compare();
        });
    }

    /**
     * Perform the comparison
     */
    public static Compare(): void {
        Utility.AjaxCall(this.startCompareUrl, Utility.HttpMethod.Get, undefined, (): void => {
            // TODO: move the polling functionality in Utility
            const pollingTime: number = 500;
            const polling: VoidFunction = (): void => {
                setTimeout(() => {
                    if ($("#stopPolling").length > 0) {
                        // Open the main page only if there aren't failed tasks
                        if ($("#taskFailed").length === 0) {
                            DialogManager.CloseModalDialog();
                            Main.Open();
                        }
                    } else {
                        DialogManager.OpenModalDialog("/TaskStatusPageModel", Utility.HttpMethod.Get);
                        polling();
                    }
                }, pollingTime);
            };
            polling();
        });
    }
}
