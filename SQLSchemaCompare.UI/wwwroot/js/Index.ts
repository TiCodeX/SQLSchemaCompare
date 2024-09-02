declare const amdRequire: Require;

$(() => {
    void Utility.ApplicationStartup().then(() => {
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

        void MenuManager.CreateMenu();

        void PageManager.LoadPage(Page.Welcome);

        $(document).on("keydown", (event: JQuery.Event): void => {
            const keyUp: number = 38;
            const keyDown: number = 40;

            switch (event.which) {
                case keyUp:
                case keyDown: {
                    if (PageManager.GetOpenPage() !== Page.Main) {
                        return;
                    }
                    if ($(".tcx-selected-row").length === 0) {
                        return;
                    }
                    if (event.which === keyUp) {
                        Main.SelectPrevRow();
                    } else {
                        Main.SelectNextRow();
                    }
                    break;
                }

                default: {
                    // Exit this handler for other keys
                    return;
                }
            }

            // Prevent the default action (scroll / move caret)
            event.preventDefault();
        });

        electron.ipcRenderer.on("LoadProject", (_event, projectToLoad) => {
            void Project.Load(false, projectToLoad as string);
        });

        electron.ipcRenderer.send("CheckLoadProject");

        electron.ipcRenderer.send("OpenMainWindow");
    });
});
