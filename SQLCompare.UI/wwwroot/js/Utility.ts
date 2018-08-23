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
    public static ApplicationStartup(): void {
        // Disable context menu
        window.addEventListener("contextmenu", (e: PointerEvent) => {
            e.preventDefault();
        }, false);

        // Prevent app zooming
        if (electron !== undefined) {
            electron.webFrame.setVisualZoomLevelLimits(1, 1);
            electron.webFrame.setLayoutZoomLevelLimits(0, 0);
        }

        // Enable bootstrap tooltips and popovers
        $("[data-toggle='tooltip']").tooltip();
        $("[data-toggle='popover']").popover();

        Localization.Load();
    }

    /**
     * Encode the HTML tags inside the string
     * @param s - The string to encode
     * @returns The string properly encoded
     */
    public static EncodeHtmlEntities(s: string): string {
        return $("<div/>").text(s).html();
    }

    /**
     * Contact electron to open an external browser at the specified url
     * @param url - The url to be opened in external browser
     */
    public static OpenExternalBrowser(url: string): void {
        electron.shell.openExternal(url);
    }

    /**
     * Decode the HTML tags inside the string
     * @param s - The string to decode
     * @returns The decoded string
     */
    public static DecodeHtmlEntities(s: string): string {
        return $("<div/>").html(s).text();
    }

    /**
     * Indicates whether the specified string is null or an Empty string.
     * @param s - The string to test.
     */
    public static IsNullOrEmpty(s: string): boolean {
        return s === null || typeof s === "undefined" || s.length < 1;
    }

    /**
     * Indicates whether a specified string is null, empty, or consists only of white-space characters.
     * @param s - The string to test.
     */
    public static IsNullOrWhitespace(s: string): boolean {
        return this.IsNullOrEmpty(s) || s.trim().length < 1;
    }

    /**
     * Get a Logger for the specified category
     * @param category The logger category
     */
    public static GetLogger(category: string): Logger {
        return new Logger(category);
    }

    /**
     * Perform an ajax call and open the modal dialog filled with the response
     * @param url The URL of the ajax call
     * @param method The method (GET/POST)
     * @param data? The object data to send when the method is POST
     * @param callbackFunction? A function which will be called after opening the dialog
     */
    public static OpenModalDialog(url: string, method: Utility.HttpMethod, data?: object, callbackFunction?: () => void): void {
        this.AjaxCall(url, method, data, (result: string): void => {
            $("#myModalBody").html(result);
            $("#myModal").modal("show");
            $(".tab-pane").matchHeight({
                byRow: false,
            });
            if (callbackFunction !== undefined) {
                callbackFunction();
            }
        });
    }

    /**
     * Check if the modal dialog is open
     * @returns Whether the modal dialog is open
     */
    public static IsModalDialogOpen(): boolean {
        return $("#myModal").is(":visible");
    }

    /**
     * Close the modal dialog
     */
    public static CloseModalDialog(): void {
        $("#myModal").modal("hide");
        $("#myModalBody").empty();
    }

    /**
     * Parse all the input elements in JSON format
     * @param element - The container to search for input elements
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

        Utility.AjaxCall(url, method, data, (result: Array<string>): void => {
            select.find("option").remove();
            let options: string = "";
            $.each(result, (index: number, value: string): void => {
                options += `<option value="${value}">${value}</option>`;
            });
            select.append(options);
            select.removeAttr("disabled");
            button.removeClass("spin").removeAttr("disabled");
        });
    }

    /**
     * Perform an asynchronous ajax call
     * @param url - The URL of the ajax call
     * @param method - The method (GET/POST)
     * @param data - The object data to send when the method is POST
     * @param successCallback - The callback function in case of success
     */
    public static AjaxCall(url: string, method: Utility.HttpMethod, data: object, successCallback: JQuery.Ajax.SuccessCallback<object>): void {
        this.AjaxCallInternal(url, method, true, data, successCallback);
    }

    /**
     * Perform an synchronous ajax call
     * @param url - The URL of the ajax call
     * @param method - The method (GET/POST)
     * @param data - The object data to send when the method is POST
     * @param successCallback - The callback function in case of success
     */
    public static AjaxSyncCall(url: string, method: Utility.HttpMethod, data: object, successCallback: JQuery.Ajax.SuccessCallback<object>): void {
        this.AjaxCallInternal(url, method, false, data, successCallback);
    }

    /**
     * Perform an ajax call
     * @param url - The URL of the ajax call
     * @param method - The method (GET/POST)
     * @param async - Whether to perform a synchronous or asynchronous call
     * @param data - The object data to send when the method is POST
     * @param successCallback - The callback function in case of success
     */
    private static AjaxCallInternal(url: string, method: Utility.HttpMethod, async: boolean, data: object, successCallback: JQuery.Ajax.SuccessCallback<object>): void {
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
        $.ajax(url, {
            type: ajaxMethod,
            beforeSend: (xhr: JQuery.jqXHR): void => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $("input:hidden[name='__RequestVerificationToken']").val().toString());
            },
            contentType: "application/json",
            data: data !== undefined ? JSON.stringify(data) : "",
            cache: false,
            async: async,
            success: successCallback,
            error: (error: JQuery.jqXHR): void => {
                this.logger.error(`Error executing AjaxCall: ${error.responseText}`);
                $("#myModalBody").html(error.responseText);
                $("#myModal").modal("show");
            },
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
