$(() => {
    Utility.ApplicationStartup().then(() => {
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

        MenuManager.CreateMenu();

        PageManager.LoadPage(Page.Welcome);

        $(document).on("keydown", (e: JQuery.Event): void => {
            const keyUp = 38;
            const keyDown = 40;

            switch (e.which) {
                case keyUp:
                case keyDown:
                    if (PageManager.GetOpenPage() !== Page.Main) {
                        return;
                    }
                    if ($(".tcx-selected-row").length === 0) {
                        return;
                    }
                    if (e.which === keyUp) {
                        Main.SelectPrevRow();
                    } else {
                        Main.SelectNextRow();
                    }
                    break;

                default:
                    // Exit this handler for other keys
                    return;
            }

            // Prevent the default action (scroll / move caret)
            e.preventDefault();
        });

        electron.ipcRenderer.on("LoadProject", (event: Electron.Event, projectToLoad: string) => {
            Project.Load(false, projectToLoad).catch((): void => {
                // Do nothing
            });
        });

        electron.ipcRenderer.send("CheckLoadProject");

        electron.ipcRenderer.send("OpenMainWindow");
    });
});
