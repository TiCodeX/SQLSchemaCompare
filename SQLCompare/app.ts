/* tslint:disable:no-require-imports no-implicit-dependencies max-file-line-count only-arrow-functions no-magic-numbers */
import builderUtilRuntime = require("builder-util-runtime");
import childProcess = require("child_process");
import portfinder = require("detect-port");
import electron = require("electron");
import electronUpdater = require("electron-updater");
import windowStateKeeper = require("electron-window-state");
import fs = require("fs");
import glob = require("glob");
import log4js = require("log4js");
/* tslint:enable:no-require-imports no-implicit-dependencies */

electron.app.setAppUserModelId("ch.ticodex.sqlcompare");

const initialPort: number = 5000;
const splashUrl: string = `file://${__dirname}/splash.html`;
const servicePath: string = `./bin/SQLCompare.UI${process.platform === "win32" ? ".exe" : ""}`;
const loggerPath: string = "C:/ProgramData/SqlCompare/log/SqlCompare";
const loggerPattern: string = "-yyyy-MM-dd-ui.log";
const loggerLayout: string = "%d{yyyy-MM-dd hh:mm:ss.SSS}|%z|%p|%c|%m";
const loggerMaxArchiveFiles: number = 9;
const autoUpdaterUrl: string = "https://ticodex.blob.core.windows.net/releases";
let serviceUrl: string = "https://127.0.0.1:{port}";
let loginUrl: string = "https://127.0.0.1:{port}/login";
let serviceProcess: childProcess.ChildProcess;
let autoUpdaterInfo: electronUpdater.UpdateInfo;

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
            appenders: (process.defaultApp ? ["default", "console"] : ["default"]),
            level: (process.defaultApp ? "debug" : "info"),
        },
    },
});

const logger: log4js.Logger = log4js.getLogger("electron");
logger.info("Starting application...");

// Configure electron auto-updater
const autoUpdaterLogger: log4js.Logger = log4js.getLogger("electron-updater");
autoUpdaterLogger.level = (process.defaultApp ? "debug" : "info");
electronUpdater.autoUpdater.logger = autoUpdaterLogger;
electronUpdater.autoUpdater.autoDownload = true;
electronUpdater.autoUpdater.autoInstallOnAppQuit = true;
const autoUpdaterPublishOptions: builderUtilRuntime.GenericServerOptions = {
    provider: "generic",
    url: autoUpdaterUrl,
};
electronUpdater.autoUpdater.setFeedURL(autoUpdaterPublishOptions);
electronUpdater.autoUpdater.on("update-available", (info: electronUpdater.UpdateInfo) => {
    autoUpdaterInfo = info;
});

// Start an asynchronous function to delete old log files
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

// Register the renderer callback for logging the UI
electron.ipcMain.on("log", (event: electron.Event, data: { category: string; level: string; message: string }) => {
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
electron.ipcMain.on("OpenMainWindow", (event: electron.Event) => {
    createMainWindow();
});
// Register the renderer callback for opening the login window
electron.ipcMain.on("OpenLoginWindow", (event: electron.Event) => {
    createLoginWindow(true);
});
// Register the renderer callback for opening the login window
electron.ipcMain.on("ShowLoginWindow", (event: electron.Event) => {
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
electron.ipcMain.on("GetUpdateInfo", (event: electron.Event) => {
    event.sender.send("GetUpdateInfoResponse", autoUpdaterInfo);
});

/**
 * Create the main window ensuring to destroy the login window
 * @param url The url to pass for authentication purpose
 */
function createMainWindow(): void {
    const workAreaSize: electron.Size = electron.screen.getPrimaryDisplay().workAreaSize;
    logger.debug("Primary display size: %dx%d", workAreaSize.width, workAreaSize.height);

    // Load the previous state with fall-back to defaults
    const mainWindowState: { width: number; height: number; x: number; y: number; manage: Function } = windowStateKeeper({
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

    mainWindow.setMenu(electron.Menu.buildFromTemplate([{
        label: "File",
        submenu: [
            {
                role: "close",
                label: "Exit",
            },
        ],
    }]));

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
    });

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
 * Start the SQL Compare backend process
 * @param webPort The port to start the web server
 */
function startSqlCompareBackend(webPort: number): void {
    if (fs.existsSync(servicePath)) {
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
    }
}

/**
 * Startup function called when Electron is ready
 */
function startup(): void {
    // Setup request default auth header
    const filter: electron.OnBeforeSendHeadersFilter = {
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
    });
    splashWindow.loadURL(splashUrl);
    splashWindow.show();
    splashWindow.focus();
    logger.debug("Splashscreen window started");

    electronUpdater.autoUpdater.checkForUpdates().catch(() => {
        logger.error("Error checking for updates");
    });

    portfinder(initialPort, (errorWebPort: Error, webPort: number) => {
        serviceUrl = serviceUrl.replace("{port}", `${webPort}`);
        loginUrl = loginUrl.replace("{port}", `${webPort}`);

        startSqlCompareBackend(webPort);

        createLoginWindow(false);
        logger.debug("Login window started");

        let loadFailed: boolean = false;
        loginWindow.webContents.on("did-fail-load", () => {
            loadFailed = true;
        });
        loginWindow.webContents.on("did-finish-load", () => {
            if (loadFailed) {
                logger.debug("Unable to contact service, retrying...");
                // Reset the flag and trigger a new load
                loadFailed = false;
                loginWindow.loadURL(loginUrl);
            } else {
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
        serviceProcess.kill();
    }
});

// Quit when all windows are closed.
electron.app.on("window-all-closed", () => {
    /**
     * On OS X it is common for applications and their menu bar
     * to stay active until the user quits explicitly with Cmd + Q
     */
    if (process.platform !== "darwin") {
        electron.app.quit();
    }
    log4js.shutdown(() => {
        // Do nothing
    });
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
electron.app.on("certificate-error", (event: electron.Event, webContents: electron.WebContents, url: string, error: string, certificate: electron.Certificate, callback: Function) => {
    /**
     * On certificate error we disable default behaviour (stop loading the page)
     * and we then say "it is all fine - true" to the callback
     */
    event.preventDefault();
    callback(true);
});
