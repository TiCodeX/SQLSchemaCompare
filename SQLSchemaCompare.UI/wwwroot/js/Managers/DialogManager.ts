/**
 * Contains utility to handle the modal dialog
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class DialogManager {
    /**
     * The modal id
     */
    private static readonly modalId: string = "#tcxModal";

    /**
     * The modal title id
     */
    private static readonly modalTitleId: string = "#tcxModalTitle";

    /**
     * The modal message id
     */
    private static readonly modalMessageId: string = "#tcxModalText";

    /**
     * The modal link id
     */
    private static readonly modalLinkId: string = "#tcxModalLink";

    /**
     * Show the error in the modal dialog
     */
    public static ShowError(title: string, message: string, link?: string, linkText?: string): void {
        this.ShowModal(title === "" ? Localization.Get("TitleError") : title, message, link, linkText);
    }

    /**
     * Show the information in the modal dialog
     */
    public static ShowInformation(title: string, message: string, link?: string, linkText?: string): void {
        this.ShowModal(title, message, link, linkText);
    }

    /**
     * Opens the question dialog
     * @param title The title of the dialog
     * @param message The question message to display
     * @param buttons The button choices
     */
    public static async OpenQuestionDialog(title: string, message: string, buttons: Array<DialogButton>): Promise<DialogButton> {

        return new Promise<DialogButton>((resolve): void => {

            const buttonLabels: Array<string> = [];
            let cancelId: number = buttons.length - 1;
            for (const button of buttons) {
                switch (button) {
                    case DialogButton.Yes:
                        buttonLabels.push(Localization.Get("ButtonYes"));
                        break;
                    case DialogButton.No:
                        buttonLabels.push(Localization.Get("ButtonNo"));
                        break;
                    case DialogButton.Cancel:
                        buttonLabels.push(Localization.Get("ButtonCancel"));
                        cancelId = buttonLabels.length - 1;
                        break;
                    default:
                }
            }

            void electronRemote.dialog.showMessageBox(
                electronRemote.getCurrentWindow(),
                {
                    type: "question",
                    message: message,
                    buttons: buttonLabels,
                    cancelId: cancelId,
                    noLink: true,
                    title: title,
                },
            ).then((value: Electron.MessageBoxReturnValue) => {
                resolve(value.response);
            });
        });
    }

    /**
     * Perform an ajax call and open the modal dialog filled with the response
     * @param title The title of the dialog
     * @param url The URL of the ajax call
     * @param maxWidth CSS value for the dialog max-width
     * @param showCloseButton Whether to show the close button
     */
    public static async OpenModalDialog(title: string, url: string, maxWidth?: string, showCloseButton?: boolean): Promise<void> {
        return Utility.AjaxGetPage(url).then((result: string): void => {
            $("#myModal .modal-title").html(title);
            $("#myModal .modal-body").html(result);
            $("#myModal .modal-dialog").css("max-width", maxWidth ?? "");
            $("#myModal .modal-header > button").css("display", showCloseButton !== undefined && showCloseButton ? "block" : "none");
            $("#myModal").attr("tabindex", showCloseButton !== undefined && showCloseButton ? "-1" : "");
            $("#myModal").modal({
                keyboard: showCloseButton !== undefined && showCloseButton,
                backdrop: "static",
                show: true,
            });
            // Fix tabs height
            $("#myModal .tab-pane").matchHeight({
                byRow: false,
            });
        });
    }

    /**
     * Close the modal dialog
     */
    public static CloseModalDialog(): void {
        $("#myModal").modal("hide");
        $("#myModal .modal-title").empty();
        $("#myModal .modal-body").empty();
    }

    /**
     * Display the modal dialog
     */
    private static ShowModal(title: string, message: string, link?: string, linkText?: string): void {
        $(this.modalTitleId).html(title);
        $(this.modalMessageId).html(message);

        if (link !== undefined) {
            if (linkText !== undefined) {
                $(this.modalLinkId).html(linkText);
            } else {
                $(this.modalLinkId).html(link);
            }

            $(this.modalLinkId).on("click", () => {
                Utility.OpenExternalBrowser(link);
            });
        }

        $(this.modalId).modal("show");
    }
}
