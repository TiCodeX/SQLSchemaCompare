/**
 * Contains utility methods related to the Logout
 */
class Logout {

    /**
     * Logout the application
     * @param ignoreDirty Whether to ignore if the project is dirty or prompt to save
     */
    public static Logout(ignoreDirty: boolean): void {
        const data: object = <object>JSON.parse(JSON.stringify(ignoreDirty));

        Utility.AjaxCall("/Login?handler=logout", Utility.HttpMethod.Post, data).then((response: ApiResponse<string>): void => {
            if (response.Success) {
                electron.ipcRenderer.send("OpenLoginWindow");
            } else {
                Project.HandleProjectNeedToBeSavedError(response).then((): void => {
                    this.Logout(true);
                });
            }
        });
    }
}
