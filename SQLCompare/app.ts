import electron = require("electron");
import childProcess = require("child_process");
import fs = require("fs");
import windowStateKeeper = require("electron-window-state");
import log4js = require("log4js");
import glob = require("glob")

electron.app.setAppUserModelId("ch.ticodex.sqlcompare");

const splashUrl = `file://${__dirname}/splash.html`;
const servicePath = `./SQLCompare.UI/SQLCompare.UI${process.platform === "win32" ? ".exe" : ""}`;
const serviceUrl = "https://127.0.0.1:5000";
const loggerPath = "C:\\ProgramData\\SqlCompare\\log\\";
const loggerFile = "SqlCompare-yyyy-MM-dd-ui.log";
const loggerLayout = "%d{yyyy-MM-dd hh:mm:ss.SSS}|%z|%p|%c|%m";
const loggerMaxArchiveFiles = 9;

log4js.configure({
    appenders: {
        default: {
            type: "dateFile",
            filename: loggerPath,
            pattern: loggerFile,
            layout: {
                type: "pattern",
                pattern: loggerLayout,
            },
            alwaysIncludePattern: true,
            keepFileExt: true,
        }
    },
    categories: {
        default: { appenders: ["default"], level: "debug" }
    }
});

const logger = log4js.getLogger("electron");
logger.info("Starting application...");

// Start an asynchronous function to delete old log files
setTimeout(() => {
    try {
        glob(loggerPath + loggerFile.replace("yyyy-MM-dd", "*"), null, (err, files) => {
            files.sort();
            files.reverse();
            files.slice(loggerMaxArchiveFiles).forEach(file => {
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
electron.ipcMain.on("log", (event, data: { category: string, level: string, message: string }) => {
    var uiLogger = log4js.getLogger(data.category);
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
    }
});

let serviceProcess: childProcess.ChildProcess;
if (fs.existsSync(servicePath)) {
    serviceProcess = childProcess.spawn(servicePath);
    //serviceProcess.stdout.on("data", data => {
    //    console.log("stdout: " + data);
    //});
    //serviceProcess.stderr.on("data", data => {
    //    console.log("stderr: " + data);
    //});
    //serviceProcess.on("close", code => {
    //    console.log("closing code: " + code);
    //});
}

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow: Electron.BrowserWindow;

function createWindow() {
    const { width, height } = electron.screen.getPrimaryDisplay().workAreaSize;
    logger.debug("Primary display size: %dx%d", width, height);

    // Load the previous state with fall-back to defaults
    const mainWindowState = windowStateKeeper({
        defaultWidth: width - 200,
        defaultHeight: height - 100
    });
    logger.debug("Window state size: %dx%d", mainWindowState.width, mainWindowState.height);
    logger.debug("Window state location: %dx%d", mainWindowState.x, mainWindowState.y);

    const splashWindow = new electron.BrowserWindow({
        width: 300,
        height: 330,
        transparent: true,
        frame: false,
        alwaysOnTop: false
    });
    splashWindow.loadURL(splashUrl);
    logger.debug("Splashscreen window started");

    // Create the browser window.
    mainWindow = new electron.BrowserWindow({
        x: mainWindowState.x,
        y: mainWindowState.y,
        width: mainWindowState.width,
        height: mainWindowState.height,
        show: false,
        webPreferences: {
            nodeIntegration: true,
        }
    });

    mainWindow.setMenu(electron.Menu.buildFromTemplate([{
        label: "File",
        submenu: [
            {
                role: "close",
                label: "Exit",
            }
        ],
    }]));

    const filter = {
        urls: ["http://*/*", "https://*/*"]
    };
    electron.session.defaultSession.webRequest.onBeforeSendHeaders(filter, (details, callback) => {
        details.requestHeaders["CustomAuthToken"] = "prova";
        callback({ cancel: false, requestHeaders: details.requestHeaders });
    });

    mainWindow.loadURL(serviceUrl);
    logger.debug("Main window started");

    let loadFailed = false;
    mainWindow.webContents.on("did-fail-load", () => {
        loadFailed = true;
    });
    mainWindow.webContents.on("did-finish-load", () => {
        if (loadFailed) {
            logger.debug("Unable to contact service, retrying...");
            // Reset the flag and trigger a new load
            loadFailed = false;
            mainWindow.loadURL(serviceUrl);
        } else {
            logger.debug("Service connected, hide splashscreen window and show main window");
            splashWindow.destroy();
            mainWindow.show();

            // Let us register listeners on the window, so we can update the state
            // automatically (the listeners will be removed when the window is closed)
            // and restore the maximized or full screen state
            mainWindowState.manage(mainWindow);

            // Bring the window to front
            mainWindow.focus();

            // Open the DevTools.
            //mainWindow.webContents.openDevTools()

            logger.info("Application started successfully");
        }
    });

    // Emitted when the window is closed.
    mainWindow.on("closed", () => {
        // Dereference the window object, usually you would store windows
        // in an array if your app supports multi windows, this is the time
        // when you should delete the corresponding element.
        mainWindow = null;
        if (serviceProcess) {
            serviceProcess.kill();
        }
    });
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
electron.app.on("ready", createWindow);

// Quit when all windows are closed.
electron.app.on("window-all-closed", () => {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform !== "darwin") {
        electron.app.quit();
    }
    log4js.shutdown((err) => { });
});

electron.app.on("activate", () => {
    // On OS X it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (mainWindow === null) {
        createWindow();
    }
});

// SSL/TSL: this is the self signed certificate support
electron.app.on("certificate-error", (event, webContents, url, error, certificate, callback) => {
    // On certificate error we disable default behaviour (stop loading the page)
    // and we then say "it is all fine - true" to the callback
    event.preventDefault();
    callback(true);
});