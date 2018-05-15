declare const amdRequire: any;
declare const require: any;
const electron: Electron.AllElectron = (typeof require !== "undefined" ? require("electron") : null);

$(document).ready(() => {
    //Configure the monaco editor loader
    amdRequire.config({
        baseUrl: "lib/monaco-editor"
    });

    // Enable bootstrap tooltips and popovers
    $("[data-toggle='tooltip']").tooltip();
    $("[data-toggle='popover']").popover();

    // Disable context menu
    window.addEventListener("contextmenu", (e) => {
        e.preventDefault();
    }, false);

    // Preload the monaco-editor
    setTimeout(() => {
        amdRequire(["vs/editor/editor.main"], () => { });
    }, 0);

    if (electron) {

        const template: Electron.MenuItemConstructorOptions[] = [
            {
                label: "File",
                submenu: [
                    {
                        label: "Nuovo progetto"
                    },
                    {
                        label: "Apri progetto"
                    },
                    {
                        label: "Chiudi progetto"
                    },
                    {
                        role: "close",
                        label: "Esci"
                    }
                ]
            },
            {
                label: "View",
                submenu: [
                    {
                        label: "Reload",
                        accelerator: "CmdOrCtrl+R",
                        click(item, focusedWindow) {
                            if (focusedWindow) focusedWindow.reload();
                        }
                    },
                    {
                        label: "Toggle Developer Tools",
                        accelerator: "Ctrl+Shift+I",
                        click(item, focusedWindow) {
                            if (focusedWindow) focusedWindow.webContents.toggleDevTools();
                        }
                    },
                    {
                        type: "separator"
                    },
                    {
                        role: "resetzoom"
                    },
                    {
                        role: "zoomin"
                    },
                    {
                        role: "zoomout"
                    },
                    {
                        type: "separator"
                    },
                    {
                        role: "togglefullscreen"
                    }
                ]
            },
            {
                role: "help",
                submenu: [
                    {
                        label: "Learn More",
                        click() { electron.shell.openExternal("http://electron.atom.io") }
                    }
                ]
            }
        ];

        electron.remote.Menu.setApplicationMenu(electron.remote.Menu.buildFromTemplate(template));

    }

    Utility.OpenModalDialog("/Welcome", "GET");

    // Register clickable attributes
    $(document).on("click", "[load-modal]", function (e) {
        e.preventDefault();
        const $this = $(this);
        const url = $this.attr("load-modal").toString();
        const method = $this.attr("load-modal-method").toString();
        const dataAttr = $this.attr("load-modal-data");
        const data = dataAttr ? JSON.parse(JSON.parse("\"" + dataAttr.toString() + "\"")) : null;
        Utility.OpenModalDialog(url, method, data);
    });
});