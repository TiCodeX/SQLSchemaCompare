/**
 * Contains the Localization
 */
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class Localization {
    /**
     * Dictionary containing the tokens and their related localized strings
     */
    private static dictionary: Record<string, string> = {};

    /**
     * Get the localized string
     * @param token - The token of the string
     * @returns The localized string
     */
    public static Get(token: string): string {
        return this.dictionary[token] ?? `[[${token}]]`;
    }

    /**
     * Load the localization
     */
    public static async Load(): Promise<void> {
        const result = await Utility.AjaxCall<Record<string, string>>("/Index?handler=LoadLocalization", HttpMethod.Get, undefined);
        if (result.Result) {
            this.dictionary = result.Result;
        }
    }
}
