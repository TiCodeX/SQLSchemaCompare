/* tslint:disable:no-require-imports no-implicit-dependencies */
const loginelectron: Electron.AllElectron = (typeof require !== "undefined" ? <Electron.AllElectron>require("electron") : undefined);
/* tslint:enable:no-require-imports no-implicit-dependencies */

const baseRedirectUrl: string = "https://queieimiugrepqueieimiucrap.chromiumapp.org";

const handleRedirect: Function = (redirectUrl: string): void => {

    if (redirectUrl.indexOf(baseRedirectUrl) === 0) {

        loginelectron.ipcRenderer.send("log", { category: "Login", level: "info", message: `Received url: ${redirectUrl}` });

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
            success: (response: object): void => {
                const res: { success: boolean; error: string } = <{ success: boolean; error: string }>response;
                if (res.success) {
                    loginelectron.ipcRenderer.send("OpenMainWindow");
                }
                else {
                    $("#webview").hide();
                    $("#error").html(res.error);
                }

            },
            error: (error: JQuery.jqXHR): void => {
                $("#webview").hide();
                $("#error").html("An error occurred");
            },
        });
    }
    else {
        return;
    }
};

$((): void => {

    // Disable context menu
    window.addEventListener("contextmenu", (e: PointerEvent) => {
        e.preventDefault();
    }, false);

    // Prevent app zooming
    if (loginelectron !== undefined) {
        loginelectron.webFrame.setVisualZoomLevelLimits(1, 1);
        loginelectron.webFrame.setLayoutZoomLevelLimits(0, 0);
    }

    const webview: JQuery = $("#webview");

    let redirectRequestDetected: boolean = false;
    webview.on("did-get-redirect-request", (e: JQuery.Event): void => {
        redirectRequestDetected = true;
        e.preventDefault();
        handleRedirect((<HashChangeEvent>e.originalEvent).newURL);
    });
    webview.on("did-fail-load", (e: JQuery.Event) => {
        e.preventDefault();
        if (redirectRequestDetected) {
            return;
        }
        $("#webview").hide();
        $("#error").html("Connection issue");
    });

    const url: JQuery = $("#url");
    // If it doesn't exist, means the saved session has been verified
    if (url.length === 0) {
        loginelectron.ipcRenderer.send("OpenMainWindow");
    }
    else {
        // Start loading the webview
        webview.attr("src", <string>url.val());
    }
});
