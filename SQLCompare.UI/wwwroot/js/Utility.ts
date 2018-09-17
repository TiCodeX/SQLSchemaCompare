/* tslint:disable:no-require-imports no-implicit-dependencies */
declare const amdRequire: Require;
const electron: Electron.AllElectron = (typeof require !== "undefined" ? <Electron.AllElectron>require("electron") : undefined);
/* tslint:enable:no-require-imports no-implicit-dependencies */

/**
 * Contains various utility methods
 */
class Utility {
    /**
     * Logger instance for this class
     */
    private static readonly logger: Logger = Utility.GetLogger("Utility");

    /**
     * Set the standard settings when the app starts
     */
    public static async ApplicationStartup(): Promise<void> {
        // Disable context menu
        window.addEventListener("contextmenu", (e: PointerEvent) => {
            e.preventDefault();
        }, false);

        // Enable bootstrap tooltips and popovers
        $("[data-toggle='tooltip']").tooltip();
        $("[data-toggle='popover']").popover();

        await Localization.Load();

        if (electron !== undefined) {
            // Prevent app zooming
            electron.webFrame.setVisualZoomLevelLimits(1, 1);
            electron.webFrame.setLayoutZoomLevelLimits(0, 0);
            // Register electron callbacks
            electron.ipcRenderer.on("UpdateAvailable", (event: Electron.Event, info: { platform: string; readyToBeInstalled: boolean; version: string }) => {
                if (info !== null && info.version !== "") {
                    if (info.platform === "linux") {
                        let message: string = "<a href=\"#\" class=\"close\" data-dismiss=\"alert\">&times;</a>";
                        message += `<strong>${Localization.Get("NotificationNewVersionAvailable")}</strong>`;
                        message += "<br/>";
                        message += `${Localization.Get("NotificationNewVersionAvailableMessage").replace("{0}", info.version)}`;
                        $("#myNotification").html(message).show();
                    } else {
                        // Windows & MacOS
                        if (info.readyToBeInstalled) {
                            let message: string = "<button type=\"button\" class=\"btn btn-primary float-right\" onclick=\"electron.ipcRenderer.send('QuitAndInstall');\">";
                            message += `${Localization.Get("ButtonUpdateAndRestart")}`;
                            message += "</button>";
                            message += `<strong>${Localization.Get("NotificationNewVersionAvailable")}</strong>`;
                            message += "<br/>";
                            message += `${Localization.Get("NotificationUpdateReadyToBeInstalled").replace("{0}", info.version)}`;
                            $("#myNotification").html(message).show();
                        }
                    }
                }
            });
            electron.ipcRenderer.send("CheckUpdateAvailable");
        }
    }

    /**
     * Contact electron to open an external browser at the specified url
     * @param url The url to be opened in external browser
     */
    public static OpenExternalBrowser(url: string): void {
        electron.shell.openExternal(url);
    }

    /**
     * Indicates whether the specified string is null or an Empty string.
     * @param s The string to test
     */
    public static IsNullOrEmpty(s: string): boolean {
        return s === null || typeof s === "undefined" || s.length < 1;
    }

    /**
     * Indicates whether a specified string is null, empty, or consists only of white-space characters.
     * @param s The string to test
     */
    public static IsNullOrWhitespace(s: string): boolean {
        return this.IsNullOrEmpty(s) || s.trim().length < 1;
    }

    /**
     * Get a Logger for the specified category
     * @param category The logger category
     * @return The logger
     */
    public static GetLogger(category: string): Logger {
        return new Logger(category);
    }

    /**
     * Parse all the input elements in JSON format
     * @param element The container to search for input elements
     * @returns The serialized JSON object
     */
    public static SerializeJSON(element: JQuery): object {
        // Ref: https://github.com/marioizquierdo/jquery.serializeJSON#options
        const settings: SerializeJSONSettings = {
            useIntKeysAsArrayIndex: true,
            checkboxUncheckedValue: "false",
        };

        // Wrap content with a form
        const form: JQuery = element.wrapInner("<form></form>").find("> form");

        // Serialize inputs
        const result: object = <object>element.find("> form").serializeJSON(settings);

        // Eliminate newly created form
        form.contents().unwrap();

        return result;
    }

    /**
     * Perform an ajax call to retrieve the select values
     * @param button The button jQuery element that triggered the load
     * @param select The select jQuery element
     * @param url The URL of the ajax call
     * @param method The method (GET/POST)
     * @param dataDiv The div with the data to serialize
     */
    public static LoadSelectValues(button: JQuery, select: JQuery, url: string, method: Utility.HttpMethod, dataDiv: JQuery): void {
        // Disable the button and start rotating it
        button.attr("disabled", "disabled").addClass("spin");
        // Close the dropdown and disable it temporarily
        select.trigger("blur").attr("disabled", "disabled");

        const data: object = Utility.SerializeJSON(dataDiv);

        Utility.AjaxCall(url, method, data).then((response: ApiResponse<Array<string>>): void => {
            select.find("option").remove();
            let options: string = "";
            $.each(response.Result, (index: number, value: string): void => {
                options += `<option value="${value}">${value}</option>`;
            });
            select.append(options);
            select.removeAttr("disabled");
            button.removeClass("spin").removeAttr("disabled");
        });
    }

    /**
     * Perform an asynchronous ajax call
     * @param url The URL of the ajax call
     * @param method The method (GET/POST)
     * @param data The object data to send when the method is POST
     * @return The ApiResponse
     */
    public static async AjaxCall<T>(url: string, method: Utility.HttpMethod, data?: object): Promise<ApiResponse<T>> {
        let ajaxMethod: string;
        switch (method) {
            case Utility.HttpMethod.Get:
                ajaxMethod = "GET";
                break;
            case Utility.HttpMethod.Post:
                ajaxMethod = "POST";
                break;
            default:
                ajaxMethod = "GET";
        }

        this.logger.debug(`Executing AjaxCall... (Method=${ajaxMethod} Url=${url})`);

        return new Promise<ApiResponse<T>>((resolve: PromiseResolve<ApiResponse<T>>): void => {
            $.ajax(url, {
                type: ajaxMethod,
                beforeSend: (xhr: JQuery.jqXHR): void => {
                    xhr.setRequestHeader("XSRF-TOKEN",
                        $("input:hidden[name='__RequestVerificationToken']").val().toString());
                },
                contentType: "application/json",
                data: data !== undefined ? JSON.stringify(data) : "",
                cache: false,
                async: false,
                success: (response: ApiResponse<T>): void => {
                    resolve(response);
                },
                error: (error: JQuery.jqXHR): void => {
                    this.logger.error(`Error executing AjaxCall: ${error.responseText}`);
                    DialogManager.ShowError("Error", "An unexpected error occured");
                },
            });
        });
    }

    /**
     * Perform an ajax call to load the html page
     * @param url The URL of the ajax call
     * @return The html of the page
     */
    public static async AjaxGetPage(url: string): Promise<string> {
        this.logger.debug(`Executing AjaxGetPage... (Url=${url})`);

        return new Promise<string>((resolve: PromiseResolve<string>): void => {
            $.ajax(url, {
                type: "GET",
                beforeSend: (xhr: JQuery.jqXHR): void => {
                    xhr.setRequestHeader("XSRF-TOKEN",
                        $("input:hidden[name='__RequestVerificationToken']").val().toString());
                },
                contentType: "application/json",
                data: "",
                cache: false,
                async: false,
                success: (response: string): void => {
                    resolve(response);
                },
                error: (error: JQuery.jqXHR): void => {
                    this.logger.error(`Error executing AjaxGetPage: ${error.responseText}`);
                    DialogManager.ShowError("Error", "An unexpected error occured");
                },
            });
        });
    }
}

namespace Utility {
    /**
     * HTTP Method for the ajax call
     */
    export enum HttpMethod {
        /**
         * HTTP Method GET
         */
        Get,
        /**
         * HTTP Method POST
         */
        Post,
    }
}
