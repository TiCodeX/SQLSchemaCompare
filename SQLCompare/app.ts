/* tslint:disable:no-require-imports no-implicit-dependencies max-file-line-count only-arrow-functions no-magic-numbers */
import builderUtilRuntime = require("builder-util-runtime");
import childProcess = require("child_process");
import portfinder = require("detect-port");
import electron = require("electron");
import electronUpdater = require("electron-updater");
import electronWindowState = require("electron-window-state");
import fs = require("fs");
import glob = require("glob");
import log4js = require("log4js");
import os = require("os");
import path = require("path");
/* tslint:enable:no-require-imports no-implicit-dependencies */

electron.app.setAppUserModelId("ch.ticodex.sqlcompare");
// Set the productName in the userData path instead of the default application name
electron.app.setPath("userData", path.join(electron.app.getPath("appData"), "SQL Compare"));

const isDebug: boolean = process.defaultApp;
const initialPort: number = 5000;
const splashUrl: string = `file://${__dirname}/splash.html`;
const loggerPath: string = path.join(os.homedir(), ".SQLCompare", "log", "SQLCompare");
const loggerPattern: string = "-yyyy-MM-dd-ui.log";
const loggerLayout: string = "%d{yyyy-MM-dd hh:mm:ss.SSS}|%z|%p|%c|%m";
const loggerMaxArchiveFiles: number = 9;
const autoUpdaterUrl: string = "https://ticodex.blob.core.windows.net/releases";
let servicePath: string;
switch (electronUpdater.getCurrentPlatform()) {
    case "linux":
        servicePath = path.join(path.dirname(process.execPath), "bin", "SQLCompare.UI");
        break;
    case "darwin":
        servicePath = path.join(path.dirname(path.dirname(process.execPath)), "bin", "SQLCompare.UI");
        break;
    default: // Windows
        servicePath = path.join(path.dirname(process.execPath), "bin", "SQLCompare.UI.exe");
}
let serviceUrl: string = "https://127.0.0.1:{port}";
let loginUrl: string = `https://127.0.0.1:{port}/login?v=${electron.app.getVersion()}`;
let serviceProcess: childProcess.ChildProcess;
let autoUpdaterInfo: electronUpdater.UpdateInfo;
let autoUpdaterReadyToBeInstalled: boolean = false;
let autoUpdaterAutoDownloadFailed: boolean = false;

/**
 * Keep a global reference of the window object, if you don't, the window will
 * be closed automatically when the JavaScript object is garbage collected.
 */
let splashWindow: Electron.BrowserWindow;
let mainWindow: Electron.BrowserWindow;
let loginWindow: Electron.BrowserWindow;

log4js.configure({
    appenders: {
        default: {
            type: "dateFile",
            filename: loggerPath,
            pattern: loggerPattern,
            layout: {
                type: "pattern",
                pattern: loggerLayout,
            },
            alwaysIncludePattern: true,
            keepFileExt: true,
        },
        console: {
            type: "console",
        },
    },
    categories: {
        default: {
            appenders: (isDebug ? ["default", "console"] : ["default"]),
            level: (isDebug ? "debug" : "info"),
        },
    },
});

const logger: log4js.Logger = log4js.getLogger("electron");
logger.info(`Starting SQL Compare v${electron.app.getVersion()}`);

//#region Check Single Instance
// If it's not able to get the lock it means that another instance already have it
const isSecondInstance: boolean = !electron.app.requestSingleInstanceLock();
electron.app.on("second-instance", () => {
    // Someone tried to run a second instance, we should focus our current window
    const currentWindow: Electron.BrowserWindow = loginWindow !== undefined ? loginWindow : mainWindow;
    if (currentWindow !== undefined && currentWindow !== null) {
        if (currentWindow.isMinimized()) {
            currentWindow.restore();
        }
        currentWindow.show();
        currentWindow.focus();
    }
});
//#endregion

//#region Configure electron auto-updater
const autoUpdaterLogger: log4js.Logger = log4js.getLogger("electron-updater");
autoUpdaterLogger.level = (isDebug ? "debug" : "info");
electronUpdater.autoUpdater.logger = {
    info: (message: string): void => { autoUpdaterLogger.info(message); },
    warn: (message: string): void => { autoUpdaterLogger.warn(message); },
    error: (message: string): void => { autoUpdaterLogger.error(message); },
    debug: (message: string): void => { autoUpdaterLogger.debug(message); },
};
electronUpdater.autoUpdater.autoDownload = !isDebug && !isSecondInstance && electronUpdater.getCurrentPlatform() !== "linux";
electronUpdater.autoUpdater.autoInstallOnAppQuit = false;
const autoUpdaterPublishOptions: builderUtilRuntime.GenericServerOptions = {
    provider: "generic",
    url: autoUpdaterUrl,
    useMultipleRangeRequest: false,
};
electronUpdater.autoUpdater.setFeedURL(autoUpdaterPublishOptions);
electronUpdater.autoUpdater.requestHeaders = {
    "x-ms-version": "2018-03-28",
};

/**
 * Send a notification to the window that an update is available
 */
function NotifyUpdateAvailable(): void {
    const currentWindow: Electron.BrowserWindow = loginWindow !== undefined ? loginWindow : mainWindow;
    if (currentWindow !== undefined && currentWindow !== null) {
        currentWindow.webContents.send("UpdateAvailable",
            {
                platform: electronUpdater.getCurrentPlatform(),
                readyToBeInstalled: autoUpdaterReadyToBeInstalled,
                autoDownloadFailed: autoUpdaterAutoDownloadFailed,
                version: (autoUpdaterInfo === undefined ? "" : autoUpdaterInfo.version),
            });
    }
}

electronUpdater.autoUpdater.on("update-available", (info: electronUpdater.UpdateInfo) => {
    autoUpdaterInfo = info;
});
electronUpdater.autoUpdater.on("update-downloaded", () => {
    autoUpdaterReadyToBeInstalled = true;
    NotifyUpdateAvailable();
});
electronUpdater.autoUpdater.on("error", () => {
    // Only set that an error occurred if there is an update available
    if (autoUpdaterInfo !== undefined) {
        autoUpdaterAutoDownloadFailed = true;
        NotifyUpdateAvailable();
    }
});
//#endregion

//#region Start an asynchronous function to delete old log files
setTimeout(() => {
    try {
        glob(loggerPath + loggerPattern.replace("yyyy-MM-dd", "*"), (err: object, files: Array<string>) => {
            files.sort();
            files.reverse();
            files.slice(loggerMaxArchiveFiles).forEach((file: string) => {
                try {
                    logger.info(`Deleting old archive file: ${file}`);
                    fs.unlinkSync(file);
                } catch (e) {
                    logger.error(`Delete file failed. Error: ${e}`);
                }
            });
        });
    } catch (ex) {
        logger.log(`Error deleting old log files: ${ex}`);
    }
}, 0);
//#endregion

//#region Register the rendered callbacks
// Register the renderer callback for logging the UI
electron.ipcMain.on("log", (event: Electron.Event, data: { category: string; level: string; message: string }) => {
    const uiLogger: log4js.Logger = log4js.getLogger(data.category);
    switch (data.level) {
        case "debug":
            uiLogger.debug(data.message);
            break;
        case "info":
            uiLogger.info(data.message);
            break;
        case "warning":
            uiLogger.warn(data.message);
            break;
        case "error":
            uiLogger.error(data.message);
            break;
        case "critical":
            uiLogger.fatal(data.message);
            break;
        default:
            uiLogger.info(data.message);
    }
});
// Register the renderer callback for opening the main window
electron.ipcMain.on("OpenMainWindow", () => {
    createMainWindow();
});
// Register the renderer callback for opening the login window
electron.ipcMain.on("OpenLoginWindow", () => {
    createLoginWindow(true);
});
// Register the renderer callback for opening the login window
electron.ipcMain.on("ShowLoginWindow", () => {
    // Destroy splash window
    if (splashWindow !== undefined) {
        splashWindow.destroy();
        splashWindow = undefined;
    }
    // Show login window
    if (loginWindow !== undefined) {
        loginWindow.show();
        loginWindow.focus();
    }
});
// Register the renderer callback to retrieve the updates
electron.ipcMain.on("CheckUpdateAvailable", () => {
    NotifyUpdateAvailable();
});
// Register the renderer callback to quit and install the update
electron.ipcMain.on("QuitAndInstall", () => {
    electronUpdater.autoUpdater.quitAndInstall(true, true);
});
// Register the renderer callback to open the logs folder
electron.ipcMain.on("OpenLogsFolder", () => {
    electron.shell.openItem(path.dirname(loggerPath));
});
//#endregion

/**
 * Set an empty application menu (except OSX)
 */
function setEmptyApplicationMenu(): void {
    if (!isDebug) {
        if (electronUpdater.getCurrentPlatform() === "darwin") {
            // On OSX is not possible to remove the menu, hence create only the Exit entry
            electron.Menu.setApplicationMenu(electron.Menu.buildFromTemplate([
                {
                    label: "File",
                    submenu: [
                        {
                            role: "close",
                            label: "Exit",
                        },
                    ],
                },
            ]));
        } else {
            // On Windows and Linux remove the menu completely
            electron.Menu.setApplicationMenu(null); // tslint:disable-line:no-null-keyword
        }
    } else {
        // In debug mode it's useful to have the Reload and the developer tools
        electron.Menu.setApplicationMenu(electron.Menu.buildFromTemplate([
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
                ],
            },
        ]));
    }
}

/**
 * Create the main window ensuring to destroy the login window
 * @param url The url to pass for authentication purpose
 */
function createMainWindow(): void {
    const workAreaSize: Electron.Size = electron.screen.getPrimaryDisplay().workAreaSize;
    logger.debug("Primary display size: %dx%d", workAreaSize.width, workAreaSize.height);

    // Load the previous state with fall-back to defaults
    const mainWindowState: electronWindowState.State = electronWindowState({
        defaultWidth: workAreaSize.width - 200,
        defaultHeight: workAreaSize.height - 100,
    });
    logger.debug("Window state size: %dx%d", mainWindowState.width, mainWindowState.height);
    logger.debug("Window state location: %dx%d", mainWindowState.x, mainWindowState.y);

    // Create the browser window.
    mainWindow = new electron.BrowserWindow({
        x: mainWindowState.x,
        y: mainWindowState.y,
        width: mainWindowState.width,
        height: mainWindowState.height,
        show: false,
        webPreferences: {
            nodeIntegration: true,
        },
    });

    setEmptyApplicationMenu();

    /**
     * Let us register listeners on the window, so we can update the state
     * automatically (the listeners will be removed when the window is closed)
     * and restore the maximized or full screen state
     */
    mainWindowState.manage(mainWindow);

    mainWindow.loadURL(serviceUrl);

    // Emitted when the window is closed.
    mainWindow.on("closed", () => {
        /**
         * Dereference the window object, usually you would store windows
         * in an array if your app supports multi windows, this is the time
         * when you should delete the corresponding element.
         */
        mainWindow = undefined;
    });

    // Destroy splash window
    if (splashWindow !== undefined) {
        splashWindow.destroy();
        splashWindow = undefined;
    }
    // Destroy login window
    if (loginWindow !== undefined) {
        loginWindow.destroy();
        loginWindow = undefined;
    }

    mainWindow.show();
    mainWindow.focus();
}

/**
 * Create the login window ensuring to destroy the main window
 * @param show True if the window should be shown
 */
function createLoginWindow(load: boolean): void {
    // Create the login window
    loginWindow = new electron.BrowserWindow({
        width: 700,
        height: 650,
        show: false,
        center: true,
        resizable: false,
        maximizable: false,
    });

    setEmptyApplicationMenu();

    // Emitted when the window is closed.
    loginWindow.on("closed", () => {
        /**
         * Dereference the window object, usually you would store windows
         * in an array if your app supports multi windows, this is the time
         * when you should delete the corresponding element.
         */
        loginWindow = undefined;
    });

    if (mainWindow !== undefined) {
        mainWindow.destroy();
        mainWindow = undefined;
    }

    if (load) {
        loginWindow.loadURL(loginUrl);
        loginWindow.show();
        loginWindow.focus();
    }
}

/**
 * Start the SQL Compare back-end process
 * @param webPort The port to start the web server
 */
function startSqlCompareBackend(webPort: number): void {
    if (fs.existsSync(servicePath)) {
        logger.info(`Starting service ${servicePath} (${webPort})`);
        serviceProcess = childProcess.spawn(servicePath, [`${webPort}`]);
        /*
        serviceProcess.stdout.on("data", data => {
            console.log("stdout: " + data);
        });
        serviceProcess.stderr.on("data", data => {
            console.log("stderr: " + data);
        });
        serviceProcess.on("close", code => {
            console.log("closing code: " + code);
        });
        */
    } else {
        logger.error(`Unable to find executable: ${servicePath}`);
    }
}

/**
 * Startup function called when Electron is ready
 */
function startup(): void {
    if (isSecondInstance) {
        logger.info("Another instance is already running");
        electron.app.quit();

        return;
    }

    // Setup request default auth header
    const filter: Electron.OnBeforeSendHeadersFilter = {
        urls: [
            "http://*/*",
            "https://*/*",
        ],
    };
    electron.session.defaultSession.webRequest.onBeforeSendHeaders(filter, (details: { requestHeaders: object }, callback: Function) => {
        details.requestHeaders["CustomAuthToken"] = "prova"; // tslint:disable-line:no-string-literal
        callback({ cancel: false, requestHeaders: details.requestHeaders });
    });

    splashWindow = new electron.BrowserWindow({
        width: 640,
        height: 400,
        transparent: true,
        frame: false,
        alwaysOnTop: false,
        resizable: false,
        maximizable: false,
    });
    splashWindow.loadURL(splashUrl);
    splashWindow.show();
    splashWindow.focus();
    logger.debug("Splashscreen window started");

    electronUpdater.autoUpdater.checkForUpdates().catch(() => {
        logger.error("Error checking for updates");
    });

    let closeLoginWindow: boolean = true;
    splashWindow.on("closed", () => {
        if (closeLoginWindow) {
            // Destroy login window
            if (loginWindow !== undefined) {
                loginWindow.destroy();
                loginWindow = undefined;
            }
        }
        splashWindow = undefined;
    });

    portfinder(initialPort, (errorWebPort: Error, webPort: number) => {
        serviceUrl = serviceUrl.replace("{port}", `${webPort}`);
        loginUrl = loginUrl.replace("{port}", `${webPort}`);

        startSqlCompareBackend(webPort);

        // Splash already closed
        if (splashWindow === undefined) {
            return;
        }

        createLoginWindow(false);
        logger.debug("Login window started");

        let loadFailed: boolean = false;
        let retries: number = 100;
        loginWindow.webContents.on("did-fail-load", () => {
            loadFailed = true;
            retries--;
        });
        loginWindow.webContents.on("did-finish-load", () => {
            if (loadFailed) {
                logger.debug(`Unable to contact service, retrying... (${retries})`);
                if (retries > 0) {
                    // Reset the flag and trigger a new load
                    loadFailed = false;
                    loginWindow.loadURL(loginUrl);
                } else {
                    electron.dialog.showErrorBox("SQL Compare - Error", "An unexpected error has occurred");
                    electron.app.quit();
                }
            } else {
                // Do not close the login window when the splash is closed
                closeLoginWindow = false;
                logger.info("Application started successfully");
            }
        });

        // Events registered, now load the URL
        loginWindow.loadURL(loginUrl);
    });
}

/**
 * This method will be called when Electron has finished
 * initialization and is ready to create browser windows.
 * Some APIs can only be used after this event occurs.
 */
electron.app.on("ready", startup);

// This method will be called when Electron will quit the application
electron.app.on("will-quit", () => {
    if (serviceProcess !== undefined) {
        logger.info("Stopping service");
        serviceProcess.kill();
    }

    logger.info("Application closed");
    log4js.shutdown(() => {
        // Do nothing
    });
});

// Quit when all windows are closed.
electron.app.on("window-all-closed", () => {
    electron.app.quit();
});

electron.app.on("activate", () => {
    /**
     * On OS X it's common to re-create a window in the app when the
     * dock icon is clicked and there are no other windows open.
     */
    if (mainWindow === undefined && loginWindow === undefined) {
        startup();
    }
});

// SSL/TSL: this is the self signed certificate support
electron.app.on("certificate-error", (event: Electron.Event, webContents: Electron.WebContents, url: string, error: string, certificate: Electron.Certificate, callback: Function) => {
    /**
     * On certificate error we disable default behavior (stop loading the page)
     * and we then say "it is all fine - true" to the callback
     */
    event.preventDefault();
    callback(true);
});
