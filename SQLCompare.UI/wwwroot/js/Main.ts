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
        PageManager.LoadPage(PageManager.Page.Main).then((): void => {
            this.RemoveSplitter();
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
            this.ScollToSelectedElement();
        }

        // Display the selected row item names
        let sourceItem: string = $(`#${rowId} td:nth-child(2)`).text();
        if (Utility.IsNullOrWhitespace(sourceItem)) {
            sourceItem = Localization.Get("LabelDoesNotExist");
        }
        let targetItem: string = $(`#${rowId} td:nth-child(3)`).text();
        if (Utility.IsNullOrWhitespace(targetItem)) {
            targetItem = Localization.Get("LabelDoesNotExist");
        }
        $(".tcx-diff-item-name").html(`${sourceItem} <span class='fa fa-long-arrow-alt-right'></span> ${targetItem}`);

        // Display the monaco editor
        $("#sqlDiff").empty();
        Utility.AjaxCall(this.createScriptUrl + rowId, Utility.HttpMethod.Get).then((response: ApiResponse<CreateScriptResult>): void => {
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
        $(".tcx-selected-row").removeClass("tcx-selected-row");
        $("#mainBottom").hide();
    }

    /**
     * Select the row and show the bottom panel
     * @param rowElement The jQuery element of the clicked row
     */
    public static SelectRow(rowElement: JQuery): void {
        // Deselect the row
        $(".tcx-selected-row").removeClass("tcx-selected-row");
        // Highlight the selected
        rowElement.addClass("tcx-selected-row");
        rowElement.parents(".card-body").addClass("show");
        this.ShowBottomPanel(rowElement.get(0).id);
    }

    /**
     * Select the previous row
     */
    public static SelectPrevRow(): void {
        const selectedElement: JQuery = $(".tcx-selected-row");
        if (selectedElement.length === 0) {
            return;
        }

        let prevElement: JQuery = selectedElement.prev();
        if (prevElement.length === 0) {
            let prevCard: JQuery = selectedElement.parents(".card");
            while (prevCard.length > 0) {
                prevCard = prevCard.prev();
                if (prevCard.length > 0) {
                    prevElement = prevCard.find("tbody > tr:last");
                    if (prevElement.length > 0) {
                        break;
                    }
                }
            }
            if (prevElement.length === 0) {
                return;
            }
        }

        this.SelectRow(prevElement);
        this.ScollToSelectedElement();
    }

    /**
     * Select the next row
     */
    public static SelectNextRow(): void {
        const selectedElement: JQuery = $(".tcx-selected-row");
        if (selectedElement.length === 0) {
            return;
        }

        let nextElement: JQuery = selectedElement.next();
        if (nextElement.length === 0) {
            let nextCard: JQuery = selectedElement.parents(".card");
            while (nextCard.length > 0) {
                nextCard = nextCard.next();
                if (nextCard.length > 0) {
                    nextElement = nextCard.find("tbody > tr:first");
                    if (nextElement.length > 0) {
                        break;
                    }
                }
            }
            if (nextElement.length === 0) {
                return;
            }
        }

        this.SelectRow(nextElement);
        this.ScollToSelectedElement();
    }

    /**
     * Scroll the main view to the selected element
     */
    private static ScollToSelectedElement(): void {
        const selectedElement: JQuery = $(".tcx-selected-row");
        if (selectedElement.length === 0) {
            return;
        }
        const mainTop: JQuery = $("#mainTop");
        const half: number = 0.5;
        mainTop.scrollTop(mainTop.scrollTop() + selectedElement.offset().top - mainTop.innerHeight() * half);
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
