/**
 * Contains utility methods related to the Main page
 */
class Main {
    /**
     * Service URL for the Main page
     */
    private static readonly pageUrl: string = "/Main/MainPageModel";

    /**
     * Service URL for retrieving the create script
     */
    private static readonly createScriptUrl: string = `${Main.pageUrl}?handler=CreateScript&id=`;

    /**
     * Contains a reference to the splitter instance
     */
    private static mainSplitter: Split.Instance;

    /**
     * Contains a reference to the splitter sizes
     */
    private static mainSplitterSizes: Array<number> = [60, 40]; // tslint:disable-line:no-magic-numbers

    /**
     * Open the Main page
     */
    public static Open(): void {
        Utility.AjaxCall(this.pageUrl, Utility.HttpMethod.Get, undefined, (result: string): void => {
            Utility.CloseModalDialog();
            this.RemoveSplitter();
            $("#mainDiv").html(result);
        });
    }

    /**
     * Show the bottom panel
     * @param rowId The id of the database object to show
     */
    public static ShowBottomPanel(rowId: string): void {

        $("#mainBottom").show();

        if (this.mainSplitter === undefined) {
            this.mainSplitter = Split(["#mainTop", "#mainBottom"], {
                direction: "vertical",
                gutterSize: 7,
                sizes: this.mainSplitterSizes,
                onDragEnd: (): void => {
                    this.mainSplitterSizes = this.mainSplitter.getSizes();
                },
            });
            const mainTop: JQuery = $("#mainTop");
            const half: number = 0.5;
            mainTop.scrollTop(mainTop.scrollTop() + $(".table-info").offset().top - mainTop.innerHeight() * half);
        }

        $("#sqlDiff").empty();

        Utility.AjaxCall(this.createScriptUrl + rowId, Utility.HttpMethod.Get, undefined, (response: ApiResponse<CreateScriptResult>): void => {
            const diffEditor: monaco.editor.IStandaloneDiffEditor = monaco.editor.createDiffEditor(document.getElementById("sqlDiff"),
                {
                    automaticLayout: true,
                    scrollBeyondLastLine: false,
                    readOnly: true,
                    stopRenderingLineAfter: -1,
                    links: false,
                    contextmenu: false,
                    quickSuggestions: false,
                    autoClosingBrackets: false,
                    selectionHighlight: false,
                    occurrencesHighlight: false,
                    folding: false,
                    matchBrackets: false,
                    ignoreTrimWhitespace: false,
                    /*fontWeight: "normal",*/
                    /*fontFamily: "Open Sans",*/
                    fontSize: 13,
                    /*lineHeight: 1,*/
                });
            diffEditor.setModel({
                original: monaco.editor.createModel(response.Result.SourceSql, "sql"),
                modified: monaco.editor.createModel(response.Result.TargetSql, "sql"),
            });
        });
    }

    /**
     * Hide the bottom panel
     */
    public static HideBottomPanel(): void {
        this.RemoveSplitter();
        // Deselect the row
        $("#mainTop .table-info").removeClass("table-info");
        $("#mainBottom").hide();
    }

    /**
     * Select the row and show the bottom panel
     * @param e The click event
     * @param rowId The id of the row to select
     */
    public static SelectRow(e: MouseEvent, rowId: string): void {
        const target: JQuery<EventTarget> = $(e.target).closest("tr");

        // Highlight the selected row only
        target.addClass("table-info").siblings().removeClass("table-info");

        Main.ShowBottomPanel(rowId);
    }

    /**
     * Remove the splitter
     */
    private static RemoveSplitter(): void {
        if (this.mainSplitter === undefined) {
            return;
        }
        try {
            this.mainSplitter.destroy();
        } catch (e) {
            // Ignore
        } finally {
            this.mainSplitter = undefined;
        }
    }
}
