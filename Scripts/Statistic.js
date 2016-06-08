$(document).ready(function () {
    // add button style
    $("[name='poll_bar'").addClass("btn btn-default");
    // Add button style with alignment to left with margin.
    $("[name='poll_bar'").css({ "text-align": "left", "margin": "5px" });

    var max = +$("[name='val_max'").val(); //Max value to display

    //loop
    $("[name='poll_bar'").each(

            function (i) {
                //get poll value
                var bar_width = (parseFloat($("[name='poll_val'").eq(i).text()));
                bar_width = (bar_width / max) * 100;
                $("[name='poll_bar'").eq(i).width(bar_width * 0.7 + "%");

                //Define rules.
                if (bar_width >= 70) { $("[name='poll_bar'").eq(i).addClass("btn btn-sm btn-success") }
                if (bar_width < 70) { $("[name='poll_bar'").eq(i).addClass("btn btn-sm btn-warning") }
                if (bar_width <= 30) { $("[name='poll_bar'").eq(i).addClass("btn btn-sm btn-danger") }

                //Hide dril down divs
                $("#" + $("[name='poll_bar'").eq(i).text()).hide();
            });

});