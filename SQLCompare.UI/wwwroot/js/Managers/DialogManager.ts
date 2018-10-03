/**
 * Contains utility to handle the modal dialog
 */
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
        if (title === undefined) {
            this.ShowModal(Localization.Get("TitleError"), message, link);

            return;
        }
        this.ShowModal(title, message, link, linkText);
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
    public static async OpenQuestionDialog(title: string, message: string, buttons: Array<DialogManager.DialogButton>): Promise<DialogManager.DialogButton> {

        return new Promise<DialogManager.DialogButton>((resolve: PromiseResolve<DialogManager.DialogButton>): void => {

            const buttonLabels: Array<string> = [];
            for (const button of buttons) {
                switch (button) {
                    case DialogManager.DialogButton.Yes:
                        buttonLabels.push(Localization.Get("ButtonYes"));
                        break;
                    case DialogManager.DialogButton.No:
                        buttonLabels.push(Localization.Get("ButtonNo"));
                        break;
                    case DialogManager.DialogButton.Cancel:
                        buttonLabels.push(Localization.Get("ButtonCancel"));
                        break;
                    default:
                }
            }

            electron.remote.dialog.showMessageBox(
                electron.remote.getCurrentWindow(),
                {
                    type: "question",
                    message: message,
                    buttons: buttonLabels,
                    title: title,
                },
                (response: number, checked: boolean) => {
                    resolve(response);
                },
            );
        });
    }

    /**
     * Perform an ajax call and open the modal dialog filled with the response
     * @param url The URL of the ajax call
     * @param method The method (GET/POST)
     * @param data? The object data to send when the method is POST
     * @param callbackFunction? A function which will be called after opening the dialog
     */
    public static async OpenModalDialog(url: string): Promise<void> {
        return Utility.AjaxGetPage(url).then((result: string): void => {
            $("#myModalBody").html(result);
            $("#myModal > .modal-dialog").css("max-width", "");
            $("#myModal").modal("show");
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
        $("#myModalBody").empty();
        $(".modal-dialog").css("max-width", "");
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
            }
            else {
                $(this.modalLinkId).html(link);
            }

            $(this.modalLinkId).on("click", () => {
                Utility.OpenExternalBrowser(link);
            });
        }

        $(this.modalId).modal("show");
    }
}

namespace DialogManager {
    /**
     * Dialog buttons
     */
    export enum DialogButton {
        /**
         * Yes
         */
        Yes = 0,
        /**
         * No
         */
        No = 1,
        /**
         * Cancel
         */
        Cancel = 2,
    }
}
