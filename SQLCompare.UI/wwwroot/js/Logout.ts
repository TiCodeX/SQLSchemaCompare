/**
 * Contains utility methods related to the Logout
 */
class Logout {

    /**
     * Logout the application
     */
    public static Logout(): void {

        Utility.AjaxCall("/Login?handler=logout", Utility.HttpMethod.Post, undefined, (response: { success: boolean; error: string }): void => {

            if (response.success) {
                electron.ipcRenderer.send("OpenLoginWindow");
            }
            else {
                alert(response.error);
            }
        });
    }
}
