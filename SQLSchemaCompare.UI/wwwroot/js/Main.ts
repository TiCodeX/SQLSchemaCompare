/**
 * Contains utility methods related to the Main page
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class Main {
    /**
     * Service URL for the Main page
     */
    private static readonly pageUrl: string = "/Main/MainPageModel";

    /**
     * Service URL for retrieving the create script
     */
    private static readonly resultItemScriptsUrl: string = `${Main.pageUrl}?handler=ResultItemScripts&id=`;

    /**
     * Service URL for retrieving the sql script
     */
    private static readonly sqlScriptUrl: string = "/Main/SqlScriptPageModel";

    /**
     * Service URL for retrieving the full sql source/target script
     */
    private static readonly fullSqlScriptUrl: string = `${Main.sqlScriptUrl}?handler=FullScript&direction=`;

    /**
     * Service URL for retrieving the full sql alter script
     */
    private static readonly fullSqlAlterScriptUrl: string = `${Main.sqlScriptUrl}?handler=FullAlterScript`;

    /**
     * Contains a reference to the splitter instance
     */
    private static mainSplitter?: Split.Instance;

    /**
     * Contains a reference to the splitter sizes
     */
    private static mainSplitterSizes?: Array<number> = [60, 40];

    /**
     * Open the Main page
     */
    public static Open(): void {
        void PageManager.LoadPage(Page.Main).then((): void => {
            this.RemoveSplitter();
            MenuManager.ToggleMainOpenRelatedMenuStatus(true);
        });
    }

    /**
     * Show the bottom panel
     * @param rowId The id of the database object to show
     */
    public static ShowBottomPanel(rowId: string): void {

        $("#mainBottom").show();

        if (this.mainSplitter === undefined || $(".gutter").length === 0) {
            this.mainSplitter = Split(["#mainTop", "#mainBottom"], {
                direction: "vertical",
                gutterSize: 7,
                sizes: this.mainSplitterSizes,
                onDragStart: (): void => {
                    // Hide overflow temporary
                    $(".tcx-row-main").css("overflow", "hidden");
                },
                onDragEnd: (): void => {
                    this.mainSplitterSizes = this.mainSplitter?.getSizes();
                    // Trigger monaco-editor recalculate height
                    $(".monaco-sash").height(0);
                    // Restore original overflow
                    $(".tcx-row-main").css("overflow", "");
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

        void Utility.AjaxCall<CompareResultItemScripts>(this.resultItemScriptsUrl + rowId, HttpMethod.Get).then((response): void => {

            EditorManager.CreateEditor(EditorType.Diff, "sqlDiff",
                {
                    original: monaco.editor.createModel(response.Result?.SourceCreateScript ?? "", "sql"),
                    modified: monaco.editor.createModel(response.Result?.TargetCreateScript ?? "", "sql"),
                });

            EditorManager.CreateEditor(EditorType.Normal, "sqlAlterScript", monaco.editor.createModel(response.Result?.AlterScript ?? "", "sql"));
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

        let previousElement: JQuery = selectedElement.prev();
        if (previousElement.length === 0) {
            let previousCard: JQuery = selectedElement.parents(".card");
            while (previousCard.length > 0) {
                previousCard = previousCard.prev();
                if (previousCard.length > 0) {
                    previousElement = previousCard.find("tbody > tr:last");
                    if (previousElement.length > 0) {
                        break;
                    }
                }
            }
            if (previousElement.length === 0) {
                return;
            }
        }

        this.SelectRow(previousElement);
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
     * Show the full script page
     * @param direction The source or target
     */
    public static ShowFullScript(direction: CompareDirection): void {
        const title: string = direction === CompareDirection.Source
            ? Localization.Get("MenuFullSourceDatabaseScript")
            : Localization.Get("MenuFullTargetDatabaseScript");
        void DialogManager.OpenModalDialog(title, this.sqlScriptUrl).then((): void => {
            void Utility.AjaxCall<string>(`${this.fullSqlScriptUrl}${direction}`, HttpMethod.Get).then((response): void => {
                EditorManager.CreateEditor(EditorType.Normal, "sqlEditor", monaco.editor.createModel(response.Result ?? "", "sql"));
            });
        });
    }

    /**
     * Show the full alter script page
     */
    public static ShowFullAlterScript(): void {
        void DialogManager.OpenModalDialog(Localization.Get("MenuFullMigrationScript"), this.sqlScriptUrl).then((): void => {
            void Utility.AjaxCall<string>(`${this.fullSqlAlterScriptUrl}`, HttpMethod.Get).then((response): void => {
                EditorManager.CreateEditor(EditorType.Normal, "sqlEditor", monaco.editor.createModel(response.Result ?? "", "sql"));
            });
        });
    }

    /**
     * Scroll the main view to the selected element
     */
    private static ScollToSelectedElement(): void {
        const selectedElement = $(".tcx-selected-row");
        if (selectedElement.length === 0) {
            return;
        }
        const mainTop = $("#mainTop");
        const scrollTop = mainTop.scrollTop() ?? 0;
        const offsetTop = selectedElement.offset()?.top ?? 0;
        const innerHeight = mainTop.innerHeight() ?? 0;
        const half = 0.5;
        mainTop.scrollTop(scrollTop + offsetTop - innerHeight * half);
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
        } catch {
            // Ignore
        } finally {
            this.mainSplitter = undefined;
        }
    }
}
