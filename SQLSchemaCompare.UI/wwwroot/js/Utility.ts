declare const amdRequire: Require;
const electron = require("electron") as Electron.AllElectron;

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
        window.addEventListener("contextmenu", (e: MouseEvent) => {
            e.preventDefault();
        }, false);

        // Enable bootstrap tooltips and popovers
        $(document).tooltip({ selector: "[data-toggle='tooltip']" });
        $(document).on("click", "[data-toggle='tooltip']", () => {
            PageManager.RemoveTooltips();
        });
        $(document).popover({ selector: "[data-toggle='popover']" });
        $(document).on("click", "[data-toggle='popover']", () => {
            PageManager.RemoveTooltips();
        });

        await Localization.Load();

        // Prevent app zooming
        electron.webFrame.setVisualZoomLevelLimits(1, 1);
        electron.webFrame.setLayoutZoomLevelLimits(0, 0);
    }

    /**
     * Contact electron to open an external browser at the specified url
     * @param url The url to be opened in external browser
     */
    public static OpenExternalBrowser(url: string): void {
        electron.shell.openExternal(url);
    }

    /**
     * Close the electron window
     */
    public static QuitWindow(): void {
        electron.remote.getCurrentWindow().close();
    }

    /**
     * Indicates whether the specified string is null or an Empty string.
     * @param s The string to test
     * @returns Whether the specified string is null or an Empty string
     */
    public static IsNullOrEmpty(s?: string): boolean {
        return s === null || typeof s === "undefined" || s.length < 1;
    }

    /**
     * Indicates whether a specified string is null, empty, or consists only of white-space characters.
     * @param s The string to test
     * @returns Whether the specified string is null, empty, or consists only of white-space characters
     */
    public static IsNullOrWhitespace(s?: string): boolean {
        return this.IsNullOrEmpty(s) || (s ?? "").trim().length < 1;
    }

    /**
     * Get a Logger for the specified category
     * @param category The logger category
     * @returns The logger
     */
    public static GetLogger(category: string): Logger {
        return new Logger(category);
    }

    /**
     * Parse all the input elements in JSON format
     * @param element The container to search for input elements
     * @returns The serialized JSON object
     */
    public static SerializeJSON(element: JQuery): object | undefined {
        // Ref: https://github.com/marioizquierdo/jquery.serializeJSON#options
        const settings: SerializeJSONSettings = {
            useIntKeysAsArrayIndex: true,
            checkboxUncheckedValue: "false",
        };

        // Wrap content with a form
        const form: JQuery = element.wrapInner("<form></form>").find("> form");
        // Disable browser default feedback
        form.addClass("novalidate");
        // Override browser default style
        form.css("display", "inherit");
        form.css("margin-top", "inherit");

        try {
            if (element.hasClass("needs-validation")) {
                const valid: boolean = (form[0] as HTMLFormElement).checkValidity();
                element.addClass("was-validated");

                // Check if there are tabs in order to color based on their content validity
                let firstTabWithErrorOpened = false;
                element.find(".tab-content > .tab-pane").each((i: number, item: HTMLElement) => {
                    const navTab: JQuery = element.find(`.nav-tabs > .nav-item > a[href='#${item.id}']`);
                    navTab.removeClass("text-danger text-success");
                    if ($(item).find("*:invalid").length > 0) {
                        navTab.addClass("text-danger");
                        if (!firstTabWithErrorOpened) {
                            navTab.tab("show");
                            firstTabWithErrorOpened = true;
                        }
                    } else {
                        navTab.addClass("text-success");
                    }
                });

                if (!valid) {
                    return undefined;
                }
            }

            // Serialize inputs
            const result: object = form.serializeJSON(settings) as object;

            return result;
        } finally {
            // Eliminate newly created form
            form.contents().unwrap();
        }
    }

    /**
     * Perform an asynchronous ajax call
     * @param url The URL of the ajax call
     * @param method The method (GET/POST)
     * @param data The object data to send when the method is POST
     * @returns The ApiResponse
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

        return new Promise<ApiResponse<T>>((resolve): void => {
            $.ajax(url, {
                type: ajaxMethod,
                beforeSend: (xhr: JQuery.jqXHR): void => {
                    xhr.setRequestHeader("XSRF-TOKEN", $("input:hidden[name='__RequestVerificationToken']").val() as string);
                },
                contentType: "application/json",
                data: data !== undefined ? JSON.stringify(data) : "",
                cache: false,
                async: true,
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
     * @returns The html of the page
     */
    public static async AjaxGetPage(url: string): Promise<string> {
        this.logger.debug(`Executing AjaxGetPage... (Url=${url})`);

        return new Promise<string>((resolve): void => {
            $.ajax(url, {
                type: "GET",
                beforeSend: (xhr: JQuery.jqXHR): void => {
                    xhr.setRequestHeader("XSRF-TOKEN", $("input:hidden[name='__RequestVerificationToken']").val() as string);
                },
                contentType: "application/json",
                data: "",
                cache: false,
                async: false,
                success: (response: string): void => {
                    resolve(response);
                },
                error: (): void => {
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
        Get = 0,
        /**
         * HTTP Method POST
         */
        Post = 1,
    }
}
