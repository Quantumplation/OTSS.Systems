// initialize lunch option autocomplete
$(function () {
    var hound = new Bloodhound({
        remote: {
            url: "../api/Lunch/Options/%QUERY",
            wildcard: "%QUERY"
        },
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        datumTokenizer: Bloodhound.tokenizers.whitespace,
        identify: function (obj) { return obj.Id; },
        sorter: function (a, b) {
            var an = a.Name.toLowerCase(),
                bn = b.Name.toLowerCase();
            if (an < bn) return -1;
            if (an > bn) return 1;
            return 0;
        }
    });
    hound.initialize();
    $("input.lunch-option").typeahead({
        minLength: 1,
        highlight: true
    }, {
        source: hound.ttAdapter(),
        display: "Name"
    });
});