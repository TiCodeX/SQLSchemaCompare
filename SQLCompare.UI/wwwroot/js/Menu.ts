/**
 * Contains utility method to handle the main application menu
 */
class Menu {

    /**
     * Creates the main application menu
     */
    public static CreateMenu(): void {
        if (electron === undefined) {
            return;
        }

        const template: Array<Electron.MenuItemConstructorOptions> = [
            {
                label: "File",
                submenu: [
                    {
                        id: "menuNewProject",
                        label: "New Project",
                        click(): void {
                            Project.New();
                        },
                    },
                    {
                        id: "menuOpenProject",
                        label: "Open Project...",
                        click(): void {
                            Project.Load();
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        id: "menuSaveProject",
                        label: "Save Project...",
                        enabled: false,
                        click(): void {
                            Project.Save();
                        },
                    },
                    {
                        id: "menuCloseProject",
                        label: "Close Project",
                        enabled: false,
                        click(): void {
                            Project.Close(true);
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        role: "close",
                        label: "Exit",
                    },
                ],
            },
            {
                label: "Project",
                submenu: [
                    {
                        id: "menuEditProject",
                        label: "Edit",
                        enabled: false,
                        click(): void {
                            Utility.OpenModalDialog("/Project/CompareProject", "GET");
                        },
                    },
                ],
            },
            {
                label: "DEBUG",
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
                        role: "togglefullscreen",
                    },
                ],
            },
            {
                role: "help",
                submenu: [
                    {
                        label: "About",
                        click(): void {
                            electron.shell.openExternal("https://ticodex.ch/");
                        },
                    },
                ],
            },
        ];

        electron.remote.Menu.setApplicationMenu(electron.remote.Menu.buildFromTemplate(template));
    }

    /**
     * Enable/Disable the Project related menu items
     * @param enabled Whether to enable or disable the menu items
     */
    public static ToggleProjectRelatedMenuStatus(enable: boolean): void {
        if (electron === undefined) {
            return;
        }

        const menu: Electron.Menu = electron.remote.Menu.getApplicationMenu();

        menu.getMenuItemById("menuSaveProject").enabled = enable;
        menu.getMenuItemById("menuCloseProject").enabled = enable;
        menu.getMenuItemById("menuEditProject").enabled = enable;
    }
}
