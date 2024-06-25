/**
 * Contains utility method to handle the main application menu
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class MenuManager {
    /**
     * Creates the main application menu
     */
    public static async CreateMenu(): Promise<void> {
        //#region Electron Menu
        const template: Array<Electron.MenuItemConstructorOptions> = [
            {
                label: Localization.Get("MenuFile"),
                submenu: [
                    {
                        id: "menuNewProject",
                        label: Localization.Get("MenuNewProject"),
                        accelerator: "CmdOrCtrl+N",
                        click: () => {
                            Project.New(false);
                        },
                    },
                    {
                        id: "menuOpenProject",
                        label: `${Localization.Get("MenuOpenProject")}...`,
                        accelerator: "CmdOrCtrl+O",
                        click: () => {
                            Project.Load().catch((): void => {
                                // Do nothing
                            });
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        id: "menuSaveProject",
                        label: `${Localization.Get("MenuSaveProject")}`,
                        accelerator: "CmdOrCtrl+S",
                        enabled: false,
                        click: () => {
                            void Project.Save();
                        },
                    },
                    {
                        id: "menuSaveProjectAs",
                        label: `${Localization.Get("MenuSaveProjectAs")}...`,
                        accelerator: "CmdOrCtrl+Shift+S",
                        enabled: false,
                        click: () => {
                            void Project.Save(true);
                        },
                    },
                    {
                        id: "menuCloseProject",
                        label: Localization.Get("MenuCloseProject"),
                        accelerator: "CmdOrCtrl+Q",
                        enabled: false,
                        click: () => {
                            Project.Close(false);
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        id: "menuSettings",
                        label: Localization.Get("MenuSettings"),
                        click: () => {
                            Settings.Open();
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        role: "close",
                        label: Localization.Get("MenuExit"),
                        accelerator: "Alt+F4",
                    },
                ],
            },
            {
                label: Localization.Get("MenuProject"),
                submenu: [
                    {
                        id: "menuEditProject",
                        label: Localization.Get("MenuEdit"),
                        accelerator: "CmdOrCtrl+E",
                        enabled: false,
                        click: () => {
                            void Project.OpenPage(false);
                        },
                    },
                    {
                        id: "menuPerformCompare",
                        label: Localization.Get("MenuCompare"),
                        accelerator: "CmdOrCtrl+R",
                        enabled: false,
                        click: () => {
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
                        click: () => {
                            electron.ipcRenderer.send("OpenLogsFolder");
                        },
                    },
                    {
                        id: "menuDeveloperTools",
                        label: Localization.Get("MenuDeveloperTools"),
                        accelerator: "F12",
                        click: (_item, focusedWindow) => {
                            if (focusedWindow) {
                                focusedWindow.webContents.toggleDevTools();
                            }
                        },
                    },
                    {
                        type: "separator",
                    },
                    {
                        id: "menuAbout",
                        label: Localization.Get("MenuAbout"),
                        accelerator: "F1",
                        click: () => {
                            void DialogManager.OpenModalDialog(Localization.Get("MenuAbout"), "/AboutPageModel", "500px", true);
                        },
                    },
                ],
            },
        ];
        electron.remote.Menu.setApplicationMenu(electron.remote.Menu.buildFromTemplate(template));
        //#endregion

        //#region Toolbar

        await Utility.AjaxGetPage("/ToolbarPageModel").then((result: string): void => {
            $(".tcx-row-header").html(result);
        });

        //#endregion

        void Promise.resolve();
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

        if (!enable) {
            this.ToggleMainOpenRelatedMenuStatus(enable);
        }
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

        if (!enable) {
            this.ToggleMainOpenRelatedMenuStatus(enable);
        }
    }

    /**
     * Enable/Disable the menu items related to the main page open
     * @param enable Whether to enable or disable the menu items
     */
    public static ToggleMainOpenRelatedMenuStatus(enable: boolean): void {
        this.ToggleMenuItems(["toolbarGenerateScript"], enable);
    }

    /**
     * Enable/Disable the items specified in the array
     * @param items The list of items
     * @param enable Whether to enable or disable the menu items
     */
    private static ToggleMenuItems(items: Array<string>, enable: boolean): void {
        const menu = electron.remote.Menu.getApplicationMenu();

        for (const item of items) {

            const menuItem = menu?.getMenuItemById(item) as Electron.MenuItem | null;
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
