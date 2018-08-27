
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
                        $("#myModalText").html(response.ErrorMessage);
                        $("#closeButton").removeClass("invisible").addClass("visible");
                        $("#quitButton").removeClass("invisible").addClass("visible");

                        switch (response.ErrorCode) {
                            case ApiResponse.EErrorCodes.ErrorNoSubscriptionAvailable:
                            case ApiResponse.EErrorCodes.ErrorSubscriptionExpired:
                            case ApiResponse.EErrorCodes.ErrorTrialSubscriptionExpired:
                                $("#myModalLink").html("Get a subscription");
                                $("#myModalLink").on("click", () => {
                                    Utility.OpenExternalBrowser(`https://www.ticodex.com/Login?ReturnUrl=${encodeURI(`/Checkout&s=${response.Result}`)}`);
                                });
                                break;
                            default:
                        }

                        $("#myModal").modal({ show: true, backdrop: "static", keyboard: false });
                    }

                },
                error: (error: JQuery.jqXHR): void => {
                    $("#webview").attr("src", webviewUrl);
                    $("#myModalText").html(Localization.Get("ErrorGeneric"));
                    $("#closeButton").removeClass("invisible").addClass("visible");
                    $("#quitButton").removeClass("invisible").addClass("visible");
                    $("#myModal").modal({ show: true, backdrop: "static", keyboard: false });
                },
            });
        }
        else {
            return;
        }
    }

    /**
     * Close the electron window
     */
    public static QuitWindow(): void {
        electron.remote.getCurrentWindow().close();
    }
}

$((): void => {
    Utility.ApplicationStartup();

    // If it doesn't exist, means the saved session has been verified
    const url: JQuery = $("#url");
    if (url.length === 0) {
        setTimeout(() => {
            electron.ipcRenderer.send("OpenMainWindow");
        }, 0);

        return;
    }

    const webview: JQuery = $("#webview");
    const webviewUrl: string = <string>url.val();

    let redirectRequestDetected: boolean = false;
    webview.on("did-get-redirect-request", (e: JQuery.Event): void => {
        redirectRequestDetected = true;
        e.preventDefault();
        Login.handleRedirect((<HashChangeEvent>e.originalEvent).newURL, webviewUrl);
    });
    webview.on("did-fail-load", (e: JQuery.Event) => {
        e.preventDefault();
        if (redirectRequestDetected) {
            return;
        }
        $("#webview").hide();
        $("#myModalText").html(Localization.Get("ErrorCannotContactTiCodeXWebsite"));
        $("#myModalLink").html("www.ticodex.com");
        $("#myModalLink").on("click", () => { Utility.OpenExternalBrowser("https://www.ticodex.com"); });
        $("#closeButton").removeClass("visible").addClass("invisible");
        $("#quitButton").removeClass("invisible").addClass("visible");
        $("#myModal").modal({ show: true, backdrop: "static", keyboard: false });
    });

    // Start loading the webview
    webview.attr("src", webviewUrl);
});
