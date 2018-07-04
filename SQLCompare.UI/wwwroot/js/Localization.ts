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
    public static Load(): void {
        Utility.AjaxSyncCall("/Index?handler=LoadLocalization", Utility.HttpMethod.Get, undefined, (result: object): void => {

            // Remove old localization
            this.dictionary = new Array<[string, string]>();

            // Add new values
            for (const key in result) {
                if (result.hasOwnProperty(key)) {
                    this.dictionary.push([key, <string>result[key]]);
                }
            }
        });
    }

}
