/**
 * Logger class which provides methods for logging
 * @param category The logger category
 */
class Logger {
    /**
     * The logger category
     */
    private readonly category: string;

    /**
     * Creates a new instance of the Logger
     * @param category The logger category
     */
    public constructor(category: string) {
        this.category = category;
    }

    /**
     * Writes a debug log message
     * @param message The log message
     */
    public debug(message: string): void {
        this.log("debug", message);
    }

    /**
     * Writes an info log message
     * @param message The log message
     */
    public info(message: string): void {
        this.log("info", message);
    }

    /**
     * Writes a warning log message
     * @param message The log message
     */
    public warning(message: string): void {
        this.log("warning", message);
    }

    /**
     * Writes an error log message
     * @param message The log message
     */
    public error(message: string): void {
        this.log("error", message);
    }

    /**
     * Writes a critical log message
     * @param message The log message
     */
    public critical(message: string): void {
        this.log("critical", message);
    }

    /**
     * Send the message to the main process
     * @param level The log level
     * @param message The log message
     */
    private log(level: string, message: string): void {
        if (electron === undefined) {
            return;
        }

        electron.ipcRenderer.send("log", { category: this.category, level: level, message: message });
    }
}
