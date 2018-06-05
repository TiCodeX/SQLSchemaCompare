import electron = require("electron");
// Module to control application life.
const app = electron.app;

//const servicePath = "../SQLCompare.UI/bin/Debug/netcoreapp2.0/SQLCompare.UI.dll";
//const childProcess = require("child_process");
//const serviceProcess = childProcess.spawn("dotnet", [servicePath]);
//serviceProcess.stdout.on("data", data => {
//    console.log("stdout: " + data);
//});
//serviceProcess.stderr.on("data", data => {
//    console.log("stderr: " + data);
//});
//serviceProcess.on("close", code => {
//    console.log("closing code: " + code);
//});

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow: Electron.BrowserWindow;

function createWindow() {

    const { width, height } = electron.screen.getPrimaryDisplay().workAreaSize;

    const splashWindow = new electron.BrowserWindow({
        width: 300,
        height: 250,
        transparent: true,
        frame: false,
        alwaysOnTop: true
    });
    splashWindow.loadURL(`file://${__dirname}/splash.html`);

    // Create the browser window.
    mainWindow = new electron.BrowserWindow({
        width: width - 200,
        height: height - 100,
        show: false,
        webPreferences: {
            nodeIntegration: true,
        }
    });

    const filter = {
        urls: ["http://*/*", "https://*/*"]
    };
    electron.session.defaultSession.webRequest.onBeforeSendHeaders(filter, (details, callback) => {
        details.requestHeaders["CustomAuthToken"] = "prova";
        callback({ cancel: false, requestHeaders: details.requestHeaders });
    });

    mainWindow.loadURL("https://localhost:5000");

    let loadFailed = false;
    mainWindow.webContents.on("did-fail-load", () => {
        loadFailed = true;
    });
    mainWindow.webContents.on("did-finish-load", () => {
        if (loadFailed) {
            // Reset the flag and trigger a new load
            loadFailed = false;
            mainWindow.loadURL("https://localhost:5000");
        } else {
            splashWindow.destroy();
            mainWindow.show();
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
    });
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on("ready", createWindow);

// Quit when all windows are closed.
app.on("window-all-closed", () => {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform !== "darwin") {
        app.quit();
    }
});

app.on("activate", () => {
    // On OS X it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (mainWindow === null) {
        createWindow();
    }
});

// SSL/TSL: this is the self signed certificate support
app.on("certificate-error", (event, webContents, url, error, certificate, callback) => {
    // On certificate error we disable default behaviour (stop loading the page)
    // and we then say "it is all fine - true" to the callback
    event.preventDefault();
    callback(true);
});