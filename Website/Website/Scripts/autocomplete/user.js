// initialize user autocomplete
$(function () {
    var hound = new Bloodhound({
        prefetch: {
            url: "../api/Users/"
        },
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace("UserName"),
        identify: function (obj) { return obj.Id; },
        sorter: function (a, b) {
            var an = a.UserName.toLowerCase(),
                bn = b.UserName.toLowerCase();
            if (an < bn) return -1;
            if (an > bn) return 1;
            return 0;
        }
    });
    hound.initialize();
    $("input.user-autocomplete").typeahead({
        minLength: 1,
        highlight: true
    }, {
        source: hound.ttAdapter(),
        display: "UserName"
    });
});