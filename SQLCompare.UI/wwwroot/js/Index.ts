
let mainSplitter: Split.Instance;

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

        Menu.CreateMenu();

        PageManager.LoadPage(PageManager.Page.Welcome);

        $(document).on("keydown", (e: JQuery.Event): void => {
            const keyUp: number = 38;
            const keyDown: number = 40;

            switch (e.which) {
                case keyUp:
                case keyDown:
                    if (PageManager.GetOpenPage() !== PageManager.Page.Main) {
                        return;
                    }
                    const selectedElement: JQuery = $(".tcx-selected-row");
                    if (selectedElement.length === 0) {
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
    });
});
