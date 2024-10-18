const electron = (require as NodeRequireFunction)("electron");
const electronRemote = require("@electron/remote") as ElectronRemote;

/**
 * Contains various utility methods
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
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
        window.addEventListener("contextmenu", (event) => {
            event.preventDefault();
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
    }

    /**
     * Contact electron to open an external browser at the specified url
     * @param url The url to be opened in external browser
     */
    public static OpenExternalBrowser(url: string): void {
        void electron.shell.openExternal(url);
    }

    /**
     * Close the electron window
     */
    public static QuitWindow(): void {
        electronRemote.getCurrentWindow().close();
    }

    /**
     * Indicates whether the specified string is null or an Empty string.
     * @param s The string to test
     */
    public static IsNullOrEmpty(s?: string | null): boolean {
        return s === null || s === undefined || s.length === 0;
    }

    /**
     * Indicates whether a specified string is null, empty, or consists only of white-space characters.
     * @param s The string to test
     */
    public static IsNullOrWhitespace(s?: string | null): boolean {
        return this.IsNullOrEmpty(s) || (s ?? "").trim().length === 0;
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
    public static SerializeJSON(element: JQuery) {
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
                const valid: boolean = (<HTMLFormElement>form[0]).checkValidity();
                element.addClass("was-validated");

                // Check if there are tabs in order to color based on their content validity
                let firstTabWithErrorOpened: boolean = false;
                element.find(".tab-content > .tab-pane").each((_index, item) => {
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
                    return;
                }
            }

            return form.serializeJSON(settings) as object;
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
     * @return The ApiResponse
     */
    public static async AjaxCall<T>(url: string, method: HttpMethod, data?: object | string | boolean): Promise<ApiResponse<T>> {
        let ajaxMethod: string;
        switch (method) {
            case HttpMethod.Get: {
                ajaxMethod = "GET";
                break;
            }
            case HttpMethod.Post: {
                ajaxMethod = "POST";
                break;
            }
            default: {
                ajaxMethod = "GET";
            }
        }

        return new Promise<ApiResponse<T>>((resolve): void => {
            void $.ajax(url, {
                type: ajaxMethod,
                beforeSend: (xhr: JQuery.jqXHR): void => {
                    xhr.setRequestHeader("XSRF-TOKEN", $("input:hidden[name='__RequestVerificationToken']").val() as string);
                },
                contentType: "application/json",
                data: data === undefined ? "" : JSON.stringify(data),
                cache: false,
                async: true,
                success: (response: ApiResponse<T>): void => {
                    resolve(response);
                },
                error: (error: JQuery.jqXHR): void => {
                    this.logger.error(`Error executing AjaxCall: ${error.responseText}`);
                    DialogManager.ShowErrorModal("Error", "An unexpected error occured");
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

        return new Promise<string>((resolve): void => {
            void $.ajax(url, {
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
                    DialogManager.ShowErrorModal("Error", "An unexpected error occured");
                },
            });
        });
    }
}
