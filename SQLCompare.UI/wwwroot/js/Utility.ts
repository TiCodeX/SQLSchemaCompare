
class Utility {

    static EncodeHtmlEntities(s) {
        return $("<div/>").text(s).html();
    }

    static DecodeHtmlEntities(s) {
        return $("<div/>").html(s).text();
    }

    static OpenModalDialog(url) {
        $.ajax(url, {
            type: "GET",
            cache: false
        }).done(result => {
            $("#myModalBody").html(result);
            $("#myModal").modal("show");
        });
    }
}