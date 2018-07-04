/* tslint:disable:no-require-imports no-implicit-dependencies */
declare const amdRequire: Require;
const electron: Electron.AllElectron = (typeof require !== "undefined" ? <Electron.AllElectron>require("electron") : undefined);
/* tslint:enable:no-require-imports no-implicit-dependencies */

let mainSplitter: Split.Instance;

$(() => {
    // Configure the monaco editor loader
    amdRequire.config({
        baseUrl: "lib/monaco-editor",
    });

    // Enable bootstrap tooltips and popovers
    $("[data-toggle='tooltip']").tooltip();
    $("[data-toggle='popover']").popover();

    // Disable context menu
    window.addEventListener("contextmenu", (e: PointerEvent) => {
        e.preventDefault();
    }, false);

    // Prevent app zooming
    if (electron !== undefined) {
        electron.webFrame.setVisualZoomLevelLimits(1, 1);
        electron.webFrame.setLayoutZoomLevelLimits(0, 0);
    }

    // Preload the monaco-editor
    setTimeout((): void => {
        amdRequire(["vs/editor/editor.main"], (): void => {
            // Nothing to do
        });
    }, 0);

    Localization.Load();

    Menu.CreateMenu();

    Utility.OpenModalDialog("/WelcomePageModel", Utility.HttpMethod.Get);
});
