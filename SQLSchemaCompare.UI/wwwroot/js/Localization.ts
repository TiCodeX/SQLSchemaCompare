/**
 * Contains the Localization
 */
class Localization {
    /**
     * Dictionary containing the tokens and their related localized strings
     */
    private static dictionary: Array<[string, string]> = new Array<[string, string]>();

    /**
     * Get the localized string
     * @param token - The token of the string
     * @returns The localized string
     */
    public static Get(token: string): string {
        for (const keyValuePair of this.dictionary) {
            if (keyValuePair[0] === token) {
                return keyValuePair[1];
            }
        }

        return `[[${token}]]`;
    }

    /**
     * Load the localization
     */
    public static async Load(): Promise<void> {
        const result: ApiResponse<object> = await Utility.AjaxCall("/Index?handler=LoadLocalization", Utility.HttpMethod.Get, undefined);

        // Remove old localization
        this.dictionary = new Array<[string, string]>();

        // Add new values
        for (const key in result.Result) {
            if (result.Result.hasOwnProperty(key)) {
                this.dictionary.push([key, <string>result.Result[key]]);
            }
        }
    }

}
