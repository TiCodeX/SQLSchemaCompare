
let mainSplitter: Split.Instance;

$(() => {
    Utility.ApplicationStartup();

    // Configure the monaco editor loader
    amdRequire.config({
        baseUrl: "lib/monaco-editor",
    });

    // Preload the monaco-editor
    setTimeout((): void => {
        amdRequire(["vs/editor/editor.main"], (): void => {
            // Nothing to do
        });
    }, 0);

    Menu.CreateMenu();

    PageManager.LoadPage(PageManager.Page.Welcome);
});
