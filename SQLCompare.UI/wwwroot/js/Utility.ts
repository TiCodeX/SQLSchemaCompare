
class Utility {

    static EncodeHtmlEntities(s) {
        return $("<div/>").text(s).html();
    }

    static DecodeHtmlEntities(s) {
        return $("<div/>").html(s).text();
    }

}