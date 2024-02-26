$.ajaxSetup({ cache: false });
$(document).ready(function () {
    $('.table-responsive').on('click')
});
var tables = {
    loader: '',
    config: {},
    items_arr: {},

    get: function (table, url,filter) {

        var $tableContainer = $("#" + table),
            $table =  $tableContainer;
        $.ajax({
            type: "GET",
            dataType: "html",
            url: url,
            cache: false,
            data: filter,
            beforeSend: function () {
                loader.add($tableContainer);
            },
            success: function (res) {
                $table.html(res);

            },
            complete: function () {
                loader.remove($tableContainer);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                dd('table.get.fail', jqXHR);
            }
        });
    },
    refresh(table, callbackurl, filter = {}) {
        this.get(table, callbackurl, filter)
    }
}

