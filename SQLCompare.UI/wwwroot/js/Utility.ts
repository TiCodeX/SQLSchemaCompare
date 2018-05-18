/**
 * Contains various utility methods
 */
class Utility {
    /**
     * Encode the HTML tags inside the string
     * @param s - The string to encode
     * @returns The string properly encoded
     */
    public static EncodeHtmlEntities(s: string): string {
        return $("<div/>").text(s).html();
    }

    /**
     * Decode the HTML tags inside the string
     * @param s - The string to decode
     * @returns The decoded string
     */
    public static DecodeHtmlEntities(s: string): string {
        return $("<div/>").html(s).text();
    }

    /**
     * Perform an ajax call and open the modal dialog filled with the response
     * @param url - The URL of the ajax call
     * @param method - The method (GET/POST)
     * @param data? - The object data to send when the method is POST
     */
    public static OpenModalDialog(url: string, method: string, data?: object): void {
        $.ajax(url, {
            type: method,
            beforeSend: (xhr: JQuery.jqXHR): void => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $("input:hidden[name='__RequestVerificationToken']").val().toString());
            },
            contentType: "application/json",
            data: data !== undefined ? JSON.stringify(data) : "",
            cache: false,
        }).done((result: string): void => {
            $("#myModalBody").html(result);
            $("#myModal").modal("show");
        });
    }

    /**
     * Parse all the input elements in JSON format
     * @param element - The container to search for input elements
     * @returns The serialized JSON string
     */
    public static SerializeJSON(element: JQuery): string {
        // Ref: https://github.com/marioizquierdo/jquery.serializeJSON#options
        const settings: SerializeJSONSettings = {
            useIntKeysAsArrayIndex: true,
        };

        return <string>element.serializeJSON(settings);
    }
}
