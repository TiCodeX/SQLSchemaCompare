/**
 * Contains utility methods to handle the monaco editor
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class EditorManager {

    /**
     * Default monaco editor options
     */
    public static readonly defaultOptions: monaco.editor.IEditorOptions = {
        automaticLayout: true,
        scrollBeyondLastLine: false,
        readOnly: true,
        stopRenderingLineAfter: -1,
        links: false,
        contextmenu: false,
        quickSuggestions: false,
        autoClosingBrackets: false,
        lineNumbers: "on",
        selectionHighlight: false,
        occurrencesHighlight: false,
        folding: false,
        minimap: {
            enabled: false,
        },
        matchBrackets: false,
        scrollbar: {
            vertical: "visible",
        },
        /*fontWeight: "normal",*/
        /*fontFamily: "Open Sans",*/
        fontSize: 13,
        /*lineHeight: 1,*/
    };

    /**
     * Default monaco diff editor options
     */
    public static readonly defaultOptionsDiff: monaco.editor.IDiffEditorConstructionOptions = $.extend({}, EditorManager.defaultOptions,
        {
            ignoreTrimWhitespace: false,
        });

    /**
     * List of currently active editor instances
     */
    private static readonly instances: Array<monaco.editor.IStandaloneCodeEditor | monaco.editor.IStandaloneDiffEditor> = [];

    /**
     * Create the monaco editor
     * @param type The editor type (normal or diff)
     * @param domElementId The id of the dom element to create the editor
     * @param model The editor data model
     */
    public static CreateEditor(type: EditorType, domElementId: string, model: monaco.editor.IEditorModel): void {
        const domElement = $(`#${domElementId}`);
        // Clear the dom element
        domElement.empty();
        // And clear the old instances
        this.ClearOldInstances();

        const editor: monaco.editor.IStandaloneCodeEditor | monaco.editor.IStandaloneDiffEditor = type === EditorType.Normal ?
            monaco.editor.create(domElement.get(0), this.defaultOptions) :
            monaco.editor.createDiffEditor(domElement.get(0), this.defaultOptionsDiff);

        $(`#${domElementId}`).attr("editorId", editor.getId());

        this.instances.push(editor);

        // Disable the command palette
        editor.addCommand(monaco.KeyCode.F1, (): void => { /* Do nothing */ }, "");

        editor.setModel(model);

        // Register context menu
        $(`#${domElementId} .monaco-editor .monaco-scrollable-element`).on("contextmenu", (event: JQuery.Event) => {

            let currentEditor: monaco.editor.IStandaloneCodeEditor;
            if (type === EditorType.Normal) {
                currentEditor = <monaco.editor.IStandaloneCodeEditor>editor;
            } else {
                currentEditor = $(event.currentTarget).parents(".editor").hasClass("original") ?
                    (<monaco.editor.IStandaloneDiffEditor>editor).getOriginalEditor() :
                    (<monaco.editor.IStandaloneDiffEditor>editor).getModifiedEditor();
            }

            electronRemote.Menu.buildFromTemplate([
                {
                    label: Localization.Get("MenuCopy"),
                    accelerator: "CmdOrCtrl+C",
                    click: () => {
                        electronRemote.clipboard.writeText(currentEditor.getModel().getValueInRange(currentEditor.getSelection()));
                    },
                },
                {
                    label: Localization.Get("MenuSelectAll"),
                    accelerator: "CmdOrCtrl+A",
                    click: () => {
                        currentEditor.setSelection(currentEditor.getModel().getFullModelRange());
                    },
                },
                {
                    type: "separator",
                },
                {
                    label: Localization.Get("MenuFind"),
                    accelerator: "CmdOrCtrl+F",
                    click: () => {
                        currentEditor.trigger("menu", "actions.find", "");
                    },
                },
                {
                    label: Localization.Get("MenuGoToLine"),
                    accelerator: "CmdOrCtrl+G",
                    click: () => {
                        currentEditor.trigger("menu", "editor.action.gotoLine", "");
                    },
                },
                {
                    type: "separator",
                },
                {
                    label: Localization.Get("MenuShowLineNumbers"),
                    type: "checkbox",
                    checked: this.defaultOptions.lineNumbers === "on",
                    click: () => {
                        // Set the default option to both editor types
                        EditorManager.defaultOptions.lineNumbers = EditorManager.defaultOptions.lineNumbers === "on" ? "off" : "on";
                        EditorManager.defaultOptionsDiff.lineNumbers = EditorManager.defaultOptions.lineNumbers;
                        // Change the option in all active instances
                        for (const instance of EditorManager.instances) {
                            instance.updateOptions({
                                lineNumbers: EditorManager.defaultOptions.lineNumbers,
                            });
                        }
                    },
                },
            ]).popup({});
        });
    }

    /**
     * Loop the editor instances and remove the old ones
     */
    private static ClearOldInstances(): void {
        for (let index: number = this.instances.length - 1; index >= 0; index--) {
            if ($(`div[editorId='${this.instances[index]?.getId()}']`).length === 0) {
                this.instances.splice(index, 1);
            }
        }
    }
}
