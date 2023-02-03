$(document).ready(function() {
    $('body').on('click', function() {
        $('.off-canvas').removeClass('show');
    });
    $('.open-chat-nav').on('click', function(e) {
        e.stopPropagation();
        var target = $(this).attr('href');
        $(target).addClass('show');
    });
    $('.close-sidenav').on('click', function(e) {
        e.preventDefault();
        $(this).closest('.off-canvas').removeClass('show');
    });
    $('body').on('click', '.off-canvas', function(e) {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
        return false;
    });

    //Drag tabble
    var elementScroll = document.querySelectorAll(".table-responsive");

    if (elementScroll != undefined || elementScroll != null) {
        elementScroll.forEach(function(element) {
            var mx = 0;
            element.addEventListener("mousedown", function(e) {
                this.sx = this.scrollLeft;
                mx = e.pageX - this.offsetLeft;

                this.addEventListener("mousemove", mouseMoveFunction);
            });
            element.addEventListener("mouseup", function(e) {
                this.removeEventListener("mousemove", mouseMoveFunction);
                mx = 0;
            });

            function mouseMoveFunction(e) {
                var mx2 = e.pageX - this.offsetLeft;
                if (mx) this.scrollLeft = this.sx + mx - mx2;
            }
        });
    }


    //detail info
    $("body").on("click", ".edit-mode", function(e) {
        e.preventDefault();
        $($(this).attr("data-target")).addClass("show");
        $(".detail-fixed").animate({ scrollTop: 0 }, "fast");
    });
    $("body").on("click", ".close-editmode,.bg-overlay", function(e) {
        $(this)
            .parents(".detail-fixed")
            .removeClass("show");
    });

    // Fix table head
    function tableFixHead(e) {
        const el = e.target,
            sT = el.scrollTop - 1;
        el.querySelectorAll("thead th").forEach(th =>
            th.style.transform = `translateY(${sT}px)`
        );
    }
    document.querySelectorAll(".tableFixHead").forEach(el =>
        el.addEventListener("scroll", tableFixHead)
    );
    //Select2 in modal 

    $('.select2').each(function() {
        let $this = $(this);
        let $parent = $(this).closest('.modal');
        if ($parent.length > 0) {
            $this.select2({
                dropdownParent: $parent.find('.modal-body'),
            })
        } else {
            $this.select2();
        }
    });


    $('#scroll-to-top').on('click', function() {
        $('html,body').animate({
            scrollTop: 0
        }, 1000);
    });
    //datepicker
    $.datetimepicker.setLocale('en');
    $('.datetimepicker').each(function() {
        $(this).attr('autocomplete', 'off');
        $(this).datetimepicker({
            format: 'd/m/Y H:i',
            formatDate: 'd/m/Y',
            allowTimes: [
                '06:00',
                '06:15',
                '06:30',
                '06:45',
                '07:00',
                '07:15',
                '07:30',
                '07:45',
                '08:00',
                '08:15',
                '08:30',
                '08:45',
                '09:00',
                '09:15',
                '09:30',
                '09:45',
                '10:00',
                '10:15',
                '10:30',
                '10:45',
                '11:00',
                '11:15',
                '11:30',
                '11:45',
                '12:00',
                '12:15',
                '12:30',
                '12:45',
                '13:00',
                '13:15',
                '13:30',
                '13:45',
                '14:00',
                '14:15',
                '14:30',
                '14:45',
                '15:00',
                '15:15',
                '15:30',
                '15:45',
                '16:00',
                '16:15',
                '16:30',
                '16:45',
                '17:00',
                '17:15',
                '17:30',
                '17:45',
                '18:00',
                '18:15',
                '18:30',
                '18:45',
                '19:00',
                '19:15',
                '19:30',
                '19:45',
                '20:00',
                '20:15',
                '20:30',
                '20:45',
                '21:00',
                '21:15',
                '21:30',
                '21:45',
                '22:00',
                '22:15',
                '22:30',
                '22:45',
                '23:00',
                '23:15',
                '23:30',
                '23:45',
            ],
            // theme:'dark',
            // defaultDate:new Date(),
            onShow: function(ct, element) {
                if ($(element).hasClass('to-date')) {
                    var minDate = $(element).closest('.row').find('.from-date').val();
                    console.log(minDate);
                    this.setOptions({
                        minDate: minDate ? minDate : false,
                    })
                }
                if ($(element).hasClass('date-only')) {
                    this.setOptions({
                        timepicker: false,
                        format: 'd/m/Y',
                        mask: true,
                    })
                }
            }
        });
    })
    $('body').on('show.bs.dropdown', '.table-responsive', function() { $(this).css("overflow", "visible"); }).on('hide.bs.dropdown', '.table-responsive', function() { $(this).css("overflow", "auto"); });

    //sticky tinymce
    const replaceInputType = (type, value) => {
        switch (type) {
            case 'currency':
                {
                    let regx = /\D+/g;
                    let number = value.replace(regx, "");
                    return number.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1,");
                    break;
                }
            case 'number':
                {
                    return value.replace(/[^0-9]/, "");
                    break;
                }
        }
    }
    $('body').on('input', '[data-type="number"],[data-type="currency"]', function() {
        let value = replaceInputType($(this).attr('data-type'), $(this).val());
        $(this).val(value);
    });
});