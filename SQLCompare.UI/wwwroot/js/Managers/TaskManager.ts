/**
 * Contains utility methods to handle the task
 */
class TaskManager {
    /**
     * Polling time in milliseconds
     */
    private static readonly checkFrequency: number = 500;

    /**
     * Check the status of the current running task
     */
    public static async CheckTask(): Promise<void> {
        // Disable the menu/toolbar
        MenuManager.ToggleRunningTaskRelatedMenuStatus(false);

        return new Promise<void>((resolve: PromiseResolve<void>, reject: PromiseReject): void => {

            const pollingFunc: VoidFunction = (): void => {
                PageManager.LoadPage(PageManager.Page.TaskStatus, false).then(() => {
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
                });
            };
            pollingFunc();
        });
    }

    /**
     * Abort the current running task
     */
    public Abort(): void {
        // TODO: implement
    }
}
