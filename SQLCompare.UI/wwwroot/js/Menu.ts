/**
 * Contains utility method to handle the main application menu
 */
class Menu {
    /**
     * Keep a reference to the created menu
     */
    private static menu: Electron.Menu;

    /**
     * Creates the main application menu
     */
    public static CreateMenu(): void {
        if (electron === undefined) {
            return;
        }

        const template: Array<Electron.MenuItemConstructorOptions> = [
            {
                label: Localization.Get("MenuFile"),
                submenu: [
                    {
                        id: "menuNewProject",
                        label: Localization.Get("MenuNewProject"),
                        click(): void {
                            Project.New();
                        },
                    },
                    {
                        id: "menuOpenProject",
                        label: `${Localization.Get("MenuOpenProject")}...`,
                        click(): void {
                            Project.Load();
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        id: "menuSaveProject",
                        label: `${Localization.Get("MenuSaveProject")}`,
                        enabled: false,
                        click(): void {
                            Project.Save();
                        },
                    },
                    {
                        id: "menuSaveProjectAs",
                        label: `${Localization.Get("MenuSaveProjectAs")}...`,
                        enabled: false,
                        click(): void {
                            Project.Save(true);
                        },
                    },
                    {
                        id: "menuCloseProject",
                        label: Localization.Get("MenuCloseProject"),
                        enabled: false,
                        click(): void {
                            Project.Close(true);
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        id: "menuSettings",
                        label: Localization.Get("MenuSettings"),
                        click(): void {
                            Settings.Open();
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        role: "close",
                        label: Localization.Get("MenuExit"),
                    },
                ],
            },
            {
                label: Localization.Get("MenuProject"),
                submenu: [
                    {
                        id: "menuEditProject",
                        label: Localization.Get("MenuEditProject"),
                        enabled: false,
                        click(): void {
                            Project.Open();
                        },
                    },
                    {
                        id: "menuPerformCompare",
                        label: Localization.Get("MenuCompare"),
                        enabled: false,
                        click(): void {
                            Project.Compare();
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
                        accelerator: "F12",
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
                label: Localization.Get("MenuHelp"),
                submenu: [
                    {
                        label: Localization.Get("MenuAbout"),
                        click(): void {
                            electron.shell.openExternal("https://www.ticodex.com/");
                        },
                    },
                ],
            },
        ];

        this.menu = electron.remote.Menu.buildFromTemplate(template);

        electron.remote.getCurrentWindow().setMenu(this.menu);
    }

    /**
     * Enable/Disable the Project related menu items
     * @param enabled Whether to enable or disable the menu items
     */
    public static ToggleProjectRelatedMenuStatus(enable: boolean): void {
        if (this.menu === undefined) {
            return;
        }

        this.menu.getMenuItemById("menuSaveProject").enabled = enable;
        this.menu.getMenuItemById("menuSaveProjectAs").enabled = enable;
        this.menu.getMenuItemById("menuCloseProject").enabled = enable;
        this.menu.getMenuItemById("menuEditProject").enabled = enable;
        this.menu.getMenuItemById("menuPerformCompare").enabled = enable;
    }
}
