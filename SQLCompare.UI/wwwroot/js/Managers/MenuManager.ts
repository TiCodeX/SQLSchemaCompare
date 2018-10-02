/**
 * Contains utility method to handle the main application menu
 */
class MenuManager {
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
                            Project.New(false);
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
                            Project.Save().catch((): void => {
                                // Do nothing
                            });
                        },
                    },
                    {
                        id: "menuSaveProjectAs",
                        label: `${Localization.Get("MenuSaveProjectAs")}...`,
                        enabled: false,
                        click(): void {
                            Project.Save(true).catch((): void => {
                                // Do nothing
                            });
                        },
                    },
                    {
                        id: "menuCloseProject",
                        label: Localization.Get("MenuCloseProject"),
                        enabled: false,
                        click(): void {
                            Project.Close(false);
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
                            PageManager.LoadPage(PageManager.Page.Project, false);
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
                label: Localization.Get("MenuHelp"),
                submenu: [
                    {
                        id: "menuOpenLogsFolder",
                        label: `${Localization.Get("MenuOpenLogsFolder")}...`,
                        click(): void {
                            electron.ipcRenderer.send("OpenLogsFolder");
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        label: Localization.Get("MenuAbout"),
                        click(): void {
                            electron.shell.openExternal("https://www.ticodex.com/");
                        },
                    },
                ],
            },
        ];

        if (electron.remote.process.defaultApp) { // tslint:disable-line:no-unsafe-any
            template.push({
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
                ],
            });
        }

        electron.remote.Menu.setApplicationMenu(electron.remote.Menu.buildFromTemplate(template));
    }

    /**
     * Enable/Disable the Project related menu items
     * @param enabled Whether to enable or disable the menu items
     */
    public static ToggleProjectRelatedMenuStatus(enable: boolean): void {
        this.ToggleMenuItems([
            "menuSaveProject",
            "menuSaveProjectAs",
            "menuCloseProject",
            "menuEditProject",
            "menuPerformCompare",
            "toolbarSaveProject",
            "toolbarEditProject",
            "toolbarPerformCompare",
        ], enable);
    }

    /**
     * Enable/Disable the menu items during the running task
     * @param enabled Whether to enable or disable the menu items
     */
    public static ToggleRunningTaskRelatedMenuStatus(enable: boolean): void {
        this.ToggleMenuItems([
            "menuNewProject",
            "menuOpenProject",
            "menuSaveProject",
            "menuSaveProjectAs",
            "menuCloseProject",
            "menuSettings",
            "menuEditProject",
            "menuPerformCompare",
            "toolbarNewProject",
            "toolbarOpenProject",
            "toolbarSaveProject",
            "toolbarEditProject",
            "toolbarPerformCompare",
        ], enable);
    }

    /**
     * Enable/Disable the items specified in the array
     * @param items The list of items
     * @param enable Whether to enable or disable the menu items
     */
    private static ToggleMenuItems(items: Array<string>, enable: boolean): void {
        if (electron === undefined) {
            return;
        }

        const menu: Electron.Menu = electron.remote.Menu.getApplicationMenu();

        for (const item of items) {

            const menuItem: Electron.MenuItem = menu.getMenuItemById(item);
            if (menuItem !== undefined && menuItem !== null) {
                menuItem.enabled = enable;
                continue;
            }

            const toolbarItem: JQuery = $(`#${item}`);
            if (toolbarItem.length > 0) {
                if (enable) {
                    toolbarItem.removeAttr("disabled");
                } else {
                    toolbarItem.attr("disabled", "disabled");
                }
            }
        }
    }
}
