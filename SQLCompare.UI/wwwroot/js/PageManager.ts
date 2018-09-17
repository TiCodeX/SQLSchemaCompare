/**
 * Contains utility methods to handle the page displayed
 */
class PageManager {
    /**
     * Reference to the main container div
     */
    private static readonly pageContainer: JQuery = $(".tcx-row-main");

    /**
     * Close the current page, if it's last it will open the welcome page
     */
    public static ClosePage(): void {
        this.pageContainer.children("div:last").remove();
        this.pageContainer.children("div:last").removeClass("d-none");
        if (this.pageContainer.children("div").length === 0) {
            this.LoadPage(PageManager.Page.Welcome);
        }
    }

    /**
     * Get the currently opened page
     */
    public static GetOpenPage(): PageManager.Page {
        const currentPage: JQuery = this.pageContainer.children("div:last");
        if (currentPage.length > 0) {
            const pageAttribute: number = Number(currentPage.attr("page"));

            return PageManager.Page[PageManager.Page[pageAttribute]]; // tslint:disable-line:no-unsafe-any
        }

        return undefined;
    }

    /**
     * Open the page
     * @param page The page to open
     * @param closePreviousPage Tell if the previous page needs to be closed
     * @param callbackFunction? A function which will be called after opening the dialog
     */
    public static LoadPage(page: PageManager.Page, closePreviousPage: boolean = true, callbackFunction?: () => void): void {
        Utility.AjaxCall(this.GetPageUrl(page), Utility.HttpMethod.Get, undefined, (result: string): void => {

            if (closePreviousPage) {
                this.pageContainer.children("div:last").remove();
            }

            this.pageContainer.children("div").addClass("d-none");

            const newPageDiv: JQuery = $("<div />");
            newPageDiv.appendTo(this.pageContainer);
            newPageDiv.attr("page", page);
            newPageDiv.addClass("col-12 p-0");
            if (page !== PageManager.Page.Main) {
                newPageDiv.addClass("my-auto");
            }
            newPageDiv.html(result);

            if (callbackFunction !== undefined) {
                callbackFunction();
            }
        });
    }

    /**
     * Get the page url
     */
    private static GetPageUrl(page: PageManager.Page): string {
        switch (page) {
            case PageManager.Page.Main: return "/Main/MainPageModel";
            case PageManager.Page.Welcome: return "/WelcomePageModel";
            case PageManager.Page.Project: return "/Project/ProjectPageModel";
            default: return undefined;
        }
    }
}

namespace PageManager {
    /**
     * Pages handled by PageManager
     */
    export enum Page {
        /**
         * The main page
         */
        Main,
        /**
         * The welcome page
         */
        Welcome,
        /**
         * The project page
         */
        Project,
    }
}
