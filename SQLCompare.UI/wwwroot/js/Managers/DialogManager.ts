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
     * Opens the save question dialog
     * @param callback - the action that should be done when the dialog returns
     */
    public static async OpenSaveQuestionDialog(): Promise<DialogManager.SaveDialogAnswers> {
        return new Promise<DialogManager.SaveDialogAnswers>((resolve: PromiseResolve<DialogManager.SaveDialogAnswers>): void => {
            electron.remote.dialog.showMessageBox(
                electron.remote.getCurrentWindow(),
                {
                    type: "question",
                    message: Localization.Get("MessageDoYouWantToSaveProjectChanges"),
                    buttons: [Localization.Get("ButtonYes"), Localization.Get("ButtonNo"), Localization.Get("ButtonCancel")],
                    title: Localization.Get("Error"),
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
    private static ShowModal(title: string, message: string, link?: string, linkText?: string ): void {
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
     * Save dialog answers
     */
    export enum SaveDialogAnswers {
        /**
         * Yes answer
         */
        Yes = 0,
        /**
         * No answer
         */
        No = 1,
        /**
         * Cancel
         */
        Cancel = 2,
    }
}
