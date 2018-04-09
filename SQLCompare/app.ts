const electron = require('electron')
// Module to control application life.
const app = electron.app
// Module to create native browser window.
const BrowserWindow = electron.BrowserWindow

//const servicePath = '../SQLCompare.UI/bin/Debug/netcoreapp2.0/SQLCompare.UI.dll'
//const child_process = require('child_process')
//const serviceProcess = child_process.spawn("dotnet", [servicePath])
//serviceProcess.stdout.on('data', function (data) {
//    console.log('stdout: ' + data)
//})
//serviceProcess.stderr.on('data', function (data) {
//    console.log('stderr: ' + data)
//})
//serviceProcess.on('close', function (code) {
//    console.log('closing code: ' + code)
//})

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow

function createWindow() {

    const { width, height } = electron.screen.getPrimaryDisplay().workAreaSize

    // Create the browser window.
    mainWindow = new BrowserWindow({
        width: width - 200,
        height: height - 100,
        show: false,
        webPreferences: {
            nodeIntegration: false
        }
    })

    const { session } = require('electron')
    const filter = {
        urls: ["http://*/*", "https://*/*"]
    }
    session.defaultSession.webRequest.onBeforeSendHeaders(filter, (details, callback) => {
        details.requestHeaders["CustomAuthToken"] = "prova";
        callback({ cancel: false, requestHeaders: details.requestHeaders })
    })

    mainWindow.loadURL('http://localhost:5000')

    mainWindow.on('ready-to-show', () => {
        mainWindow.show();
        // Open the DevTools.
        //mainWindow.webContents.openDevTools()
    });

    // Emitted when the window is closed.
    mainWindow.on('closed', function () {
        // Dereference the window object, usually you would store windows
        // in an array if your app supports multi windows, this is the time
        // when you should delete the corresponding element.
        mainWindow = null
    })
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow)

// Quit when all windows are closed.
app.on('window-all-closed', function () {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform !== 'darwin') {
        app.quit()
    }
})

app.on('activate', function () {
    // On OS X it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (mainWindow === null) {
        createWindow()
    }
})
