// Type definitions for jquery.serializeJSON
// Project: https://github.com/marioizquierdo/jquery.serializeJSON
// Definitions by: Fabian Maurer
// Definitions: https://github.com/DefinitelyTyped/DefinitelyTyped

interface SerializeJSONSettings {
    parseNumbers?: boolean;
    parseBooleans?: boolean;
    parseNulls?: boolean;
    parseAll?: boolean;
    parseWithFunction?: Function;
    checkboxUncheckedValue?: any;
    useIntKeysAsArrayIndex?: boolean;
}

interface JQuery<TElement = HTMLElement> extends Iterable<TElement> {
    serializeJSON(settings?: SerializeJSONSettings): any;
}