/**
 * Contains utility methods to handle the monaco editor
 */
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
    public static CreateEditor(type: EditorManager.Type, domElementId: string, model: monaco.editor.IEditorModel): void {
        // Clear the dom element
        $(`#${domElementId}`).empty();
        // And clear the old instances
        this.ClearOldInstances();

        const editor: monaco.editor.IStandaloneCodeEditor | monaco.editor.IStandaloneDiffEditor = type === EditorManager.Type.Normal ?
            monaco.editor.create(document.getElementById(domElementId), this.defaultOptions) :
            monaco.editor.createDiffEditor(document.getElementById(domElementId), this.defaultOptionsDiff);

        $(`#${domElementId}`).attr("editorId", editor.getId());

        this.instances.push(editor);

        // Disable the command palette
        editor.addCommand(monaco.KeyCode.F1, (): void => { /* Do nothing */ }, "");

        editor.setModel(model);

        // Register context menu
        $(`#${domElementId} .monaco-editor .monaco-scrollable-element`).on("contextmenu", (e: JQuery.Event) => {

            let currentEditor: monaco.editor.IStandaloneCodeEditor;
            if (type === EditorManager.Type.Normal) {
                currentEditor = <monaco.editor.IStandaloneCodeEditor>editor;
            } else {
                currentEditor = $(e.currentTarget).parents(".editor").hasClass("original") ?
                    (<monaco.editor.IStandaloneDiffEditor>editor).getOriginalEditor() :
                    (<monaco.editor.IStandaloneDiffEditor>editor).getModifiedEditor();
            }

            electron.remote.Menu.buildFromTemplate([
                {
                    label: Localization.Get("MenuCopy"),
                    accelerator: "CmdOrCtrl+C",
                    click(): void {
                        electron.remote.clipboard.writeText(currentEditor.getModel().getValueInRange(currentEditor.getSelection()));
                    },
                },
                {
                    label: Localization.Get("MenuSelectAll"),
                    accelerator: "CmdOrCtrl+A",
                    click(): void {
                        currentEditor.setSelection(currentEditor.getModel().getFullModelRange());
                    },
                },
                {
                    type: "separator",
                },
                {
                    label: Localization.Get("MenuFind"),
                    accelerator: "CmdOrCtrl+F",
                    click(): void {
                        currentEditor.trigger("menu", "actions.find", undefined);
                    },
                },
                {
                    label: Localization.Get("MenuGoToLine"),
                    accelerator: "CmdOrCtrl+G",
                    click(): void {
                        currentEditor.trigger("menu", "editor.action.gotoLine", undefined);
                    },
                },
                {
                    type: "separator",
                },
                {
                    label: Localization.Get("MenuShowLineNumbers"),
                    type: "checkbox",
                    checked: this.defaultOptions.lineNumbers === "on",
                    click(): void {
                        // Set the default option to both editor types
                        EditorManager.defaultOptions.lineNumbers = EditorManager.defaultOptions.lineNumbers === "on" ? "off" : "on";
                        EditorManager.defaultOptionsDiff.lineNumbers = EditorManager.defaultOptions.lineNumbers;
                        // Change the option in all active instances
                        EditorManager.instances.forEach((instance: monaco.editor.IStandaloneCodeEditor | monaco.editor.IStandaloneDiffEditor) => {
                            instance.updateOptions({
                                lineNumbers: EditorManager.defaultOptions.lineNumbers,
                            });
                        });
                    },
                },
            ]).popup({});
        });
    }

    /**
     * Loop the editor instances and remove the old ones
     */
    private static ClearOldInstances(): void {
        for (let i: number = this.instances.length - 1; i >= 0; i--) {
            if ($(`div[editorId='${this.instances[i].getId()}']`).length === 0) {
                this.instances.splice(i, 1);
            }
        }
    }
}

namespace EditorManager {
    /**
     * HTTP Method for the ajax call
     */
    export enum Type {
        /**
         * Normal editor
         */
        Normal,
        /**
         * Diff editor
         */
        Diff,
    }
}
