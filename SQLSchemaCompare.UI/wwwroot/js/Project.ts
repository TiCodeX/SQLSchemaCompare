/**
 * Contains utility methods related to the Project
 */
class Project {
    /**
     * Extension of the project file
     */
    private static readonly projectFileExtension: string = "tcxsc";

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
     * Service URL for starting the comparison
     */
    private static readonly removeRecentUrl: string = `${Project.pageUrl}?handler=RemoveRecentProject`;

    /**
     * Current opened project file
     */
    private static filename: string | undefined;

    /**
     * Defines if the current project is dirty
     */
    private static isDirty = false;

    /**
     * Open the project page
     * @param closePreviousPage Tell if the previous page needs to be closed
     */
    public static async OpenPage(closePreviousPage = true): Promise<void> {
        return PageManager.LoadPage(Page.Project, closePreviousPage).then((): void => {
            MenuManager.ToggleProjectRelatedMenuStatus(true);
            $(".editable-select").on("show.editable-select", (e: JQuery.Event) => {
                const list: JQuery = $(e.target).siblings("ul.es-list") as JQuery;
                list.empty();
                list.append("<li class=\"es-visible\" disabled>Loading...</li>");
                this.LoadDatabaseSelectValues(
                    $(e.target).siblings(".input-group-append").find("button") as JQuery,
                    (e.target as HTMLInputElement).name,
                    ($(e.target).parents(".card")[0] as HTMLDivElement).id,
                );
            });
        });
    }

    /**
     * Set the current project to dirty
     */
    public static SetDirtyState(): void {
        if (!this.isDirty) {
            this.isDirty = true;

            Utility.AjaxCall(this.dirtyUrl, HttpMethod.Post).then(() => {
                MenuManager.ToggleProjectRelatedMenuStatus(true);
            });
        }
    }

    /**
     * Open the new Project page
     * @param ignoreDirty Whether to ignore if the project is dirty or prompt to save
     * @param databaseType The database type
     */
    public static New(ignoreDirty: boolean, databaseType?: DatabaseType): void {
        const data = {
            ignoreDirty,
            databaseType,
        };

        Utility.AjaxCall<string>(this.newUrl, HttpMethod.Post, data).then((response: ApiResponse<string>): void => {
            if (response.Success === true) {
                this.isDirty = false;
                this.filename = undefined;
                this.OpenPage(true);
            } else {
                this.HandleProjectNeedToBeSavedError(response).then((): void => {
                    this.New(true, databaseType);
                }).catch((): void => {
                    // Do nothing
                });
            }
        });
    }

    /**
     * Save the Project
     * @param showDialog Whether to show the save dialog
     */
    public static async Save(showDialog = false): Promise<void> {
        if (PageManager.GetOpenPage() === Page.Project) {
            try {
                await this.Edit();
            } catch (e) {
                return Promise.reject();
            }
        }

        let { filename } = this;
        if (filename === undefined || showDialog) {
            filename = (await electron.remote.dialog.showSaveDialog(
                electron.remote.getCurrentWindow(),
                {
                    title: Localization.Get("TitleSaveProject"),
                    buttonLabel: Localization.Get("ButtonSave"),
                    filters: [
                        {
                            name: Localization.Get("LabelSQLSchemaCompareProjectFile"),
                            extensions: [this.projectFileExtension],
                        },
                    ],
                },
            )).filePath;
        }

        if (Utility.IsNullOrWhitespace(filename)) {
            return Promise.resolve();
        }

        const data: object = JSON.parse(JSON.stringify(filename)) as object;

        return Utility.AjaxCall<object>(this.saveUrl, HttpMethod.Post, data).then((response: ApiResponse<object>): void => {
            if (response.Success === true) {
                this.filename = filename;
                this.isDirty = false;
                MenuManager.ToggleProjectRelatedMenuStatus(true);
                DialogManager.ShowInformation(Localization.Get("TitleSaveProject"), Localization.Get("MessageProjectSavedSuccessfully"));
            } else {
                DialogManager.ShowError(response.ErrorMessage);
            }
        });
    }

    /**
     * Load the Project from the file, if not specified show the open file dialog
     * @param ignoreDirty Whether to ignore if the project is dirty or prompt to save
     * @param filename The Project file path
     */
    public static async Load(ignoreDirty = false, filename?: string): Promise<void> {
        let file = filename;
        if (file === undefined) {
            const filenames: string[] = (await electron.remote.dialog.showOpenDialog(
                electron.remote.getCurrentWindow(),
                {
                    title: Localization.Get("TitleOpenProject"),
                    buttonLabel: Localization.Get("ButtonOpen"),
                    filters: [
                        {
                            name: Localization.Get("LabelSQLSchemaCompareProjectFile"),
                            extensions: [this.projectFileExtension],
                        },
                    ],
                    properties: ["openFile"],
                },
            )).filePaths;

            if (!Array.isArray(filenames) || filenames.length < 1 || Utility.IsNullOrWhitespace(filenames[0])) {
                return Promise.resolve();
            }

            file = filenames.shift();
        }

        return Utility.AjaxCall<string>(
            this.loadUrl,
            HttpMethod.Post,
            { IgnoreDirty: ignoreDirty, Filename: file },
        ).then((response: ApiResponse<string>): void => {
            if (response.Success === true) {
                this.isDirty = false;
                this.filename = file;
                this.OpenPage(true);
            } else {
                this.HandleProjectNeedToBeSavedError(response).then((): void => {
                    this.Load(true, file).catch((): void => {
                        // Do nothing
                    });
                }).catch((): void => {
                    // Do nothing
                });
            }
        });
    }

    /**
     * Serialize the project values in the UI and send them to the service
     */
    public static async Edit<T>(): Promise<ApiResponse<T>> {
        return new Promise<ApiResponse<T>>((resolve, reject): void => {
            const data = Utility.SerializeJSON($("#ProjectPage"));
            if (data === undefined) {
                reject();
            } else {
                Utility.AjaxCall<T>(this.editUrl, HttpMethod.Post, data).then((response: ApiResponse<T>): void => {
                    resolve(response);
                });
            }
        });
    }

    /**
     * Close the project, prompt for save
     * @param ignoreDirty Whether to ignore if the project is dirty or prompt to save
     */
    public static Close(ignoreDirty: boolean): void {
        const data: object = JSON.parse(JSON.stringify(ignoreDirty)) as object;
        Utility.AjaxCall<string>(this.closeUrl, HttpMethod.Post, data).then((response: ApiResponse<string>) => {
            if (response.Success === true) {
                this.isDirty = false;
                this.filename = undefined;
                PageManager.CloseAllPages().then((): void => {
                    MenuManager.ToggleProjectRelatedMenuStatus(false);
                });
            } else {
                this.HandleProjectNeedToBeSavedError(response).then((): void => {
                    this.Close(true);
                }).catch((): void => {
                    // Do nothing
                });
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
        // Disable the button and start rotating it
        button.attr("disabled", "disabled").addClass("spin");
        // Close the dropdown and disable it temporarily
        const select: JQuery = $(`input[name="${selectId}"]`);
        select.trigger("blur").attr("disabled", "disabled");

        const data = Utility.SerializeJSON($(`#${dataDivId}`));
        if (data === undefined) {
            select.removeAttr("disabled");
            button.removeClass("spin").removeAttr("disabled");
            setTimeout(() => {
                select.editableSelect("hide");
            }, 0);

            return;
        }

        const databaseType: JQuery = $("[name='DatabaseType']");
        $.extend(data, { DatabaseType: databaseType.val() });

        Utility.AjaxCall<string[]>(this.loadDatabaseListUrl, HttpMethod.Post, data).then((response: ApiResponse<string[]>): void => {
            if (response.Success === true) {
                select.editableSelect("clear");
                $.each(response.Result, (index: number, value: string): void => {
                    select.editableSelect("add", value);
                    if (Utility.IsNullOrWhitespace(select.val() as string) && index === 0) {
                        select.val(value);
                    }
                });
            } else {
                select.editableSelect("hide");
                DialogManager.ShowError(response.ErrorMessage);
            }
            select.removeAttr("disabled");
            button.removeClass("spin").removeAttr("disabled");

            // If everything is ok, the select should be open, so lets focus it
            if (response.Success === true) {
                select.trigger("focus");
            }
        });
    }

    /**
     * Save the project and perform the comparison
     */
    public static EditAndCompare(): void {
        this.Edit().then((): void => {
            Project.Compare();
        }).catch((): void => {
            // Do nothing
        });
    }

    /**
     * Perform the comparison
     */
    public static Compare(): void {
        Utility.AjaxCall(this.startCompareUrl, HttpMethod.Get).then((): void => {
            TaskManager.CheckTask()
                .then((): void => {
                    // Close the task page and open the main page
                    PageManager.ClosePage();
                    Main.Open();
                })
                .catch((): void => {
                    // Do nothing
                });
        });
    }

    /**
     * Remove the project from the recent list
     * @param filename The Project file path
     */
    public static RemoveRecentProject(filename: string): void {
        const data: object = JSON.parse(JSON.stringify(filename)) as object;

        Utility.AjaxCall(this.removeRecentUrl, HttpMethod.Post, data).then((): void => {
            if (PageManager.GetOpenPage() === Page.Welcome) {
                PageManager.LoadPage(Page.Welcome);
            }
        });
    }

    /**
     * Handle the response with the ProjectNeedToBeSaved error code by prompt the user if wants to save
     * @param response The response to handle
     */
    public static async HandleProjectNeedToBeSavedError(response: ApiResponse<string>): Promise<void> {
        return new Promise<void>((resolve: PromiseResolve<void>, reject: PromiseReject): void => {
            if (response.ErrorCode === ApiErrorCode.ErrorProjectNeedToBeSaved) {
                DialogManager.OpenQuestionDialog(
                    Localization.Get("TitleSaveProject"),
                    Localization.Get("MessageDoYouWantToSaveProjectChanges"),
                    [DialogButton.Yes, DialogButton.No, DialogButton.Cancel],
                ).then((answer: DialogButton): void => {
                    switch (answer) {
                        case DialogButton.Yes:
                            this.Save(false).then((): void => {
                                resolve();
                            }).catch(() => {
                                reject();
                            });
                            break;
                        case DialogButton.No:
                            resolve();
                            break;
                        default:
                    }
                });
            } else {
                DialogManager.ShowError(response.ErrorMessage);
                reject();
            }
        });
    }

    /**
     * Handle the onChange event of the DatabaseType select
     * @param select The jQuery element of the select
     */
    public static HandleDatabaseTypeOnChange(select: JQuery): void {
        const useWindowAuthentication: JQuery = $("input[name$='UseWindowsAuthentication']").parents(".form-group");
        const useAzureAuthentication: JQuery = $("input[name$='UseAzureAuthentication']").parents(".form-group");
        const sourcePort: JQuery = $("input[name='SourcePort']");
        const targetPort: JQuery = $("input[name='TargetPort']");
        switch (parseInt(select.val() as string, 10)) {
            case DatabaseType.MicrosoftSql:
                useWindowAuthentication.show();
                useAzureAuthentication.show();
                this.HandleHostnameOnInput($("input[name='SourceHostname']"), "Source");
                this.HandleHostnameOnInput($("input[name='TargetHostname']"), "Target");
                sourcePort.val($("input[name='DefaultMicrosoftSqlPort']").val() as string);
                targetPort.val($("input[name='DefaultMicrosoftSqlPort']").val() as string);
                break;
            case DatabaseType.MySql:
                useWindowAuthentication.hide();
                useAzureAuthentication.hide();
                $("input[name$='Port']").prop("disabled", false);
                sourcePort.val($("input[name='DefaultMySqlPort']").val() as string);
                targetPort.val($("input[name='DefaultMySqlPort']").val() as string);
                break;
            case DatabaseType.PostgreSql:
                useWindowAuthentication.hide();
                useAzureAuthentication.hide();
                $("input[name$='Port']").prop("disabled", false);
                sourcePort.val($("input[name='DefaultPostgreSqlPort']").val() as string);
                targetPort.val($("input[name='DefaultPostgreSqlPort']").val() as string);
                break;
            case DatabaseType.MariaDb:
                useWindowAuthentication.hide();
                useAzureAuthentication.hide();
                $("input[name$='Port']").prop("disabled", false);
                sourcePort.val($("input[name='DefaultMariaDbPort']").val() as string);
                targetPort.val($("input[name='DefaultMariaDbPort']").val() as string);
                break;
            default:
        }
        this.SetDirtyState();
    }

    /**
     * Handle the onInput event of the Hostname field
     * @param input The jQuery element of the input
     * @param prefix The page prefix (Source/Target)
     */
    public static HandleHostnameOnInput(input: JQuery, prefix: string): void {
        const databaseType = parseInt($("[name='DatabaseType']").val() as string, 10);
        const databaseTypeEnum = DatabaseType[DatabaseType[databaseType] as keyof typeof DatabaseType];
        if (databaseTypeEnum === DatabaseType.MicrosoftSql) {
            $(`input[name='${prefix}Port']`).prop("disabled", (input.val() as string).includes("\\"));
        }
        this.SetDirtyState();
    }

    /**
     * Handle the onChange event of the UseWindowsAuthentication checkbox
     * @param checkbox The jQuery element of the checkbox
     * @param prefix The page prefix (Source/Target)
     */
    public static HandleUseWindowsAuthenticationOnChange(checkbox: JQuery, prefix: string): void {
        const checked: boolean = checkbox.is(":checked");
        $(`input[name='${prefix}Username']`).prop("disabled", checked);
        $(`input[name='${prefix}Password']`).prop("disabled", checked);
        $(`input[name='${prefix}SavePassword']`).prop("disabled", checked);
        if (checked) {
            $(`input[name='${prefix}UseAzureAuthentication']`).prop("checked", false);
        }
        this.SetDirtyState();
    }

    /**
     * Handle the onChange event of the UseAzureAuthentication checkbox
     * @param checkbox The jQuery element of the checkbox
     * @param prefix The page prefix (Source/Target)
     */
    public static HandleUseAzureAuthenticationOnChange(checkbox: JQuery, prefix: string): void {
        const checked: boolean = checkbox.is(":checked");
        $(`input[name='${prefix}Username']`).prop("disabled", checked);
        $(`input[name='${prefix}Password']`).prop("disabled", checked);
        $(`input[name='${prefix}SavePassword']`).prop("disabled", checked);
        if (checked) {
            $(`input[name='${prefix}UseWindowsAuthentication']`).prop("checked", false);
        }
        this.SetDirtyState();
    }

    /**
     * Handle the onChange event of the UseSSL checkbox
     * @param checkbox The jQuery element of the checkbox
     * @param prefix The page prefix (Source/Target)
     */
    public static HandleUseSSLOnChange(checkbox: JQuery, prefix: string): void {
        if (checkbox.is(":checked")) {
            $(`input[name='${prefix}IgnoreServerCertificate']`).parents(".form-group").show();
        } else {
            $(`input[name='${prefix}IgnoreServerCertificate']`).parents(".form-group").hide();
        }
        this.SetDirtyState();
    }

    /**
     * Handle the onMouseOver event of the project option by displaying the related description
     * @param id The id of the option
     */
    public static HandleOptionOnMouseOver(id: string): void {
        $(`.tcx-project-option-descriptions > div[id='${id}']`).show();
    }

    /**
     * Handle the onMouseOut event of the project option by hiding all the descriptions
     */
    public static HandleOptionOnMouseOut(): void {
        $(".tcx-project-option-descriptions > div").hide();
    }

    /**
     * Copy the project settings left/right or exchange
     * @param direction The direction to copy the options
     */
    public static CopySettings(direction: SettingsCopyDirection): void {
        const prefixFrom: string = direction === SettingsCopyDirection.Left ? "Target" : "Source";
        const prefixTo: string = direction === SettingsCopyDirection.Left ? "Source" : "Target";

        const inputFields: string[] = ["Hostname", "Port", "Username", "Password", "Database"];
        const checkboxFields: string[] = ["SavePassword", "UseWindowsAuthentication", "UseSSL"];

        for (const field of inputFields) {
            const tmpValue: string = $(`input[name='${prefixTo}${field}'`).val() as string;
            const tmpDisabled: boolean = $(`input[name='${prefixTo}${field}'`).is(":disabled");

            $(`input[name='${prefixTo}${field}'`).val($(`input[name='${prefixFrom}${field}'`).val() as string);
            $(`input[name='${prefixTo}${field}'`).prop("disabled", $(`input[name='${prefixFrom}${field}'`).is(":disabled"));

            if (direction === SettingsCopyDirection.Exchange) {
                $(`input[name='${prefixFrom}${field}'`).val(tmpValue);
                $(`input[name='${prefixFrom}${field}'`).prop("disabled", tmpDisabled);
            }
        }

        for (const field of checkboxFields) {
            const tmpValue: boolean = $(`input[name='${prefixTo}${field}'`).is(":checked");
            const tmpDisabled: boolean = $(`input[name='${prefixTo}${field}'`).is(":disabled");

            $(`input[name='${prefixTo}${field}'`).prop("checked", $(`input[name='${prefixFrom}${field}'`).is(":checked"));
            $(`input[name='${prefixTo}${field}'`).prop("disabled", $(`input[name='${prefixFrom}${field}'`).is(":disabled"));

            if (direction === SettingsCopyDirection.Exchange) {
                $(`input[name='${prefixFrom}${field}'`).prop("checked", tmpValue);
                $(`input[name='${prefixFrom}${field}'`).prop("disabled", tmpDisabled);
            }
        }

        this.SetDirtyState();
    }

    /**
     * Remove the filter clause row
     * @param button The element that triggered the function
     */
    public static RemoveFilterClause(button: JQuery): void {
        const tr: JQuery = button.closest("tr");
        const tbody: JQuery = tr.parent();

        // Find the first row of the group
        let trGroupStart: JQuery = tr;
        if (!tr.is("[group-start]")) {
            trGroupStart = trGroupStart.prevAll("tr[group-start]:first");
        }

        // Get the first column which has the rowspan and reduce the value by 1
        const rowSpanCol: JQuery = trGroupStart.children("td:first");
        rowSpanCol.attr("rowspan", parseInt(rowSpanCol.attr("rowspan") ?? "", 10) - 1);

        /* If we are removing the first row of the group, it means that there aren't any other clauses
         * so we should also remove the last row of the group which contains the button to
         * add new clause
         */
        if (tr === trGroupStart) {
            tr.next("tr[group-end]").remove();
            // If we are removing the first clause in the table clear the content of the first cell of the next group which contains the OR label
            if (tr.prev("tr").length === 0) {
                tr.next("tr[group-start]").find("td:first").empty();
            }
        }

        tr.remove();

        // If the group now contains only one clause, enable the button to remove it
        if (trGroupStart.next("tr").is("[group-end]")) {
            trGroupStart.find("td:last > i").show();
        }

        // If the table now contains only one group, hide the remove button
        if (tbody.find("tr[group-start]").length === 1) {
            tbody.find("tr[group-start]:first > td:last > i").hide();

            // If the group has only one clause, remove the required attribute
            const rowCountWithOnlyOneClause = 3;
            if (tbody.find("tr").length === rowCountWithOnlyOneClause) {
                tbody.find("tr[group-start]:first > td > input[name='ProjectOptions[Filtering[Clauses[][Value]]']").removeAttr("required");
            }
        }

        // Reset the form validation
        $("div.tcx-project").removeClass("was-validated");
    }

    /**
     * Add a new filter clause row
     * @param button The element that triggered the function
     */
    public static AddFilterClause(button: JQuery): void {
        const tr: JQuery = button.closest("tr");
        const tbody: JQuery = tr.parent();
        const inputGroup: JQuery = tr.parent().find("input[name='ProjectOptions[Filtering[Clauses[][Group]]']:last");
        const trGroupStart: JQuery = tr.prevAll("tr[group-start]:first");
        const trGroupEnd: JQuery = tr.prev("tr[group-end]");
        let trGroupStartNew: JQuery;

        if (tr.is("[group-end]")) {
            // Add AND clause
            trGroupStartNew = trGroupStart.clone().insertBefore(tr);
            // Remove the attribute and the first column which is spanned
            trGroupStartNew.removeAttr("group-start");
            trGroupStartNew.children("td:first").remove();

            // Get the first column which has the rowspan and increment the value by 1
            const rowSpanCol: JQuery = trGroupStart.children("td:first");
            rowSpanCol.attr("rowspan", parseInt(rowSpanCol.attr("rowspan") ?? "", 10) + 1);

            // Hide the object type select and add the AND label
            trGroupStartNew.find("select[name='ProjectOptions[Filtering[Clauses[][ObjectType]]']").hide().after("AND");

            // Hide the remove button from the first row
            trGroupStart.find("td:last > i").hide();
        } else {
            // Add OR clause
            trGroupStartNew = trGroupStart.clone().insertBefore(tr);
            // Reset the rowspan on the first column to 1 and write the OR label
            trGroupStartNew.children("td:first").attr("rowspan", "2").text("OR");
            // Increment the group number
            trGroupStartNew.find("input[name='ProjectOptions[Filtering[Clauses[][Group]]']").val(parseInt(inputGroup.val() as string, 10) + 1);
            trGroupEnd.clone().insertAfter(trGroupStartNew);

            // Show the remove button on the first clause of the first group if it there is only one clause
            if (tbody.find("tr[group-start]:first").next("tr").is("[group-end]")) {
                tbody.find("tr[group-start]:first > td:last > i").show();
            }
        }

        // Remove the value of the cloned field and mark it as required
        trGroupStartNew.find("input[name='ProjectOptions[Filtering[Clauses[][Value]]']").val("").attr("required", "required");
        // Show the remove button on the new row
        trGroupStartNew.find("td:last > i").show();
        // Set the required attribute also to the first clause of the first group
        tbody.find("tr[group-start]:first > td > input[name='ProjectOptions[Filtering[Clauses[][Value]]']").attr("required", "required");

        // Reset the form validation
        $("div.tcx-project").removeClass("was-validated");
    }
}
