interface ElectronRemote {
    // Taken from `RemoteMainInterface`
    app: Electron.App;
    autoUpdater: Electron.AutoUpdater;
    BrowserView: typeof Electron.BrowserView;
    BrowserWindow: typeof Electron.BrowserWindow;
    clipboard: Electron.Clipboard;
    contentTracing: Electron.ContentTracing;
    crashReporter: Electron.CrashReporter;
    desktopCapturer: Electron.DesktopCapturer;
    dialog: Electron.Dialog;
    globalShortcut: Electron.GlobalShortcut;
    inAppPurchase: Electron.InAppPurchase;
    ipcMain: Electron.IpcMain;
    Menu: typeof Electron.Menu;
    MenuItem: typeof Electron.MenuItem;
    MessageChannelMain: typeof Electron.MessageChannelMain;
    nativeImage: typeof Electron.nativeImage;
    nativeTheme: Electron.NativeTheme;
    net: Electron.Net;
    netLog: Electron.NetLog;
    Notification: typeof Electron.Notification;
    powerMonitor: Electron.PowerMonitor;
    powerSaveBlocker: Electron.PowerSaveBlocker;
    protocol: Electron.Protocol;
    screen: Electron.Screen;
    session: typeof Electron.session;
    ShareMenu: typeof Electron.ShareMenu;
    shell: Electron.Shell;
    systemPreferences: Electron.SystemPreferences;
    TouchBar: typeof Electron.TouchBar;
    Tray: typeof Electron.Tray;
    webContents: typeof Electron.webContents;
    webFrameMain: typeof Electron.webFrameMain;

    // Taken from `Remote`
    getCurrentWebContents(): Electron.WebContents;
    getCurrentWindow(): Electron.BrowserWindow;
    getGlobal(name: string): any;
}
