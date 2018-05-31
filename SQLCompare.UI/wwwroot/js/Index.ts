/* tslint:disable:no-require-imports no-implicit-dependencies */
declare const amdRequire: Require;
const electron: Electron.AllElectron = (typeof require !== "undefined" ? <Electron.AllElectron>require("electron") : undefined);
/* tslint:enable:no-require-imports no-implicit-dependencies */

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

    if (electron) {

        const template: Array<Electron.MenuItemConstructorOptions> = [
            {
                label: "File",
                submenu: [
                    {
                        label: "Nuovo progetto",
                    },
                    {
                        label: "Apri progetto",
                    },
                    {
                        label: "Chiudi progetto",
                    },
                    {
                        role: "close",
                        label: "Esci",
                    },
                ],
            },
            {
                label: "View",
                submenu: [
                    {
                        label: "Reload",
                        accelerator: "CmdOrCtrl+R",
                        click(item: Electron.MenuItem, focusedWindow?: Electron.BrowserWindow): void {
                            if (focusedWindow) {
                                focusedWindow.reload();
                            }
                        },
                    },
                    {
                        label: "Toggle Developer Tools",
                        accelerator: "Ctrl+Shift+I",
                        click(item: Electron.MenuItem, focusedWindow?: Electron.BrowserWindow): void {
                            if (focusedWindow) {
                                focusedWindow.webContents.toggleDevTools();
                            }
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        role: "resetzoom",
                    },
                    {
                        role: "zoomin",
                    },
                    {
                        role: "zoomout",
                    },
                    {
                        type: "separator",
                    },
                    {
                        role: "togglefullscreen",
                    },
                ],
            },
            {
                role: "help",
                submenu: [
                    {
                        label: "Learn More",
                        click(): void {
                            electron.shell.openExternal("http://electron.atom.io");
                        },
                    },
                ],
            },
        ];

        electron.remote.Menu.setApplicationMenu(electron.remote.Menu.buildFromTemplate(template));

    }

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
        if (target.attr("load-main-data-from-div") !== undefined) {
            const dataDivId: string = target.attr("load-main-data-from-div").toString();
            data = Utility.SerializeJSON($(`#${dataDivId}`));
        }

        Utility.AjaxCall(url, method, data, (): void => {
            $("#myModal").modal("hide");
            Utility.AjaxCall("/Main", "GET", undefined, (result: string): void => {
                $("#mainDiv").html(result);
                Split(["#mainTop", "#mainBottom"], {
                    direction: "vertical",
                });
            });
        });
    });

    $(document).on("click", "[load-sql]", (e: JQuery.Event) => {
        e.preventDefault();
        const target: JQuery = $(e.target).closest("tr");
        const url: string = target.attr("load-sql").toString();
        const method: string = target.attr("load-sql-method").toString();
        let data: object;
        if (target.attr("load-sql-data") !== undefined) {
            data = <object>JSON.parse(<string>JSON.parse(`"${target.attr("load-sql-data").toString()}"`));
        }
        $("#mainBottom").empty();
        Utility.AjaxCall(url, method, data, (result: { sourceSql: string; targetSql: string }): void => {
            const diffEditor: monaco.editor.IStandaloneDiffEditor = monaco.editor.createDiffEditor(document.getElementById("mainBottom"),
                {
                    automaticLayout: true,
                });
            diffEditor.setModel({
                original: monaco.editor.createModel(result.sourceSql, "sql"),
                modified: monaco.editor.createModel(result.targetSql, "sql"),
            });
        });
    });
});
