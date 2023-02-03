/*================================================================================
	Item Name: Materialize - Material Design Admin Template
	Version: 5.0
	Author: PIXINVENT
	Author URL: https://themeforest.net/user/pixinvent/portfolio
================================================================================

NOTE:
------
PLACE HERE YOUR OWN JS CODES AND IF NEEDED.
WE WILL RELEASE FUTURE UPDATES SO IN ORDER TO NOT OVERWRITE YOUR CUSTOM SCRIPT IT'S BETTER LIKE THIS. */

/*=====Toast Notification=====*/

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

$(document).ready(function () {
    $('.modal').modal();
    $('.tooltipped').tooltip();
    $('#filter-adv').on('click', function () {
        $('#filter').slideToggle();
    });

    /*Date picker*/
    $('.datepicker').datepicker({
        format: 'dd/mm/yyyy',
        container: "body"
    });

    /*Ajax*/
    $(document).ajaxStart(function () {
        $('.ajax-wrap').show();
    }).ajaxStop(function () {
        $('.ajax-wrap').hide();
    });

});

window.addEventListener('DOMContentLoaded', function () {

    /*
     * Lấy dữ liệu trung bình
     * Trả về Array với các giá trị bằng nhau và số lượng phần tử bằng array truyền vào     
     */
    function getAverage(csData,num){
        var data = csData.datasets[num].data;
        var sum = 0;
        var arr = []
        for (let i = 0; i < data.length; i++) {
            sum += data[i];
        }
        for (let i = 0; i < data.length; i++) {
            arr[i] = Math.round(sum / data.length);
        }
        return arr;
    }
  
    /*
     Chart doanh thu Yên Lãng HN
     */
    if (document.getElementById('chartCs1') != undefined) {
        var cs1Data = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [
                {
                    label: 'Doanh thu',
                    data: [24000000, 19000000, 66000000, 45000000, 87000000, 99000000, 22000000, 11000000, 55000000, 77000000, 99000000, 50000000],
                    backgroundColor: [
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                    ],
                    borderWidth: 1
                }
            ]
        }
        var ctx = document.getElementById('chartCs1').getContext('2d');
        var chartCs1 = new Chart(ctx, {
            type: 'bar',
            data: cs1Data,
            options: {
                title: {
                    display: true,
                    fontSize: 16,
                    padding:15,
                    fontStyle: 'bold',
                    text: 'ZIM Yên Lãng',
             
                },
                layout: {
                    padding: {
                        left: 15,
                        right: 0,
                        top: 0,
                        bottom: 0
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                         
                        },
                    }],
                    xAxes: [
                        {                
                            ticks: {
                                fontSize: 8
                            },
                            display: true,
                            gridLines: {
                                drawOnChartArea: false,
                            },
                        }
                    ]

                }
            }
        });
        var averageObj = {
            label: 'Trung bình',
            type: 'line',
            data: getAverage(cs1Data, 0),
            fill: false,
            borderColor: '#E3004D',
            backgroundColor:'#E3004D',
            options: {
                scales: {
                    xAxes: [{
                        beginAtZero:true
                    }]
                }
            }

        }
        chartCs1.data.datasets.unshift(averageObj);
        chartCs1.update();        
    }

    /*
     Chart doanh thu Thái Thịnh HN
     */
    if (document.getElementById('chartCs2') != undefined) {
        var cs2Data = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [
                {
                    label: 'Doanh thu',
                    data: [24000000, 19000000, 11000000, 22000000, 87000000, 66000000, 22000000, 11000000, 55000000, 77000000, 89000000, 50000000],
                    backgroundColor: [
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                    ],
                    borderWidth: 1
                }
            ]
        }
        var ctx = document.getElementById('chartCs2').getContext('2d');
        var chartCs2 = new Chart(ctx, {
            type: 'bar',
            data: cs2Data,
            options: {
                title: {
                    display: true,
                    fontSize: 16,
                    fontStyle: 'bold',
                    text: 'ZIM Thái Thịnh',
                    padding: 15
                },
                layout: {
                    padding: {
                        left: 15,
                        right: 0,
                        top: 0,
                        bottom: 0
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,

                        },
                    }],
                    xAxes: [
                        {
                            ticks: {
                                fontSize: 8
                            },
                            display: true,
                            gridLines: {
                                drawOnChartArea: false,
                            },
                        }
                    ]

                }
            }
        });
        var averageObj = {
            label: 'Trung bình',
            type: 'line',
            data: getAverage(cs2Data, 0),
            fill: false,
            borderColor: '#E3004D',
            backgroundColor: '#E3004D',
            options: {
                scales: {
                    xAxes: [{
                        beginAtZero: true
                    }]
                }
            }

        }
        chartCs2.data.datasets.unshift(averageObj);
        chartCs2.update();
    }

    /*
    Chart doanh thu ZIM Quận 5 HCM
    */
    if (document.getElementById('chartCs3') != undefined) {
        var cs3Data = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [
                {
                    label: 'Doanh thu',
                    data: [24000000, 19000000, 11000000, 22000000, 87000000, 66000000, 22000000, 11000000, 55000000, 77000000, 89000000, 50000000],
                    backgroundColor: [
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                    ],
                    borderWidth: 1
                }
            ]
        }
        var ctx = document.getElementById('chartCs3').getContext('2d');
        var chartCs3 = new Chart(ctx, {
            type: 'bar',
            data: cs3Data,
            options: {
                title: {
                    display: true,
                    fontSize: 16,
                    fontStyle: 'bold',
                    text: 'ZIM Quận 5',
                    padding: 15
                },
                layout: {
                    padding: {
                        left: 15,
                        right: 0,
                        top: 0,
                        bottom: 0
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,

                        },
                    }],
                    xAxes: [
                        {
                            ticks: {
                                fontSize: 8
                            },
                            display: true,
                            gridLines: {
                                drawOnChartArea: false,
                            },
                        }
                    ]

                }
            }
        });
        var averageObj = {
            label: 'Trung bình',
            type: 'line',
            data: getAverage(cs3Data, 0),
            fill: false,
            borderColor: '#E3004D',
            backgroundColor: '#E3004D',
            options: {
                scales: {
                    xAxes: [{
                        beginAtZero: true
                    }]
                }
            }

        }
        chartCs3.data.datasets.unshift(averageObj);
        chartCs3.update();
    }

    /*
     Chart doanh thu TÂN BÌNH HCM
     */
    if (document.getElementById('chartCs4') != undefined) {
        var cs4Data = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [
                {
                    label: 'Doanh thu',
                    data: [24000000, 19000000, 66000000, 45000000, 87000000, 99000000, 22000000, 11000000, 55000000, 77000000, 99000000, 50000000],
                    backgroundColor: [
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                    ],
                    borderWidth: 1
                }
            ]
        }
        var ctx = document.getElementById('chartCs4').getContext('2d');
        var chartCs4 = new Chart(ctx, {
            type: 'bar',
            data: cs4Data,
            options: {
                title: {
                    display: true,
                    fontSize: 16,
                    padding: 15,
                    fontStyle: 'bold',
                    text: 'ZIM Tân Bình',
                },
                layout: {
                    padding: {
                        left: 15,
                        right: 0,
                        top: 0,
                        bottom: 0
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,
                        
                        },
                    }],
                    xAxes: [
                        {   
                            ticks:{
                                fontSize: 8
                            },
                            display: true,
                            gridLines: {
                                drawOnChartArea: false,
                            },
                        }
                    ]

                }
            }
        });
        var averageObj = {
            label: 'Trung bình',
            type: 'line',
            data: getAverage(cs4Data, 0),
            fill: false,
            borderColor: '#E3004D',
            backgroundColor: '#E3004D',
            options: {
                scales: {
                    xAxes: [{
                        beginAtZero: true
                    }]
                }
            }

        }
        chartCs4.data.datasets.unshift(averageObj);
        chartCs4.update();
    }

        /*
      Chart doanh thu Bình Thạnh HCM
      */
        if (document.getElementById('chartCs5') != undefined) {
            var cs5Data = {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [
                    {
                        label: 'Doanh thu',
                        data: [24000000, 19000000, 66000000, 45000000, 87000000, 99000000, 22000000, 11000000, 55000000, 77000000, 99000000, 50000000],
                        backgroundColor: [
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                            '#00CCF1',
                        ],
                        borderWidth: 1
                    }
                ]
            }
            var ctx = document.getElementById('chartCs5').getContext('2d');
            var chartCs5 = new Chart(ctx, {
                type: 'bar',
                data: cs5Data,
                options: {
                    title: {
                        display: true,
                        fontSize: 16,
                        padding: 15,
                        fontStyle: 'bold',
                        text: 'ZIM Bình Thạnh',
                    },
                    layout: {
                        padding: {
                            left: 15,
                            right: 0,
                            top: 0,
                            bottom: 0
                        }
                    },
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true,

                            },
                        }],
                        xAxes: [
                            {
                                ticks: {
                                    fontSize: 8
                                },
                                display: true,
                                gridLines: {
                                    drawOnChartArea: false,
                                },
                            }
                        ]

                    }
                }
            });
            var averageObj = {
                label: 'Trung bình',
                type: 'line',
                data: getAverage(cs5Data, 0),
                fill: false,
                borderColor: '#E3004D',
                backgroundColor: '#E3004D',
                options: {
                    scales: {
                        xAxes: [{
                            beginAtZero: true
                        }]
                    }
                }

            }
            chartCs5.data.datasets.unshift(averageObj);
            chartCs5.update();
        }

    /* Chart phân bố thành phố*/
    if (document.getElementById('chartcity') != undefined) {
        var ctx = document.getElementById('chartcity').getContext('2d');
        var chartCity = new Chart(ctx, {
            type: 'pie',
            data: {                
                labels: ['Hồ Chí Minh', 'Hà Nội'],
                datasets: [
                    {
                        data: [35200, 27880],
                        backgroundColor: [
                            '#00C31B',
                            '#FFAD33'
                        ]
                    }
                ]
                },
            options: {
                title: {
                    display: true,
                    text: 'Phân bố theo thành phố',
                    position: 'top',
                    fontSize: 16,
                    fontStyle:'bold'
                },
                legends: {
                    display:true,
                    position:'top'
                },
                rotation: -0.7 * Math.PI
            }
        });
    }



    /* Chart phân bố theo quận HN*/
    if (document.getElementById('chart-village-hn') != undefined) {
        var ctx = document.getElementById('chart-village-hn').getContext('2d');
        var chartVillageHN = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['Đống Đa'],
                datasets: [
                    {
                        data: [35200],
                        backgroundColor: [
                            '#FFAD33'
                        ]
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: 'Phân bố theo quận Hà Nội',
                    position: 'top',
                    fontSize: 16,
                    fontStyle: 'bold'
                },
                legends: {
                    display: true,
                    position: 'top'
                },
                rotation: -0.7 * Math.PI
            }
        });
    }


    /* Chart phân bố theo quận HN*/
    if (document.getElementById('chart-village-hcm') != undefined) {
        var ctx = document.getElementById('chart-village-hcm').getContext('2d');
        var chartVillageHCM = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['Bình Thạnh','Tân Bình','Quận 5'],
                datasets: [
                    {
                        data: [35200,22000,11000],
                        backgroundColor: [
                            '#FFAD33',
                            '#D05926',
                            '#79C05C'
                        ]
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: 'Phân bố theo quận HCM',
                    position: 'top',
                    fontSize: 16,
                    fontStyle: 'bold'
                },
                legends: {
                    display: true,
                    position: 'top'
                },
                rotation: -0.7 * Math.PI
            }
        });
    }

    /*
     Chart số lượng thành viên tăng theo từng tháng
     */
    if (document.getElementById('chart-member') != undefined) {
        var csData = {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [
                {
                    label: 'Học viên',
                    data: [24000000, 19000000, 66000000, 45000000, 87000000, 99000000, 22000000, 11000000, 55000000, 77000000, 99000000, 50000000],
                    backgroundColor: [
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                        '#00CCF1',
                    ],
                    borderWidth: 1
                }
            ]
        }
        var ctx = document.getElementById('chart-member').getContext('2d');
        var chartCs = new Chart(ctx, {
            type: 'bar',
            data: csData,
            options: {
                title: {
                    display: true,
                    fontSize: 16,
                    padding: 15,
                    fontStyle: 'bold',
                    text: 'Học viên tăng ',

                },
                layout: {
                    padding: {
                        left: 15,
                        right: 0,
                        top: 0,
                        bottom: 0
                    }
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            beginAtZero: true,

                        },
                    }],
                    xAxes: [
                        {
                            ticks: {
                                fontSize: 8
                            },
                            display: true,
                            gridLines: {
                                drawOnChartArea: false,
                            },
                        }
                    ]

                }
            }
        });
        var averageObj = {
            label: 'Trung bình',
            type: 'line',
            data: getAverage(csData, 0),
            fill: false,
            borderColor: '#E3004D',
            backgroundColor: '#E3004D',
            options: {
                scales: {
                    xAxes: [{
                        beginAtZero: true
                    }]
                }
            }

        }
        chartCs.data.datasets.unshift(averageObj);
        chartCs.update();
    }

});