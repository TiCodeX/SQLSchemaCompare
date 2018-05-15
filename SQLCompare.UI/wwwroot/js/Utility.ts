
class Utility {

    static EncodeHtmlEntities(s: string) {
        return $("<div/>").text(s).html();
    }

    static DecodeHtmlEntities(s: string) {
        return $("<div/>").html(s).text();
    }

    static OpenModalDialog(url: string, method: string, data: object = null) {
        $.ajax(url, {
            type: method,
            beforeSend: xhr => {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $("input:hidden[name='__RequestVerificationToken']").val().toString());
            },
            contentType: "application/json",
            data: data ? JSON.stringify(data) : "",
            cache: false
        }).done(result => {
            $("#myModalBody").html(result);
            $("#myModal").modal("show");
        });
    }
}
