/* tslint:disable:no-require-imports no-implicit-dependencies */
declare const amdRequire: Require;
const electron: Electron.AllElectron = (typeof require !== "undefined" ? <Electron.AllElectron>require("electron") : undefined);
/* tslint:enable:no-require-imports no-implicit-dependencies */

let mainSplitter: Split.Instance;

$(() => {
    // Configure the monaco editor loader
    amdRequire.config({
        baseUrl: "lib/monaco-editor",
    });

    // Enable bootstrap tooltips and popovers
    $("[data-toggle='tooltip']").tooltip();
    $("[data-toggle='popover']").popover();

    // Disable context menu
    window.addEventListener("contextmenu", (e: PointerEvent) => {
        e.preventDefault();
    }, false);

    // Preload the monaco-editor
    setTimeout((): void => {
        amdRequire(["vs/editor/editor.main"], (): void => {
            // Nothing to do
        });
    }, 0);

    Menu.CreateMenu();

    Utility.OpenModalDialog("/Welcome", "GET");

    // Register clickable attributes
    $(document).on("click", "[load-modal]", (e: JQuery.Event) => {
        e.preventDefault();
        const target: JQuery = $(e.target);
        const url: string = target.attr("load-modal").toString();
        const method: string = target.attr("load-modal-method").toString();
        let data: object;
        if (target.attr("load-modal-data") !== undefined) {
            data = <object>JSON.parse(<string>JSON.parse(`"${target.attr("load-modal-data").toString()}"`));
        } else if (target.attr("load-modal-data-from-div") !== undefined) {
            const dataDivId: string = target.attr("load-modal-data-from-div").toString();
            data = Utility.SerializeJSON($(`#${dataDivId}`));
        }
        Utility.OpenModalDialog(url, method, data);
    });

    $(document).on("click", "[load-select]", (e: JQuery.Event) => {
        e.preventDefault();
        const target: JQuery = $(e.target);
        const url: string = target.attr("load-select").toString();
        const method: string = target.attr("load-select-method").toString();
        const selectName: string = target.attr("load-select-target").toString();
        const dataDivId: string = target.attr("load-select-data-from-div").toString();
        const data: object = Utility.SerializeJSON($(`#${dataDivId}`));

        Utility.AjaxCall(url, method, data, (result: Array<string>): void => {
            const select: JQuery = $(`select[name=${selectName}]`);
            select.find("option").remove();
            let options: string = "";
            $.each(result, (index: number, value: string): void => {
                options += `<option value="${value}">${value}</option>`;
            });
            select.append(options);
        });
    });

    $(document).on("click", "[load-main]", (e: JQuery.Event) => {
        e.preventDefault();
        const target: JQuery = $(e.target);
        const url: string = target.attr("load-main").toString();
        const method: string = target.attr("load-main-method").toString();
        let data: object;
        if (target.attr("load-main-data") !== undefined) {
            data = <object>JSON.parse(<string>JSON.parse(`"${target.attr("load-main-data").toString()}"`));
        } else if (target.attr("load-main-data-from-div") !== undefined) {
            const dataDivId: string = target.attr("load-main-data-from-div").toString();
            data = Utility.SerializeJSON($(`#${dataDivId}`));
        }

        Utility.AjaxCall(url, method, data, (): void => {
            const pollingTime: number = 200;
            const polling: VoidFunction = (): void => {
                setTimeout(() => {
                    if ($("#stopPolling").length > 0) {
                        $("#myModal").modal("hide");
                        Utility.AjaxCall("/Main", "GET", undefined, (result: string): void => {
                            $("#mainDiv").html(result);
                        });
                    } else {
                        Utility.OpenModalDialog("/Project/CompareStatus", "GET", undefined);
                        polling();
                    }
                }, pollingTime);
            };
            polling();
        });
    });

    $(document).on("click", "[load-sql]", (e: JQuery.Event) => {
        e.preventDefault();
        const target: JQuery = $(e.target).closest("tr");
        const url: string = target.attr("load-sql").toString();
        const method: string = target.attr("load-sql-method").toString();

        // Highlight the selected row only
        target.addClass("table-info").siblings().removeClass("table-info");

        $("#mainBottom").show();
        if ($(".gutter").length === 0) {
            mainSplitter = Split(["#mainTop", "#mainBottom"], {
                direction: "vertical",
            });
            const mainTop: JQuery = $("#mainTop");
            const half: number = 0.5;
            mainTop.scrollTop(mainTop.scrollTop() + target.offset().top - mainTop.innerHeight() * half);
        }

        $("#sqlDiff").empty();
        Utility.AjaxCall(url, method, undefined, (result: { sourceSql: string; targetSql: string }): void => {
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
    });

    $(document).on("click", "[close-split]", (e: JQuery.Event) => {
        e.preventDefault();
        mainSplitter.destroy();
        $("#mainTop .table-info").removeClass("table-info");
        $("#mainBottom").hide();
    });

});
