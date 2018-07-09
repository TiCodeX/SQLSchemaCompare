import electron = require("electron");
import childProcess = require("child_process");
import fs = require("fs");
import windowStateKeeper = require("electron-window-state");

electron.app.setAppUserModelId("ch.ticodex.sqlcompare");

const servicePath = `./SQLCompare.UI/SQLCompare.UI${process.platform === "win32" ? ".exe" : ""}`;
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

    // Load the previous state with fall-back to defaults
    const mainWindowState = windowStateKeeper({
        defaultWidth: width - 200,
        defaultHeight: height - 100
    });

    const splashWindow = new electron.BrowserWindow({
        width: 300,
        height: 330,
        transparent: true,
        frame: false,
        alwaysOnTop: false
    });
    splashWindow.loadURL(`file://${__dirname}/splash.html`);

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

    mainWindow.loadURL("https://127.0.0.1:5000");

    let loadFailed = false;
    mainWindow.webContents.on("did-fail-load", () => {
        loadFailed = true;
    });
    mainWindow.webContents.on("did-finish-load", () => {
        if (loadFailed) {
            // Reset the flag and trigger a new load
            loadFailed = false;
            mainWindow.loadURL("https://127.0.0.1:5000");
        } else {
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