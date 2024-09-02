/**
 * Contains utility methods to handle the page displayed
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class PageManager {
    /**
     * Reference to the main container div
     */
    private static readonly pageContainer: JQuery = $(".tcx-row-main");

    /**
     * Close the current page, if it's last it will open the welcome page
     */
    public static async ClosePage(): Promise<void> {
        this.pageContainer.children("div:last").remove();
        this.pageContainer.children("div:last").removeClass("d-none");

        if (this.pageContainer.children("div").length === 0) {
            return this.LoadPage(Page.Welcome);
        }

        this.RemoveTooltips();
    }

    /**
     * Closes all the pages and open the welcome page
     */
    public static async CloseAllPages(): Promise<void> {
        this.pageContainer.children("div").remove();

        return this.ClosePage();
    }

    /**
     * Get the currently opened page
     */
    public static GetOpenPage(): Page | undefined {
        const currentPage: JQuery = this.pageContainer.children("div:last");
        if (currentPage.length > 0) {
            const pageAttribute: number = Number(currentPage.attr("page"));

            return Page[Page[pageAttribute] as keyof typeof Page];
        }

        return undefined;
    }

    /**
     * Refresh all the currently opened pages
     */
    public static async RefreshOpenPages(): Promise<void> {
        const pages: Array<Page> = [];
        this.pageContainer.children("div").each((_index, element) => {
            const pageAttribute: number = Number($(element).attr("page"));

            pages.push(Page[Page[pageAttribute] as keyof typeof Page]);
        });

        // Remove all the pages
        this.pageContainer.children("div").remove();

        for (const page of pages) {
            await this.LoadPage(page, false);
        }

        void Promise.resolve();
    }

    /**
     * Open the page
     * @param page The page to open
     * @param closePreviousPage Tell if the previous page needs to be closed
     */
    public static async LoadPage(page: Page, closePreviousPage: boolean = true): Promise<void> {
        return Utility.AjaxGetPage(this.GetPageUrl(page) ?? "").then((result: string): void => {

            if (closePreviousPage) {
                this.pageContainer.children("div:last").remove();
            }

            // Remove all the divs with the page attribute like the page we are opening
            this.pageContainer.children(`div[page=${page}]`).remove();
            // Hide all the remaining div
            this.pageContainer.children("div").addClass("d-none");
            this.RemoveTooltips();

            const newPageDiv: JQuery = $("<div />");
            newPageDiv.appendTo(this.pageContainer);
            newPageDiv.attr("page", page);
            newPageDiv.addClass("col-12 p-0");
            if (page === Page.Main) {
                newPageDiv.addClass("tcx-main-page");
            } else {
                newPageDiv.addClass("my-auto");
            }
            newPageDiv.html(result);

            // Activate the editable selects in the new page
            $(".editable-select", newPageDiv).editableSelect({
                filter: false,
                effects: "fade",
                duration: "fast",
            });

            // Fix tabs height
            $(".tab-pane", newPageDiv).matchHeight({
                byRow: false,
            });
        });
    }

    /**
     * Remove all the tooltips/popovers from the page
     */
    public static RemoveTooltips(): void {
        try {
            $("div.tooltip").tooltip("hide");
        } catch {
            $("div.tooltip").remove();
        }
        try {
            $("div.popover").popover("hide");
        } catch {
            $("div.popover").remove();
        }
    }

    /**
     * Get the page url
     */
    private static GetPageUrl(page: Page): string | undefined {
        switch (page) {
            case Page.Main: {
                return "/Main/MainPageModel";
            }
            case Page.Welcome: {
                return "/WelcomePageModel";
            }
            case Page.Project: {
                return "/Project/ProjectPageModel";
            }
            case Page.TaskStatus: {
                return "/TaskStatusPageModel";
            }
            default: {
                return undefined;
            }
        }
    }
}
