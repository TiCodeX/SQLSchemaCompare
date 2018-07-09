/**
 * Contains utility methods related to the Main page
 */
class Main {
    /**
     * Service URL for the Main page
     */
    public static readonly pageUrl: string = "/Main/MainPageModel";

    /**
     * Service URL for retrieving the create script
     */
    private static readonly createScriptUrl: string = `${Main.pageUrl}?handler=CreateScript&id=`;

    /**
     * Contains a reference to the splitter instance
     */
    private static mainSplitter: Split.Instance;

    /**
     * Show the bottom panel
     * @param rowId The id of the database object to show
     */
    public static ShowBottomPanel(rowId: string): void {

        $("#mainBottom").show();

        if (this.mainSplitter === undefined) {
            this.mainSplitter = Split(["#mainTop", "#mainBottom"], {
                direction: "vertical",
            });
            const mainTop: JQuery = $("#mainTop");
            const half: number = 0.5;
            mainTop.scrollTop(mainTop.scrollTop() + $(".table-info").offset().top - mainTop.innerHeight() * half);
        }

        $("#sqlDiff").empty();

        Utility.AjaxCall(this.createScriptUrl + rowId, Utility.HttpMethod.Get, undefined, (result: { sourceSql: string; targetSql: string }): void => {
            const diffEditor: monaco.editor.IStandaloneDiffEditor = monaco.editor.createDiffEditor(document.getElementById("sqlDiff"),
                {
                    automaticLayout: true,
                    scrollBeyondLastLine: false,
                });
            diffEditor.setModel({
                original: monaco.editor.createModel(result.sourceSql, "sql"),
                modified: monaco.editor.createModel(result.targetSql, "sql"),
            });
        });
    }

    /**
     * Hide the bottom panel
     */
    public static HideBottomPanel(): void {
        if (this.mainSplitter === undefined) {
            return;
        }

        this.mainSplitter.destroy();
        this.mainSplitter = undefined;

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
        const target: JQuery = $(e.target).closest("tr");

        // Highlight the selected row only
        target.addClass("table-info").siblings().removeClass("table-info");

        Main.ShowBottomPanel(rowId);
    }
}
