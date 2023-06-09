/**
 * Contains utility methods to handle the task
 */
class TaskManager {
    /**
     * Polling time in milliseconds
     */
    private static readonly checkFrequency: number = 500;

    /**
     * Service URL for the TaskStatus page
     */
    private static readonly abortUrl: string = "/TaskStatusPageModel?handler=AbortTask";

    /**
     * Check the status of the current running task
     */
    public static async CheckTask(): Promise<void> {
        // Disable the menu/toolbar
        MenuManager.ToggleRunningTaskRelatedMenuStatus(false);

        return new Promise<void>((resolve: PromiseResolve<void>, reject: PromiseReject): void => {
            const pollingFunc: VoidFunction = (): void => {
                PageManager.LoadPage(Page.TaskStatus, false).then(() => {
                    if ($("#stopPolling").length > 0) {
                        // Enable the menu/toolbar and resolve/reject depending if there are failed tasks
                        MenuManager.ToggleRunningTaskRelatedMenuStatus(true);
                        if ($("#taskFailed").length === 0) {
                            resolve();
                        } else {
                            reject();
                        }
                    } else {
                        setTimeout(() => {
                            pollingFunc();
                        }, this.checkFrequency);
                    }
                }).catch(() => {
                    // Do nothing
                });
            };
            pollingFunc();
        });
    }

    /**
     * Abort the current running task
     */
    public static Abort(): void {
        DialogManager.OpenQuestionDialog(
            Localization.Get("TitleAbortCompare"),
            Localization.Get("MessageConfirmAbortOperation"),
            [DialogButton.Yes, DialogButton.No],
        )
        .then((answer: DialogButton): void => {
            if (answer === DialogButton.Yes) {
                Utility.AjaxCall(this.abortUrl, HttpMethod.Get).then((): void => {
                    // Allows to call abort only once, disable button
                    $("#btnAbortTask").attr("disabled", "disabled");
                }).catch(() => {
                    // Do nothing
                });
            }
        }).catch(() => {
            // Do nothing
        });
    }
}
