/**
 * Contains utility methods related to the Logout
 */
class Logout {

    /**
     * Logout the application
     */
    public static Logout(): void {

        Utility.AjaxCall("/Login?handler=logout", Utility.HttpMethod.Post, undefined, (response: ApiResponse<object>): void => {
            if (response.Success) {
                electron.ipcRenderer.send("OpenLoginWindow");
            } else {
                alert(response.ErrorMessage);
            }
        });
    }
}
