/**
 * Contains utility methods related to the Logout
 */
class Logout {

 /**
  * Logout the application
  */
    public static Logout(): void {

        $.ajax("/Login?handler=logout", {
            type: "POST",
            beforeSend: (xhr: JQuery.jqXHR): void => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $("input:hidden[name='__RequestVerificationToken']").val().toString());
            },
            contentType: "application/json",
            cache: false,
            async: true,
            success: (response: object): void => {
                const res: { success: boolean; error: string } = <{ success: boolean; error: string }>response;
                if (res.success) {
                    electron.ipcRenderer.send("OpenLoginWindow");
                }
                else {
                    alert(res.error);
                }
            },
            error: (error: JQuery.jqXHR): void => {
                alert(error);
            },
        });
    }
}
