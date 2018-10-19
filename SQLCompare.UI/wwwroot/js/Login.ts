
/**
 * Contains utility methods related to the Login
 */
class Login {

    /**
     * The base redirect url to check
     */
    private static readonly baseRedirectUrl: string = "https://queieimiugrepqueieimiucrap.chromiumapp.org";

    /**
     * Handle the redirect event
     * @param redirectUrl The redirect url received
     */
    public static handleRedirect(redirectUrl: string, webviewUrl: string): void {

        if (redirectUrl.indexOf(this.baseRedirectUrl) === 0) {

            $.ajax("/Login?handler=verify", {
                type: "POST",
                beforeSend: (xhr: JQuery.jqXHR): void => {
                    xhr.setRequestHeader("XSRF-TOKEN",
                        $("input:hidden[name='__RequestVerificationToken']").val().toString());
                },
                contentType: "application/json",
                data: JSON.stringify(redirectUrl),
                cache: false,
                async: true,
                success: (response: ApiResponse<string>): void => {
                    if (response.Success) {
                        electron.ipcRenderer.send("OpenMainWindow");
                    } else {
                        $("#webview").attr("src", webviewUrl);
                        Login.ShowErrorModal(response.ErrorMessage, response.ErrorCode, response.Result);
                    }

                },
                error: (): void => {
                    $("#webview").attr("src", webviewUrl);
                    Login.ShowErrorModal(Localization.Get("ErrorGeneric"), ApiResponse.EErrorCodes.ErrorUnexpected, undefined);
                },
            });
        }
        else {
            return;
        }
    }

    /**
     * Show the error modal
     * @param errorMessage The error message to display
     * @param errorCode The error code
     * @param result The session token that can be used in the link
     */
    public static ShowErrorModal(errorMessage: string, errorCode: ApiResponse.EErrorCodes, result: string): void {
        $("#myModalText").html(errorMessage);
        switch (errorCode) {
            case ApiResponse.EErrorCodes.ErrorNoSubscriptionAvailable:
            case ApiResponse.EErrorCodes.ErrorSubscriptionExpired:
            case ApiResponse.EErrorCodes.ErrorTrialSubscriptionExpired:
                $("#myModalLink").html(Localization.Get("MessageGetASubscription"));
                $("#myModalLink").on("click", () => {
                    Utility.OpenExternalBrowser(`${$("#urlSubscribe").val()}&s=${encodeURIComponent(result)}`);
                });
                $("#myModalLink").show();
                break;
            case ApiResponse.EErrorCodes.ErrorApplicationUpdateNeeded:
                $("#myModalLink").html(Localization.Get("MessageDownloadLatestVersion"));
                $("#myModalLink").on("click", () => {
                    Utility.OpenExternalBrowser("https://www.ticodex.com/");
                });
                $("#myModalLink").show();
                break;
            default:
                $("#myModalLink").hide();
        }
        $("#myModal").modal("show");
    }

    /**
     * Close the electron window
     */
    public static QuitWindow(): void {
        electron.remote.getCurrentWindow().close();
    }
}

$((): void => {
    Utility.ApplicationStartup().then((): void => {
        // If it doesn't exist, means the saved session has been verified
        const url: JQuery = $("#url");
        if (url.length === 0) {
            electron.ipcRenderer.send("OpenMainWindow");

            return;
        }

        electron.ipcRenderer.send("ShowLoginWindow");

        const webview: JQuery = $("#webview");
        let redirectRequestDetected: boolean = false;
        electron.ipcRenderer.on("LoginRedirect", (event: Electron.Event, redirectURL: string) => {
            redirectRequestDetected = true;
            Login.handleRedirect(redirectURL, <string>url.val());
        });
        webview.on("did-fail-load", (e: JQuery.Event) => {
            e.preventDefault();
            if (redirectRequestDetected) {
                return;
            }
            // Handle regression in Electron 3.0 (https://github.com/electron/electron/issues/14004)
            if ((<any>e.originalEvent).errorCode === -3) { // tslint:disable-line:no-any no-magic-numbers
                return;
            }
            webview.hide();
            $("#myModalText").html(Localization.Get("ErrorCannotContactTiCodeXWebsite"));
            $("#myModalLink").html("www.ticodex.com");
            $("#myModalLink").on("click", () => {
                Utility.OpenExternalBrowser("https://www.ticodex.com");
            });
            $("#myModalLink").show();
            $("#closeButton").hide();
            $("#myModal").modal("show");
        });
        webview.on("did-finish-load", (): void => {
            $(".tcx-loader").hide();
            const errorMessageElement: JQuery = $("#verifySessionResultErrorMessage");
            const errorCodeElement: JQuery = $("#verifySessionResultErrorCode");
            const resultElement: JQuery = $("#verifySessionResultResult");
            if (errorMessageElement.length !== 0) {
                const errorMessage: string = <string>errorMessageElement.val();
                const errorCode: ApiResponse.EErrorCodes = <ApiResponse.EErrorCodes>+errorCodeElement.val();
                const result: string = <string>resultElement.val();

                errorMessageElement.remove();
                errorCodeElement.remove();
                resultElement.remove();

                Login.ShowErrorModal(errorMessage, errorCode, result);
            }
        });

        // Start loading the webview
        webview.attr("src", <string>url.val());
    });
});
