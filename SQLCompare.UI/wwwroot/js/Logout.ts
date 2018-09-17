/**
 * Contains utility methods related to the Logout
 */
class Logout {

    /**
     * Logout the application
     */
    public static Logout(ignoreDirty: boolean): void {
        const data: object = <object>JSON.parse(JSON.stringify(ignoreDirty));

        Utility.AjaxCall("/Login?handler=logout", Utility.HttpMethod.Post, data, (response: ApiResponse<object>): void => {
            if (response.Success) {
                electron.ipcRenderer.send("OpenLoginWindow");
            } else {
                if (response.ErrorCode === ApiResponse.EErrorCodes.ErrorProjectNeedToBeSaved) {
                    DialogManager.OpenSaveQuestionDialog((answer: number, checked: boolean): void => {
                        switch (answer) {
                            case DialogManager.SaveDialogAnswers.Yes:
                                Project.Save(false, (): void => { this.Logout(false); });
                                break;
                            case DialogManager.SaveDialogAnswers.No:
                                this.Logout(true);
                                break;
                            default:
                        }
                    });
                }
                else {
                    DialogManager.ShowError(Localization.Get("TitleError"), response.ErrorMessage);
                }
            }
        });
    }
}
