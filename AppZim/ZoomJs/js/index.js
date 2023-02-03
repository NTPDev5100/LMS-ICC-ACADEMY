window.addEventListener('DOMContentLoaded', function (event) {
    websdkready();
});

function websdkready() {
    var testTool = window.testTool;
    if (testTool.isMobileDevice()) {
        vConsole = new VConsole();
    }
    //console.log("checkSystemRequirements");
    //console.log(JSON.stringify(ZoomMtg.checkSystemRequirements()));

    // it's option if you want to change the WebSDK dependency link resources. setZoomJSLib must be run at first
    // if (!china) ZoomMtg.setZoomJSLib('https://source.zoom.us/1.9.1/lib', '/av'); // CDN version default
    // else ZoomMtg.setZoomJSLib('https://jssdk.zoomus.cn/1.9.1/lib', '/av'); // china cdn option
    // ZoomMtg.setZoomJSLib('http://localhost:9999/node_modules/@zoomus/websdk/dist/lib', '/av'); // Local version default, Angular Project change to use cdn version
    ZoomMtg.preLoadWasm(); // pre download wasm file to save time.

    var API_KEY = "BQMrBCtJRsOgShv81OQK4w";
    /**
     * NEVER PUT YOUR ACTUAL API SECRET IN CLIENT SIDE CODE, THIS IS JUST FOR QUICK PROTOTYPING
     * The below generateSignature should be done server side as not to expose your api secret in public
     * You can find an eaxmple in here: https://marketplace.zoom.us/docs/sdk/native-sdks/web/essential/signature
     */
    var API_SECRET = "7ltYSsOvahjPOP8DobbEEzlKC60tJ4juPAxj";
    //if (testTool.getCookie("meeting_number")) {
    //    alert("haha")
    //    testTool.deleteAllCookies();
    //}
    // some help code, remember mn, pwd, lang to cookie, and autofill.
    //document.getElementById("display_name").value =
    //  "CDN" +
    //  ZoomMtg.getJSSDKVersion()[0] +
    //  testTool.detectOS() +
    //  "#" +
    //  testTool.getBrowserInfo();
    document.getElementById("display_name").value = "Mona Media";
    //document.getElementById("meeting_number").value = testTool.getCookie(
    //  "meeting_number"
    //);
    //document.getElementById("meeting_pwd").value = testTool.getCookie(
    //  "meeting_pwd"
    //);
    if (testTool.getCookie("meeting_lang"))
        document.getElementById("meeting_lang").value = testTool.getCookie(
            "meeting_lang"
        );

    document
        .getElementById("meeting_lang")
        .addEventListener("change", function (e) {
            testTool.setCookie(
                "meeting_lang",
                document.getElementById("meeting_lang").value
            );
            testTool.setCookie(
                "_zm_lang",
                document.getElementById("meeting_lang").value
            );
        });
    // copy zoom invite link to mn, autofill mn and pwd.
    document
        .getElementById("meeting_number")
        .addEventListener("input", function (e) {
            var tmpMn = e.target.value.replace(/([^0-9])+/i, "");
            if (tmpMn.match(/([0-9]{9,11})/)) {
                tmpMn = tmpMn.match(/([0-9]{9,11})/)[1];
            }
            var tmpPwd = e.target.value.match(/pwd=([\d,\w]+)/);
            if (tmpPwd) {
                document.getElementById("meeting_pwd").value = tmpPwd[1];
                testTool.setCookie("meeting_pwd", tmpPwd[1]);
            }
            document.getElementById("meeting_number").value = tmpMn;
            testTool.setCookie(
                "meeting_number",
                document.getElementById("meeting_number").value
            );
        });

    document.getElementById("clear_all").addEventListener("click", function (e) {
        testTool.deleteAllCookies();
        document.getElementById("display_name").value = "";
        document.getElementById("meeting_number").value = "";
        document.getElementById("meeting_pwd").value = "";
        document.getElementById("meeting_lang").value = "en-US";
        document.getElementById("meeting_role").value = 0;
        window.location.href = "/home/zoom";
    });

    // click join meeting button
    document
        .getElementById("join_meeting")
        .addEventListener("click", function (e) {
            e.preventDefault();
            var meetingConfig = testTool.getMeetingConfig();
            if (!meetingConfig.mn || !meetingConfig.name) {
                //alert("Meeting number or username is empty");
                window.location.href = "/Admin/Course/CourseList";
                console.log("Meeting number or username is empty");
                return false;
            }


            //testTool.setCookie("meeting_number", meetingConfig.mn);
            //testTool.setCookie("meeting_pwd", meetingConfig.pwd);

            var signature = ZoomMtg.generateSignature({
                meetingNumber: meetingConfig.mn,
                apiKey: API_KEY,
                apiSecret: API_SECRET,
                role: meetingConfig.role,
                success: function (res) {
                    console.log(res.result);
                    meetingConfig.signature = res.result;
                    meetingConfig.apiKey = API_KEY;
                    let joinUrl = "/admin/zoommeeting/loadmeeting?" + testTool.serialize(meetingConfig);
                    //console.log(joinUrl);
                    //window.open(joinUrl, "_blank");
                    //window.location.href = "/home/meeting?data=" + testTool.serialize(meetingConfig);
                    window.location.href = joinUrl;
                },
            });
        });

    function copyToClipboard(elementId) {
        var aux = document.createElement("input");
        aux.setAttribute("value", document.getElementById(elementId).getAttribute('link'));
        document.body.appendChild(aux);
        aux.select();
        document.execCommand("copy");
        document.body.removeChild(aux);
    }

    // click copy jon link button
    window.copyJoinLink = function (element) {
        var meetingConfig = testTool.getMeetingConfig();
        if (!meetingConfig.mn || !meetingConfig.name) {
            alert("Meeting number or username is empty");
            return false;
        }
        var signature = ZoomMtg.generateSignature({
            meetingNumber: meetingConfig.mn,
            apiKey: API_KEY,
            apiSecret: API_SECRET,
            role: meetingConfig.role,
            success: function (res) {
                console.log(res.result);
                meetingConfig.signature = res.result;
                meetingConfig.apiKey = API_KEY;
                var joinUrl =
                    testTool.getCurrentDomain() +
                    "/home/meeting?" +
                    testTool.serialize(meetingConfig);
                document.getElementById('copy_link_value').setAttribute('link', joinUrl);
                copyToClipboard('copy_link_value');

            },
        });
    };

}
