$(document).ready(function () {



    // checkbox tình trạng bật tắt
    $('body').on('click', '.btnActive', function (e) {

        e.preventDefault();
        var btn = $(this);
        var id = btn.data("id");
        $.ajax({
            url: activeUrl, // chay vào IActionResult IsActicve(int id)
            type: 'POST',
            data: { id: id },
            success: function (rs) {
                if (rs.success) {
                    if (rs.isDelete) {
                        btn.html("<i class='fa fa-check text-success'></i>");
                    } else {
                        btn.html("<i class='fas fa-times text-danger'></i>");
                    }
                }

            }
        });
    });
});