/* tslint:disable:no-require-imports no-implicit-dependencies max-file-line-count only-arrow-functions no-magic-numbers file-name-casing */
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

electron.app.setAppUserModelId("ch.ticodex.sqlschemacompare");
// Set the productName in the userData path instead of the default application name
electron.app.setPath("userData", path.join(electron.app.getPath("appData"), "SQL Schema Compare"));

const isDebug: boolean = process.defaultApp;
const initialPort: number = 25436;
const splashUrl: string = `file://${__dirname}/splash.html`;
const loggerPath: string = path.join(os.homedir(), ".SQLSchemaCompare", "log", "SQLSchemaCompare.log");
const loggerPattern: string = "yyyy-MM-dd-ui";
const loggerLayout: string = "%d{yyyy-MM-dd hh:mm:ss.SSS}|%z|%p|%c|%m";
const loggerMaxArchiveFiles: number = 9;
const authorizationHeaderName: string = "CustomAuthToken";
const authorizationHeaderValue: string = "d6e9b4c2-25d3-a625-e9a6-2135f3d2f809";
let servicePath: string;
switch (process.platform) {
    case "linux":
        servicePath = path.join(path.dirname(process.execPath), "bin", "TiCodeX.SQLSchemaCompare.UI");
        break;
    case "darwin":
        servicePath = path.join(path.dirname(path.dirname(process.execPath)), "bin", "TiCodeX.SQLSchemaCompare.UI");
        break;
    default: // Windows
        servicePath = path.join(path.dirname(process.execPath), "bin", "TiCodeX.SQLSchemaCompare.UI.exe");
}
let serviceUrl: string = `https://127.0.0.1:{port}/?v=${electron.app.getVersion()}`;
let serviceProcess: childProcess.ChildProcess;
let serviceCommunicationSuccessful: boolean = false;

// Read the first argument to get the project file
// In debug or during an auto-update multiple arguments are passed, so we check that is actually a file
let projectToOpen: string;
if (process.argv.length > 1) {
    try {
        const stat: fs.Stats = fs.lstatSync(process.argv[1]);
        if (stat.isFile()) {
            projectToOpen = process.argv[1];
        }
    } catch (ex) {
        // Ignore
    }
}

/**
 * Keep a global reference of the window object, if you don't, the window will
 * be closed automatically when the JavaScript object is garbage collected.
 */
let splashWindow: Electron.BrowserWindow;
let mainWindow: Electron.BrowserWindow;

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
logger.info(`Starting SQL Schema Compare v${electron.app.getVersion()}`);

//#region Check Single Instance
// If it's not able to get the lock it means that another instance already have it
const isSecondInstance: boolean = !electron.app.requestSingleInstanceLock();
electron.app.on("second-instance", (event: Event, argv: Array<string>) => {
    // Someone tried to run a second instance, we should focus our current window
    const currentWindow: Electron.BrowserWindow = mainWindow;
    if (currentWindow !== undefined && currentWindow !== null) {
        if (currentWindow.isMinimized()) {
            currentWindow.restore();
        }
        currentWindow.show();
        currentWindow.focus();
        if (!isDebug && argv.length > 1 && argv[1] !== "--updated") {
            currentWindow.webContents.send("LoadProject", argv[1]);
        }
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
// tslint:disable-next-line:completed-docs
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
    serviceCommunicationSuccessful = true;
   // Destroy splash window
    if (splashWindow !== undefined) {
        splashWindow.destroy();
        splashWindow = undefined;
    }

    // Show main Window
    if (mainWindow !== undefined) {
        mainWindow.show();
        mainWindow.focus();
    }
});

// Register the renderer callback to check if need to load a project
electron.ipcMain.on("CheckLoadProject", () => {
    if (projectToOpen !== undefined && mainWindow !== undefined) {
        mainWindow.webContents.send("LoadProject", projectToOpen);
        projectToOpen = undefined;
    }
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
        if (process.platform === "darwin") {
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
                        accelerator: "F5",
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
 * Create the main window
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
        x: mainWindowState.x < workAreaSize.width ? mainWindowState.x : undefined,
        y: mainWindowState.y < workAreaSize.height ? mainWindowState.y : undefined,
        width: mainWindowState.width,
        height: mainWindowState.height,
        title: "SQL Schema Compare - TiCodeX",
        show: false,
        webPreferences: {
            nodeIntegration: true,
            webviewTag: true,
        },
    });

    setEmptyApplicationMenu();

    /**
     * Let us register listeners on the window, so we can update the state
     * automatically (the listeners will be removed when the window is closed)
     * and restore the maximized or full screen state
     */
    mainWindowState.manage(mainWindow);

    // Emitted when the window is closed.
    mainWindow.on("closed", () => {
        /**
         * Dereference the window object, usually you would store windows
         * in an array if your app supports multi windows, this is the time
         * when you should delete the corresponding element.
         */
        mainWindow = undefined;
    });

    let loadFailed: boolean = false;
    let loadFailedError: string;
    let retries: number = 100;
    mainWindow.webContents.on("did-fail-load", (event: electron.Event, errorCode: number, errorDescription: string) => {
        loadFailed = true;
        loadFailedError = errorDescription;
        retries--;
    });
    mainWindow.webContents.on("did-finish-load", () => {
        if (loadFailed) {
            logger.debug(`Unable to contact service (${loadFailedError}), retrying... (${retries})`);
            if (retries > 0) {
                // Reset the flag and trigger a new load
                loadFailed = false;
                if (process.platform !== "linux") {
                    mainWindow.loadURL(serviceUrl);
                } else {
                    // Add a small delay on linux because the fail event is triggered very fast
                    setTimeout(() => {
                        mainWindow.loadURL(serviceUrl);
                    }, 400);
                }
            } else {
                logger.error(`Unable to contact service (${loadFailedError})`);
                electron.dialog.showErrorBox("SQL Schema Compare - Error", "An unexpected error has occurred");
                electron.app.quit();
            }
        } else {
            logger.info("Application started successfully");

            setTimeout(() => {
                if (!serviceCommunicationSuccessful) {
                    logger.error("Service unable to contact main application");
                    electron.dialog.showErrorBox("SQL Schema Compare - Error", "An unexpected error has occurred");
                    electron.app.quit();
                }
            }, 10000);
        }
    });

    // Events registered, now load the URL
    mainWindow.loadURL(serviceUrl);

}

/**
 * Start the SQL Schema Compare back-end process
 * @param webPort The port to start the web server
 */
function startService(webPort: number): void {
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
    const filter: Electron.Filter = {
        urls: [
            "http://*/*",
            "https://*/*",
        ],
    };
    electron.session.defaultSession.webRequest.onBeforeSendHeaders(filter,
        (details: electron.OnBeforeSendHeadersListenerDetails, callback: (beforeSendResponse: electron.BeforeSendResponse) => void) => {
            details.requestHeaders[authorizationHeaderName] = authorizationHeaderValue;
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
        webPreferences: {
            nodeIntegration: true,
            webviewTag: true,
        },
    });
    splashWindow.loadURL(splashUrl);
    splashWindow.show();
    splashWindow.focus();
    logger.debug("Splashscreen window started");

    splashWindow.on("closed", () => {
        if (!serviceCommunicationSuccessful) {
            if (mainWindow !== undefined) {
                mainWindow.destroy();
                mainWindow = undefined;
            }
        }
        splashWindow = undefined;
    });

    portfinder(initialPort, (errorWebPort: Error, webPort: number) => {
        if (isDebug) {
            // In debug the service always use the initial port and there's no need to start it
            serviceUrl = serviceUrl.replace("{port}", `${initialPort}`);
        } else {
            serviceUrl = serviceUrl.replace("{port}", `${webPort}`);
            startService(webPort);
        }

        // Splash already closed
        if (splashWindow === undefined) {
            return;
        }

        createMainWindow();
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
    if (mainWindow === undefined) {
        startup();
    }
});

// SSL/TSL: this is the self signed certificate support
electron.app.on("certificate-error", (
    event: Electron.Event,
    webContents: Electron.WebContents,
    url: string,
    error: string,
    certificate: Electron.Certificate,
    callback: (isTrusted: boolean) => void) => {
    /**
     * On certificate error we disable default behavior (stop loading the page)
     * and we then say "it is all fine - true" to the callback
     */
    event.preventDefault();
    callback(true);
});
