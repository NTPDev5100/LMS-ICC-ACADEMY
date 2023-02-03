(function (root, factory) {
    try {
        // commonjs
        if (typeof exports === 'object') {
            module.exports = factory();
            // global
        } else {
            root.toast = factory();
        }
    } catch (error) {
        console.log('Isomorphic compatibility is not supported at this time for toast.')
    }
})(this, function () {

    // We need DOM to be ready
    if (document.readyState === 'complete') {
        init();
    } else {
        window.addEventListener('DOMContentLoaded', init);
    }

    // Create toast object
    toast = {
        // In case toast creation is attempted before dom has finished loading!
        create: function () {
            console.error([
                'DOM has not finished loading.',
                '\tInvoke create method when DOM\s readyState is complete'
            ].join('\n'))
        }
    };
    var autoincrement = 0;

    // Initialize library
    function init() {
        // Toast container
        var container = document.createElement('div');
        container.id = 'cooltoast-container';
        document.body.appendChild(container);


        // Replace create method when DOM has finished loading
        toast.create = function (options) {
            var toast = document.createElement('div');
            toast.id = ++autoincrement;
            toast.id = 'toast-' + toast.id;
            toast.className = 'cooltoast-toast clear';
            var toastContent = document.createElement('div');
            toastContent.className = 'toast-content';
            // background
            if (options.classBackground) {
                toast.className += ' ' + options.classBackground;
            }
            // title
            if (options.title) {

                var h4 = document.createElement('h4');
                h4.className = 'cooltoast-title';
                h4.innerHTML = options.title;
                toastContent.appendChild(h4);
            }

            // text
            if (options.text) {
                var p = document.createElement('p');
                p.className = 'cooltoast-text';
                p.innerHTML = options.text;
                toastContent.appendChild(p);
            }

            // icon
            if (options.icon) {
                var icon = document.createElement('i');
                icon.innerHTML = options.icon;
                icon.className = 'cooltoast-icon material-icons';
                toast.appendChild(icon);
            }

            // click callback
            if (typeof options.callback === 'function') {
                toast.addEventListener('click', options.callback);
            }

            // toast api
            toast.hide = function () {
                toast.className += ' cooltoast-fadeOut';
                toast.addEventListener('animationend', removeToast, false);
            };

            // autohide
            if (options.timeout) {
                setTimeout(toast.hide, options.timeout);
            }
            // else setTimeout(toast.hide, 2000);

            if (options.type) {
                toast.className += ' cooltoast-' + options.type;
            }
            toast.appendChild(toastContent);
            toast.addEventListener('click', toast.hide);


            function removeToast() {
                var container = document.getElementById('cooltoast-container');
                container.removeChild(toast);
            }

            document.getElementById('cooltoast-container').appendChild(toast);
            return toast;

        }
    }

    return toast;

});
/*===========*/
//định dạng số thực
$(document).on("keyup", "input[data-type='currency']", function () {
    formatCurrency($(this));
});
$(document).on("blur", "input[data-type='currency']", function () {
    formatCurrency($(this), "blur");
});
function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",")
}
function formatCurrency(input, blur) {
    // appends $ to value, validates decimal side
    // and puts cursor back in right position.

    // get input value
    var input_val = input.val();

    // don't validate empty input
    if (input_val === "") { return; }

    // original length
    var original_len = input_val.length;

    // initial caret position
    var caret_pos = input.prop("selectionStart");

    // check for decimal
    if (input_val.indexOf(".") >= 0) {

        // get position of first decimal
        // this prevents multiple decimals from
        // being entered
        var decimal_pos = input_val.indexOf(".");

        // split number by decimal point
        var left_side = input_val.substring(0, decimal_pos);
        var right_side = input_val.substring(decimal_pos);

        // add commas to left side of number
        left_side = formatNumber(left_side);

        // validate right side
        right_side = formatNumber(right_side);

        // On blur make sure 2 numbers after decimal
        if (blur === "blur") {
            right_side += "00";
        }

        // Limit decimal to only 2 digits
        right_side = right_side.substring(0, 2);

        // join number by .
        input_val = left_side + "." + right_side;

    } else {
        // no decimal entered
        // add commas to number
        // remove all non-digits
        input_val = formatNumber(input_val);
        input_val = input_val;

        // final formatting
        if (blur === "blur") {
            //input_val += ".00";
            input_val;
        }
    }

    // send updated string to input
    input.val(input_val);

    // put caret back in the right position
    var updated_len = input_val.length;
    caret_pos = updated_len - original_len + caret_pos;
    input[0].setSelectionRange(caret_pos, caret_pos);
}

function formatPoint(n) {

    return n.replace(/\D/g, "").replace(/\B(?=(\d{2})+(?!\d))/g, ".")
}
$('body').on('input', '[data-type="point"]', function () {
    formatPoints($(this));
});

function formatPoints(input, blur) {
    // appends $ to value, validates decimal side
    // and puts cursor back in right position.

    // get input value
    var input_val = input.val();

    // don't validate empty input
    if (input_val === "") { return; }

    // original length
    var original_len = input_val.length;

    // initial caret position
    var caret_pos = input.prop("selectionStart");

    // check for decimal
    if (input_val.indexOf(".") >= 0) {

        // get position of first decimal
        // this prevents multiple decimals from
        // being entered
        var decimal_pos = input_val.indexOf(".");

        // split number by decimal point
        var left_side = input_val.substring(0, decimal_pos);
        var right_side = input_val.substring(decimal_pos);

        // add commas to left side of number
        left_side = formatPoint(left_side);

        // validate right side
        right_side = formatPoint(right_side);

        // On blur make sure 2 numbers after decimal
        if (blur === "blur") {
            right_side += "00";
        }

        // Limit decimal to only 2 digits
        right_side = right_side.substring(0, 2);

        // join number by .
        input_val = left_side + "." + right_side;

    } else {
        // no decimal entered
        // add commas to number
        // remove all non-digits
        input_val = formatPoint(input_val);
        input_val = input_val;

        // final formatting
        if (blur === "blur") {
            //input_val += ".00";
            input_val;
        }
    }

    // send updated string to input
    input.val(input_val);

    // put caret back in the right position
    var updated_len = input_val.length;
    caret_pos = updated_len - original_len + caret_pos;
    input[0].setSelectionRange(caret_pos, caret_pos);

}
